using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureGuildAPI.Models
{
    [Keyless]
    public class Friendship
    {
        public int RequestId { get; set; }
        public int AcceptId { get; set; }
        public bool Confirmed { get; set; }

        [ForeignKey("RequestId")]
        public User RequestUser { get; set; }
        [ForeignKey("AcceptId")]
        public User AcceptUser { get; set; }
    }

}
