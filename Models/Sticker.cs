using System;
namespace GaryPortalAPI.Models
{
    public class Sticker
    {
        public int StickerId { get; set; }
        public string StickerURL { get; set; }
        public string StickerName { get; set; }
        public bool StickerStaffOnly { get; set; }
        public bool StickerAdminOnly { get; set; }
        public bool StickerIsDeleted { get; set; }
    }
}
