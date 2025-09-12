using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.TestData;

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
    
    public Category CreateCategory(Action<Category>? configure = null)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = $"Category {_random.Next(1, 1000)}",
            IconName = Guid.NewGuid().ToString(),
            Type = _random.GetItems([CategoryType.Income, CategoryType.Expense], 1).First(),
            CreatedDate = DateTimeOffset.UtcNow,
            ModifiedDate = DateTimeOffset.UtcNow,
            Status = CategoryStatus.Active
        };

        configure?.Invoke(category);
        return category;
    }

    public Transaction CreateTransaction(List<Action<Transaction>>? configure = null)
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

        if (configure != null)
        {
            configure.ForEach(c => c.Invoke(tx));
        }

        return tx;
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
    
    public Goal CreateGoal(List<Action<Goal>>? configure = null)
    {
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            Name = $"Goal {_random.Next(1, 1000)}",
            PlannedMoneyAmount = _random.Next(1, 1000),
            ActualMoneyAmount = 0,
            Type = GoalType.Income,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CreatedDate = DateTimeOffset.UtcNow,
            ModifiedDate = DateTimeOffset.UtcNow
        };

        if (configure != null)
        {
            configure.ForEach(c => c.Invoke(goal));
        }
        
        return goal;
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
    
    public (Currency currency, Budget budget, List<Wallet> wallets) CreateSingleBudgetWithMultipleWalletsScenario(int walletCount)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallets = Enumerable.Range(0, walletCount).Select(i => CreateWallet(w =>
        {
            w.Name = $"Wallet {i + 1}";
            w.CurrencyId = currency.Id;
            w.BudgetId = budget.Id;
        })).ToList();
        return (currency, budget, wallets);
    }
        
    public (Currency currency, List<Budget> budgets, List<Wallet> wallets, int numberOfWallets) CreateMultiBudgetsScenario(int budgetCount)
    {
        var currency = CreateCurrency();
        var walletIndex = 1;
        var numberOfWallets = 2;
        var budgets = Enumerable.Range(0, budgetCount).Select(i =>
            CreateBudget(b =>
            {
                b.Name = $"Budget {i + 1}";
                b.CurrencyId = currency.Id;
                b.Wallets = Enumerable.Range(0, numberOfWallets).Select(_ => CreateWallet(w =>
                {
                    w.Name = $"Wallet {walletIndex++}";
                    w.CurrencyId = currency.Id;
                })).ToList();
            })
        ).ToList();

        var wallets = budgets.SelectMany(b => b.Wallets).ToList();
        return (currency, budgets, wallets, numberOfWallets);
    }
        
    public (Currency currency, Budget budget, Wallet sourceWallet, Wallet targetWallet, TransferTransaction transfer, Transaction transaction) CreateBudgetWithAllRelatedEntitiesScenario()
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var sourceWallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; w.Name = "Source"; w.Balance = 1000M; });
        var targetWallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; w.Name = "Target"; w.Balance = 0M; });
        var transfer = CreateTransfer(t => { t.SourceWalletId = sourceWallet.Id; t.TargetWalletId = targetWallet.Id; t.Amount = 500M; });
        var transaction = CreateTransaction(t => t.WalletId = sourceWallet.Id);
        return (currency, budget, sourceWallet, targetWallet, transfer, transaction);
    }

    // Category scenarios
    public (Category parent, List<Category> children) CreateSingleCategoryHierarchyScenario(int numberOfChildren)
    {
        var parent = CreateCategory(p =>
        {
            p.Type = _random.GetItems([CategoryType.Income, CategoryType.Expense], 1).First();
            p.ChildCategories = Enumerable.Range(0, numberOfChildren).Select(i => CreateCategory(c =>
            {
                c.Name = $"Child Category {i + 1}";
                c.Type = p.Type;
            })).ToList();
        });
        
        return (parent, parent.ChildCategories);
    }
    
    public (Category parent, Category child) CreateCategoryHierarchyWithSingleChildScenario()
    {
        var result = CreateSingleCategoryHierarchyScenario(1);
        return (result.parent, result.children.First());
    }
    
    public (List<Category> parents, List<Category> children) CreateCategoryHierarchyScenario(int numberOfParent, int numberOfChildren)
    {
        var categoryIndex = 1;
        var categories = Enumerable.Range(0, numberOfParent).Select(i =>
            CreateCategory(p =>
            {
                p.Name = $"Parent Category {i + 1}";
                p.Type = _random.GetItems([CategoryType.Income, CategoryType.Expense], 1).First();
                p.ChildCategories = Enumerable.Range(0, numberOfChildren).Select(_ => CreateCategory(c =>
                {
                    c.Name = $"Child Category {categoryIndex++}";
                    c.Type = p.Type;
                    c.ParentCategoryId = p.Id;
                })).ToList();
            })
        ).ToList();
        
        var childCategories = categories.SelectMany(c => c.ChildCategories).ToList();
        return (categories, childCategories);
    }
    
    public (Category firstParent, Category secondCategory, List<Category> firstChildren, List<Category> secondChildren) CreateTwoPairsOfCategoryHierarchyScenario(int numberOfChildren)
    {
        var result = CreateCategoryHierarchyScenario(2, numberOfChildren);
        
        var firstCategoryChildren = result.children.Where(c => c.ParentCategoryId == result.parents[0].Id).ToList();
        var secondCategoryChildren = result.children.Where(c => c.ParentCategoryId == result.parents[1].Id).ToList();
        
        return (result.parents[0], result.parents[1], firstCategoryChildren, secondCategoryChildren);
    }
    
    public (List<Category> parents, List<Category> children) CreateSystemCategoryHierarchyScenario(int numberOfParent, int numberOfChildren)
    {
        var categoryIndex = 1;
        var categories = Enumerable.Range(0, numberOfParent).Select(i =>
            CreateCategory(p =>
            {
                p.Name = $"Parent Category {i + 1}";
                p.Type = CategoryType.System;
                p.ChildCategories = Enumerable.Range(0, numberOfChildren).Select(_ => CreateCategory(c =>
                {
                    c.Name = $"Child Category {categoryIndex++}";
                    c.Type = p.Type;
                })).ToList();
            })
        ).ToList();
        
        var childCategories = categories.SelectMany(c => c.ChildCategories).ToList();
        return (categories, childCategories);
    }
    
    public (Category category, Goal goal) CreateSingleCategoryWithGoalScenario()
    {
        var goal = CreateGoal();
        var category = CreateCategory(c => c.Goals = [goal]);
        return (category, goal);
    }
    
    public (Category category, Transaction transaction, Wallet wallet, Budget buddget, Currency currency) CreateCategoryWithTransactionScenario()
    {
        var category = CreateCategory();
        var (currency, budget, wallet) = CreateSingleBudgetScenario();
        var transaction = CreateTransaction([t => t.CategoryId = category.Id, t => t.WalletId = wallet.Id]);
        return (category, transaction, wallet, budget, currency);
    }
    
    public (List<Category> categories, List<Transaction> transactions, Wallet wallet, Budget buddget, Currency currency, Goal goal) CreateCategoriesWithGoalAndTransactionsScenario()
    {
        var category1 = CreateCategory();
        var category2 = CreateCategory();
        var (currency, budget, wallet) = CreateSingleBudgetScenario();
        var transaction1 = CreateTransaction([t => t.CategoryId = category1.Id, t => t.WalletId = wallet.Id]);
        var transaction2 = CreateTransaction([t => t.CategoryId = category2.Id, t => t.WalletId = wallet.Id]);
        var goal = CreateGoal([g => g.Categories = [category1, category2], g => g.ActualMoneyAmount = transaction1.Amount + transaction2.Amount]);
        return ([category1, category2], [transaction1, transaction2], wallet, budget, currency, goal);
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

    public (Currency currency, Budget budget, Wallet source, Wallet target, TransferTransaction transfer, Transaction transaction) CreateTransferScenario()
    {
        return CreateBudgetWithAllRelatedEntitiesScenario();
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
        var (currency, budgets, wallets, numberOfWallets) = CreateMultiBudgetsScenario(1);
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