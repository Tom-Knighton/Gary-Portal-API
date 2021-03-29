using System;
namespace GaryPortalAPI.Models
{
	public class Commandment
	{
		public int CommandmentId { get; set; }
		public string CommandmentName { get; set; }
		public string CommandmentDescription { get; set; }
		public string? CommandmentCoverUrl { get; set; }
		public bool CommandmentIsDeleted { get; set; }
	}
}
