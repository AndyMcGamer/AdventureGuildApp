using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureGuildAPI.Models
{
    [Keyless]
    public class PartyInvite
    {
        public int PartyId { get; set; }
        public string InviteName { get; set; }
        public int AcceptId { get; set; }

        [ForeignKey("PartyId")]
        public Party Party { get; set; }

        [ForeignKey("AcceptId")]
        public User AcceptUser { get; set; }
    }
}
