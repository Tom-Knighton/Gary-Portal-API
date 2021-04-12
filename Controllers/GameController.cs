using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GaryPortalAPI.Models.Games;
using GaryPortalAPI.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GaryPortalAPI.Controllers
{
    [Route("api/[controller]")]
    public class GameController : Controller
    {
        private readonly IGameTypeService _gameTypeService;

        public GameController(IGameTypeService gameTypeService)
        {
            _gameTypeService = gameTypeService;
        }


        [HttpGet("GameTypes")]
        [Produces(typeof(ICollection<GameType>))]
        public async Task<IActionResult> GetGameTypes(int teamId = 0, CancellationToken ct = default)
        {
            return Ok(await _gameTypeService.GetGameTypesAsync(teamId, ct));
        }
    }
}
