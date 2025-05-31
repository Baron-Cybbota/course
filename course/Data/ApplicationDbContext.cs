// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using course.Models; // Убедитесь, что здесь есть все ваши модели

namespace course.Data
{
    // Наследуемся от IdentityDbContext<User, Role, int>
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets для ваших сущностей
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
            // Важно! Вызовите base.OnModelCreating(modelBuilder); ПЕРЕД вашей кастомной конфигурацией.
            // Это необходимо для того, чтобы Identity настроил свои таблицы.
            base.OnModelCreating(modelBuilder);

            // --- Конфигурация для Identity (не трогаем, base.OnModelCreating это делает) ---
            // modelBuilder.Entity<User>().ToTable("Users"); // IdentityDbContext уже делает это
            // modelBuilder.Entity<Role>().ToTable("Roles"); // IdentityDbContext уже делает это

            // --- КОНФИГУРАЦИИ ДЛЯ ВАШИХ МОДЕЛЕЙ БЕЗ НАВИГАЦИОННЫХ СВОЙСТВ ---

            // Настройка таблицы Moderators
            modelBuilder.Entity<Moderator>().ToTable("Moderators");
            modelBuilder.Entity<Moderator>().HasKey(m => m.Id);
            // Если у Moderators нет навигационного свойства User (а только UserId),
            // то этот блок с .HasOne().WithOne() должен быть удален.
            // Если вы оставили это навигационное свойство в Moderator.cs, то оставляем:
            // modelBuilder.Entity<Moderator>()
            //     .HasOne(m => m.User) // Предполагается, что User - это виртуальное свойство в Moderator
            //     .WithOne(u => u.ModeratorProfile) // Предполагается, что ModeratorProfile - это виртуальное свойство в User
            //     .HasForeignKey<Moderator>(m => m.UserId)
            //     .IsRequired();


            // Настройка таблицы Posts
            modelBuilder.Entity<Post>().ToTable("Posts");
            modelBuilder.Entity<Post>().HasKey(p => p.Id);
            // Все отношения Posts (Author, Comments, Ratings, Complaints) теперь определяются только по ID.
            // Конфигурации HasOne/WithMany для них должны быть удалены.


            // Настройка таблицы Comments
            modelBuilder.Entity<Comment>().ToTable("Comments");
            modelBuilder.Entity<Comment>().HasKey(c => c.Id);
            // Все отношения Comments (Post, Author, Ratings, Complaints) теперь определяются только по ID.
            // Конфигурации HasOne/WithMany для них должны быть удалены.


            // Настройка таблицы Chats
            modelBuilder.Entity<Chat>().ToTable("Chats");
            modelBuilder.Entity<Chat>().HasKey(c => c.Id);
            // Отношения Chats (Creator, Messages, ChatParticipants) теперь определяются только по ID.
            // Конфигурации HasOne/WithMany для них должны быть удалены.


            // Настройка таблицы ChatParticipants
            modelBuilder.Entity<ChatParticipant>().ToTable("ChatParticipants");
            modelBuilder.Entity<ChatParticipant>().HasKey(cp => cp.Id);
            // Уникальный индекс для ChatId и UserId, чтобы участник был уникален в чате
            modelBuilder.Entity<ChatParticipant>()
                .HasIndex(cp => new { cp.ChatId, cp.UserId })
                .IsUnique();
            // Отношения ChatParticipants (Chat, User) теперь определяются только по ID.
            // Конфигурации HasOne/WithMany для них должны быть удалены.


            // Настройка таблицы Messages
            modelBuilder.Entity<Message>().ToTable("Messages");
            modelBuilder.Entity<Message>().HasKey(m => m.Id);
            // Отношения Messages (Chat, Sender) теперь определяются только по ID.
            // Конфигурации HasOne/WithMany для них должны быть удалены.


            // Настройка таблицы Ratings
            modelBuilder.Entity<Rating>().ToTable("Ratings");
            modelBuilder.Entity<Rating>().HasKey(r => r.Id);
            // Отношения Ratings (User, Post, Comment) теперь определяются только по ID.
            // Конфигурации HasOne/WithMany для них должны быть удалены.

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


            // Настройка таблицы BlacklistEntries
            modelBuilder.Entity<BlacklistEntry>().ToTable("BlacklistEntries");
            modelBuilder.Entity<BlacklistEntry>().HasKey(b => b.Id);
            // Отношения BlacklistEntry (Moderator, User) теперь определяются только по ID.
            // Конфигурации HasOne/WithMany для них должны быть удалены.

            // Уникальный индекс для UserId в BlacklistEntry, чтобы пользователь мог быть заблокирован только один раз
            modelBuilder.Entity<BlacklistEntry>()
                .HasIndex(b => b.UserId)
                .IsUnique();


            // Настройка таблицы Complaints
            modelBuilder.Entity<Complaint>().ToTable("Complaints");
            modelBuilder.Entity<Complaint>().HasKey(c => c.Id);
            // Отношения Complaints (Author, Post, Comment, HandlerModerator) теперь определяются только по ID.
            // Конфигурации HasOne/WithMany для них должны быть удалены.
        }
    }
}