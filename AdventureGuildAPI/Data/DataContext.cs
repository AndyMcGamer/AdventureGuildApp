using Microsoft.EntityFrameworkCore;
using AdventureGuildAPI.Models;

namespace AdventureGuildAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options): base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QuestCheck>().HasKey(x => new {x.QuestId});
            modelBuilder.Entity<Approval>().HasKey(x => new { x.ApproverId, x.QuestId});
            modelBuilder.Entity<User>().HasOne(x => x.Guild).WithMany().OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<User>().HasOne(x => x.Party).WithMany().OnDelete(DeleteBehavior.SetNull);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<GuildRequest> GuildRequests { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<QuestCheck> QuestChecks { get; set; }
        public DbSet<Approval> Approvals { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<PartyInvite> PartyInvites { get; set; }
    }
}
