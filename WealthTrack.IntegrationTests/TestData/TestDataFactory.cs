using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.TestData
{
    public class TestDataFactory
    {
        private readonly Random _random = new();

        // Core entity creators
        public Currency CreateCurrency(Action<Currency>? configure = null)
        {
            var currency = new Currency
            {
                Id = Guid.NewGuid(),
                Code = $"C{_random.Next(1000, 9999)}",
                Name = $"Currency {_random.Next(1, 1000)}",
                Symbol = $"S{_random.Next(1, 100)}",
                ExchangeRate = (decimal)(_random.NextDouble() * 10 + 0.1),
                Status = CurrencyStatus.Active,
                Type = CurrencyType.Fiat
            };

            configure?.Invoke(currency);
            return currency;
        }

        public Budget CreateBudget(Action<Budget>? configure = null)
        {
            var budget = new Budget
            {
                Id = Guid.NewGuid(),
                Name = $"Budget {_random.Next(1, 1000)}",
                OverallBalance = 0M,
                Status = BudgetStatus.Active,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };

            configure?.Invoke(budget);
            return budget;
        }

        public Wallet CreateWallet(Action<Wallet>? configure = null)
        {
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                Name = $"Wallet {_random.Next(1, 1000)}",
                Balance = 0M,
                IsPartOfGeneralBalance = true,
                Type = WalletType.Cash,
                Status = WalletStatus.Active,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };

            configure?.Invoke(wallet);
            return wallet;
        }

        public Transaction CreateTransaction(Action<Transaction>? configure = null)
        {
            var tx = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = Math.Round((decimal)(_random.NextDouble() * 1000), 2),
                Description = $"Tx {_random.Next(1, 1000)}",
                TransactionDate = DateTimeOffset.UtcNow,
                Type = TransactionType.Income,
                CreatedDate = DateTimeOffset.UtcNow
            };

            configure?.Invoke(tx);
            return tx;
        }

        public TransferTransaction CreateTransfer(Action<TransferTransaction>? configure = null)
        {
            var tx = new TransferTransaction
            {
                Id = Guid.NewGuid(),
                Amount = Math.Round((decimal)(_random.NextDouble() * 1000), 2),
                Description = $"Transfer {_random.Next(1, 1000)}",
                TransactionDate = DateTimeOffset.UtcNow,
                CreatedDate = DateTimeOffset.UtcNow
            };

            configure?.Invoke(tx);
            return tx;
        }

        // Budget-focused scenarios
        public (Currency currency, Budget budget, Wallet wallet) CreateSingleBudgetScenario(Action<Budget>? configure = null)
        {
            var currency = CreateCurrency();
            var budget = CreateBudget(b => b.CurrencyId = currency.Id);
            var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
            configure?.Invoke(budget);
            return (currency, budget, wallet);
        }
        
        public (Currency currency, List<Budget> budgets, List<Wallet> wallets) CreateMultiBudgetsScenario(int budgetCount = 2, int walletCount = 2)
        {
            var currency = CreateCurrency();
            var budgets = Enumerable.Range(0, budgetCount).Select(i =>
                CreateBudget(b =>
                {
                    b.Name = $"Budget {i + 1}";
                    b.CurrencyId = currency.Id;
                    b.Wallets = Enumerable.Range(0, walletCount).Select(j => CreateWallet(w =>
                    {
                        w.Name = $"Wallet {i + j + 1}";
                        w.CurrencyId = currency.Id;
                    })).ToList();
                })
            ).ToList();

            var wallets = budgets.SelectMany(b => b.Wallets).ToList();
            return (currency, budgets, wallets);
        }
        
        public (Currency currency, Budget budget, Wallet sourceWallet, Wallet targetWallet, TransferTransaction transfer) CreateBudgetWithTransferScenario()
        {
            var currency = CreateCurrency();
            var budget = CreateBudget(b => b.CurrencyId = currency.Id);
            var sourceWallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; w.Name = "Source"; w.Balance = 1000M; });
            var targetWallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; w.Name = "Target"; w.Balance = 0M; });
            var transfer = CreateTransfer(t => { t.SourceWalletId = sourceWallet.Id; t.TargetWalletId = targetWallet.Id; t.Amount = 500M; });
            return (currency, budget, sourceWallet, targetWallet, transfer);
        }

        // Category scenarios
        public (Category parent, List<Category> children) CreateCategoryHierarchyScenario(int childrenCount = 2)
        {
            var parent = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Parent Category",
                IconName = "parent",
                Type = CategoryType.Expense,
                Status = CategoryStatus.Active,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };

            var children = Enumerable.Range(0, childrenCount).Select(i => new Category
            {
                Id = Guid.NewGuid(),
                Name = $"Child {i + 1}",
                IconName = "child",
                Type = CategoryType.Expense,
                Status = CategoryStatus.Active,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow,
                ParentCategoryId = parent.Id
            }).ToList();

            return (parent, children);
        }

        // Goal scenarios
        public (Currency currency, Budget budget, Category category, Wallet wallet, Goal goal, List<Transaction> applicable, List<Transaction> nonApplicable) CreateGoalWithTransactionsScenario()
        {
            var currency = CreateCurrency();
            var budget = CreateBudget(b => b.CurrencyId = currency.Id);
            var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Goal Category",
                IconName = "goal",
                Type = CategoryType.Expense,
                Status = CategoryStatus.Active,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                Name = "My Goal",
                PlannedMoneyAmount = 1000M,
                ActualMoneyAmount = 0M,
                StartDate = DateTimeOffset.UtcNow.AddDays(-10),
                EndDate = DateTimeOffset.UtcNow.AddDays(-5),
                Type = GoalType.Expense,
                Categories = [category],
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };

            var applicable = new List<Transaction>
            {
                CreateTransaction(t =>
                {
                    t.WalletId = wallet.Id;
                    t.CategoryId = category.Id;
                    t.Amount = 100M;
                    t.Type = TransactionType.Expense;
                    t.TransactionDate = DateTimeOffset.UtcNow.AddDays(-9);
                }),
                CreateTransaction(t =>
                {
                    t.WalletId = wallet.Id;
                    t.CategoryId = category.Id;
                    t.Amount = 50M;
                    t.Type = TransactionType.Expense;
                    t.TransactionDate = DateTimeOffset.UtcNow.AddDays(-6);
                })
            };
            var nonApplicable = new List<Transaction>
            {
                CreateTransaction(t =>
                {
                    t.WalletId = wallet.Id;
                    t.CategoryId = category.Id;
                    t.Amount = 25M;
                    t.Type = TransactionType.Expense;
                    t.TransactionDate = DateTimeOffset.UtcNow.AddDays(-4);
                }),
                CreateTransaction(t =>
                {
                    t.WalletId = wallet.Id;
                    t.CategoryId = category.Id;
                    t.Amount = 25M;
                    t.Type = TransactionType.Expense;
                    t.TransactionDate = DateTimeOffset.UtcNow.AddDays(-15);
                })
            };

            goal.ActualMoneyAmount += applicable.Sum(t => t.Amount);
            return (currency, budget, category, wallet, goal, applicable, nonApplicable);
        }

        // Transaction scenarios
        public (Currency currency, Budget budget, Wallet wallet, Category category, Transaction transaction) CreateTransactionScenario()
        {
            var currency = CreateCurrency();
            var budget = CreateBudget(b => b.CurrencyId = currency.Id);
            var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Food",
                IconName = "food",
                Type = CategoryType.Expense,
                Status = CategoryStatus.Active,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow
            };
            var tx = CreateTransaction(t => { t.WalletId = wallet.Id; t.CategoryId = category.Id; t.Amount = 42.5M; t.Type = TransactionType.Expense; });
            return (currency, budget, wallet, category, tx);
        }

        public (Currency currency, Budget budget, Wallet source, Wallet target, TransferTransaction transfer) CreateTransferScenario()
        {
            return CreateBudgetWithTransferScenario();
        }

        // Currency scenarios
        public List<Currency> CreateCurrenciesScenario(int count)
        {
            return Enumerable.Range(0, count).Select(i => CreateCurrency(c =>
            {
                c.Code = $"C{i + 1}";
                c.Name = $"Currency {i + 1}";
            })).ToList();
        }

        // Wallet scenarios (built on top of budget)
        public (Currency currency, Budget budget, Wallet wallet) CreateWalletScenario()
        {
            return CreateSingleBudgetScenario();
        }

        public (Currency currency, Budget budget, List<Wallet> wallets) CreateWalletsForBudgetScenario(int walletCount = 2)
        {
            var (currency, budgets, wallets) = CreateMultiBudgetsScenario(1, walletCount);
            return (currency, budgets[0], wallets);
        }

        public (Currency currency, Budget budget, List<Wallet> wallets, List<Transaction> transactions) CreateWalletsWithTransactionsScenario(int walletCount = 2, int transactionsPerWallet = 1)
        {
            var (currency, budget, wallets) = CreateWalletsForBudgetScenario(walletCount);
            var transactions = new List<Transaction>();
            foreach (var wallet in wallets)
            {
                for (int j = 0; j < transactionsPerWallet; j++)
                {
                    transactions.Add(CreateTransaction(t =>
                    {
                        t.WalletId = wallet.Id;
                        t.Amount = Math.Round((decimal)(_random.NextDouble() * 200 + 1), 2);
                        t.Type = j % 2 == 0 ? TransactionType.Income : TransactionType.Expense;
                    }));
                }
            }
            return (currency, budget, wallets, transactions);
        }
    }
}