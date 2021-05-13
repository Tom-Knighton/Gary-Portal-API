using System;
using System.Threading.Tasks;
using GaryPortalAPI.Models.Chat;
using Microsoft.AspNetCore.SignalR;

namespace GaryPortalAPI.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string senderUUID, string groupToSendTo, string messageUUID)
        {
            await Clients.Group(groupToSendTo).SendAsync("MessageReceived", groupToSendTo, senderUUID, messageUUID);
        }

        public async Task AddedToChat(string chatUUID, string memberUUID)
        {
            await Clients.All.SendAsync("AddedToChat", chatUUID, memberUUID);
        }

        public async Task DeleteMessage(string groupToSendTo, string messageUUID)
        {
            await Clients.Group(groupToSendTo).SendAsync("RemoveMessage", groupToSendTo, messageUUID);
        }

        public async Task EditChatName(string chatUUID, string newName)
        {
            await Clients.Group(chatUUID).SendAsync("NewChatName", chatUUID, newName);
        }

        public async Task SubscribeToGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task AddedUserToChat(string chatUUID, ChatMember member)
        {
            await Clients.Group(chatUUID).SendAsync("NewChatUser", chatUUID, member);
        }

        public async Task RemoveFromGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task NotifyStartTyping(string chatUUID, string memberUUID)
        {
            await Clients.Groups(chatUUID).SendAsync("UserIsTyping", memberUUID);
        }

        public async Task KeepAlive()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("KeepAlive");
        }
    }
}
