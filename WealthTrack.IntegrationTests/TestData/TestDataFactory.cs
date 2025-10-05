using FluentAssertions;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;
using WealthTrack.Shared.Extensions;

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
            Status = EntityStatus.Active,
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
            Status = EntityStatus.Active,
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
            ModifiedDate = DateTimeOffset.UtcNow
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
            ModifiedDate = DateTimeOffset.UtcNow
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
            Type = _random.GetItems([TransactionType.Income, TransactionType.Expense], 1).First(),
            CreatedDate = DateTimeOffset.UtcNow
        };

        configure?.Invoke(tx);
        return tx;
    }

    public Transaction CreateTransfer(Action<Transaction>? configure = null)
    {
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            Amount = _random.Next(1, 1000),
            Description = $"Transfer description {_random.Next(1, 1000)}",
            Type = TransactionType.Transfer,
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

    public List<Transaction> CreateManyTransfers(int numberOfTransactions = 2, Action<Transaction>? configure = null)
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
    
    public (Currency currency, List<Budget> activeBudgets, List<Budget> archivedBudgets) CreateMixOfActiveAndArchivedBudgets(int numberOfActiveBudgets = 2, int numberOfArchivedBudgets = 2)
    {
        var currency = CreateCurrency();
        var activeBudgets = Enumerable.Range(0, numberOfActiveBudgets).Select(i =>
            CreateBudget(b =>
            {
                b.Name = $"Active Budget {i + 1}";
                b.CurrencyId = currency.Id;
                b.Status = EntityStatus.Active;
            })
        ).ToList();
        var archivedBudgets = Enumerable.Range(0, numberOfArchivedBudgets).Select(i =>
            CreateBudget(b =>
            {
                b.Name = $"Archived Budget {i + 1}";
                b.CurrencyId = currency.Id;
                b.Status = EntityStatus.Archived;
            })
        ).ToList();
        
        
        return (currency, activeBudgets, archivedBudgets);
    }
    
    public (Currency currency, Budget budget, List<Wallet> activeWallets, List<Wallet> archivedWallets) CreateMixOfActiveAndArchivedWallets(int numberOfActiveWallets = 2, int numberOfArchivedWallets = 2)
    {
        var (currency, budget) = CreateBudgetWithDependencies();
        var activeWallets = Enumerable.Range(0, numberOfActiveWallets).Select(i =>
            CreateWallet(w =>
            {
                w.Name = $"Active Wallet {i + 1}";
                w.CurrencyId = currency.Id;
                w.Status = EntityStatus.Active;
                w.BudgetId = budget.Id;
            })
        ).ToList();
        var archivedWallets = Enumerable.Range(0, numberOfArchivedWallets).Select(i =>
            CreateWallet(w =>
            {
                w.Name = $"Archived Budget {i + 1}";
                w.CurrencyId = currency.Id;
                w.Status = EntityStatus.Archived;
                w.BudgetId = budget.Id;
            })
        ).ToList();
        
        
        return (currency, budget, activeWallets, archivedWallets);
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
    
    public (Currency currency, Budget budget, List<Wallet> wallets) CreateManyWallets(int numberOfWallets = 2, Action<Wallet>? configureWallets = null)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallets = Enumerable.Range(0, numberOfWallets).Select(i => CreateWallet(w =>
        {
            w.Name = $"Wallet {i + 1}";
            w.CurrencyId = currency.Id;
            w.BudgetId = budget.Id;
        })).ToList();
        
        wallets.ForEach(w => configureWallets?.Invoke(w));
        
        return (currency, budget, wallets);
    }
    
    public (Currency currency, Category category, Budget budget, List<Wallet> wallets, List<Transaction> transactions, List<Transaction> transferTransactions) CreateMixOfTransactionsScenario(int numberOfTransactions = 2, int numberOfTransfers = 2, Action<Wallet>? configureWallets = null, Action<Transaction>? configureTransaction = null, Action<Transaction>? configureTransferTransaction = null)
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
        transactions.ForEach(t => configureTransaction?.Invoke(t));
        transferTransactions.ForEach(t => configureTransferTransaction?.Invoke(t));
        
        return (currency, category, budget, [wallet1, wallet2], transactions, transferTransactions);
    }
    
    public (Currency currency, Category category, Budget budget, List<Wallet> wallets, List<Transaction> transactions, List<Transaction> transferTransactions) CreateWalletsWithTransactions(
        Action<Wallet>? configureWallets = null,
        Action<Transaction>? configureTransaction = null,
        Action<Transaction>? configureTransferTransaction = null)
    {
        var result = CreateMixOfTransactionsScenario(
            configureWallets: configureWallets, 
            configureTransaction: configureTransaction, 
            configureTransferTransaction: configureTransferTransaction);
        return result;
    }
    
    public (Currency currency, Budget budget, List<Wallet> wallets, Transaction transaction) CreateSingleTransferScenario(Action<Wallet>? configureWallet = null, Action<Transaction>? configureTransferTransaction = null)
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
        configureTransferTransaction?.Invoke(transaction);
        
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

    public (Currency currency, Budget budget) CreateBudgetWithDependencies(Action<Budget>? configureBudget = null)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.Currency = currency);
        
        configureBudget?.Invoke(budget);
        
        return (currency, budget);
    }
    
    public (List<Currency> currencies, List<Budget> budgets, List<Wallet> wallets) CreateManyBudgetsWithManyWalletsWithDifferentCurrencies(int numberOfBudgets = 2, int numberOfWallets = 2, Action<Wallet>? configureWallet = null)
    {
        var budgetsWithCurrencies = Enumerable.Range(0, numberOfBudgets).Select(_ => CreateBudgetWithDependencies(b =>
        {
            b.Wallets = Enumerable.Range(0, numberOfWallets).Select(i => CreateWallet(w =>
            {
                w.Currency = CreateCurrency();
            })).ToList();
        })).ToList();

        var budgets = budgetsWithCurrencies.Select(x => x.budget).ToList();
        var wallets = budgets.SelectMany(b => b.Wallets).ToList();
        var currencies = budgets.Select(b => b.Currency).ToList();
        currencies.AddRange(wallets.Select(b => b.Currency));
        
        wallets.ForEach(w => configureWallet?.Invoke(w));
        return (currencies, budgets, wallets);
    }
    
    public (List<Currency> currencies, List<Budget> budgets, List<Wallet> firstSetOfWallets, List<Wallet> secondSetOfWallets) CreateManyBudgetsWithTwoSetsOfWalletsWithDifferentCurrencies(int numberOfBudgets = 2, int numberOfWallets = 2, Action<Wallet>? configureWallet1 = null, Action<Wallet>? configureWallet2 = null)
    {
        var budgetsWithCurrencies = Enumerable.Range(0, numberOfBudgets).Select(_ => CreateBudgetWithDependencies(b =>
        {
            b.Wallets = Enumerable.Range(0, numberOfWallets).Select(i => CreateWallet(w =>
            {
                w.Currency = CreateCurrency();
            })).ToList();
        })).ToList();

        var firstPairOfWallets = new List<Wallet>();
        var secondPairOfWallets = new List<Wallet>();
        foreach (var pair in budgetsWithCurrencies)
        {
            var wallets1 = Enumerable.Range(0, numberOfWallets).Select(_ => CreateWallet(w =>
            {
                w.Currency = CreateCurrency();
            })).ToList();
        
            var wallets2 = Enumerable.Range(0, numberOfWallets).Select(_ => CreateWallet(w =>
            {
                w.Currency = CreateCurrency();
            })).ToList();
            
            pair.budget.Wallets.AddRange(wallets1);
            pair.budget.Wallets.AddRange(wallets2);
            firstPairOfWallets.AddRange(wallets1);
            secondPairOfWallets.AddRange(wallets2);
        }
        
        var budgets = budgetsWithCurrencies.Select(x => x.budget).ToList();
        var currencies = budgets.Select(w => w.Currency).ToList();
        currencies.AddRange(firstPairOfWallets.Select(w => w.Currency));
        currencies.AddRange(secondPairOfWallets.Select(w => w.Currency));
        
        firstPairOfWallets.ForEach(w => configureWallet1?.Invoke(w));
        secondPairOfWallets.ForEach(w => configureWallet2?.Invoke(w));
        return (currencies, budgets, firstPairOfWallets, secondPairOfWallets);
    }
    
    public (List<Currency> currencies, Budget budget, List<Wallet> wallets) CreateBudgetAndWalletsWithDifferentCurrencies(int numberOfWallets = 2, Action<Wallet>? configureWallet = null)
    {
        var budgetCurrency = CreateCurrency();
        var budget = CreateBudget(b => b.Currency = budgetCurrency);
        var wallets = Enumerable.Range(0, numberOfWallets).Select(i => CreateWallet(w =>
        {
            w.BudgetId = budget.Id;
            w.Currency = CreateCurrency();
        })).ToList();

        var currencies = wallets.Select(w => w.Currency).ToList();
        currencies.Add(budgetCurrency);
        
        wallets.ForEach(w => configureWallet?.Invoke(w));
        return (currencies, budget, wallets);
    }
    
    public (List<Currency> currencies, Budget budget, List<Wallet> firstSetOfWallets, List<Wallet> secondSetOfWallets) CreateBudgetWithTwoSetsOfWalletsWithDifferentCurrencies(int numberOfWallets = 2, Action<Wallet>? configureWallet1 = null, Action<Wallet>? configureWallet2 = null)
    {
        var budgetCurrency = CreateCurrency();
        var budget = CreateBudget(b => b.Currency = budgetCurrency);
        var wallets1 = Enumerable.Range(0, numberOfWallets).Select(i => CreateWallet(w =>
        {
            w.BudgetId = budget.Id;
            w.Currency = CreateCurrency();
        })).ToList();
        
        var wallets2 = Enumerable.Range(0, numberOfWallets).Select(i => CreateWallet(w =>
        {
            w.BudgetId = budget.Id;
            w.Currency = CreateCurrency();
        })).ToList();

        var currencies = wallets1.Select(w => w.Currency).ToList();
        currencies.AddRange(wallets2.Select(w => w.Currency));
        currencies.Add(budgetCurrency);
        
        wallets1.ForEach(w => configureWallet1?.Invoke(w));
        wallets2.ForEach(w => configureWallet2?.Invoke(w));
        return (currencies, budget, wallets1, wallets2);
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
    
    public (Currency currency, Budget budget, Wallet wallet, Category category, List<Transaction> applicableTransactions, List<Transaction> notApplicableTransactions, Goal goal) CreateSingleGoalWithManyTransactions(int numberOfTransactions = 2)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var category = CreateCategory();
        var goal = CreateGoal(g =>
        {
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow;
            g.Categories = [category];
        });
        var applicableTransactions = CreateManyTransactions(numberOfTransactions, t =>
        {
            t.CategoryId = category.Id;
            t.TransactionDate = GetRandomDateInDiapason(goal.StartDate, goal.EndDate);
            t.WalletId = wallet.Id;
            t.Type = goal.Type.ToTransactionType();
        });
        var notApplicableTransactions = CreateManyTransactions(numberOfTransactions, t =>
        {
            t.CategoryId = category.Id;
            t.TransactionDate = GetRandomDateInDiapason(goal.StartDate.AddDays(-30), goal.StartDate);
            t.WalletId = wallet.Id;
            t.Type = goal.Type.ToTransactionType();
        });
        
        return (currency, budget, wallet, category, applicableTransactions, notApplicableTransactions, goal);
    }
    
    public (Currency currency, Budget budget, Wallet wallet, Category category, List<Transaction> pastTransactions, List<Transaction> futureTransactions, Goal goal) CreateSingleGoalWithPastAndFutureApplicableTransactions(int numberOfTransactions = 2)
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
        var applicableTransactions = CreateManyTransactions(numberOfTransactions, t =>
        {
            t.CategoryId = category.Id;
            t.TransactionDate = GetRandomDateInDiapason(goal.StartDate, DateTimeOffset.UtcNow);
            t.WalletId = wallet.Id;
            t.Type = goal.Type.ToTransactionType();
        });
        var notApplicableTransactions = CreateManyTransactions(numberOfTransactions, t =>
        {
            t.CategoryId = category.Id;
            t.TransactionDate = GetRandomDateInDiapason(DateTimeOffset.UtcNow, goal.EndDate);
            t.WalletId = wallet.Id;
            t.Type = goal.Type.ToTransactionType();
        });
        
        return (currency, budget, wallet, category, applicableTransactions, notApplicableTransactions, goal);
    }
    
    public (
        Currency currency, 
        Budget budget, 
        List<Wallet> wallets, 
        Category category, 
        List<Transaction> applicableTransactions, 
        List<Transaction> notApplicableTransactions,
        List<Transaction> transferTransactions, 
        List<Goal> goals
        ) CreateManyGoalsWithManyTransactions(int numberOfGoals = 2, int numberOfTransactions = 2, int numberOfNotApplicableTransactions = 2, int numberOfTransferTransactions = 2)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet1 = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var wallet2 = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var category = CreateCategory();
        var goals = CreateManyGoals(numberOfGoals, g =>
        {
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow;
            g.Categories = [category];
        });
        
        var transfers = CreateManyTransfers(numberOfTransferTransactions, t =>
        {
            t.SourceWalletId = wallet1.Id;
            t.TargetWalletId = wallet2.Id;
        });
        
        var applicableTransactions = Enumerable.Range(0, goals.Count).Select(i =>
            CreateManyTransactions(numberOfTransactions, t =>
            {
                t.CategoryId = goals[i].Categories.First().Id;
                t.TransactionDate = goals[i].StartDate.AddDays(_random.Next(1, (goals[i].EndDate - goals[i].StartDate).Days - 1));
                t.WalletId = wallet1.Id;
                t.Type = goals[i].Type.ToTransactionType();
            })
        ).SelectMany(t => t).ToList();
        
        var notApplicableTransactions = Enumerable.Range(0, goals.Count).Select(i =>
            CreateManyTransactions(numberOfNotApplicableTransactions, t =>
            {
                t.CategoryId = category.Id;
                t.TransactionDate = goals.Min(g => g.StartDate).AddDays(-1);
                t.WalletId = wallet1.Id;
            })
        ).SelectMany(t => t).ToList();
        
        return (currency, budget, [wallet1, wallet2], category, applicableTransactions, notApplicableTransactions, transfers, goals);
    }
    
        public (
        Currency currency, 
        Budget budget, 
        Wallet wallet, 
        List<Category> categories, 
        List<Transaction> pastDateTransactions, 
        List<Transaction> futureTransactions,
        List<Goal> goals
        ) CreateManyGoalsWithPastAndFutureApplicableTransactions(int numberOfGoals = 2, int numberOfPastTransactions = 2, int numberOfFutureTransactions = 2)
    {
        var currency = CreateCurrency();
        var budget = CreateBudget(b => b.CurrencyId = currency.Id);
        var wallet = CreateWallet(w => { w.CurrencyId = currency.Id; w.BudgetId = budget.Id; });
        var categories = Enumerable.Range(0, numberOfGoals).Select(_ => CreateCategory()).ToList();
        var goals = Enumerable.Range(0, numberOfGoals).Select(i => CreateGoal(g =>
        {
            g.StartDate =  DateTimeOffset.UtcNow.AddDays(-30);
            g.EndDate = DateTimeOffset.UtcNow.AddDays(30);
            g.Categories = [categories[i]];
        })).ToList();
        
        var applicableTransactions = Enumerable.Range(0, goals.Count).Select(i =>
            CreateManyTransactions(numberOfPastTransactions, t =>
            {
                t.CategoryId = goals[i].Categories.First().Id;
                t.TransactionDate = goals[i].StartDate.AddDays(_random.Next(1, (DateTimeOffset.UtcNow - goals[i].StartDate).Days - 1));
                t.WalletId = wallet.Id;
                t.Type = goals[i].Type.ToTransactionType();
            })
        ).SelectMany(t => t).ToList();
        
        var notApplicableTransactions = Enumerable.Range(0, goals.Count).Select(i =>
            CreateManyTransactions(numberOfFutureTransactions, t =>
            {
                t.CategoryId = goals[i].Categories.First().Id;
                t.TransactionDate = DateTimeOffset.UtcNow.AddDays(_random.Next(1, (goals[i].EndDate - DateTimeOffset.UtcNow).Days - 1));
                t.WalletId = wallet.Id;
            })
        ).SelectMany(t => t).ToList();
        
        return (currency, budget, wallet, categories, applicableTransactions, notApplicableTransactions, goals);
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

    private DateTimeOffset GetRandomDateInDiapason(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("endDate must be greater than or equal to startDate.");

        var range = endDate - startDate;
        var randomTicks = (long)(_random.NextDouble() * range.Ticks);
        return startDate.AddTicks(randomTicks);
    }
}