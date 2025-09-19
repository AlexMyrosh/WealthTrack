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
            Type = _random.GetItems([OperationType.Income, OperationType.Expense], 1).First(),
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
            Amount = Math.Round((decimal)(_random.NextDouble() * 1000), 2),
            Description = $"Tx {_random.Next(1, 1000)}",
            TransactionDate = DateTimeOffset.UtcNow,
            Type = OperationType.Income,
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

    public List<Transaction> CreateManyTransactions(int numberOfTransactions = 1, Action<Transaction>? configure = null)
    {
        var transactions = Enumerable.Range(0, numberOfTransactions).Select(i =>
            CreateTransaction(t => t.Description = $"Transaction {i}")
        ).ToList();
        
        transactions.ForEach(t => configure?.Invoke(t));
        return transactions;
    }
    
    public List<TransferTransaction> CreateManyTransferTransactions(int numberOfTransactions = 1, Action<TransferTransaction>? configure = null)
    {
        var transactions = Enumerable.Range(0, numberOfTransactions).Select(i =>
            CreateTransfer(t => t.Description = $"Transfer {i}")
        ).ToList();
        
        transactions.ForEach(t => configure?.Invoke(t));
        return transactions;
    }
    
    public Goal CreateGoal(Action<Goal>? configure = null)
    {
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            Name = $"Goal {_random.Next(1, 1000)}",
            PlannedMoneyAmount = _random.Next(1, 1000),
            ActualMoneyAmount = 0,
            Type = OperationType.Income,
            StartDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = DateTimeOffset.UtcNow.AddDays(30),
            CreatedDate = DateTimeOffset.UtcNow,
            ModifiedDate = DateTimeOffset.UtcNow
        };

        configure?.Invoke(goal);
        return goal;
    }
    
    // Budget-focused scenarios
    
    public (Currency currency, Budget budget) CreateBudgetScenario(Action<Budget>? configure = null)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        configure?.Invoke(budget);
        return (currency, budget);
    }
    public (Currency currency, Budget budget, Wallet wallet) CreateSingleBudgetScenario(
        Action<Budget>? configureBudget = null, 
        Action<Wallet>? configureWallet = null,
        Action<Currency>? configureCurrency = null)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        configureBudget?.Invoke(budget);
        configureWallet?.Invoke(wallet);
        configureCurrency?.Invoke(currency);
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
            p.Type = _random.GetItems([OperationType.Income, OperationType.Expense], 1).First();
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
                p.Type = _random.GetItems([OperationType.Income, OperationType.Expense], 1).First();
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
                p.IsSystem = true;
                p.ChildCategories = Enumerable.Range(0, numberOfChildren).Select(_ => CreateCategory(c =>
                {
                    c.Name = $"Child Category {categoryIndex++}";
                    c.IsSystem = true;
                })).ToList();
            })
        ).ToList();
        
        var childCategories = categories.SelectMany(c => c.ChildCategories).ToList();
        return (categories, childCategories);
    }
    
    public (Category category, Goal goal) CreateSingleCategoryWithGoalScenario()
    {
        var goal = CreateGoal(g => g.Type = OperationType.Income);
        var category = CreateCategory(c =>
        {
            c.Goals = [goal];
            c.Type = OperationType.Income;
        });
        return (category, goal);
    }
    
    public (Category category, Transaction transaction, Wallet wallet, Budget buddget, Currency currency) CreateCategoryWithTransactionScenario()
    {
        var category = CreateCategory();
        var (currency, budget, wallet) = CreateSingleBudgetScenario();
        var transaction = CreateTransaction(t =>
        {
            t.CategoryId = category.Id;
            t.WalletId = wallet.Id;
        });
        return (category, transaction, wallet, budget, currency);
    }
    
    public (List<Category> categories, List<Transaction> transactions, Wallet wallet, Budget buddget, Currency currency, Goal goal) CreateCategoriesWithGoalAndTransactionsScenario()
    {
        var category1 = CreateCategory();
        var category2 = CreateCategory();
        var (currency, budget, wallet) = CreateSingleBudgetScenario();
        var transaction1 = CreateTransaction(t =>
        {
            t.CategoryId = category1.Id;
            t.WalletId = wallet.Id;
        });
        var transaction2 = CreateTransaction(t =>
        {
            t.CategoryId = category2.Id;
            t.WalletId = wallet.Id;
        });
        var goal = CreateGoal(g =>
        {
            g.Categories = [category1, category2];
            g.ActualMoneyAmount = transaction1.Amount + transaction2.Amount;
        });
        
        return ([category1, category2], [transaction1, transaction2], wallet, budget, currency, goal);
    }

    // Goal scenarios
    
    public (Currency currency, Budget budget, Wallet wallet, Category category, Goal goal) CreateWalletHierarchyWithCategoryAndGoalScenario()
    {
        var (currency, budget, wallet) = CreateSingleBudgetScenario();
        var category = CreateCategory();
        var goal = CreateGoal(g => g.Categories = [category]);
        return (currency, budget, wallet, category, goal);
    }
    
    public (List<Goal> goals, List<Category> categories) CreateMultiGoalsScenario(int numberOfGoals)
    {
        var numberOfCategories = 2;
        var categoryIndex = 1;
        var goals = Enumerable.Range(0, numberOfGoals).Select(i =>
            CreateGoal(g =>
            {
                g.Name = $"Goal {i + 1}";
                g.Categories = Enumerable.Range(0, numberOfCategories).Select(_ => CreateCategory(c =>
                {
                    c.Name = $"Category {categoryIndex++}";
                })).ToList();
            })
                
        ).ToList();

        var categories = goals.SelectMany(b => b.Categories).ToList();
        return (goals, categories);
    }
    
    public (Goal goal, List<Category> categories) CreateSingleGoalsWithMultipleCategoriesScenario(int numberOfCategories)
    {
        var categories = Enumerable.Range(0, numberOfCategories).Select(i => CreateCategory(c =>
        {
            c.Name = $"Category {i}";
        })).ToList();

        var goal = CreateGoal(g =>
        {
            g.Categories = categories;
        });
        
        return (goal, categories);
    }
    
    // Transaction scenarios

    public (List<Transaction> transactions, List<TransferTransaction> transferTransactions, List<Wallet> wallets, Budget budget, Currency currency, Category category) CreateMixOfTransactionsScenario(int numberOfTransactions, int numberOfTransfers)
    {
        var (currency, budget, wallet) = CreateSingleBudgetScenario();
        var category = CreateCategory();
        var sourceWallet = CreateWallet(w =>
        {
            w.BudgetId = budget.Id;
            w.CurrencyId = currency.Id;
        });
        var targetWallet = CreateWallet(w =>
        {
            w.BudgetId = budget.Id;
            w.CurrencyId = currency.Id;
        });
        var transactions = CreateManyTransactions(numberOfTransactions, t =>
        {
            t.CategoryId = category.Id;
            t.WalletId = wallet.Id;
        });
        var transferTransactions = CreateManyTransferTransactions(numberOfTransfers, t =>
        {
            t.SourceWalletId = sourceWallet.Id;
            t.TargetWalletId = targetWallet.Id;
        });
        
        return (transactions, transferTransactions, [wallet, sourceWallet, targetWallet], budget, currency, category);
    }
    
    public (List<Transaction> transactions, Wallet wallet, Budget budget, Currency currency, Category category) CreateManyTransactionsScenario(int numberOfTransactions)
    {
        var (currency, budget, wallet) = CreateSingleBudgetScenario();
        var category = CreateCategory();
        var transactions = CreateManyTransactions(numberOfTransactions, t =>
        {
            t.CategoryId = category.Id;
            t.WalletId = wallet.Id;
        });
        
        return (transactions, wallet, budget, currency, category);
    }
    
    public (Currency currency, Budget budget, Category category, Wallet wallet, Goal goal, List<Transaction> transactions) CreateTransactionsHierarchyWithGoalScenario()
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var category = CreateCategory(c => c.Type = OperationType.Expense);
        var applicableTransactions = new List<Transaction>
        {
            CreateTransaction(t =>
            {
                t.WalletId = wallet.Id;
                t.CategoryId = category.Id;
                t.Type = OperationType.Expense;
                t.TransactionDate = DateTimeOffset.UtcNow;
            }),
            CreateTransaction(t =>
            {
                t.WalletId = wallet.Id;
                t.CategoryId = category.Id;
                t.Type = OperationType.Expense;
                t.TransactionDate = DateTimeOffset.UtcNow;
            })
        };
        var goal = CreateGoal(g =>
        {
            g.StartDate = DateTimeOffset.UtcNow.AddDays(-10);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(10);
            g.Type = OperationType.Expense;
            g.Categories = [category];
            g.ActualMoneyAmount = applicableTransactions.Sum(t => t.Amount);
        });
        
        return (currency, budget, category, wallet, goal, applicableTransactions);
    }
    
    public (Currency currency, Budget budget, Wallet wallet, Category category, Transaction transaction) CreateSingleTransactionScenario(
        Action<Budget>? configureBudget = null, 
        Action<Wallet>? configureWallet = null,
        Action<Currency>? configureCurrency = null,
        Action<Category>? configureCategory = null,
        Action<Transaction>? configureTransaction = null)
    {
        var (transactions, wallet, budget, currency, category) = CreateManyTransactionsScenario(1);
        configureBudget?.Invoke(budget);
        configureWallet?.Invoke(wallet);
        configureCurrency?.Invoke(currency);
        configureCategory?.Invoke(category);
        configureTransaction?.Invoke(transactions.First());
        
        return (currency, budget, wallet, category, transactions.First());
    }

    public (Currency currency, Budget budget, List<Wallet> wallets, TransferTransaction transaction) CreateSingleTransferScenario(
        Action<Budget>? configureBudget = null, 
        Action<Wallet>? configureWallet = null,
        Action<Currency>? configureCurrency = null,
        Action<TransferTransaction>? configureTransaction = null)
    {
        var (currency, budget, wallet) = CreateSingleBudgetScenario();
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
        
        configureBudget?.Invoke(budget);
        configureWallet?.Invoke(wallet);
        configureWallet?.Invoke(targetWallet);
        configureCurrency?.Invoke(currency);
        configureTransaction?.Invoke(transaction);
        
        return (currency, budget, [wallet, targetWallet], transaction);
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

    // Wallet scenarios
    
    public (Currency currency, Budget budget, List<Wallet> wallets, List<Transaction> transactions, List<TransferTransaction> transferTransactions) CreatePairOfWalletHierarchyWithTransactionsScenario()
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet1 = CreateWallet(w =>
        {
            w.Name = "Wallet 1";
            w.CurrencyId = currency.Id;
            w.BudgetId = budget.Id;
            w.Transactions = [CreateTransaction()];
        });
        var wallet2 = CreateWallet(w =>
        {
            w.Name = "Wallet 2";
            w.CurrencyId = currency.Id;
            w.BudgetId = budget.Id;
            w.Transactions = [CreateTransaction()];
        });
        var transferTransaction1 = CreateTransfer(t =>
        {
            t.SourceWalletId = wallet1.Id;
            t.TargetWalletId = wallet2.Id;
        });
        var transferTransaction2 = CreateTransfer(t =>
        {
            t.SourceWalletId = wallet2.Id;
            t.TargetWalletId = wallet1.Id;
        });
        
        List<Transaction> transactions = [wallet1.Transactions.First(), wallet2.Transactions.First()];
        return (currency, budget, [wallet1, wallet2], transactions, [transferTransaction1, transferTransaction2]);
    }
    
    public (Currency currency, Budget budget, Wallet wallet) CreateWalletScenario(Action<Wallet>? configure = null)
    {
        var (currency, budget, wallet) = CreateSingleBudgetScenario();
        configure?.Invoke(wallet);
        return (currency, budget, wallet);
    }

    public (Currency currency, Budget budget, Wallet wallet1, Wallet wallet2) CreateTwoWalletsScenario()
    {
        var (currency, budget, wallets) = CreateSingleBudgetWithMultipleWalletsScenario(2);
        return (currency, budget, wallets[0], wallets[1]);
    }
}