﻿using CET96_ProjetoFinal.web.Entities;
using CET96_ProjetoFinal.web.Models;
using CET96_ProjetoFinal.web.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CET96_ProjetoFinal.web.Controllers
{
    [Authorize(Roles = "Condominium Manager")]
    public class CondominiumStaffController : Controller
    {
        private readonly IApplicationUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICondominiumRepository _condominiumRepository;

        public CondominiumStaffController(
            IApplicationUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            ICondominiumRepository condominiumRepository)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _condominiumRepository = condominiumRepository;
        }

        // GET: CondominiumStaff/Create
        public async Task<IActionResult> Create()
        {
            var loggedInUser = await _userRepository.GetUserByEmailasync(User.Identity.Name);
            var managedCondominium = await _condominiumRepository.GetCondominiumByManagerIdAsync(loggedInUser.Id);

            if (managedCondominium == null)
            {
                TempData["StatusMessage"] = "Error: You must be assigned a condominium to create staff.";
                return RedirectToAction("CondominiumManagerDashboard", "Home");
            }

            var model = new RegisterCondominiumStaffViewModel
            {
                CondominiumId = managedCondominium.Id
            };

            return View(model);
        }

        // POST: CondominiumStaff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterCondominiumStaffViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userExists = await _userRepository.GetUserByEmailasync(model.Username);
                if (userExists != null)
                {
                    ModelState.AddModelError("Username", "This email is already in use.");
                }

                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserName = model.Username,
                        Email = model.Username,
                        PhoneNumber = model.PhoneNumber,
                        DocumentType = model.DocumentType,
                        IdentificationDocument = model.IdentificationDocument,
                        CondominiumId = model.CondominiumId,
                        Profession = model.Profession,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await _userRepository.AddUserAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await _userRepository.AddUserToRoleAsync(user, "Condominium Staff");
                        TempData["StatusMessage"] = $"Condominium staff member '{user.FirstName} {user.LastName}' created successfully.";
                        return RedirectToAction("CondominiumManagerDashboard", "Home");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        // GET: CondominiumStaff/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var loggedInUser = await _userRepository.GetUserByEmailasync(User.Identity.Name);
            var managedCondominium = await _condominiumRepository.GetCondominiumByManagerIdAsync(loggedInUser.Id);

            if (managedCondominium == null)
            {
                TempData["StatusMessage"] = "Error: You are not assigned to a condominium.";
                return RedirectToAction("CondominiumManagerDashboard", "Home");
            }

            var staffMember = await _userRepository.GetUserByIdAsync(id);
            if (staffMember == null)
            {
                return NotFound();
            }

            // CRUCIAL SECURITY CHECK: Ensure the staff member belongs to the logged-in manager's condominium.
            if (staffMember.CondominiumId != managedCondominium.Id)
            {
                TempData["StatusMessage"] = "Error: You do not have permission to edit this staff member.";
                return RedirectToAction("CondominiumManagerDashboard", "Home");
            }

            var model = new EditAccountViewModel
            {
                Id = staffMember.Id,
                FirstName = staffMember.FirstName,
                LastName = staffMember.LastName,
                PhoneNumber = staffMember.PhoneNumber,
                CompanyId = managedCondominium.Id // Using CompanyId for consistency in redirects, though it represents CondominiumId here.
            };

            return View(model);
        }

        // POST: CondominiumStaff/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, return to the form with errors.
                return View(model);
            }

            var staffMember = await _userRepository.GetUserByIdAsync(model.Id);
            if (staffMember == null)
            {
                return NotFound();
            }

            var loggedInUser = await _userRepository.GetUserByEmailasync(User.Identity.Name);
            var managedCondominium = await _condominiumRepository.GetCondominiumByManagerIdAsync(loggedInUser.Id);

            // CRUCIAL SECURITY CHECK (again): Double-check ownership before saving changes.
            if (managedCondominium == null || staffMember.CondominiumId != managedCondominium.Id)
            {
                TempData["StatusMessage"] = "Error: You do not have permission to perform this action.";
                return RedirectToAction("CondominiumManagerDashboard", "Home");
            }

            // Update the user properties from the ViewModel.
            staffMember.FirstName = model.FirstName;
            staffMember.LastName = model.LastName;
            staffMember.PhoneNumber = model.PhoneNumber;
            staffMember.UpdatedAt = DateTime.UtcNow;
            staffMember.UserUpdatedId = _userManager.GetUserId(User);

            var result = await _userManager.UpdateAsync(staffMember);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Staff member updated successfully.";
                return RedirectToAction("CondominiumManagerDashboard", "Home");
            }

            // If update fails, display errors.
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: CondominiumStaff/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            var loggedInUser = await _userRepository.GetUserByEmailasync(User.Identity.Name);
            var managedCondominium = await _condominiumRepository.GetCondominiumByManagerIdAsync(loggedInUser.Id);

            if (managedCondominium == null)
            {
                TempData["StatusMessage"] = "Error: You are not assigned to a condominium.";
                return RedirectToAction("CondominiumManagerDashboard", "Home");
            }

            var staffMember = await _userRepository.GetUserByIdAsync(id);
            if (staffMember == null)
            {
                return NotFound();
            }

            // CRUCIAL SECURITY CHECK: Ensure the staff member belongs to the logged-in manager's condominium.
            if (staffMember.CondominiumId != managedCondominium.Id)
            {
                TempData["StatusMessage"] = "Error: You do not have permission to perform this action.";
                return RedirectToAction("CondominiumManagerDashboard", "Home");
            }

            // Deactivate the user
            staffMember.DeactivatedAt = DateTime.UtcNow;
            staffMember.DeactivatedByUserId = _userManager.GetUserId(User);
            staffMember.UpdatedAt = DateTime.UtcNow;
            staffMember.UserUpdatedId = _userManager.GetUserId(User);

            var result = await _userManager.UpdateAsync(staffMember);

            if (result.Succeeded)
            {
                // Set a lockout end date to prevent login
                await _userManager.SetLockoutEndDateAsync(staffMember, DateTimeOffset.MaxValue);
                TempData["StatusMessage"] = "Staff member deactivated successfully.";
            }
            else
            {
                TempData["StatusMessage"] = "Error deactivating staff member.";
            }

            return RedirectToAction("CondominiumManagerDashboard", "Home");
        }

        // POST: CondominiumStaff/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            var loggedInUser = await _userRepository.GetUserByEmailasync(User.Identity.Name);
            var managedCondominium = await _condominiumRepository.GetCondominiumByManagerIdAsync(loggedInUser.Id);

            if (managedCondominium == null)
            {
                TempData["StatusMessage"] = "Error: You are not assigned to a condominium.";
                return RedirectToAction("CondominiumManagerDashboard", "Home");
            }

            var staffMember = await _userRepository.GetUserByIdAsync(id);
            if (staffMember == null)
            {
                return NotFound();
            }

            // CRUCIAL SECURITY CHECK: Ensure the staff member belongs to the logged-in manager's condominium.
            if (staffMember.CondominiumId != managedCondominium.Id)
            {
                TempData["StatusMessage"] = "Error: You do not have permission to perform this action.";
                return RedirectToAction("CondominiumManagerDashboard", "Home");
            }

            // Activate the user
            staffMember.DeactivatedAt = null;
            staffMember.DeactivatedByUserId = null;
            staffMember.UpdatedAt = DateTime.UtcNow;
            staffMember.UserUpdatedId = _userManager.GetUserId(User);

            var result = await _userManager.UpdateAsync(staffMember);

            if (result.Succeeded)
            {
                // Remove any lockout end date
                await _userManager.SetLockoutEndDateAsync(staffMember, null);
                TempData["StatusMessage"] = "Staff member activated successfully.";
            }
            else
            {
                TempData["StatusMessage"] = "Error activating staff member.";
            }

            return RedirectToAction("CondominiumManagerDashboard", "Home");
        }
    }
}