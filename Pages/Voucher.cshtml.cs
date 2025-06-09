// ✅ Complete and corrected VoucherModel with Export to Excel functionality

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using OfficeOpenXml;

namespace MiniAccountSystemDB.Pages
{
    [Authorize(Roles = "Admin,Accountant")]
    public class VoucherModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public VoucherModel(IConfiguration configuration)
        {
            _configuration = configuration;
            ReferenceNo = string.Empty;
            VoucherType = string.Empty;
            AccountList = new List<SelectListItem>();
        }

        [BindProperty] public string VoucherType { get; set; }
        [BindProperty] public DateTime VoucherDate { get; set; } = DateTime.Today;
        [BindProperty] public string ReferenceNo { get; set; }
        [BindProperty] public List<VoucherEntry> Entries { get; set; } = new() { new(), new() };

        public List<SelectListItem> AccountList { get; set; }

        public class VoucherEntry
        {
            public int AccountId { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
        }

        public class VoucherDisplay
        {
            public string VoucherNo { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public string Description { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }

        public void OnGet()
        {
            LoadAccounts();
        }

        public IActionResult OnPost()
        {
            LoadAccounts();

            if (Entries == null || Entries.Count == 0 || !Entries.Exists(e => e.Debit > 0 || e.Credit > 0))
            {
                ModelState.AddModelError(string.Empty, "Please enter valid debit or credit entries.");
                return Page();
            }

            decimal totalDebit = 0, totalCredit = 0;
            foreach (var entry in Entries)
            {
                totalDebit += entry.Debit;
                totalCredit += entry.Credit;
            }

            if (totalDebit != totalCredit)
            {
                ModelState.AddModelError(string.Empty, "Total Debit must equal Total Credit.");
                return Page();
            }

            var entryTable = new DataTable();
            entryTable.Columns.Add("AccountId", typeof(int));
            entryTable.Columns.Add("Debit", typeof(decimal));
            entryTable.Columns.Add("Credit", typeof(decimal));

            foreach (var entry in Entries)
            {
                if ((entry.Debit > 0 || entry.Credit > 0) && entry.AccountId > 0)
                {
                    entryTable.Rows.Add(entry.AccountId, entry.Debit, entry.Credit);
                }
            }

            try
            {
                using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();

                using SqlCommand cmd = new("sp_SaveVoucher", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VoucherType", VoucherType);
                cmd.Parameters.AddWithValue("@VoucherDate", VoucherDate);
                cmd.Parameters.AddWithValue("@ReferenceNo", ReferenceNo);

                var tvpParam = cmd.Parameters.AddWithValue("@Entries", entryTable);
                tvpParam.SqlDbType = SqlDbType.Structured;
                tvpParam.TypeName = "VoucherEntryType";

                cmd.ExecuteNonQuery();

                TempData["Success"] = "Voucher saved successfully!";
                return RedirectToPage();
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, "Database Error: " + ex.Message);
                return Page();
            }
        }

        private void LoadAccounts()
        {
            AccountList = new List<SelectListItem>();
            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            using SqlCommand cmd = new("SELECT Id, Name FROM ChartOfAccounts", conn);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                AccountList.Add(new SelectListItem
                {
                    Value = reader.GetInt32(0).ToString(),
                    Text = reader.GetString(1)
                });
            }
        }

        public IActionResult OnPostExport()
        {
            var vouchers = LoadVouchers();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Vouchers");

            worksheet.Cells[1, 1].Value = "Voucher No";
            worksheet.Cells[1, 2].Value = "Date";
            worksheet.Cells[1, 3].Value = "Reference No";
            worksheet.Cells[1, 4].Value = "Amount";

            for (int i = 0; i < vouchers.Count; i++)
            {
                var v = vouchers[i];
                worksheet.Cells[i + 2, 1].Value = v.VoucherNo;
                worksheet.Cells[i + 2, 2].Value = v.Date.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 3].Value = v.Description;
                worksheet.Cells[i + 2, 4].Value = v.Amount;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            var stream = new MemoryStream(package.GetAsByteArray());

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Vouchers.xlsx");
        }

        private List<VoucherDisplay> LoadVouchers()
        {
            var result = new List<VoucherDisplay>();
            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            using SqlCommand cmd = new(@"
        SELECT 
            v.Id AS VoucherNo,
            v.VoucherDate,
            v.ReferenceNo,
            SUM(e.Debit) - SUM(e.Credit) AS Amount
        FROM dbo.Vouchers v
        JOIN dbo.VoucherEntries e ON v.Id = e.VoucherId
        GROUP BY v.Id, v.VoucherDate, v.ReferenceNo
        ORDER BY v.Id DESC", conn);

            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new VoucherDisplay
                {
                    VoucherNo = reader["VoucherNo"].ToString() ?? "",
                    Date = reader.GetDateTime(1),
                    Description = reader["ReferenceNo"].ToString() ?? "",
                    Amount = reader.GetDecimal(3)
                });
            }

            return result;
        }

    }
}
