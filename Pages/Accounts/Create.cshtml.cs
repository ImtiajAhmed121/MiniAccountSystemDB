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

            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            using SqlCommand cmd = new("INSERT INTO ChartOfAccounts (Name) VALUES (@Name)", conn);
            cmd.Parameters.AddWithValue("@Name", Account.Name);
            cmd.ExecuteNonQuery();

            return RedirectToPage("Index");
        }
    }
}
