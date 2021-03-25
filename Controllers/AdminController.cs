using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GaryPortalAPI.Models;
using GaryPortalAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaryPortalAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [Route("api/[controller]")]
    public class AdminController : Controller
    {

        private readonly IUserService _userService;
        private readonly IStaffService _staffService;
        private readonly IChatService _chatService;

        public AdminController(IUserService userService, IStaffService staffService, IChatService chatService)
        {
            _userService = userService;
            _staffService = staffService;
            _chatService = chatService;
        }
    

        [HttpPut("ClearAllPrayers")]
        public async Task<IActionResult> AdminClearAllPrayers(string uuid, CancellationToken ct = default)
        {
            if (!string.IsNullOrWhiteSpace(uuid))
            {
                await _userService.ClearPrayersForUserAsync(uuid, ct);
            }
            else
            {
                await _userService.ClearAllPrayersAsync(ct);
            }
            return Ok();

        }


        [HttpPost("PostStaffAnnouncement")]
        public async Task<IActionResult> PostStaffAnnouncement([FromBody] StaffRoomAnnouncement announcement, CancellationToken ct = default)
        {
            return Ok(await _staffService.PostStaffRoomAnnouncementAsync(announcement, ct));
        }

        [HttpPut("MarkAnnouncementAsDeleted/{id}")]
        public async Task<IActionResult> MarkAnnouncementAsDeleted(int id, CancellationToken ct = default)
        {
            await _staffService.MarkAnnouncementAsDeletedAsync(id, ct);
            return Ok();
        }

        [HttpGet("QueuedUsers")]
        [Produces(typeof(ICollection<User>))]
        public async Task<IActionResult> GetQueuedUsers(CancellationToken ct = default)
        {
            return Ok(await _userService.GetAllQueuedAsync(ct));
        }

        [HttpPut("MarkChatReportAsDeleted/{reportId}")]
        public async Task<IActionResult> MarkUserReportAsDeleted(int reportId, CancellationToken ct = default)
        {
            await _chatService.MarkReportAsDeletedAsync(reportId, ct);
            return Ok();
        }

        [HttpPost("PostNotification")]
        public async Task<IActionResult> PostNotificiation([FromBody] APSAlert notification, int teamId = 0, CancellationToken ct = default)
        {
            await _staffService.PostNotification(teamId, notification, ct);
            return Ok();
        }
    }
}
