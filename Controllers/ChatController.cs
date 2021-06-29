using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GaryPortalAPI.Models;
using GaryPortalAPI.Models.Chat;
using GaryPortalAPI.Services;
using GaryPortalAPI.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaryPortalAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IChatBotService _chatBot;

        public ChatController(IChatService chatService, IUserService userService, IChatBotService chatbot)
        {
            _chatService = chatService;
            _userService = userService;
            _chatBot = chatbot;
        }

        [HttpGet("Chats/{userUUID}")]
        [Produces(typeof(ICollection<Chat>))]
        public async Task<IActionResult> GetChatsForUser(string userUUID, CancellationToken ct = default)
        {
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            return Ok(await _chatService.GetAllChatsForUserAsync(userUUID, ct));
        }

        [HttpGet("{chatUUID}")]
        [Produces(typeof(Chat))]
        public async Task<IActionResult> GetChatById(string chatUUID, CancellationToken ct = default)
        {
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            return Ok(await _chatService.GetChatByIdAsync(chatUUID, ct));
        }

        [HttpGet("Messages/{chatUUID}")]
        [Produces(typeof(ICollection<ChatMessage>))]
        public async Task<IActionResult> GetMessagesForChat(string chatUUID, long startfrom, int limit = 20, CancellationToken ct = default)
        {
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            return Ok(await _chatService.GetMessagesForChatAsync(chatUUID, startfrom, limit, ct));
        }

        [HttpGet("Message/{messageUUID}")]
        [Produces(typeof(ChatMessage))]
        public async Task<IActionResult> GetMessageById(string messageUUID, CancellationToken ct = default)
        {
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            return Ok(await _chatService.GetMessageByIdAsync(messageUUID, ct));
        }

        [HttpPut("Chats/AddUser/{userUUID}/{chatUUID}")]
        [Produces(typeof(ChatMember))]
        public async Task<IActionResult> AddUserToChat(string userUUID, string chatUUID, CancellationToken ct = default)
        {
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            return Ok(await _chatService.AddUserToChatAsync(userUUID, chatUUID, ct));
        }

        [HttpPut("Chats/AddUserByUsername/{username}/{chatUUID}")]
        [Produces(typeof(ChatMember))]
        public async Task<IActionResult> AddUserToChatByUsername(string username, string chatUUID, CancellationToken ct = default)
        {
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            Chat chat = await _chatService.GetChatByIdAsync(chatUUID, ct);
            if (chat.ChatMembers.Where(cm => cm.UserUUID == AuthenticationUtilities.GetUUIDFromIdentity(User)).Any()) {
                string uuid = await _userService.GetUUIDFromUsername(username, ct);
                if (string.IsNullOrWhiteSpace(uuid))
                    return BadRequest("User does not exist");
                return Ok(await _chatService.AddUserToChatAsync(uuid, chatUUID, ct));
            } else
            {
                return BadRequest("You cannot add to a chat you are not in");
            }
        }

        [HttpPut("Chats/RemoveUser/{userUUID}/{chatUUID}")]
        public async Task<IActionResult> RemoveUserFromChat(string userUUID, string chatUUID, CancellationToken ct = default)
        {
            await _chatService.RemoveFromChatAsync(userUUID, chatUUID, ct);
            return Ok();
        }

        [HttpPost("{chatUUID}/NewMessage")]
        [Produces(typeof(ChatMessage))]
        public async Task<IActionResult> AddMessageToChat(string chatUUID, [FromBody] ChatMessage message, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid message details");
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            return Ok(await _chatService.AddMessageToChatAsync(message, chatUUID, ct));
        }

        [HttpPut("Messages/Update/{messageUUID}")]
        [Produces(typeof(ChatMessage))]
        public async Task<IActionResult> EditMessageAsync(string messageUUID, [FromBody] string newContent, CancellationToken ct = default)
        {
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            return Ok(await _chatService.EditMessageAsync(messageUUID, newContent, ct));
        }

        [HttpPut("Messages/Delete/{messageUUID}")]
        public async Task<IActionResult> DeleteMessage(string messageUUID, CancellationToken ct = default)
        {
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            return Ok(await _chatService.RemoveMessageAsync(messageUUID, ct));
        }

        [HttpPost("{chatUUID}/Attachment")]
        [Produces(typeof(string))]
        public async Task<IActionResult> UploadChatAttachment(string chatUUID, CancellationToken ct = default)
        {
            if (HttpContext.Request.Form.Files.Count > 0)
                return Ok(await _chatService.UploadChatAttachmentAsync(HttpContext.Request.Form.Files[0], chatUUID, ct));
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            return BadRequest("No files specified");
        }

        [HttpPost]
        [Produces(typeof(Chat))]
        public async Task<IActionResult> CreateChat([FromBody] Chat chat, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid chat object");
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            return Ok(await _chatService.CreateNewChatAsync(chat, ct));
        }

        [HttpPut]
        [Produces(typeof(Chat))]
        public async Task<IActionResult> EditChat([FromBody] ChatEditDetails details, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid chat details");
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            return Ok(await _chatService.EditChatAsync(details, ct));
        }


        [HttpPut("Chats/{chatUUID}/MarkAsRead")]
        public async Task<IActionResult> MarkChatAsRead(string chatUUID, CancellationToken ct = default)
        {
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            await _chatService.MarkChatAsReadAsync(AuthenticationUtilities.GetUUIDFromIdentity(User), chatUUID, ct);
            return Ok();
        }

        [HttpPost("ReportMessage/{messageUUID}")]
        public async Task<IActionResult> ReportUser([FromBody] ChatMessageReport report, CancellationToken ct = default)
        {
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            await _chatService.ReportMessageAsync(report, ct);
            return Ok();
        }

        [HttpPost("BotMessage")]
        public async Task<IActionResult> BotMessage([FromBody] ChatBotRequest request, CancellationToken ct = default)
        {
            if (!AuthenticationUtilities.IsAllowedChat(User))
                return BadRequest("User has been banned from Chat");
            string uuid = AuthenticationUtilities.GetUUIDFromIdentity(User);
            return Ok(await _chatBot.GetResponseForCommand(request.input, uuid, request.version));
        }

        [HttpPost("Notification/{chatUUID}/{senderUUID}")]
        public async Task<IActionResult> PostNotification([FromBody] string content, string chatUUID, string senderUUID, CancellationToken ct = default)
        {
            await _chatService.PostNotificationToChat(chatUUID, senderUUID, content, ct);
            return Ok();
        }

    }
}
