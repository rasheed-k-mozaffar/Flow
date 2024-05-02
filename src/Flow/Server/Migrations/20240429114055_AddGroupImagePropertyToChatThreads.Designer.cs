﻿// <auto-generated />
using System;
using Flow.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Flow.Server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240429114055_AddGroupImagePropertyToChatThreads")]
    partial class AddGroupImagePropertyToChatThreads
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AppUserChatThread", b =>
                {
                    b.Property<Guid>("ChatsId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ParticipantsId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ChatsId", "ParticipantsId");

                    b.HasIndex("ParticipantsId");

                    b.ToTable("AppUserChatThread");
                });

            modelBuilder.Entity("Flow.Server.Models.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("Bio")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Flow.Server.Models.ChatThread", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Threads");
                });

            modelBuilder.Entity("Flow.Server.Models.ColorScheme", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AccentsColor")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("varchar");

                    b.Property<string>("ReceivedMsgBubbleColor")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<string>("SelectedMessageColor")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<string>("SentMsgBubbleColor")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.HasKey("Id");

                    b.ToTable("ColorSchemes");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            AccentsColor = "text-blue-700 bg-blue-600",
                            Name = "Flow's Default",
                            ReceivedMsgBubbleColor = "bg-gray-100 text-gray-600",
                            SelectedMessageColor = "bg-red-500 text-white",
                            SentMsgBubbleColor = "bg-blue-600 text-white"
                        },
                        new
                        {
                            Id = 2,
                            AccentsColor = "text-violet-500 bg-violet-500",
                            Name = "Lavender Mist",
                            ReceivedMsgBubbleColor = "bg-violet-100 text-gray-600",
                            SelectedMessageColor = "bg-red-500 text-white",
                            SentMsgBubbleColor = "bg-violet-500 text-white"
                        },
                        new
                        {
                            Id = 3,
                            AccentsColor = "text-black bg-black",
                            Name = "Steel Tones",
                            ReceivedMsgBubbleColor = "bg-gray-100 text-gray-600",
                            SelectedMessageColor = "bg-red-500 text-white",
                            SentMsgBubbleColor = "bg-black text-white"
                        },
                        new
                        {
                            Id = 4,
                            AccentsColor = "text-orange-400 bg-orange-400",
                            Name = "Sunny Hues",
                            ReceivedMsgBubbleColor = "bg-orange-100 text-gray-700",
                            SelectedMessageColor = "bg-red-500 text-white",
                            SentMsgBubbleColor = "bg-orange-400 text-white"
                        },
                        new
                        {
                            Id = 5,
                            AccentsColor = "text-green-500 bg-green-500",
                            Name = "Fresh Leaves",
                            ReceivedMsgBubbleColor = "bg-green-100 text-gray-700",
                            SelectedMessageColor = "bg-red-500 text-white",
                            SentMsgBubbleColor = "bg-green-500 text-white"
                        },
                        new
                        {
                            Id = 6,
                            AccentsColor = "text-lime-500 bg-lime-500",
                            Name = "Lemon Lime",
                            ReceivedMsgBubbleColor = "bg-lime-100 text-gray-700",
                            SelectedMessageColor = "bg-red-500 text-white",
                            SentMsgBubbleColor = "bg-lime-500 text-white"
                        },
                        new
                        {
                            Id = 7,
                            AccentsColor = "text-rose-500 bg-rose-500",
                            Name = "Blossom Vibes",
                            ReceivedMsgBubbleColor = "bg-rose-100 text-gray-700",
                            SelectedMessageColor = "bg-red-500 text-white",
                            SentMsgBubbleColor = "bg-rose-500 text-white"
                        },
                        new
                        {
                            Id = 8,
                            AccentsColor = "text-emerald-500 bg-emerald-500",
                            Name = "Fresh Mint",
                            ReceivedMsgBubbleColor = "bg-emerald-100 text-gray-700",
                            SelectedMessageColor = "bg-red-500 text-white",
                            SentMsgBubbleColor = "bg-emerald-500 text-white"
                        });
                });

            modelBuilder.Entity("Flow.Server.Models.ContactRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ChatThreadId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("RecipientId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("SenderId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ChatThreadId")
                        .IsUnique()
                        .HasFilter("[ChatThreadId] IS NOT NULL");

                    b.HasIndex("RecipientId");

                    b.HasIndex("SenderId");

                    b.ToTable("ContactRequests");
                });

            modelBuilder.Entity("Flow.Server.Models.Image", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AppUserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid?>("ChatThreadId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RelativeUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AppUserId")
                        .IsUnique()
                        .HasFilter("[AppUserId] IS NOT NULL");

                    b.HasIndex("ChatThreadId")
                        .IsUnique()
                        .HasFilter("[ChatThreadId] IS NOT NULL");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("Flow.Server.Models.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SenderId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("SentOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<Guid>("ThreadId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SenderId");

                    b.HasIndex("ThreadId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Flow.Server.Models.PDF", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AppUserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RelativeUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("PDFs");
                });

            modelBuilder.Entity("Flow.Server.Models.UserSettings", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ActivityStatus")
                        .HasColumnType("int");

                    b.Property<string>("AppUserId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ColorSchemeId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("EditedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("EnableNotificationSounds")
                        .HasColumnType("bit");

                    b.Property<bool>("EnableSentMessageSounds")
                        .HasColumnType("bit");

                    b.Property<int>("Theme")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AppUserId")
                        .IsUnique();

                    b.HasIndex("ColorSchemeId");

                    b.ToTable("SettingsEntries");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("AppUserChatThread", b =>
                {
                    b.HasOne("Flow.Server.Models.ChatThread", null)
                        .WithMany()
                        .HasForeignKey("ChatsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Flow.Server.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("ParticipantsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Flow.Server.Models.ContactRequest", b =>
                {
                    b.HasOne("Flow.Server.Models.ChatThread", "ChatThread")
                        .WithOne()
                        .HasForeignKey("Flow.Server.Models.ContactRequest", "ChatThreadId");

                    b.HasOne("Flow.Server.Models.AppUser", "Recipient")
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Flow.Server.Models.AppUser", "Sender")
                        .WithMany("ContactRequests")
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("ChatThread");

                    b.Navigation("Recipient");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("Flow.Server.Models.Image", b =>
                {
                    b.HasOne("Flow.Server.Models.AppUser", null)
                        .WithOne("ProfilePicture")
                        .HasForeignKey("Flow.Server.Models.Image", "AppUserId");

                    b.HasOne("Flow.Server.Models.ChatThread", null)
                        .WithOne("GroupImage")
                        .HasForeignKey("Flow.Server.Models.Image", "ChatThreadId");
                });

            modelBuilder.Entity("Flow.Server.Models.Message", b =>
                {
                    b.HasOne("Flow.Server.Models.AppUser", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Flow.Server.Models.ChatThread", "Thread")
                        .WithMany("Messages")
                        .HasForeignKey("ThreadId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sender");

                    b.Navigation("Thread");
                });

            modelBuilder.Entity("Flow.Server.Models.UserSettings", b =>
                {
                    b.HasOne("Flow.Server.Models.AppUser", "AppUser")
                        .WithOne("Settings")
                        .HasForeignKey("Flow.Server.Models.UserSettings", "AppUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Flow.Server.Models.ColorScheme", "ColorScheme")
                        .WithMany()
                        .HasForeignKey("ColorSchemeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppUser");

                    b.Navigation("ColorScheme");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Flow.Server.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Flow.Server.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Flow.Server.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Flow.Server.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Flow.Server.Models.AppUser", b =>
                {
                    b.Navigation("ContactRequests");

                    b.Navigation("ProfilePicture");

                    b.Navigation("Settings");
                });

            modelBuilder.Entity("Flow.Server.Models.ChatThread", b =>
                {
                    b.Navigation("GroupImage");

                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
