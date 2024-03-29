﻿using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Account;
using Web.Services.Abstract;

namespace Web.Services.Concrete
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ModelStateDictionary _modelState;

        public AccountService(UserManager<User> userManager,
                             IActionContextAccessor actionContextAccessor,
                             SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _modelState = actionContextAccessor.ActionContext.ModelState;
        }
        public async Task<bool> RegisterUserAsync(AccountRegisterVM model)
        {
            if (!_modelState.IsValid) return false;
            var dbUserEmail = await _userManager.FindByEmailAsync(model.Email);
            var dbUserName = await _userManager.FindByNameAsync(model.Username);
            if (dbUserEmail != null)
            {
                _modelState.AddModelError("Email", "This email is already exist");
                return false;
            }
            if (dbUserName != null)
            {
                _modelState.AddModelError("Username", "This username is already exist");
                return false;
            }
            var user = new User
            {
                FullName = model.Fullname,
                Email = model.Email,
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _modelState.AddModelError(string.Empty, error.Description);
                }
                return false;
            }
            return true;
        }
        public async Task<bool> LoginUserAsync(AccountLoginVM model)
        {
            if (!_modelState.IsValid) return false;

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                _modelState.AddModelError(string.Empty, "Username or password is incorrect");
                return false;
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, false);

            if (!result.Succeeded)
            {
                _modelState.AddModelError(string.Empty, "Username or password is incorrect");
                return false;
            }
            return true;
        }

        [HttpGet]
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
