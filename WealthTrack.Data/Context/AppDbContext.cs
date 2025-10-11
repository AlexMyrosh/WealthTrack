using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Goal> Goals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.IconName)
                    .HasMaxLength(100);
                
                entity.Property(e => e.Type)
                    .HasConversion<string>();

                entity.Property(e => e.IsSystem)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedDate)
                    .IsRequired();

                entity.Property(e => e.ModifiedDate)
                    .IsRequired();
                
                entity.HasOne(e => e.ParentCategory)
                    .WithMany(e => e.ChildCategories)
                    .HasForeignKey(e => e.ParentCategoryId)
                    .OnDelete(DeleteBehavior.NoAction);
                
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Category_Name_NotEmpty", $"LEN([{nameof(Category.Name)}]) > 0");
                    t.HasCheckConstraint("CK_Category_IconName_NotEmpty", $"LEN([{nameof(Category.IconName)}]) > 0");
                });
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Symbol)
                    .IsRequired()
                    .IsUnicode()
                    .HasMaxLength(10);

                entity.Property(e => e.ExchangeRate)
                    .HasColumnType("decimal(18,9)")
                    .IsRequired();
                
                entity.Property(e => e.Type)
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

                entity.Property(e => e.CurrencyId)
                    .IsRequired();
                
                entity.HasOne(e => e.Currency)
                    .WithMany(e => e.Wallets)
                    .HasForeignKey(e => e.CurrencyId)
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
                
                entity.Property(e => e.TransactionDate)
                    .IsRequired();

                entity.HasIndex(e => e.TransactionDate);

                entity.Property(e => e.CreatedDate)
                    .IsRequired();
                
                entity.Property(e => e.ModifiedDate)
                    .IsRequired();
                
                entity.Property(e => e.Type)
                    .HasConversion<string>()
                    .IsRequired();
                
                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.HasOne(e => e.Category)
                    .WithMany(e => e.Transactions)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Wallet)
                    .WithMany(e => e.Transactions)
                    .HasForeignKey(e => e.WalletId)
                    .OnDelete(DeleteBehavior.NoAction);
                
                entity.HasOne(e => e.SourceWallet)
                    .WithMany(w => w.OutgoingTransferTransactions)
                    .HasForeignKey(e => e.SourceWalletId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.TargetWallet)
                    .WithMany(w => w.IncomeTransferTransactions)
                    .HasForeignKey(e => e.TargetWalletId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Goal>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.PlannedMoneyAmount)
                    .HasColumnType("decimal(18,9)")
                    .IsRequired();
                
                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.StartDate)
                    .IsRequired();

                entity.Property(e => e.EndDate)
                    .IsRequired();

                entity.Property(e => e.CreatedDate)
                    .IsRequired();

                entity.Property(e => e.ModifiedDate)
                    .IsRequired();

                entity.HasMany(s => s.Categories)
                    .WithMany(c => c.Goals)
                    .UsingEntity<Dictionary<string, object>>(
                        "GoalCategory",
                        j => j
                            .HasOne<Category>()
                            .WithMany()
                            .HasForeignKey("CategoryId")
                            .IsRequired()
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j
                            .HasOne<Goal>()
                            .WithMany()
                            .HasForeignKey("GoalId")
                            .IsRequired()
                            .OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey("GoalId", "CategoryId");
                            j.ToTable("GoalCategory");
                            j.HasIndex("GoalId");
                            j.HasIndex("CategoryId");
                        });

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Goal_Name_NotEmpty", $"LEN([{nameof(Goal.Name)}]) > 0");
                });
            });
        }
    }
}