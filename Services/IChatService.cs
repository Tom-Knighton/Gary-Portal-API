using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GaryPortalAPI.Models;
using GaryPortalAPI.Models.Chat;
using GaryPortalAPI.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GaryPortalAPI.Services
{
    public interface IChatService : IDisposable
    {
        Task<Chat> CreateNewChatAsync(Chat chat, CancellationToken ct = default);
        Task<Chat> GetChatByIdAsync(string uuid, CancellationToken ct = default);
        Task<Chat> EditChatAsync(ChatEditDetails newChat, CancellationToken ct = default);
        Task<ICollection<Chat>> GetAllChatsForUserAsync(string userUUID, CancellationToken ct = default);
        Task<ChatMember> GetChatMemberAsync(string chatUUID, string userUUID, CancellationToken ct = default);
        Task<ICollection<ChatMember>> GetMembersForChatAsync(string chatUUID, CancellationToken ct = default);

        Task<ChatMessage> GetLastMessageForChat(string chatUUID, CancellationToken ct = default);
        Task<ICollection<ChatMessage>> GetMessagesForChatAsync(string chatUUID, long startfrom, int limit = 20, CancellationToken ct = default);
        Task<ChatMessage> GetMessageByIdAsync(string messageUUID, CancellationToken ct = default);
        Task MarkChatAsReadAsync(string userUUID, string chatUUID, CancellationToken ct = default);

        Task<ChatMember> AddUserToChatAsync(string userUUID, string chatUUID, CancellationToken ct = default);
        Task RemoveFromChatAsync(string userUUID, string chatUUID, CancellationToken ct = default);

        Task<ChatMessage> AddMessageToChatAsync(ChatMessage msg, string chatUUID, CancellationToken ct = default);
        Task<bool> RemoveMessageAsync(string messageUUID, CancellationToken ct = default);
        Task<string> UploadChatAttachmentAsync(IFormFile file, string chatUUID, CancellationToken ct = default);

        Task ReportMessageAsync(ChatMessageReport report, CancellationToken ct = default);
        Task MarkReportAsDeletedAsync(int reportId, CancellationToken ct = default);

        Task PostNotificationToChat(string chatUUID, string senderUUID, string content, CancellationToken ct = default);
    }

    public class ChatService : IChatService
    {

        private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public ChatService(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<Chat> CreateNewChatAsync(Chat chat, CancellationToken ct = default)
        {
            chat.ChatUUID = Guid.NewGuid().ToString("N");
            await _context.Chats.AddAsync(chat, ct);
            await _context.SaveChangesAsync(ct);
            return chat;
        }

        public async Task<Chat> GetChatByIdAsync(string uuid, CancellationToken ct = default)
        {
            Chat chat = await _context.Chats
                .AsNoTracking()
                .Include(c => c.ChatMembers.Where(cm => cm.IsInChat))
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(c => c.ChatUUID == uuid, cancellationToken: ct);
            chat.LastChatMessage = await GetLastMessageForChat(chat.ChatUUID, ct);

            foreach (ChatMember member in chat.ChatMembers)
            {
                member.UserDTO = member.User.ConvertToDTO();
                member.User = null;
            }
            return chat;
        }

        public async Task<string> GetChatNameAsync(string chatUUID)
        {
            return await _context.Chats.Where(c => c.ChatUUID == chatUUID).Select(c => c.ChatName).FirstOrDefaultAsync();
        }

        public async Task<Chat> EditChatAsync(ChatEditDetails newChat, CancellationToken ct = default)
        {
            Chat chat = await _context.Chats.AsNoTracking().FirstOrDefaultAsync(c => c.ChatUUID == newChat.ChatUUID, cancellationToken: ct);
            if (chat == null)
                return null;

            chat.ChatName = newChat.ChatName;
            chat.ChatIsProtected = newChat.ChatIsProtected;
            chat.ChatIsDeleted = newChat.ChatIsDeleted;
            chat.ChatIsPublic = newChat.ChatIsPublic;
            _context.Update(chat);
            await _context.SaveChangesAsync(ct);
            return await GetChatByIdAsync(newChat.ChatUUID, ct);
        }

        public async Task<ICollection<Chat>> GetAllChatsForUserAsync(string userUUID, CancellationToken ct = default)
        {
            ICollection<Chat> chats = await _context.Chats
                .AsNoTracking()
                .Include(c => c.ChatMembers.Where(cm => cm.IsInChat))
                    .ThenInclude(cm => cm.User)
                .Where(c => c.ChatMembers.Any(cm => cm.UserUUID == userUUID && cm.IsInChat))
                .ToListAsync(ct);

            foreach (Chat chat in chats)
            {
                chat.LastChatMessage = await GetLastMessageForChat(chat.ChatUUID, ct);
                foreach (ChatMember member in chat.ChatMembers)
                {
                    member.UserDTO = member.User.ConvertToDTO();
                    member.User = null;
                }
            }

            return chats.OrderByDescending(c => c.LastChatMessage != null ? c.LastChatMessage.MessageCreatedAt : c.ChatCreatedAt).ToList();
        }



        public async Task<ICollection<ChatMember>> GetMembersForChatAsync(string chatUUID, CancellationToken ct = default)
        {
            ICollection<ChatMember> members = await _context.ChatMembers
                .AsNoTracking()
                .Where(cm => cm.ChatUUID == chatUUID && cm.IsInChat)
                .Include(cm => cm.User)
                .ToListAsync();

            foreach (ChatMember member in members)
            {
                member.UserDTO = member.User.ConvertToDTO();
                member.User = null;
            }
            return members;
        }

        public async Task<ChatMember> GetChatMemberAsync(string chatUUID, string userUUID, CancellationToken ct = default)
        {
            ChatMember member = await _context.ChatMembers
                .AsNoTracking()
                .Include(cm => cm.User)
                .FirstOrDefaultAsync(cm => cm.ChatUUID == chatUUID && cm.UserUUID == userUUID, ct);
            member.UserDTO = member.User.ConvertToDTO();
            member.User = null;
            return member;
        }



        public async Task<ChatMessage> GetLastMessageForChat(string chatUUID, CancellationToken ct = default)
        {
            return await _context.ChatMessages
                .AsNoTracking()
                .Where(cm => cm.ChatUUID == chatUUID && !cm.MessageIsDeleted)
                .OrderByDescending(cm => cm.MessageCreatedAt)
                .Include(cm => cm.User)
                .Select(cm => new ChatMessage{ MessageContent = cm.MessageContent, MessageCreatedAt = cm.MessageCreatedAt, MessageTypeId = cm.MessageTypeId, UserDTO = cm.User.ConvertToDTO() })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<ChatMessage> GetMessageByIdAsync(string messageUUID, CancellationToken ct = default)
        {
            ChatMessage msg = await _context.ChatMessages
                .AsNoTracking()
                .Include(cm => cm.User)
                .FirstOrDefaultAsync(cm => cm.ChatMessageUUID == messageUUID, cancellationToken: ct);
            msg.UserDTO = msg.User.ConvertToDTO();
            msg.User = null;
            return msg;
        }

        public async Task MarkChatAsReadAsync(string userUUID, string chatUUID, CancellationToken ct = default)
        {
            ChatMember member = await _context.ChatMembers.AsNoTracking().FirstOrDefaultAsync(cm => cm.UserUUID == userUUID && cm.ChatUUID == chatUUID && cm.IsInChat, ct);
            if (member == null)
                return;
            member.LastReadAt = DateTime.UtcNow;
            _context.Update(member);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<ICollection<ChatMessage>> GetMessagesForChatAsync(string chatUUID, long startfrom, int limit = 20, CancellationToken ct = default)
        {
            DateTime fromDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(startfrom);
            ICollection<ChatMessage> chatMessages = await _context.ChatMessages
                .AsNoTracking()
                .Include(cm => cm.User)
                .Where(cm => cm.ChatUUID == chatUUID && !cm.MessageIsDeleted && fromDate >= cm.MessageCreatedAt)
                .OrderByDescending(cm => cm.MessageCreatedAt)
                .Take(limit)
                .ToListAsync(ct);

            foreach (ChatMessage msg in chatMessages)
            {
                msg.UserDTO = msg.User.ConvertToDTO();
                msg.User = null;
            }
            return chatMessages;
        }



        public async Task<ChatMember> AddUserToChatAsync(string userUUID, string chatUUID, CancellationToken ct = default)
        {
            Console.WriteLine(userUUID);
            ChatMember member = new ChatMember
            {
                ChatUUID = chatUUID,
                UserUUID = userUUID,
                IsInChat = true
            };
            ChatMember existingMember = await _context.ChatMembers.AsNoTracking().FirstOrDefaultAsync(cm => cm.UserUUID == userUUID && cm.ChatUUID == chatUUID, ct);
            if (existingMember != null)
            {
                member.IsInChat = true;
                _context.Update(member);
            } else
            {
                await _context.ChatMembers.AddAsync(member, ct);
            }
            await _context.SaveChangesAsync(ct);
            return await GetChatMemberAsync(chatUUID, userUUID);
        }

        public async Task RemoveFromChatAsync(string userUUID, string chatUUID, CancellationToken ct = default)
        {
            ChatMember member = await _context.ChatMembers.AsNoTracking().FirstOrDefaultAsync(cm => cm.UserUUID == userUUID && cm.ChatUUID == chatUUID, ct);
            if (member == null)
                return;
            member.IsInChat = false;
            _context.Update(member);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<ChatMessage> AddMessageToChatAsync(ChatMessage msg, string chatUUID, CancellationToken ct = default)
        {
            msg.ChatMessageUUID = Guid.NewGuid().ToString("N");
            await _context.ChatMessages.AddAsync(msg, ct);
            await _context.SaveChangesAsync(ct);
            return msg;
        }

        public async Task<bool> RemoveMessageAsync(string messageUUID, CancellationToken ct = default)
        {
            ChatMessage message = await _context.ChatMessages.AsNoTracking().FirstOrDefaultAsync(cm => cm.ChatMessageUUID == messageUUID, ct);
            if (message == null)
                return false;
            message.MessageIsDeleted = true;
            _context.Update(message);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<string> UploadChatAttachmentAsync(IFormFile file, string chatUUID, CancellationToken ct = default)
        {
            if (file == null) return null;

            string uuid = Guid.NewGuid().ToString();
            Directory.CreateDirectory($"/var/www/cdn/GaryPortal/Chat/{chatUUID}/Attachments/");
            string newFileName = file.FileName.Replace(Path.GetFileNameWithoutExtension(file.FileName), uuid);
            var filePath = $"/var/www/cdn/GaryPortal/Chat/{chatUUID}/Attachments/{newFileName}";
            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream, ct);
            return $"https://cdn.tomk.online/GaryPortal/Chat/{chatUUID}/Attachments/{newFileName}";
        }

        public async Task ReportMessageAsync(ChatMessageReport report, CancellationToken ct = default)
        {
            await _context.ChatMessageReports.AddAsync(report);
            await _context.SaveChangesAsync(ct);
        }

        public async Task MarkReportAsDeletedAsync(int reportId, CancellationToken ct = default)
        {
            ChatMessageReport report = await _context.ChatMessageReports.FindAsync(reportId);
            if (report != null)
            {
                report.IsDeleted = true;
                _context.Update(report);
                await _context.SaveChangesAsync();
            }
        }

        public async Task PostNotificationToChat(string chatUUID, string senderUUID, string content, CancellationToken ct = default)
        {
            User sender = await _userService.GetByIdAsync(senderUUID, ct);
            if (sender == null) return;
            ICollection<ChatMember> members = await GetMembersForChatAsync(chatUUID, ct);

            bool isGroupChat = members.Count > 2;
            string notificationTitle = isGroupChat ? await GetChatNameAsync(chatUUID) : members.FirstOrDefault(cm => cm.UserUUID == senderUUID).UserDTO.UserFullName;

            foreach (ChatMember member in members.Where(u => u.UserUUID != senderUUID))
            {
                UserBan globalBan = await _userService.GetFirstBanOfTypeIfAnyAsnc(member.UserUUID, 1, ct);
                UserBan chatBan = await _userService.GetFirstBanOfTypeIfAnyAsnc(member.UserUUID, 2, ct);
                if (globalBan != null || chatBan != null)
                    return;

                ICollection<string> userAPNSTokens = await _userService.GetAPNSFromUUIDAsync(member.UserUUID, ct);
                foreach (string token in userAPNSTokens)
                {
                    Notification notification = Notification.CreateNotification(
                        new APSAlert { title = notificationTitle, subtitle = isGroupChat ? $"From: {members.FirstOrDefault(cm => cm.UserUUID == senderUUID).UserDTO.UserFullName}" : string.Empty, body = content },
                        chatUUID: chatUUID);

                    await _userService.PostNotification(token, notification);
                }
            }
        }

        public async void Dispose()
        {
            await _context.DisposeAsync();
        }
    }
}
