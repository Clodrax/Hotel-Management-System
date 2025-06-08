using Hotel.Helper;
using Hotel.Models;
using Hotel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Hotel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        public UsersController(UserManager<AppUser> userManager,
         RoleManager<IdentityRole> roleManager,
         SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
        public async Task<IActionResult> Index()
        {
            List<AppUser> DbUsers = await _userManager.Users.OrderByDescending(x => x.Id).ToListAsync();
            List<UserVM> usersVM = new List<UserVM>();
            foreach (AppUser Dbuser in DbUsers)
            {
                UserVM userVm = new UserVM
                {
                    Name = Dbuser.Name,
                    Email = Dbuser.Email,
                    Username = Dbuser.UserName,
                    IsDeactive = Dbuser.IsDeactive,
                    Id = Dbuser.Id,
                    Role = (await _userManager.GetRolesAsync(Dbuser))[0]
                };
                usersVM.Add(userVm);
            }
            return View(usersVM);
        }
        #region ResetPassword
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (id == null)
                return BadRequest();

            AppUser user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return NotFound();

            return View(new ResetPasswordVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResertPassword(string id, ResetPasswordVM resetPasswordVM)
        {
            if (id == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(resetPasswordVM);
            }

            AppUser user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(resetPasswordVM);
            }

            var oldPasswordCheck = await _userManager.CheckPasswordAsync(user, resetPasswordVM.OldPassword);
            if (!oldPasswordCheck)
            {
                ModelState.AddModelError("OldPassword", "Current password is incorrect");
                return View(resetPasswordVM);
            }

            if (resetPasswordVM.OldPassword == resetPasswordVM.Password)
            {
                ModelState.AddModelError("Password", "New password cannot be the same as the old password");
                return View(resetPasswordVM);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordVM.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(resetPasswordVM);
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                // Redirect to logout action upon successful password change
                await _signInManager.SignOutAsync();
                return RedirectToAction("Logout", "Account");
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion
        #region Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = new List<string>
            {
                Roles.Admin.ToString(),
                Roles.Moderator.ToString(),
                Roles.Receptionist.ToString()
            };
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVM createVM, string role)
        {
            ViewBag.Roles = new List<string>
            {
                Roles.Admin.ToString(),
                Roles.Moderator.ToString(),
                Roles.Receptionist.ToString()
            };
            AppUser NewUser = new AppUser
            {
                UserName = createVM.Username,
                Email = createVM.Email,
                Name = createVM.Name,
            };
            if (createVM.Role == "Admin")
            {
                ModelState.AddModelError("", "Admin Rolu Mövcuddur!");
                return View(createVM);
            }
            else
            {
                IdentityResult identityResult = await _userManager.CreateAsync(NewUser, createVM.Password);
                if (!identityResult.Succeeded)
                {
                    foreach (IdentityError error in identityResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View();
                }
            }
            await _userManager.AddToRoleAsync(NewUser, role);
            return RedirectToAction("Index");
        }
        #endregion
        #region Activity
        public async Task<IActionResult> Activity(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser appUser = await _userManager.FindByIdAsync(id);
            if (appUser == null)
            {
                return BadRequest();
            }
            if (appUser.IsDeactive)
            {
                appUser.IsDeactive = false;
            }
            else
            {
                appUser.IsDeactive = true;
            }
            await _userManager.UpdateAsync(appUser);
            return RedirectToAction("Index");
        }
        #endregion
        #region Update
        public async Task<IActionResult> Update(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser DbUser = await _userManager.FindByIdAsync(id);
            if (DbUser == null)
            {
                return BadRequest();
            }
            UpdateVM DbupdateVM = new UpdateVM
            {
                Name = DbUser.Name,
                Username = DbUser.UserName,
                Email = DbUser.Email,
                Role = (await _userManager.GetRolesAsync(DbUser))[0]

            };
            ViewBag.Roles = new List<string>
            {
                Roles.Admin.ToString(),
                Roles.Moderator.ToString()
            };
            return View(DbupdateVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, UpdateVM updateVM, string role)
        {
            if (id == null)
            {
                return NotFound();
            }
            AppUser DbUser = await _userManager.FindByIdAsync(id);
            if (DbUser == null)
            {
                return BadRequest();
            }
            UpdateVM DbupdateVM = new UpdateVM
            {
                Name = DbUser.Name,
                Username = DbUser.UserName,
                Email = DbUser.Email,
                Role = (await _userManager.GetRolesAsync(DbUser))[0]

            };
            ViewBag.Roles = new List<string>
            {
                Roles.Admin.ToString(),
                Roles.Moderator.ToString()
            };
            DbUser.Name = updateVM.Name;
            DbUser.UserName = updateVM.Username;
            DbUser.Email = updateVM.Email;
            if (DbupdateVM.Role != role && DbupdateVM.Role != "Admin")
            {
                if (DbupdateVM.Role != "Admin" && role == "Admin")
                {
                    ModelState.AddModelError("", "Admin Rolu Mövcuddur!");
                    return View(DbupdateVM);
                }
                else
                {
                    IdentityResult removeIdentityResult = await _userManager.RemoveFromRoleAsync(DbUser, DbupdateVM.Role);
                    if (!removeIdentityResult.Succeeded)
                    {

                        foreach (IdentityError error in removeIdentityResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View();
                    }
                    IdentityResult addIdentityResult = await _userManager.AddToRoleAsync(DbUser, role);
                    if (!addIdentityResult.Succeeded)
                    {

                        foreach (IdentityError error in addIdentityResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View();
                    }
                }
            }
            else if (DbupdateVM.Role == "Admin" && role != "Admin")
            {
                ModelState.AddModelError("", "Admin Rolu Üzrə Dəyişiklik Edilə Bilməz!");
                return View(DbupdateVM);
            };
            await _userManager.UpdateAsync(DbUser);
            return RedirectToAction("Index");
        }
        #endregion



    }
}
