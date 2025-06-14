using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniAccountSystemDB.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserManagementModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserWithRoles> Users { get; set; } = new();

        [BindProperty]
        public string SelectedUserId { get; set; }
        [BindProperty]
        public string SelectedRole { get; set; }

        public class UserWithRoles
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public IList<string> Roles { get; set; } = new List<string>();
        }

        public async Task OnGetAsync()
        {
            var users = _userManager.Users;
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserWithRoles { Id = user.Id, Email = user.Email, Roles = roles });
            }
        }

        public async Task<IActionResult> OnPostAssignRoleAsync()
        {
            var user = await _userManager.FindByIdAsync(SelectedUserId);
            if (user != null && !string.IsNullOrEmpty(SelectedRole))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, SelectedRole);
            }
            return RedirectToPage();
        }
    }
}
