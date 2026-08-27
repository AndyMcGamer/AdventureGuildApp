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
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QuestCheck>().HasKey(x => new {x.QuestId});
            modelBuilder.Entity<Approval>().HasKey(x => new { x.ApproverId, x.QuestId});

            modelBuilder.Entity<Friendship>().HasKey(x => new { x.RequestId, x.AcceptId });
            modelBuilder.Entity<Friendship>().HasOne(x => x.RequestUser).WithMany().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Friendship>().HasOne(x => x.AcceptUser).WithMany().HasForeignKey(x => x.AcceptId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuildRequest>().HasKey(x => new { x.RequestId, x.GuildId });
            modelBuilder.Entity<GuildRequest>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<GuildRequest>().HasOne(x => x.Guild).WithMany().HasForeignKey(x => x.GuildId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PartyInvite>().HasKey(x => new { x.PartyId, x.AcceptId });
            modelBuilder.Entity<PartyInvite>().HasOne(x => x.Party).WithMany().HasForeignKey(x => x.PartyId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PartyInvite>().HasOne(x => x.Inviter).WithMany().HasForeignKey(x => x.InviterId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PartyInvite>().HasOne(x => x.AcceptUser).WithMany().HasForeignKey(x => x.AcceptId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>().HasOne(x => x.Guild).WithMany().HasForeignKey(x => x.GuildId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<User>().HasOne(x => x.Party).WithMany().OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Guild>().HasOne(x => x.Leader).WithMany().HasForeignKey(x => x.LeaderId).OnDelete(DeleteBehavior.Cascade);
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
