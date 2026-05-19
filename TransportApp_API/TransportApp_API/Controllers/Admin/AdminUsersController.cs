using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TransportApp_API.Models;
using TransportApp_API.DTOs.Admin.Users;

namespace TransportApp_API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users.Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                Roles = _userManager.GetRolesAsync(u).Result.ToList(),
                IsDisabled = u.IsDisabled
            }).ToList();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Roles = roles.ToList(),
                IsDisabled = user.IsDisabled
            });
        }

        [HttpPost("create-controller")]
        public async Task<IActionResult> CreateController(CreateUserRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            if (!await _roleManager.RoleExistsAsync("Controller"))
                await _roleManager.CreateAsync(new IdentityRole("Controller"));
            await _userManager.AddToRoleAsync(user, "Controller");
            return Ok();
        }

        [HttpPost("create-admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateAdmin(CreateUserRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            await _userManager.AddToRoleAsync(user, "Admin");
            return Ok();
        }

        [HttpPost("{id}/roles")]
        public async Task<IActionResult> AddRole(string id, RoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (!await _roleManager.RoleExistsAsync(request.Role)) return BadRequest("Role does not exist");
            await _userManager.AddToRoleAsync(user, request.Role);
            return Ok();
        }

        [HttpDelete("{id}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRole(string id, string roleName)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (!await _roleManager.RoleExistsAsync(roleName)) return BadRequest("Role does not exist");
            await _userManager.RemoveFromRoleAsync(user, roleName);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DisableUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            user.IsDisabled = true;
            await _userManager.UpdateAsync(user);
            return Ok();
        }
    }
}