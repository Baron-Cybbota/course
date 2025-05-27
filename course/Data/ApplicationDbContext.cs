using Microsoft.EntityFrameworkCore;
using course.Models; // Убедитесь, что это пространство имен ваших моделей

namespace course.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Moderator> Moderators { get; set; }
		public DbSet<Post> Posts { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Chat> Chats { get; set; }
		public DbSet<ChatParticipant> ChatParticipants { get; set; }
		public DbSet<Message> Messages { get; set; }
		public DbSet<Rating> Ratings { get; set; }
		public DbSet<BlacklistEntry> BlacklistEntries { get; set; }
		public DbSet<Complaint> Complaints { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Настройка таблицы Users
			modelBuilder.Entity<User>().ToTable("Users");
			modelBuilder.Entity<User>().HasKey(u => u.Id);
			modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();
			modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

			// Настройка таблицы Moderators
			modelBuilder.Entity<Moderator>().ToTable("Moderators");
			modelBuilder.Entity<Moderator>().HasKey(m => m.Id);
			modelBuilder.Entity<Moderator>()
				.HasOne(m => m.User)
				.WithOne(u => u.ModeratorProfile)
				.HasForeignKey<Moderator>(m => m.UserId)
				.IsRequired();

			// Настройка таблицы Posts
			modelBuilder.Entity<Post>().ToTable("Posts");
			modelBuilder.Entity<Post>().HasKey(p => p.Id);
			modelBuilder.Entity<Post>()
				.HasOne(p => p.Author)
				.WithMany(u => u.Posts)
				.HasForeignKey(p => p.AuthorId)
				.IsRequired();

			// Настройка таблицы Comments
			modelBuilder.Entity<Comment>().ToTable("Comments");
			modelBuilder.Entity<Comment>().HasKey(c => c.Id);
			modelBuilder.Entity<Comment>()
				.HasOne(c => c.Post)
				.WithMany(p => p.Comments)
				.HasForeignKey(c => c.PostId)
				.IsRequired();
			modelBuilder.Entity<Comment>()
				.HasOne(c => c.Author)
				.WithMany(u => u.Comments)
				.HasForeignKey(c => c.AuthorId)
				.IsRequired();

			// Настройка таблицы Chats
			modelBuilder.Entity<Chat>().ToTable("Chats");
			modelBuilder.Entity<Chat>().HasKey(c => c.Id);
			modelBuilder.Entity<Chat>()
				.HasOne(c => c.Creator)
				.WithMany(u => u.CreatedChats)
				.HasForeignKey(c => c.CreatorId)
				.IsRequired();

			// Настройка таблицы ChatParticipants (многие-ко-многим через промежуточную сущность)
			modelBuilder.Entity<ChatParticipant>().ToTable("ChatParticipants");
			modelBuilder.Entity<ChatParticipant>().HasKey(cp => cp.Id);
			modelBuilder.Entity<ChatParticipant>()
				.HasIndex(cp => new { cp.ChatId, cp.UserId })
				.IsUnique();

			modelBuilder.Entity<ChatParticipant>()
				.HasOne(cp => cp.Chat)
				.WithMany(c => c.ChatParticipants)
				.HasForeignKey(cp => cp.ChatId)
				.IsRequired();

			modelBuilder.Entity<ChatParticipant>()
				.HasOne(cp => cp.User)
				.WithMany(u => u.ChatParticipants)
				.HasForeignKey(cp => cp.UserId)
				.IsRequired();

			// Настройка таблицы Messages
			modelBuilder.Entity<Message>().ToTable("Messages");
			modelBuilder.Entity<Message>().HasKey(m => m.Id);
			modelBuilder.Entity<Message>()
				.HasOne(m => m.Chat)
				.WithMany(c => c.Messages)
				.HasForeignKey(m => m.ChatId)
				.IsRequired();
			modelBuilder.Entity<Message>()
				.HasOne(m => m.Sender)
				.WithMany(u => u.SentMessages)
				.HasForeignKey(m => m.SenderId)
				.IsRequired();

			// Настройка таблицы Ratings
			modelBuilder.Entity<Rating>().ToTable("Ratings");
			modelBuilder.Entity<Rating>().HasKey(r => r.Id);
			modelBuilder.Entity<Rating>()
				.HasOne(r => r.User)
				.WithMany(u => u.Ratings)
				.HasForeignKey(r => r.UserId)
				.IsRequired();

			modelBuilder.Entity<Rating>()
				.HasOne(r => r.Post)
				.WithMany(p => p.Ratings)
				.HasForeignKey(r => r.PostId)
				.IsRequired(false);

			modelBuilder.Entity<Rating>()
				.HasOne(r => r.Comment)
				.WithMany(c => c.Ratings)
				.HasForeignKey(r => r.CommentId)
				.IsRequired(false);

			modelBuilder.Entity<Rating>()
				.HasIndex(r => new { r.UserId, r.PostId })
				.IsUnique()
				.HasFilter("\"PostId\" IS NOT NULL");

			modelBuilder.Entity<Rating>()
				.HasIndex(r => new { r.UserId, r.CommentId })
				.IsUnique()
				.HasFilter("\"CommentId\" IS NOT NULL");


			// Настройка таблицы BlacklistEntries
			modelBuilder.Entity<BlacklistEntry>().ToTable("BlacklistEntries");
			modelBuilder.Entity<BlacklistEntry>().HasKey(b => b.Id);
			modelBuilder.Entity<BlacklistEntry>()
				.HasOne(b => b.Moderator)
				.WithMany(m => m.ManagedBlacklistEntries)
				.HasForeignKey(b => b.ModeratorId)
				.IsRequired();

			modelBuilder.Entity<BlacklistEntry>()
				.HasOne(b => b.User)
				.WithOne(u => u.CurrentBlacklistEntry)
				.HasForeignKey<BlacklistEntry>(b => b.UserId)
				.IsRequired();

			modelBuilder.Entity<BlacklistEntry>()
				.HasIndex(b => b.UserId)
				.IsUnique();

			// Настройка таблицы Complaints
			modelBuilder.Entity<Complaint>().ToTable("Complaints");
			modelBuilder.Entity<Complaint>().HasKey(c => c.Id);
			modelBuilder.Entity<Complaint>()
				.HasOne(c => c.Author)
				.WithMany(u => u.AuthoredComplaints)
				.HasForeignKey(c => c.AuthorId)
				.IsRequired();

			modelBuilder.Entity<Complaint>()
				.HasOne(c => c.Post)
				.WithMany(p => p.Complaints)
				.HasForeignKey(c => c.PostId)
				.IsRequired(false);

			modelBuilder.Entity<Complaint>()
				.HasOne(c => c.Comment)
				.WithMany(c => c.Complaints)
				.HasForeignKey(c => c.CommentId)
				.IsRequired(false);

			modelBuilder.Entity<Complaint>()
				.HasOne(c => c.HandlerModerator)
				.WithMany(m => m.HandledComplaints)
				.HasForeignKey(c => c.HandledByModeratorId)
				.IsRequired(false);
		}
	}
}