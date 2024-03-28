using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Flow.Server.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
	private readonly IConfiguration _config;

	public DbSet<Image> Images { get; set; }
	public DbSet<PDF> PDFs { get; set; }
	public DbSet<ContactRequest> ContactRequests { get; set; }
	public DbSet<ChatThread> Threads { get; set; }
	public DbSet<Message> Messages { get; set; }
	public DbSet<UserSettings> SettingsEntries { get; set; }
	public DbSet<ColorScheme> ColorSchemes { get; set; }

	public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration config) : base(options)
	{
		_config = config;
	}

	protected override void OnConfiguring(DbContextOptionsBuilder builder)
	{
		builder.UseLazyLoadingProxies();
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// define the database relations
		builder.Entity<ChatThread>()
				.HasMany(p => p.Participants)
				.WithMany(p => p.Chats);

		builder.Entity<AppUser>()
				.HasMany(p => p.Chats);

		builder.Entity<ContactRequest>()
				.HasOne(p => p.Sender)
				.WithMany(p => p.ContactRequests)
				.HasForeignKey(p => p.SenderId)
				.OnDelete(DeleteBehavior.NoAction);

		builder.Entity<ContactRequest>()
				.HasOne(p => p.ChatThread)
				.WithOne();

		builder.Entity<Message>()
				.HasOne(p => p.Thread)
				.WithMany(p => p.Messages)
				.HasForeignKey(p => p.ThreadId);

		builder.Entity<AppUser>()
				.HasOne(p => p.Settings)
				.WithOne(p => p.AppUser)
				.HasForeignKey<UserSettings>(p => p.AppUserId)
				.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<UserSettings>()
				.HasOne(p => p.ColorScheme);


		builder.Entity<ColorScheme>()
				.HasData(
					new ColorScheme
					{
						Id = 1,
						Name = "Flow's Default",
						SentMsgBubbleColor = "bg-blue-600 text-white",
						ReceivedMsgBubbleColor = "bg-gray-100 text-gray-600",
						SelectedMessageColor = "bg-red-500 text-white",
						AccentsColor = "text-blue-700 bg-blue-600"
					},
					new ColorScheme
					{
						Id = 2,
						Name = "Lavender Mist",
						SentMsgBubbleColor = "bg-violet-500 text-white",
						ReceivedMsgBubbleColor = "bg-violet-100 text-gray-600",
						AccentsColor = "text-violet-500 bg-violet-500",
						SelectedMessageColor = "bg-red-500 text-white"
					},
					new ColorScheme
					{
						Id = 3,
						Name = "Steel Tones",
						SentMsgBubbleColor = "bg-black text-white",
						ReceivedMsgBubbleColor = "bg-gray-100 text-gray-600",
						AccentsColor = "text-black bg-black",
						SelectedMessageColor = "bg-red-500 text-white"
					},
					new ColorScheme
					{
						Id = 4,
						Name = "Sunny Hues",
						SentMsgBubbleColor = "bg-orange-400 text-white",
						ReceivedMsgBubbleColor = "bg-orange-100 text-gray-700",
						AccentsColor = "text-orange-400 bg-orange-400",
						SelectedMessageColor = "bg-red-500 text-white"
					},
					new ColorScheme
					{
						Id = 5,
						Name = "Fresh Leaves",
						SentMsgBubbleColor = "bg-green-500 text-white",
						ReceivedMsgBubbleColor = "bg-green-100 text-gray-700",
						AccentsColor = "text-green-500 bg-green-500",
						SelectedMessageColor = "bg-red-500 text-white"
					},
					new ColorScheme
					{
						Id = 6,
						Name = "Lemon Lime",
						SentMsgBubbleColor = "bg-lime-500 text-white",
						ReceivedMsgBubbleColor = "bg-lime-100 text-gray-700",
						AccentsColor = "text-lime-500 bg-lime-500",
						SelectedMessageColor = "bg-red-500 text-white"
					},
					new ColorScheme
					{
						Id = 7,
						Name = "Blossom Vibes",
						SentMsgBubbleColor = "bg-rose-500 text-white",
						ReceivedMsgBubbleColor = "bg-rose-100 text-gray-700",
						AccentsColor = "text-rose-500 bg-rose-500",
						SelectedMessageColor = "bg-red-500 text-white"
					},
					new ColorScheme
					{
						Id = 8,
						Name = "Fresh Mint",
						SentMsgBubbleColor = "bg-emerald-500 text-white",
						ReceivedMsgBubbleColor = "bg-emerald-100 text-gray-700",
						AccentsColor = "text-emerald-500 bg-emerald-500",
						SelectedMessageColor = "bg-red-500 text-white"
					}
					);
	}
}
