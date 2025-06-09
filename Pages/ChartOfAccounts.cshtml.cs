using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using System.IO;

namespace MiniAccountSystemDB.Pages
{
    [Authorize(Roles = "Admin,Accountant")]
    public class ChartOfAccountsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ChartOfAccountsModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        [BindProperty]
        public string ActionType { get; set; } = string.Empty;

        [BindProperty]
        public int? EditId { get; set; }

        [BindProperty]
        public int? DeleteId { get; set; }

        [BindProperty]
        public AccountInputModel Input { get; set; } = new();

        public List<Account> AllAccounts { get; set; } = new();

        public class Account
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string AccountType { get; set; } = string.Empty;
            public int? ParentId { get; set; }
        }

        public class AccountInputModel
        {
            public string Name { get; set; } = string.Empty;
            public string AccountType { get; set; } = string.Empty;
            public int? ParentId { get; set; }
        }

        public void OnGet()
        {
            LoadAccounts();
        }

        public IActionResult OnPost()
        {
            if (ActionType == "Delete" && DeleteId.HasValue)
            {
                DeleteAccount(DeleteId.Value);
                return RedirectToPage();
            }

            if (ActionType == "Edit" && EditId.HasValue)
            {
                var account = GetAccountById(EditId.Value);
                if (account != null)
                {
                    Input.Name = account.Name;
                    Input.AccountType = account.AccountType;
                    Input.ParentId = account.ParentId;
                    EditId = account.Id;
                }

                LoadAccounts();
                return Page();
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", EditId.HasValue ? "UPDATE" : "INSERT");
                    cmd.Parameters.AddWithValue("@Id", EditId.HasValue ? (object)EditId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Name", Input.Name);
                    cmd.Parameters.AddWithValue("@ParentId", (object?)Input.ParentId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccountType", Input.AccountType);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToPage();
        }

        public IActionResult OnPostExport()
        {
            LoadAccounts();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("ChartOfAccounts");

            // Header row
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "Account Type";
            worksheet.Cells[1, 4].Value = "Parent ID";

            // Data rows
            for (int i = 0; i < AllAccounts.Count; i++)
            {
                var acc = AllAccounts[i];
                worksheet.Cells[i + 2, 1].Value = acc.Id;
                worksheet.Cells[i + 2, 2].Value = acc.Name;
                worksheet.Cells[i + 2, 3].Value = acc.AccountType;
                worksheet.Cells[i + 2, 4].Value = acc.ParentId?.ToString() ?? "Root";
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream(package.GetAsByteArray());
            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ChartOfAccounts.xlsx");
        }

        private void LoadAccounts()
        {
            AllAccounts.Clear();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("SELECT Id, Name, AccountType, ParentId FROM ChartOfAccounts", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                AllAccounts.Add(new Account
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    AccountType = reader.GetString(2),
                    ParentId = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3)
                });
            }
        }

        private void DeleteAccount(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "DELETE");
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", DBNull.Value);
            cmd.Parameters.AddWithValue("@ParentId", DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountType", DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        private Account? GetAccountById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("SELECT Id, Name, AccountType, ParentId FROM ChartOfAccounts WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Account
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    AccountType = reader.GetString(2),
                    ParentId = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3)
                };
            }

            return null;
        }
    }
}
