﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using GaryPortalAPI.Models;
using GaryPortalAPI.Services;
using GaryPortalAPI.Services.Authentication;
using Microsoft.AspNetCore.Mvc;
using User = GaryPortalAPI.Models.User;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GaryPortalAPI.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthenticationService authenticationService, ITokenService tokenService, IUserService userService)
        {
            _authenticationService = authenticationService;
            _tokenService = tokenService;
            _userService = userService;
        }

        [HttpPost("Authenticate")]
        [Produces(typeof(User))]
        public async Task<IActionResult> AuthenticateUser([FromBody] AuthenticatingUser authUser, bool needsTokens = true, CancellationToken ct = default)
        {
            //TODO: require email confirmation
            try
            {
                User user = await _authenticationService.Authenticate(authUser, needsTokens, ct);
                if (user == null)
                    return BadRequest("Invalid login attempt");
                return Ok(user);
            }
            catch (AuthenticationException ex) {
                return Unauthorized(ex.Message);
            }
          
        }

        [HttpPost("Refresh/{uuid}")]
        [Produces(typeof(UserAuthenticationTokens))]
        public async Task<IActionResult> RefreshTokens(string uuid, [FromBody] UserAuthenticationTokens tokens, CancellationToken ct = default)
        {
            UserAuthenticationTokens newTokens = await _tokenService.RefreshTokensForUserAsync(uuid, tokens.RefreshToken, ct);
            if (newTokens == null)
                return Unauthorized("Invalid refresh token");
            return Ok(newTokens);
        }

        [HttpPost("RegisterUser")]
        [Produces(typeof(User))]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegistration newUser, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid user registration details");
            User user = await _userService.CreateNewUserAsync(newUser);
            if (user == null)
                return BadRequest("An Error Occurred");
            return Ok(user);
        }

    }
}
