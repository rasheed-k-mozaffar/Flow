using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Flow.Server.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
	public DbSet<Image> Images { get; set; }
	public DbSet<PDF> PDFs { get; set; }
	public DbSet<ContactRequest> ContactRequests { get; set; }
	public DbSet<ChatThread> Threads { get; set; }
	public DbSet<Message> Messages { get; set; }
	public DbSet<Notification> Notifications { get; set; }

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{

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

		builder.Entity<Notification>()
				.HasOne(p => p.Recipient);

		builder.Entity<Message>()
				.HasOne(p => p.Thread)
				.WithMany(p => p.Messages)
				.HasForeignKey(p => p.ThreadId);
	}
}
