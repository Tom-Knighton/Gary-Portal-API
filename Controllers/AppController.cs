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

        [HttpGet("GetStickers")]
        public async Task<IActionResult> GetStickers()
        {
            return Ok(await _appService.GetStickersAsync());
        }

        [HttpGet("GetEvents")]
        public async Task<IActionResult> GetEvents(int teamId = 0, CancellationToken ct = default)
        {
            return Ok(await _appService.GetEventsAsync(teamId, ct));
        }

        [HttpGet("GetCommandments")]
        public async Task<IActionResult> GetCommandments(CancellationToken ct = default)
        {
            return Ok(await _appService.GetCommandmentsAsync(ct));
        }
    }
}
