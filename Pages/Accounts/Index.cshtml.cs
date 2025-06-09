using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MiniAccountSystemDB.Models;
using Microsoft.AspNetCore.Authorization;

namespace MiniAccountSystemDB.Pages.Accounts
{
    [Authorize(Roles = "Admin,Accountant")]

    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<ChartOfAccount> Accounts { get; set; } = new();

        public void OnGet()
        {
            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            using SqlCommand cmd = new("SELECT Id, Name FROM ChartOfAccounts", conn);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Accounts.Add(new ChartOfAccount
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
        }
    }
}
