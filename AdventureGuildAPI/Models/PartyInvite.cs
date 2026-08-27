using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureGuildAPI.Models
{
    public class PartyInvite
    {
        public int PartyId { get; set; }
        public int InviterId { get; set; }
        public int AcceptId { get; set; }

        [ForeignKey("PartyId")]
        public Party Party { get; set; } = null!;

        [ForeignKey("InviterId")]
        public User Inviter { get; set; } = null!;

        [ForeignKey("AcceptId")]
        public User AcceptUser { get; set; } = null!;
    }
}
