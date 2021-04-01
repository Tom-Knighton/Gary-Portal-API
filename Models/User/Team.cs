using System;
using System.Collections.Generic;
using GaryPortalAPI.Models.Feed;

namespace GaryPortalAPI.Models
{
    public class Team
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int TeamAccessLevel { get; set; }

        public virtual ICollection<UserTeam> UserTeams { get; set; }
        public virtual ICollection<FeedPost> FeedPosts { get; set; }
        public virtual ICollection<AditLog> AditLogs { get; set; }
        public virtual ICollection<Event> TeamEvents { get; set; }
    }
}
