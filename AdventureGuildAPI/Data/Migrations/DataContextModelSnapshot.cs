﻿// <auto-generated />
using System;
using AdventureGuildAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AdventureGuildAPI.Data.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AdventureGuildAPI.Models.Approval", b =>
                {
                    b.Property<int>("ApproverId")
                        .HasColumnType("integer")
                        .HasColumnName("approverid");

                    b.Property<int>("QuestId")
                        .HasColumnType("integer")
                        .HasColumnName("questid");

                    b.Property<bool>("Approved")
                        .HasColumnType("boolean")
                        .HasColumnName("approved");

                    b.HasKey("ApproverId", "QuestId")
                        .HasName("pk_approvals");

                    b.HasIndex("QuestId")
                        .HasDatabaseName("ix_approvals_questid");

                    b.ToTable("approvals", (string)null);
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.Friendship", b =>
                {
                    b.Property<int>("AcceptId")
                        .HasColumnType("integer")
                        .HasColumnName("acceptid");

                    b.Property<bool>("Confirmed")
                        .HasColumnType("boolean")
                        .HasColumnName("confirmed");

                    b.Property<int>("RequestId")
                        .HasColumnType("integer")
                        .HasColumnName("requestid");

                    b.HasIndex("AcceptId")
                        .HasDatabaseName("ix_friendships_acceptid");

                    b.HasIndex("RequestId")
                        .HasDatabaseName("ix_friendships_requestid");

                    b.ToTable("friendships", (string)null);
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.Guild", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<bool>("IsPrivate")
                        .HasColumnType("boolean")
                        .HasColumnName("isprivate");

                    b.Property<int>("LeaderId")
                        .HasColumnType("integer")
                        .HasColumnName("leaderid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_guilds");

                    b.HasIndex("LeaderId")
                        .HasDatabaseName("ix_guilds_leaderid");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_guilds_name");

                    b.ToTable("guilds", (string)null);
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.GuildRequest", b =>
                {
                    b.Property<int>("GuildId")
                        .HasColumnType("integer")
                        .HasColumnName("guildid");

                    b.Property<int>("RequestId")
                        .HasColumnType("integer")
                        .HasColumnName("requestid");

                    b.HasIndex("GuildId")
                        .HasDatabaseName("ix_guildrequests_guildid");

                    b.HasIndex("RequestId")
                        .HasDatabaseName("ix_guildrequests_requestid");

                    b.ToTable("guildrequests", (string)null);
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.Party", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_parties");

                    b.ToTable("parties", (string)null);
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.PartyInvite", b =>
                {
                    b.Property<int>("AcceptId")
                        .HasColumnType("integer")
                        .HasColumnName("acceptid");

                    b.Property<string>("InviteName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("invitename");

                    b.Property<int>("PartyId")
                        .HasColumnType("integer")
                        .HasColumnName("partyid");

                    b.HasIndex("AcceptId")
                        .HasDatabaseName("ix_partyinvites_acceptid");

                    b.HasIndex("PartyId")
                        .HasDatabaseName("ix_partyinvites_partyid");

                    b.ToTable("partyinvites", (string)null);
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.Quest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("createddatetime");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int>("Priority")
                        .HasColumnType("integer")
                        .HasColumnName("priority");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("userid");

                    b.HasKey("Id")
                        .HasName("pk_quests");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_quests_userid");

                    b.ToTable("quests", (string)null);
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.QuestCheck", b =>
                {
                    b.Property<int>("QuestId")
                        .HasColumnType("integer")
                        .HasColumnName("questid");

                    b.Property<string>("ImageRef")
                        .HasColumnType("text")
                        .HasColumnName("imageref");

                    b.Property<int?>("PartyId")
                        .HasColumnType("integer")
                        .HasColumnName("partyid");

                    b.Property<int>("RequestId")
                        .HasColumnType("integer")
                        .HasColumnName("requestid");

                    b.HasKey("QuestId")
                        .HasName("pk_questchecks");

                    b.HasIndex("PartyId")
                        .HasDatabaseName("ix_questchecks_partyid");

                    b.HasIndex("RequestId")
                        .HasDatabaseName("ix_questchecks_requestid");

                    b.ToTable("questchecks", (string)null);
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("emailaddress");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("firstname");

                    b.Property<int?>("GuildId")
                        .HasColumnType("integer")
                        .HasColumnName("guildid");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("lastname");

                    b.Property<int>("Money")
                        .HasColumnType("integer")
                        .HasColumnName("money");

                    b.Property<int?>("PartyId")
                        .HasColumnType("integer")
                        .HasColumnName("partyid");

                    b.Property<byte[]>("Password")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("password");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("passwordsalt");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("text")
                        .HasColumnName("refreshtoken");

                    b.Property<DateTime?>("ResetPassExpires")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("resetpassexpires");

                    b.Property<byte[]>("ResetPasswordToken")
                        .HasColumnType("bytea")
                        .HasColumnName("resetpasswordtoken");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("role");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.Property<byte[]>("VerificationToken")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("verificationtoken");

                    b.Property<bool>("Verified")
                        .HasColumnType("boolean")
                        .HasColumnName("verified");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("EmailAddress")
                        .IsUnique()
                        .HasDatabaseName("ix_users_emailaddress");

                    b.HasIndex("GuildId")
                        .HasDatabaseName("ix_users_guildid");

                    b.HasIndex("PartyId")
                        .HasDatabaseName("ix_users_partyid");

                    b.HasIndex("ResetPasswordToken")
                        .IsUnique()
                        .HasDatabaseName("ix_users_resetpasswordtoken");

                    b.HasIndex("Username")
                        .IsUnique()
                        .HasDatabaseName("ix_users_username");

                    b.HasIndex("VerificationToken")
                        .IsUnique()
                        .HasDatabaseName("ix_users_verificationtoken");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.Approval", b =>
                {
                    b.HasOne("AdventureGuildAPI.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("ApproverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_approvals_users_approverid");

                    b.HasOne("AdventureGuildAPI.Models.Quest", "Quest")
                        .WithMany()
                        .HasForeignKey("QuestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_approvals_quests_questid");

                    b.Navigation("Quest");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.Friendship", b =>
                {
                    b.HasOne("AdventureGuildAPI.Models.User", "AcceptUser")
                        .WithMany()
                        .HasForeignKey("AcceptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_friendships_users_acceptid");

                    b.HasOne("AdventureGuildAPI.Models.User", "RequestUser")
                        .WithMany()
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_friendships_users_requestid");

                    b.Navigation("AcceptUser");

                    b.Navigation("RequestUser");
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.Guild", b =>
                {
                    b.HasOne("AdventureGuildAPI.Models.User", "Leader")
                        .WithMany()
                        .HasForeignKey("LeaderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_guilds_users_leaderid");

                    b.Navigation("Leader");
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.GuildRequest", b =>
                {
                    b.HasOne("AdventureGuildAPI.Models.Guild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_guildrequests_guilds_guildid");

                    b.HasOne("AdventureGuildAPI.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_guildrequests_users_requestid");

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.PartyInvite", b =>
                {
                    b.HasOne("AdventureGuildAPI.Models.User", "AcceptUser")
                        .WithMany()
                        .HasForeignKey("AcceptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_partyinvites_users_acceptid");

                    b.HasOne("AdventureGuildAPI.Models.Party", "Party")
                        .WithMany()
                        .HasForeignKey("PartyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_partyinvites_parties_partyid");

                    b.Navigation("AcceptUser");

                    b.Navigation("Party");
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.Quest", b =>
                {
                    b.HasOne("AdventureGuildAPI.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_quests_users_userid");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.QuestCheck", b =>
                {
                    b.HasOne("AdventureGuildAPI.Models.Party", "Party")
                        .WithMany()
                        .HasForeignKey("PartyId")
                        .HasConstraintName("fk_questchecks_parties_partyid");

                    b.HasOne("AdventureGuildAPI.Models.Quest", "Quest")
                        .WithMany()
                        .HasForeignKey("QuestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_questchecks_quests_questid");

                    b.HasOne("AdventureGuildAPI.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_questchecks_users_requestid");

                    b.Navigation("Party");

                    b.Navigation("Quest");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AdventureGuildAPI.Models.User", b =>
                {
                    b.HasOne("AdventureGuildAPI.Models.Guild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_users_guilds_guildid");

                    b.HasOne("AdventureGuildAPI.Models.Party", "Party")
                        .WithMany()
                        .HasForeignKey("PartyId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_users_parties_partyid");

                    b.Navigation("Guild");

                    b.Navigation("Party");
                });
#pragma warning restore 612, 618
        }
    }
}
