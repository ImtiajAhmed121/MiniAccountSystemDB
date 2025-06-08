using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountSystemDB.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MiniAccountSystemDB.Pages.Accounts
{
    public class CreateModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public CreateModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public ChartOfAccount Account { get; set; } = new();

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // ? Correctly create and open the connection
            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            // ? Now use the connection in the command
            using SqlCommand cmd = new("INSERT INTO ChartOfAccounts (Name, AccountType, ParentId) VALUES (@Name, @AccountType, @ParentId)", conn);
            cmd.Parameters.AddWithValue("@Name", Account.Name);
            cmd.Parameters.AddWithValue("@AccountType", Account.AccountType);
            cmd.Parameters.AddWithValue("@ParentId", (object?)Account.ParentId ?? DBNull.Value);

            cmd.ExecuteNonQuery();

            return RedirectToPage("Index");
        }
    }
}
