using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GaryPortalAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace GaryPortalAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class AppController : Controller
    {
        private readonly IAppService _appService;

        public AppController(IAppService appService)
        {
            _appService = appService;
        }

        [AllowAnonymous]
        [HttpGet("Health")]
        public IActionResult TestAppHealth()
        {
            return Ok(_appService.TestAppHealthAsync() == true ? 200 : 500);
        }

        [Obsolete("Use the /app/stickers endpoint instead")]
        [HttpGet("GetStickers")]
        public async Task<IActionResult> GetStickers_Obsolete()
        {
            return Ok(await _appService.GetStickersAsync());
        }

        [HttpGet("Stickers")]
        public async Task<IActionResult> GetStickers()
        {
            return Ok(await _appService.GetStickersAsync());
        }

        [HttpGet("Events")]
        public async Task<IActionResult> GetEvents(int teamId = 0, CancellationToken ct = default)
        {
            return Ok(await _appService.GetEventsAsync(teamId, ct));
        }

        [HttpGet("Commandments")]
        public async Task<IActionResult> GetCommandments(CancellationToken ct = default)
        {
            return Ok(await _appService.GetCommandmentsAsync(ct));
        }

        [HttpGet("Flags")]
        public async Task<IActionResult> GetAllFlags(CancellationToken ct = default)
        {
            return Ok(await _appService.GetAllFlagsAsync(ct));
        }
    }
}
