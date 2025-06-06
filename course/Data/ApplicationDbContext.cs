// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using course.Models; // Убедитесь, что здесь есть все ваши модели

namespace course.Data
{
    // Теперь наследуемся напрямую от DbContext
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets для ваших сущностей
        public DbSet<User> Users { get; set; } // Ваша кастомная модель User
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<BlacklistEntry> BlacklistEntries { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; } // Переименовано из Message
        public DbSet<ChatParticipant> ChatParticipants { get; set; } // ДОБАВЛЕНО: DbSet для ChatParticipant
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<BoardGame> BoardGames { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Здесь НЕ нужно вызывать base.OnModelCreating(modelBuilder),
            // так как мы больше не используем IdentityDbContext.

            // --- КОНФИГУРАЦИИ ДЛЯ ВАШИХ МОДЕЛЕЙ БЕЗ НАВИГАЦИОННЫХ СВОЙСТВ ---

            // Настройка таблицы Users
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            // Для поля PasswordHash в User, если вы хотите его настроить, например, максимальную длину
            // modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasMaxLength(256); // Пример

            // Настройка таблицы Administrators
            modelBuilder.Entity<Administrator>().ToTable("Administrators");
            modelBuilder.Entity<Administrator>().HasKey(a => a.Id);
            // Уникальный индекс на UserId, так как один User может быть только одним Administrator
            modelBuilder.Entity<Administrator>()
                .HasIndex(a => a.UserId)
                .IsUnique();

            // Настройка таблицы BlacklistEntries
            modelBuilder.Entity<BlacklistEntry>().ToTable("BlacklistEntries");
            modelBuilder.Entity<BlacklistEntry>().HasKey(b => b.Id);
            // Уникальный индекс для UserId в BlacklistEntry, чтобы пользователь мог быть заблокирован только один раз
            modelBuilder.Entity<BlacklistEntry>()
                .HasIndex(b => b.UserId)
                .IsUnique();

            // Настройка таблицы Chats
            modelBuilder.Entity<Chat>().ToTable("Chats");
            modelBuilder.Entity<Chat>().HasKey(c => c.Id);

            // Настройка таблицы ChatMessages
            modelBuilder.Entity<ChatMessage>().ToTable("ChatMessages"); // Переименовано из Messages
            modelBuilder.Entity<ChatMessage>().HasKey(m => m.Id);

            // ДОБАВЛЕНО: Настройка таблицы ChatParticipants
            // Первичный ключ здесь должен быть составным из ChatId и UserId.
            modelBuilder.Entity<ChatParticipant>().ToTable("ChatParticipants");
            modelBuilder.Entity<ChatParticipant>()
                .HasKey(cp => new { cp.ChatId, cp.UserId }); // Составной первичный ключ

            // Настройка таблицы Posts
            modelBuilder.Entity<Post>().ToTable("Posts");
            modelBuilder.Entity<Post>().HasKey(p => p.Id);

            // Настройка таблицы Comments
            modelBuilder.Entity<Comment>().ToTable("Comments");
            modelBuilder.Entity<Comment>().HasKey(c => c.Id);

            // Настройка таблицы Ratings
            modelBuilder.Entity<Rating>().ToTable("Ratings");
            modelBuilder.Entity<Rating>().HasKey(r => r.Id);
            // Уникальные индексы для Ratings (если рейтинг уникален для пары пользователь-пост ИЛИ пользователь-комментарий)
            // Это важно, чтобы пользователь не мог оценить одно и то же дважды.
            modelBuilder.Entity<Rating>()
                .HasIndex(r => new { r.UserId, r.PostId })
                .IsUnique()
                .HasFilter("\"PostId\" IS NOT NULL"); // Применяется только если PostId не null

            modelBuilder.Entity<Rating>()
                .HasIndex(r => new { r.UserId, r.CommentId })
                .IsUnique()
                .HasFilter("\"CommentId\" IS NOT NULL"); // Применяется только если CommentId не null

            // Настройка таблицы Complaints
            modelBuilder.Entity<Complaint>().ToTable("Complaints");
            modelBuilder.Entity<Complaint>().HasKey(c => c.Id);

            // Настройка таблицы BoardGames
            modelBuilder.Entity<BoardGame>().ToTable("BoardGames");
            modelBuilder.Entity<BoardGame>().HasKey(bg => bg.Id);

            // Настройка таблицы Events
            modelBuilder.Entity<Event>().ToTable("Events");
            modelBuilder.Entity<Event>().HasKey(e => e.Id);

            // Настройка таблицы EventParticipants
            modelBuilder.Entity<EventParticipant>().ToTable("EventParticipants");
            // Определение составного первичного ключа
            modelBuilder.Entity<EventParticipant>()
                .HasKey(ep => new { ep.UserId, ep.EventId });
            // Дополнительно можно добавить уникальный индекс, если это не явно следует из составного ключа
            // (в данном случае составной ключ уже гарантирует уникальность)
        }
    }
}
