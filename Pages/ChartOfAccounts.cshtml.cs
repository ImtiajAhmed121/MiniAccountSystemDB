using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace MiniAccountSystemDB.Pages
{
    public class ChartOfAccountsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ChartOfAccountsModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        //adding
        [BindProperty]
        public string ActionType { get; set; } = string.Empty;

        [BindProperty]
        public int? EditId { get; set; }

        [BindProperty]
        public int? DeleteId { get; set; }
        [BindProperty]
        public AccountInputModel Input { get; set; } = new AccountInputModel();

        public List<Account> AllAccounts { get; set; } = new List<Account>();

        public class Account
        {
            public int Id { get; set; }
            public string Name { get; set; } = String.Empty;
            public string AccountType { get; set; } = String.Empty;
            public int? ParentId { get; set; }
        }

        public class AccountInputModel
        {
            public string Name { get; set; } = String.Empty;
            public string AccountType { get; set; } = String.Empty;
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
                    EditId = account.Id; // Stay in form for update
                }
                LoadAccounts();
                return Page();
            }

            // Default: Insert or Update
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
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
        private void DeleteAccount(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", DBNull.Value);
                    cmd.Parameters.AddWithValue("@ParentId", DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccountType", DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private Account? GetAccountById(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Id, Name, AccountType, ParentId FROM ChartOfAccounts WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Account
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                AccountType = reader.GetString(2),
                                ParentId = reader.IsDBNull(3) ? (int?)null : (int?)reader.GetInt32(3)
                            };
                        }
                    }
                }
            }
            return null;
        }



        private void LoadAccounts()
        {
            AllAccounts = new List<Account>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Id, Name, AccountType, ParentId FROM ChartOfAccounts", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AllAccounts.Add(new Account
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            AccountType = reader.GetString(2),
                            ParentId = reader.IsDBNull(3) ? (int?)null : (int?)reader.GetInt32(3)
                        });
                    }
                }
            }
        }
    }
}
