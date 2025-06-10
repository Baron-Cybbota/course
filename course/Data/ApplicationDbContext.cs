using Microsoft.EntityFrameworkCore;
using course.Models; // Убедитесь, что это пространство имен соответствует вашим моделям

namespace course.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSet for each of your models
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Administrator> Administrators { get; set; } = null!;
        public DbSet<BlacklistEntry> BlacklistEntries { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Rating> Ratings { get; set; } = null!;
        public DbSet<Complaint> Complaints { get; set; } = null!;
        public DbSet<BoardGame> BoardGames { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<EventParticipant> EventParticipants { get; set; } = null!;
        public DbSet<Image> Images { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite primary key for EventParticipant
            modelBuilder.Entity<EventParticipant>()
                .HasKey(ep => new { ep.IdUser, ep.IdEvent }); // Используем IdUser, IdEvent

            // --- Configure relationships between models ---

            // User - Administrator (1:1)
            // An Administrator requires an existing User.
            modelBuilder.Entity<Administrator>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Administrator>(a => a.IdUser) // Используем IdUser
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // If a User is deleted, delete the associated Administrator entry

            // User - BlacklistEntry (1:M)
            // A BlacklistEntry refers to a User being blocked.
            modelBuilder.Entity<BlacklistEntry>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(be => be.IdUser) // Используем IdUser
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction); // Prevents user deletion if still in blacklist

            // Administrator - BlacklistEntry (1:M)
            // A BlacklistEntry is created by an Administrator.
            modelBuilder.Entity<BlacklistEntry>()
                .HasOne<Administrator>()
                .WithMany()
                .HasForeignKey(be => be.IdAdministrator) // Используем IdAdministrator
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction); // Prevents administrator deletion if they created blacklist entries

            // Event - Message (1:M)
            // Messages are sent within the context of an Event.
            modelBuilder.Entity<Message>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(m => m.IdEvent) // Используем IdEvent
                .IsRequired(false); // IdEvent is nullable in Message as per ERD

            // User - Message (1:M) (Sender)
            // A Message is sent by a User.
            modelBuilder.Entity<Message>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(m => m.IdUser) // Используем IdUser
                .IsRequired();

            // User - Post (1:M) (Author)
            // A Post is authored by a User.
            modelBuilder.Entity<Post>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.IdUser) // Используем IdUser
                .IsRequired();

            // Event - Post (1:M)
            // A Post can be associated with an Event.
            modelBuilder.Entity<Post>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(p => p.IdEvent) // Используем IdEvent
                .IsRequired(false); // IdEvent is nullable in Post as per ERD

            // Post - Comment (1:M)
            // A Comment belongs to a Post.
            modelBuilder.Entity<Comment>()
                .HasOne<Post>()
                .WithMany()
                .HasForeignKey(c => c.IdPost) // Используем IdPost
                .IsRequired();

            // User - Comment (1:M) (Author)
            // A Comment is authored by a User.
            modelBuilder.Entity<Comment>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.IdUser) // Используем IdUser
                .IsRequired();

            // User - Rating (1:M)
            // A Rating is given by a User.
            modelBuilder.Entity<Rating>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.IdUser) // Используем IdUser
                .IsRequired();

            // Post - Rating (1:M, nullable)
            // A Rating can be for a Post.
            modelBuilder.Entity<Rating>()
                .HasOne<Post>()
                .WithMany()
                .HasForeignKey(r => r.IdPost) // Используем IdPost
                .IsRequired(false); // IdPost is nullable in Rating

            // Comment - Rating (1:M, nullable)
            // A Rating can be for a Comment.
            modelBuilder.Entity<Rating>()
                .HasOne<Comment>()
                .WithMany()
                .HasForeignKey(r => r.IdComment) // Используем IdComment
                .IsRequired(false); // IdComment is nullable in Rating

            // Enforce that only one of IdPost or IdComment is set for Rating
            modelBuilder.Entity<Rating>().ToTable(tb => tb.HasCheckConstraint("CHK_Rating_Target",
    "(\"IdPost\" IS NULL AND \"IdComment\" IS NOT NULL) OR (\"IdPost\" IS NOT NULL AND \"IdComment\" IS NULL)"));



            // User - Complaint (1:M) (Author)
            // A Complaint is made by a User.
            modelBuilder.Entity<Complaint>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.IdUser) // Используем IdUser
                .IsRequired();

            // Administrator - Complaint (1:M, nullable)
            // A Complaint can be handled by an Administrator.
            modelBuilder.Entity<Complaint>()
                .HasOne<Administrator>()
                .WithMany()
                .HasForeignKey(c => c.IdAdministrator) // Используем IdAdministrator
                .IsRequired(false); // IdAdministrator is nullable

            // Post - Complaint (1:M, nullable)
            // A Complaint can be about a Post.
            modelBuilder.Entity<Complaint>()
                .HasOne<Post>()
                .WithMany()
                .HasForeignKey(c => c.IdPost) // Используем IdPost
                .IsRequired(false); // IdPost is nullable

            // Comment - Complaint (1:M, nullable)
            // A Complaint can be about a Comment.
            modelBuilder.Entity<Complaint>()
                .HasOne<Comment>()
                .WithMany()
                .HasForeignKey(c => c.IdComment) // Используем IdComment
                .IsRequired(false); // IdComment is nullable
            
            // Event - Complaint (1:M, nullable)
            // A Complaint can be about an Event.
            modelBuilder.Entity<Complaint>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(c => c.IdEvent) // Используем IdEvent
                .IsRequired(false); // IdEvent is nullable

            // Message - Complaint (1:M, nullable)
            // A Complaint can be about a Message.
            modelBuilder.Entity<Complaint>()
                .HasOne<Message>()
                .WithMany()
                .HasForeignKey(c => c.IdMessage) // Используем IdMessage
                .IsRequired(false); // IdMessage is nullable

            // Enforce that only one of IdPost, IdComment, IdEvent, or IdMessage is set for Complaint
            modelBuilder.Entity<Complaint>().ToTable(tb => tb.HasCheckConstraint("CHK_Complaint_Target",
    "(" +
        "(\"IdPost\" IS NOT NULL AND \"IdComment\" IS NULL AND \"IdEvent\" IS NULL AND \"IdMessage\" IS NULL) OR " +
        "(\"IdPost\" IS NULL AND \"IdComment\" IS NOT NULL AND \"IdEvent\" IS NULL AND \"IdMessage\" IS NULL) OR " +
        "(\"IdPost\" IS NULL AND \"IdComment\" IS NULL AND \"IdEvent\" IS NOT NULL AND \"IdMessage\" IS NULL) OR " +
        "(\"IdPost\" IS NULL AND \"IdComment\" IS NULL AND \"IdEvent\" IS NULL AND \"IdMessage\" IS NOT NULL)" +
    ")"));

            // This constraint ensures exactly one of the four foreign keys (IdPost, IdComment, IdEvent, IdMessage) is NOT NULL.
            // (NULL is 1, NOT NULL is 0. So sum of IS NULL checks should be 3 for one NOT NULL field.)


            // BoardGame - Event (1:M)
            // An Event is for a specific BoardGame.
            modelBuilder.Entity<Event>()
                .HasOne<BoardGame>()
                .WithMany()
                .HasForeignKey(e => e.IdBoardGame) // Используем IdBoardGame
                .IsRequired(false); // IdBoardGame is nullable in Event as per ERD

            // Location - Event (1:M)
            // An Event takes place at a Location.
            modelBuilder.Entity<Event>()
                .HasOne<Location>()
                .WithMany()
                .HasForeignKey(e => e.IdLocation) // Используем IdLocation
                .IsRequired();

            // EventParticipant (Many-to-Many join entity)
            modelBuilder.Entity<EventParticipant>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ep => ep.IdUser)
                .OnDelete(DeleteBehavior.Cascade); // If user deleted, remove participation

            modelBuilder.Entity<EventParticipant>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(ep => ep.IdEvent)
                .OnDelete(DeleteBehavior.Cascade); // If event deleted, remove participation


            // Image relationships (Image can belong to Post, Event, or BoardGame)
            // Note: BoardGame.ImageId in your old model was a 1:1, but the ERD implies Image is a shared entity.
            // The ERD shows dashed lines from Image to Post, Event, and BoardGame, implying nullable FKs from Image.
            // I'll configure these as 1:M relationships from Post/Event/BoardGame TO Image,
            // with FKs in Image.
            
            // Image - Post (1:M, nullable from Image side)
            modelBuilder.Entity<Image>()
                .HasOne<Post>()
                .WithMany()
                .HasForeignKey(i => i.IdPost) // Используем IdPost
                .IsRequired(false); // IdPost is nullable in Image

            // Image - Event (1:M, nullable from Image side)
            modelBuilder.Entity<Image>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(i => i.IdEvent) // Используем IdEvent
                .IsRequired(false); // IdEvent is nullable in Image

            // Image - BoardGame (1:M, nullable from Image side)
            modelBuilder.Entity<Image>()
                .HasOne<BoardGame>()
                .WithMany()
                .HasForeignKey(i => i.IdBoardGame) // Используем IdBoardGame
                .IsRequired(false); // IdBoardGame is nullable in Image

            // Enforce that only one of IdPost, IdEvent, or IdBoardGame is set for Image
            modelBuilder.Entity<Image>().ToTable(tb => tb.HasCheckConstraint("CHK_Image_Association",
    "(" +
        "(\"IdPost\" IS NOT NULL AND \"IdEvent\" IS NULL AND \"IdBoardGame\" IS NULL) OR " +
        "(\"IdPost\" IS NULL AND \"IdEvent\" IS NOT NULL AND \"IdBoardGame\" IS NULL) OR " +
        "(\"IdPost\" IS NULL AND \"IdEvent\" IS NULL AND \"IdBoardGame\" IS NOT NULL)" +
    ")"));

            // This constraint ensures exactly one of the three foreign keys (IdPost, IdEvent, IdBoardGame) is NOT NULL.
            // (NULL is 1, NOT NULL is 0. So sum of IS NULL checks should be 2 for one NOT NULL field.)

        }
    }
}