using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountSystemDB.Models;
using System.Data.SqlClient;

namespace MiniAccountSystemDB.Pages.Accounts
{
    public class DeleteModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public DeleteModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public ChartOfAccount Account { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();
            using SqlCommand cmd = new("SELECT Id, Name FROM ChartOfAccounts WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Account = new ChartOfAccount
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };
                return Page();
            }

            return NotFound();
        }

        public IActionResult OnPost()
        {
            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();
            using SqlCommand cmd = new("DELETE FROM ChartOfAccounts WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", Account.Id);
            cmd.ExecuteNonQuery();

            return RedirectToPage("Index");
        }
    }
}
