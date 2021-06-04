using System;
using System.Collections.Generic;

namespace GaryPortalAPI.Models.Chat
{
    public class Chat
    {
        public string ChatUUID { get; set; }
        public string ChatName { get; set; }
        public bool ChatIsProtected { get; set; }
        public bool ChatIsPublic { get; set; }
        public bool ChatIsDeleted { get; set; }
        public DateTime ChatCreatedAt { get; set; }

        public virtual ICollection<ChatMember> ChatMembers { get; set; }
        public virtual ICollection<ChatMessage> ChatMessages { get; set; }
        public virtual ChatMessage LastChatMessage { get; set; }
    }

    public class ChatEditDetails
    {
        public string ChatUUID { get; set; }
        public string ChatName { get; set; }
        public bool ChatIsPublic { get; set; }
        public bool ChatIsProtected { get; set; }
        public bool ChatIsDeleted { get; set; }
    }

    public class ChatMember
    {
        public string ChatUUID { get; set; }
        public string UserUUID { get; set; }
        public bool IsInChat { get; set; }
        public DateTime? LastReadAt { get; set; }

        public virtual Chat Chat { get; set; }
        public virtual User User { get; set; }
        public virtual UserDTO UserDTO { get; set; }
    }

    public class ChatMessage
    {
        public string ChatMessageUUID { get; set; }
        public string ChatUUID { get; set; }
        public string UserUUID { get; set; }
        public string MessageContent { get; set; }
        public DateTime MessageCreatedAt { get; set; }
        public bool MessageHasBeenEdited { get; set; }
        public int MessageTypeId { get; set; }
        public bool MessageIsDeleted { get; set; }
        public string ReplyingToUUID { get; set; }

        public virtual Chat Chat { get; set; }
        public virtual User User { get; set; }
        public virtual UserDTO UserDTO { get; set; }
        public virtual ChatMessageType ChatMessageType { get; set; }
        public virtual ChatMessage ReplyingTo { get; set; }
        public virtual ChatMessageReplyDTO ReplyingToDTO { get; set; }

        public ChatMessageReplyDTO ConvertToReplyDTO()
        {
            return new ChatMessageReplyDTO
            {
                ChatMessageUUID = ChatMessageUUID,
                UserUUID = UserUUID,
                MessageContent = MessageContent,
                MessageIsDeleted = MessageIsDeleted,
                UserDTO = User.ConvertToDTO()
            };
        }
    }

    public class ChatMessageReplyDTO
    {
        public string ChatMessageUUID { get; set; }
        public string UserUUID { get; set; }
        public string MessageContent { get; set; }
        public bool MessageIsDeleted { get; set; }
        public virtual UserDTO UserDTO { get; set; }
    }

    public class ChatMessageType
    {
        public int ChatMessageTypeId { get; set; }
        public string ChatMessageTypeName { get; set; }
        public bool IsProtected { get; set; }
    }
}
