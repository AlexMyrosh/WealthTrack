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
                    .HasConversion<string>();

                entity.Property(e => e.IsSystem)
                    .HasDefaultValue(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>();

                entity.HasOne(e => e.ParentCategory)
                    .WithMany(e => e.ChildCategories)
                    .HasForeignKey(e => e.ParentCategoryId)
                    .OnDelete(DeleteBehavior.ClientCascade);
                
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Category_Name_NotEmpty", $"LEN([{nameof(Category.Name)}]) > 0");
                    t.HasCheckConstraint("CK_Category_IconName_NotEmpty", $"LEN([{nameof(Category.IconName)}]) > 0");
                });
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
                
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Currency_Name_NotEmpty", $"LEN([{nameof(Currency.Name)}]) > 0");
                    t.HasCheckConstraint("CK_Currency_Code_NotEmpty", $"LEN([{nameof(Currency.Code)}]) > 0");
                    t.HasCheckConstraint("CK_Currency_Symbol_NotEmpty", $"LEN([{nameof(Currency.Symbol)}]) > 0");
                });
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
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Budget)
                    .WithMany(e => e.Wallets)
                    .HasForeignKey(e => e.BudgetId)
                    .OnDelete(DeleteBehavior.NoAction);
                
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Wallet_Name_NotEmpty", $"LEN([{nameof(Wallet.Name)}]) > 0");
                });
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

                entity.Property(e => e.SourceWalletId)
                    .IsRequired();

                entity.Property(e => e.TargetWalletId)
                    .IsRequired();

                entity.HasOne(e => e.SourceWallet)
                    .WithMany(w => w.OutgoingTransferTransactions)
                    .HasForeignKey(e => e.SourceWalletId)
                    .OnDelete(DeleteBehavior.ClientCascade);

                entity.HasOne(e => e.TargetWallet)
                    .WithMany(w => w.IncomeTransferTransactions)
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

                entity.HasMany(e => e.Wallets)
                    .WithOne(e => e.Budget)
                    .HasForeignKey(e => e.BudgetId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Budget_Name_NotEmpty", $"LEN([{nameof(Budget.Name)}]) > 0");
                });
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
                        j => j.HasOne<Category>().WithMany().HasForeignKey("CategoryId").IsRequired(),
                        j => j.HasOne<Goal>().WithMany().HasForeignKey("GoalId").IsRequired()
                    );
                
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Goal_Name_NotEmpty", $"LEN([{nameof(Goal.Name)}]) > 0");
                });
            });
        }
    }
}
