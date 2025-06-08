// Pages/Accounts/Edit.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountSystemDB.Models;
using System.Data.SqlClient;

namespace MiniAccountSystemDB.Pages.Accounts
{
    public class EditModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public EditModel(IConfiguration configuration)
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

            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Account.Id = reader.GetInt32(0);
                Account.Name = reader.GetString(1);
            }
            else
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            using SqlCommand cmd = new("UPDATE ChartOfAccounts SET Name = @Name WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Name", Account.Name);
            cmd.Parameters.AddWithValue("@Id", Account.Id);
            cmd.ExecuteNonQuery();

            return RedirectToPage("Index");
        }
    }
}
