using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransferTransaction> TransferTransactions { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Goal> Goals { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.IconName)
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDate)
                    .IsRequired();

                entity.Property(e => e.ModifiedDate)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>();

                entity.HasOne(e => e.ParentCategory)
                    .WithMany(e => e.ChildCategories)
                    .HasForeignKey(e => e.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Code)
                    .IsUnique();

                entity.HasIndex(e => e.Name)
                    .IsUnique();

                entity.Property(e => e.ExchangeRate)
                    .HasColumnType("decimal(18,9)")
                    .IsRequired();

                entity.Property(e => e.Symbol)
                    .IsRequired()
                    .IsUnicode()
                    .HasMaxLength(10);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>();
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Balance)
                    .HasColumnType("decimal(18,9)")
                    .IsRequired();

                entity.Property(e => e.IsPartOfGeneralBalance)
                    .IsRequired();

                entity.Property(e => e.CreatedDate)
                    .IsRequired();

                entity.Property(e => e.ModifiedDate)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.CurrencyId)
                    .IsRequired();

                entity.HasOne(e => e.Currency)
                    .WithMany(e => e.Wallets)
                    .HasForeignKey(e => e.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Budget)
                    .WithMany(e => e.Wallets)
                    .HasForeignKey(e => e.BudgetId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,9)")
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.CreatedDate)
                    .IsRequired();

                entity.Property(e => e.TransactionDate)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .HasConversion<string>();

                entity.HasOne(e => e.Category)
                    .WithMany(e => e.Transactions)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Wallet)
                    .WithMany(e => e.Transactions)
                    .HasForeignKey(e => e.WalletId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TransferTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,9)")
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.CreatedDate)
                    .IsRequired();

                entity.Property(e => e.TransactionDate)
                    .IsRequired();

                entity.HasOne(e => e.SourceWallet)
                    .WithMany(e => e.OutgoingTransferTransactions)
                    .HasForeignKey(e => e.SourceWalletId)
                    .OnDelete(DeleteBehavior.ClientCascade);

                entity.HasOne(e => e.TargetWallet)
                    .WithMany(e => e.IncomeTransferTransactions)
                    .HasForeignKey(e => e.TargetWalletId)
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            modelBuilder.Entity<Budget>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.OverallBalance)
                    .HasColumnType("decimal(18,9)")
                    .IsRequired();

                entity.Property(e => e.CreatedDate)
                    .IsRequired();

                entity.Property(e => e.ModifiedDate)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.CurrencyId)
                    .IsRequired();

                entity.HasOne(e => e.Currency)
                    .WithMany(e => e.Budgets)
                    .HasForeignKey(e => e.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Wallets)
                    .WithOne(e => e.Budget)
                    .HasForeignKey(e => e.BudgetId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Goal>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.StartDate)
                    .IsRequired();

                entity.Property(e => e.EndDate)
                    .IsRequired();

                entity.Property(e => e.CreatedDate)
                    .IsRequired();

                entity.Property(e => e.ModifiedDate)
                    .IsRequired();

                entity.Property(e => e.PlannedMoneyAmount)
                    .HasColumnType("decimal(18,9)")
                    .IsRequired();

                entity.Property(e => e.ActualMoneyAmount)
                    .HasColumnType("decimal(18,9)")
                    .IsRequired();

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasConversion<string>();

                entity.HasMany(s => s.Categories)
                    .WithMany(c => c.Goals)
                    .UsingEntity<Dictionary<string, object>>(
                        "GoalCategory",
                        j => j.HasOne<Category>().WithMany().HasForeignKey("CategoryId"),
                        j => j.HasOne<Goal>().WithMany().HasForeignKey("GoalId")
                    );

                entity.HasMany(s => s.Wallets)
                    .WithMany(c => c.Goals)
                    .UsingEntity<Dictionary<string, object>>(
                        "GoalWallet",
                        j => j.HasOne<Wallet>().WithMany().HasForeignKey("WalletId"),
                        j => j.HasOne<Goal>().WithMany().HasForeignKey("GoalId")
                    );
            });
        }
    }
}
