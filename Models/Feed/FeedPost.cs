using System;
using System.Collections.Generic;
using GaryPortalAPI.Data;
using Newtonsoft.Json;

namespace GaryPortalAPI.Models.Feed
{
    [JsonConverter(typeof(FeedPostJsonConverter))]
    public class FeedPost
    {
        public int PostId { get; set; }
        public string PosterUUID { get; set; }
        public int TeamId { get; set; }
        public bool PostIsGlobal { get; set; }
        public string PostType { get; set; }
        public DateTime PostCreatedAt { get; set; }
        public string PostDescription { get; set; }
        public int PostLikeCount { get; set; }
        public bool IsDeleted { get; set; }

        public virtual User Poster { get; set; }
        public virtual UserDTO PosterDTO { get; set; }
        public virtual Team PostTeam { get; set; }
        public virtual ICollection<FeedLike> Likes { get; set; }
        public virtual ICollection<FeedComment> Comments { get; set; }

        public FeedPostDTO ConvertToDTO()
        {
            return new FeedPostDTO
            {
                PostId = PostId,
                PosterUUID = PosterUUID,
                PostType = PostType
            };
        }
    }


    public class FeedPostDTO
    {
        public int PostId { get; set; }
        public string PosterUUID { get; set; }
        public string PostType { get; set; }
        public string PostUrl { get; set; }
        public bool IsVideo { get; set; }
    }

    public class FeedMediaPost : FeedPost
    {
        public string PostUrl { get; set; }
        public bool IsVideo { get; set; }

        public new FeedPostDTO ConvertToDTO()
        {
            FeedPostDTO dto = base.ConvertToDTO();
            dto.PostUrl = PostUrl;
            dto.IsVideo = IsVideo;
            return dto;
        }
    }

    public class FeedPollPost : FeedPost
    {
        public string PollQuestion { get; set; }

        public virtual ICollection<FeedPollAnswer> PollAnswers { get; set; }
    }

    public class FeedPollAnswer
    {
        public int PollAnswerId { get; set; }
        public int PollId { get; set; }
        public string Answer { get; set; }

        public virtual FeedPollPost Poll { get; set; }
        public virtual ICollection<FeedAnswerVote> Votes { get; set; }
    }

    public class FeedAnswerVote
    {
        public int PollAnswerId { get; set; }
        public string UserUUID { get; set; }
        public bool IsDeleted { get; set; }

        public virtual User User { get; set; }
        public virtual UserDTO UserDTO { get; set; }
        public virtual FeedPollAnswer PollAnswer { get; set; }
    }

    public class FeedLike
    {
        public string UserUUID { get; set; }
        public int PostId { get; set; }
        public bool IsLiked { get; set; }

        public virtual User User { get; set; }
        public virtual UserDTO UserDTO { get; set; }
        public virtual FeedPost Post { get; set; }
    }

    public class FeedComment
    {
        public int FeedCommentId { get; set; }
        public string UserUUID { get; set; }
        public int PostId { get; set; }
        public string Comment { get; set; }
        public bool IsAdminComment { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DatePosted { get; set; }

        public virtual User User { get; set; }
        public virtual UserDTO UserDTO { get; set; }
        public virtual FeedPost Post { get; set; }
    }

}
