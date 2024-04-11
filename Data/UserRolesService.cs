namespace _2AuthenticAPP.Data;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public class UserRolesService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserRolesService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> AddUserToAdminRoleAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            // User not found
            return false;
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (isAdmin)
        {
            // User is already an admin
            return true;
        }

        var hasAdminRole = await _roleManager.RoleExistsAsync("Admin");
        if (!hasAdminRole)
        {
            // Create the Admin role if it doesn't exist
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Add the user to the Admin role
        var result = await _userManager.AddToRoleAsync(user, "Admin");
        return result.Succeeded;
    }
}

