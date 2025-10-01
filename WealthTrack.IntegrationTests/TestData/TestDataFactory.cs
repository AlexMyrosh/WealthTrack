using FluentAssertions;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.TestData;

public class TestDataFactory
{
    private readonly Random _random = new();
    
    public Currency CreateCurrency(Action<Currency>? configure = null)
    {
        var currency = new Currency
        {
            Id = Guid.NewGuid(),
            Code = $"C{_random.Next(1, 1000)}",
            Name = $"Currency {_random.Next(1, 1000)}",
            Symbol = $"S{_random.Next(1, 1000)}",
            ExchangeRate = (decimal)(_random.NextDouble() * 100 + _random.NextDouble()),
            Status = CurrencyStatus.Active,
            Type = _random.GetItems([CurrencyType.Fiat, CurrencyType.Crypto], 1).First()
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
            OverallBalance = 0,
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
            Balance = _random.Next(1, 1000),
            IsPartOfGeneralBalance = _random.GetItems([true, false], 1).First(),
            Type = _random.GetItems([WalletType.Cash, WalletType.DebitCard], 1).First(),
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
            IconName = $"Category icon {_random.Next(1, 1000)}",
            Type = _random.GetItems([OperationType.Income, OperationType.Expense], 1).First(),
            CreatedDate = DateTimeOffset.UtcNow,
            ModifiedDate = DateTimeOffset.UtcNow,
            Status = CategoryStatus.Active
        };

        configure?.Invoke(category);
        return category;
    }
    
