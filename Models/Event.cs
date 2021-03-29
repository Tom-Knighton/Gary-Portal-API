using System;
namespace GaryPortalAPI.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime EventEndsAt { get; set; }
        public string? EventShortDescription { get; set; }
        public string EventDescription { get; set; }
        public string EventCoverUrl { get; set; }
        public int? EventTeamId { get; set; }
        public bool IsEventDeleted { get; set; }

        public Team EventTeam { get; set; }
    }
}
