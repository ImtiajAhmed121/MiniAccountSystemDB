using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MiniAccountSystemDB.Pages
{
    public class VoucherModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public VoucherModel(IConfiguration configuration)
        {
            _configuration = configuration;

            // Initialize non-nullables
            ReferenceNo = string.Empty;
            VoucherType = string.Empty;
            AccountList = new List<SelectListItem>();
        }

        [BindProperty]
        public string VoucherType { get; set; }

        [BindProperty]
        public DateTime VoucherDate { get; set; } = DateTime.Today;

        [BindProperty]
        public string ReferenceNo { get; set; }

        [BindProperty]
        public List<VoucherEntry> Entries { get; set; } = new() { new(), new() };

        public List<SelectListItem> AccountList { get; set; }

        public class VoucherEntry
        {
            public int AccountId { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
        }

        public void OnGet()
        {
            LoadAccounts();
        }

        public IActionResult OnPost()
        {
            LoadAccounts();

            // Validate entry list
            if (Entries == null || Entries.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Please enter at least one voucher line.");
                return Page();
            }

            // Ensure at least one row has a value
            bool hasValidRow = Entries.Exists(e => e.Debit > 0 || e.Credit > 0);
            if (!hasValidRow)
            {
                ModelState.AddModelError(string.Empty, "Each row must have at least one Debit or Credit value.");
                return Page();
            }

            // Validate Debit == Credit
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

            // Prepare table-valued parameter
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

            // Insert using stored procedure
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
    }
}