    public Category CreateSystemCategory(Action<Category>? configure = null)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = $"Category {_random.Next(1, 1000)}",
            IconName = $"Category icon {_random.Next(1, 1000)}",
            IsSystem = true,
            CreatedDate = DateTimeOffset.UtcNow,
            ModifiedDate = DateTimeOffset.UtcNow,
            Status = CategoryStatus.Active
        };

        configure?.Invoke(category);
        return category;
    }
    
    public Transaction CreateTransaction(Action<Transaction>? configure = null)
    {
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            Amount = _random.Next(1, 1000),
            Description = $"Transaction Description {_random.Next(1, 1000)}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = _random.GetItems([OperationType.Income, OperationType.Expense], 1).First(),
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
            Amount = _random.Next(1, 1000),
            Description = $"Transfer description {_random.Next(1, 1000)}",
            TransactionDate = DateTimeOffset.UtcNow,
            CreatedDate = DateTimeOffset.UtcNow
        };

        configure?.Invoke(tx);
        return tx;
    }
    
    public Goal CreateGoal(Action<Goal>? configure = null)
    {
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            Name = $"Goal {_random.Next(1, 1000)}",
            PlannedMoneyAmount = _random.Next(1, 1000),
            Type = _random.GetItems([OperationType.Income, OperationType.Expense], 1).First(),
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CreatedDate = DateTimeOffset.UtcNow,
            ModifiedDate = DateTimeOffset.UtcNow
        };

        configure?.Invoke(goal);
        return goal;
    }
    
    public List<Goal> CreateManyGoals(int numberOfGoals = 2, Action<Goal>? configure = null)
    {
        var goals = Enumerable.Range(0, numberOfGoals).Select(i =>
            CreateGoal(t => t.Name = $"Goal {i + 1}")
        ).ToList();
        
        goals.ForEach(g => configure?.Invoke(g));
        return goals;
    }

    public List<Transaction> CreateManyTransactions(int numberOfTransactions = 2, Action<Transaction>? configure = null)
    {
        var transactions = Enumerable.Range(0, numberOfTransactions).Select(i =>
            CreateTransaction(t => t.Description = $"Transaction {i + 1}")
        ).ToList();
        
        transactions.ForEach(t => configure?.Invoke(t));
        return transactions;
    }

    public List<TransferTransaction> CreateManyTransfers(int numberOfTransactions = 2, Action<TransferTransaction>? configure = null)
    {
        var transfers = Enumerable.Range(0, numberOfTransactions).Select(i =>
            CreateTransfer(t => t.Description = $"Transfer {i + 1}")
        ).ToList();

        transfers.ForEach(t => configure?.Invoke(t));
        return transfers;
    }

    public List<Currency> CreateManyCurrencies(int numberOfCurrencies = 2, Action<Currency>? configure = null)
    {
        var currencies = Enumerable.Range(0, numberOfCurrencies).Select(i =>
            CreateCurrency(c => { c.Name = $"Currency {i + 1}"; })
        ).ToList();

        currencies.ForEach(t => configure?.Invoke(t));
        return currencies;
    }
    
    public (Currency currency, List<Budget> budgets, List<Wallet> wallets) CreateManyBudgetsWithDependencies(int numberOfBudgets = 2)
    {
        var currency = CreateCurrency();
        var budgets = Enumerable.Range(0, numberOfBudgets).Select(i =>
            CreateBudget(b =>
            {
                b.Name = $"Budget {i + 1}";
                b.CurrencyId = currency.Id;
                b.Wallets =
                [
                    CreateWallet(w =>
                    {
                        w.Name = $"Wallet {i + 1}";
                        w.CurrencyId = currency.Id;
                    })
                ];
            })
        ).ToList();
    
        var wallets = budgets.SelectMany(b => b.Wallets).ToList();
        return (currency, budgets, wallets);
    }
    
    public (List<Category> parents, List<Category> children) CreateManyNotSystemCategoryHierarchies(int numberOfParent = 2, int numberOfChildren = 2)
    {
        var categoryIndex = 1;
        var categories = Enumerable.Range(0, numberOfParent).Select(i =>
            CreateCategory(p =>
            {
                p.Name = $"Parent Category {i + 1}";
                p.Type = _random.GetItems([OperationType.Income, OperationType.Expense], 1).First();
                p.IsSystem = false;
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
    
    public (List<Category> regularParents, List<Category> regularChildren, List<Category> systemParents, List<Category> systemChildren) CreateManyCategoryHierarchiesIncludingSystemType(int numberOfParent = 2, int numberOfChildren = 2)
    {
        var regularCategories = CreateManyNotSystemCategoryHierarchies(numberOfParent, numberOfChildren);
        var categoryIndex = 1;
        var systemParentCategories = Enumerable.Range(0, numberOfParent).Select(i =>
            CreateCategory(p =>
            {
                p.Name = $"Parent System Category {i + 1}";
                p.IsSystem = true;
                p.ChildCategories = Enumerable.Range(0, numberOfChildren).Select(_ => CreateCategory(c =>
                {
                    c.Name = $"Child System Category {categoryIndex++}";
                    c.IsSystem = true;
                })).ToList();
            })
        ).ToList();
        
        var systemChildCategories = systemParentCategories.SelectMany(c => c.ChildCategories).ToList();
        return (regularCategories.parents, regularCategories.children, systemParentCategories, systemChildCategories);
    }
    
    public (List<Goal> goals, List<Category> categories) CreateManyGoalsWithDependencies(int numberOfGoals = 2, int numberOfCategories = 2, Action<Goal>? configureGoal = null, Action<Category>? configureCategory = null)
    {
        var categoryIndex = 1;
        var goals = Enumerable.Range(0, numberOfGoals).Select(i =>
            CreateGoal(g =>
            {
                g.Name = $"Goal {i + 1}";
                g.Categories = Enumerable.Range(0, numberOfCategories).Select(_ => CreateCategory(c =>
                {
                    c.Name = $"Category {categoryIndex++}";
                    c.Type = g.Type;
                })).ToList();
            })
                
        ).ToList();

        var categories = goals.SelectMany(b => b.Categories).ToList();
        
        goals.ForEach(g => configureGoal?.Invoke(g));
        categories.ForEach(c => configureCategory?.Invoke(c));
        
        return (goals, categories);
    }
    
    public (Currency currency, Budget budget, List<Wallet> wallets) CreateManyWallets(int numberOfWallets = 2)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallets = Enumerable.Range(0, numberOfWallets).Select(i => CreateWallet(w =>
        {
            w.Name = $"Wallet {i + 1}";
            w.CurrencyId = currency.Id;
            w.BudgetId = budget.Id;
        })).ToList();
        
        return (currency, budget, wallets);
    }
    
    public (Currency currency, Category category, Budget budget, List<Wallet> wallets, List<Transaction> transactions, List<TransferTransaction> transferTransactions) CreateMixOfTransactionsScenario(int numberOfTransactions = 2, int numberOfTransfers = 2, Action<Wallet>? configureWallets = null)
    {
        (numberOfTransactions % 2).Should().Be(0);
        (numberOfTransfers % 2).Should().Be(0);
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var category = CreateCategory();
        var wallet1 = CreateWallet(w =>
        {
            w.BudgetId = budget.Id;
            w.CurrencyId = currency.Id;
            w.Transactions = CreateManyTransactions(numberOfTransactions / 2, t =>
            {
                t.CategoryId = category.Id;
            });
        });
        var wallet2 = CreateWallet(w =>
        {
            w.BudgetId = budget.Id;
            w.CurrencyId = currency.Id;
            w.Transactions = CreateManyTransactions(numberOfTransactions / 2, t =>
            {
                t.CategoryId = category.Id;
            });
        });
        var transferTransactions1 = CreateManyTransfers(numberOfTransfers / 2, t =>
        {
            t.SourceWalletId = wallet1.Id;
            t.TargetWalletId = wallet2.Id;
        });
        var transferTransactions2 = CreateManyTransfers(numberOfTransfers / 2, t =>
        {
            t.SourceWalletId = wallet2.Id;
            t.TargetWalletId = wallet1.Id;
        });
        
        var transactions = wallet1.Transactions.Concat(wallet2.Transactions).ToList();
        var transferTransactions = transferTransactions1.Concat(transferTransactions2).ToList();
        
        configureWallets?.Invoke(wallet1);
        configureWallets?.Invoke(wallet2);
        
        return (currency, category, budget, [wallet1, wallet2], transactions, transferTransactions);
    }
    
    public (Currency currency, Category category, Budget budget, List<Wallet> wallets, List<Transaction> transactions, List<TransferTransaction> transferTransactions) CreateWalletsWithTransactions(Action<Wallet>? configureWallets = null)
    {
        var result = CreateMixOfTransactionsScenario(configureWallets: configureWallets);
        return result;
    }
    
    public (Currency currency, Budget budget, List<Wallet> wallets, TransferTransaction transaction) CreateSingleTransferScenario(Action<Wallet>? configureWallet = null)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var targetWallet = CreateWallet(w =>
        {
            w.BudgetId = budget.Id;
            w.CurrencyId = currency.Id;
        });
        var transaction = CreateTransfer(t =>
        {
            t.SourceWalletId = wallet.Id;
            t.TargetWalletId = targetWallet.Id;
        });
        
        configureWallet?.Invoke(wallet);
        configureWallet?.Invoke(targetWallet);
        
        return (currency, budget, [wallet, targetWallet], transaction);
    }

    public (Currency currency, Budget budget, Wallet wallet) CreateSingleWalletWithDependencies(Action<Wallet>? configureWallet = null)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        
        configureWallet?.Invoke(wallet);
        
        return (currency, budget, wallet);
    }

    public (Currency currency, Budget budget) CreateBudgetWithDependencies()
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        
        return (currency, budget);
    }
    
    public (Currency currency, Budget budget, Wallet wallet, Category category, Transaction transaction) CreateSingleTransactionScenario(
        Action<Wallet>? configureWallet = null,
        Action<Category>? configureCategory = null,
        Action<Transaction>? configureTransaction = null)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var category = CreateCategory();
        var transaction = CreateTransaction(t =>
        {
            t.CategoryId = category.Id;
            t.WalletId = wallet.Id;
        });
        
        configureWallet?.Invoke(wallet);
        configureCategory?.Invoke(category);
        configureTransaction?.Invoke(transaction);
        
        return (currency, budget, wallet, category, transaction);
    }
    
    public (Currency currency, Budget budget, Wallet wallet, Category parentCategory, Category childCategory, Transaction transaction) CreateSingleTransactionWithChildCategoryScenario(
        Action<Wallet>? configureWallet = null,
        Action<Category>? configureCategory = null,
        Action<Transaction>? configureTransaction = null)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var (parentCategory, childCategory) = CreateCategoryHierarchyWithSingleChildScenario();
        var transaction = CreateTransaction(t =>
        {
            t.CategoryId = childCategory.Id;
            t.WalletId = wallet.Id;
        });
        
        configureWallet?.Invoke(wallet);
        configureCategory?.Invoke(parentCategory);
        configureCategory?.Invoke(childCategory);
        configureTransaction?.Invoke(transaction);
        
        return (currency, budget, wallet, parentCategory, childCategory, transaction);
    }
    
    public (Currency currency, Budget budget, Wallet wallet, Category category, List<Transaction> transactions) CreateManyTransactionsWithDependencies(
        int numberOfTransactions = 2,
        Action<Wallet>? configureWallet = null,
        Action<Category>? configureCategory = null,
        Action<Transaction>? configureTransactions = null
    )
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var category = CreateCategory();
        var transactions = CreateManyTransactions(numberOfTransactions, t =>
        {
            t.CategoryId = category.Id;
            t.WalletId = wallet.Id;
        });
        
        configureWallet?.Invoke(wallet);
        configureCategory?.Invoke(category);
        transactions.ForEach(t => configureTransactions?.Invoke(t));
        
        return (currency, budget, wallet, category, transactions);
    }
    
    public (Currency currency, Budget budget, Wallet wallet, Category category) CreateSingleWalletWithCategory(Action<Category>? configureCategory = null, Action<Wallet>? configureWallet = null)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var category = CreateCategory();
        
        configureCategory?.Invoke(category);
        configureWallet?.Invoke(wallet);
        
        return (currency, budget, wallet, category);
    }
    
    public (Currency currency, Budget budget, Wallet wallet1, Wallet wallet2) CreatePairOfWallets()
    {
        var (currency, budget, wallets) = CreateManyWallets(2);
        return (currency, budget, wallets[0], wallets[1]);
    }
    
    public (Currency currency, Budget budget1, Budget budget2, Wallet wallet1, Wallet wallet2) CreatePairOfWalletsForDifferentBudgets()
    {
        var (currency, budget1, wallets) = CreateManyWallets(2);
        var budget2 = CreateBudget(b => b.CurrencyId = currency.Id);
        wallets[1].BudgetId = budget2.Id;
        return (currency, budget1, budget2, wallets[0], wallets[1]);
    }
    
    public (Currency currency, Budget budget, Wallet wallet, Category category, Transaction transaction, Goal goal) CreateSingleTransactionWithApplicableGoal(
        Action<Wallet>? configureWallet = null,
        Action<Category>? configureCategory = null,
        Action<Transaction>? configureTransaction = null,
        Action<Goal>? configureGoal = null)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var category = CreateCategory();
        var transaction = CreateTransaction(t =>
        {
            t.CategoryId = category.Id;
            t.WalletId = wallet.Id;
            t.Type = category.Type!.Value;
        });
        var goal = CreateGoal(g =>
        {
            g.Type = transaction.Type;
            g.StartDate =  transaction.TransactionDate.AddDays(-30);
            g.EndDate = transaction.TransactionDate.AddDays(30);
            g.Categories = [category];
        });
        
        configureWallet?.Invoke(wallet);
        configureCategory?.Invoke(category);
        configureTransaction?.Invoke(transaction);
        configureGoal?.Invoke(goal);
        
        return (currency, budget, wallet, category, transaction, goal);
    }
    
    public (Currency currency, Budget budget, Wallet wallet, Category category, List<Transaction> transactions, Goal goal) CreateSingleGoalWithManyTransactions(int numberOfTransactions = 2)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var category = CreateCategory();
        var goal = CreateGoal(g =>
        {
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [category];
        });
        var transactions = CreateManyTransactions(numberOfTransactions, t =>
        {
            t.CategoryId = category.Id;
            t.CreatedDate = goal.StartDate.AddDays(_random.Next((goal.EndDate - goal.StartDate).Days + 1));
            t.WalletId = wallet.Id;
        });
        
        return (currency, budget, wallet, category, transactions, goal);
    }
    
    public (Currency currency, Budget budget, Wallet wallet, Category category, List<Transaction> transactions, List<Goal> goals) CreateManyGoalsWithManyApplicableTransactions(int numberOfGoals = 2, int numberOfTransactions = 2)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var category = CreateCategory();
        var goals = CreateManyGoals(numberOfGoals, g =>
        {
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [category];
        });
        var transactions = Enumerable.Range(0, goals.Count).Select(i =>
            CreateManyTransactions(numberOfTransactions, t =>
            {
                t.CategoryId = category.Id;
                t.CreatedDate = goals[i].StartDate.AddDays(_random.Next((goals[i].EndDate - goals[i].StartDate).Days + 1));
                t.WalletId = wallet.Id;
            })
        ).SelectMany(t => t).ToList();
        
        return (currency, budget, wallet, category, transactions, goals);
    }
    
    public (Currency currency, Budget budget, Wallet wallet, Category parentCategory, Category childCagegory, Transaction transaction, Goal goal) CreateSingleTransactionWithChildCategoryWithApplicableGoal(
        Action<Wallet>? configureWallet = null,
        Action<Category>? configureCategory = null,
        Action<Transaction>? configureTransaction = null,
        Action<Goal>? configureGoal = null)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var (parentCategory, childCategory) = CreateCategoryHierarchyWithSingleChildScenario();
        
        var transaction = CreateTransaction(t =>
        {
            t.CategoryId = childCategory.Id;
            t.WalletId = wallet.Id;
        });
        var goal = CreateGoal(g =>
        {
            g.Type = transaction.Type;
            g.StartDate =  transaction.TransactionDate.AddDays(-30);
            g.EndDate = transaction.TransactionDate.AddDays(30);
            g.Categories = [childCategory];
        });
        
        configureWallet?.Invoke(wallet);
        configureCategory?.Invoke(childCategory);
        configureCategory?.Invoke(parentCategory);
        configureTransaction?.Invoke(transaction);
        configureGoal?.Invoke(goal);
        
        return (currency, budget, wallet, parentCategory, childCategory, transaction, goal);
    }
    
    public (Category parent, Category child) CreateCategoryHierarchyWithSingleChildScenario()
    {
        var parentCategory = CreateCategory(p =>
        {
            p.Name = "Parent Category";
        });

        var childCategory = CreateCategory(c =>
        {
            c.Name = "Child Category";
            c.ParentCategoryId = parentCategory.Id;
            c.Type = parentCategory.Type;
        });

        return (parentCategory, childCategory);
    }

    public List<Category> CreateCategoriesChain(int numberOfLayers)
    {
        var categories = new List<Category>();
        var root = CreateCategory(c => c.Name = "Root");
        categories.Add(root);
        for (var i = 1; i < numberOfLayers; i++)
        {
            var parent = categories.Last();
            var category = CreateCategory(c =>
            {
                c.Name = $"Layer {i + 1}";
                c.ParentCategoryId = parent.Id;
            });

            categories.Add(category);
        }
        
        return categories;
    }
}