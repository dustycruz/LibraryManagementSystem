using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class LibraryDbContext : DbContext

    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.UserId);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Email).HasMaxLength(255).IsRequired();
                e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
                e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
                e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            });

            // Role
            modelBuilder.Entity<Role>(e =>
            {
                e.HasKey(r => r.RoleId);
                e.HasIndex(r => r.RoleName).IsUnique();
                e.Property(r => r.RoleName).HasMaxLength(50).IsRequired();
            });

            // UserRole
            modelBuilder.Entity<UserRole>(e =>
            {
                e.HasKey(ur => ur.UserRoleId);
                e.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
                e.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
            });

            // Category
            modelBuilder.Entity<Category>(e =>
            {
                e.HasKey(c => c.CategoryId);
                e.HasIndex(c => c.CategoryName).IsUnique();
                e.Property(c => c.CategoryName).HasMaxLength(100).IsRequired();
            });

            // Book
            modelBuilder.Entity<Book>(e =>
            {
                e.HasKey(b => b.BookId);
                e.HasIndex(b => b.ISBN).IsUnique();
                e.Property(b => b.ISBN).HasMaxLength(20).IsRequired();
                e.Property(b => b.Title).HasMaxLength(300).IsRequired();
                e.Property(b => b.Author).HasMaxLength(200).IsRequired();
                e.HasOne(b => b.Category).WithMany(c => c.Books).HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Restrict);
            });

            // BorrowRecord
            modelBuilder.Entity<BorrowRecord>(e =>
            {
                e.HasKey(br => br.BorrowId);
                e.Property(br => br.Status).HasMaxLength(20).IsRequired();
                e.HasOne(br => br.User).WithMany(u => u.BorrowRecords).HasForeignKey(br => br.UserId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(br => br.Book).WithMany(b => b.BorrowRecords).HasForeignKey(br => br.BookId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(br => br.ProcessedByUser).WithMany().HasForeignKey(br => br.ProcessedByUserId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            });

            // Fine
            modelBuilder.Entity<Fine>(e =>
            {
                e.HasKey(f => f.FineId);
                e.Property(f => f.Amount).HasPrecision(10, 2);
                e.HasOne(f => f.BorrowRecord).WithOne(br => br.Fine).HasForeignKey<Fine>(f => f.BorrowId).OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLog
            modelBuilder.Entity<AuditLog>(e =>
            {
                e.HasKey(a => a.AuditId);
                e.Property(a => a.Action).HasMaxLength(100).IsRequired();
                e.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
                e.HasOne(a => a.User).WithMany(u => u.AuditLogs).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            });
        }
    }
}
