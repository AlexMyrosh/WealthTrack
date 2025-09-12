using WealthTrack.IntegrationTests.TestData;
using WealthTrack.IntegrationTests.WebAppFactories;
using System.Net;
using System.Net.Http.Json;
using WealthTrack.API.ApiModels.Transaction;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace WealthTrack.IntegrationTests.Tests
{
    [Collection("TransactionTests")]
    public class TransactionControllerTests(EmptyWebAppFactory factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task GetAll_WithoutInclude_ReturnsTransactionsWithoutRelatedEntities()
        {
            // Arrange
            var (currency, budget, wallet, category, transaction) = DataFactory.CreateTransactionScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            DbContext.Transactions.Add(transaction);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/transaction");
            var models = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            models.Should().NotBeNullOrEmpty();
            models.Should().AllSatisfy(t => { t.Wallet.Should().BeNull(); t.Category.Should().BeNull(); });
        }

        [Fact]
        public async Task GetAll_WithIncludeAllRelatedEntities_ReturnsTransactionsWithWalletAndCategory()
        {
            // Arrange
            var (currency, budget, wallet, category, transaction) = DataFactory.CreateTransactionScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            DbContext.Transactions.Add(transaction);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/transaction?include=Wallet,Category");
            var models = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            models.Should().NotBeNullOrEmpty();
            models.Should().AllSatisfy(t => { t.Wallet.Should().NotBeNull(); t.Category.Should().NotBeNull(); });
        }

        [Fact]
        public async Task GetAll_WithIncludeCategory_ReturnsTransactionsWithCategoryOnly()
        {
            // Arrange
            var (currency, budget, wallet, category, transaction) = DataFactory.CreateTransactionScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            DbContext.Transactions.Add(transaction);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/transaction?include=Category");
            var models = await response.Content.ReadFromJsonAsync<List<TransactionDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            models.Should().NotBeNullOrEmpty();
            models.Should().AllSatisfy(t => { t.Wallet.Should().BeNull(); t.Category.Should().NotBeNull(); });
        }

        [Fact]
        public async Task GetById_WithIncludeAllRelatedEntities_ReturnsTransactionWithWalletAndCategory()
        {
            // Arrange
            var (currency, budget, wallet, category, transaction) = DataFactory.CreateTransactionScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            DbContext.Transactions.Add(transaction);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/transaction/{transaction.Id}?include=Wallet,Category");
            var model = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            model.Should().NotBeNull();
            model!.Id.Should().Be(transaction.Id);
            model.Wallet.Should().NotBeNull();
            model.Category.Should().NotBeNull();
        }

        [Fact]
        public async Task GetById_WithoutInclude_ReturnsTransactionWithoutRelatedEntities()
        {
            // Arrange
            var (currency, budget, wallet, category, transaction) = DataFactory.CreateTransactionScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            DbContext.Transactions.Add(transaction);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/transaction/{transaction.Id}");
            var model = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            model.Should().NotBeNull();
            model!.Id.Should().Be(transaction.Id);
            model.Wallet.Should().BeNull();
            model.Category.Should().BeNull();
        }

        [Fact]
        public async Task GetById_WithIncludeCategory_ReturnsTransactionWithCategoryOnly()
        {
            // Arrange
            var (currency, budget, wallet, category, transaction) = DataFactory.CreateTransactionScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            DbContext.Transactions.Add(transaction);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/transaction/{transaction.Id}?include=Category");
            var model = await response.Content.ReadFromJsonAsync<TransactionDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            model.Should().NotBeNull();
            model!.Id.Should().Be(transaction.Id);
            model.Wallet.Should().BeNull();
            model.Category.Should().NotBeNull();
        }
        
        [Fact]
        public async Task GetById_WithWrongId_ReturnsNotFound()
        {
            // Act
            var response = await Client.GetAsync($"/api/transaction/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_CreatesTransaction()
        {
            // Arrange
            var (currency, budget, wallet, category, _) = DataFactory.CreateTransactionScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            await DbContext.SaveChangesAsync();

            var upsert = new TransactionUpsertApiModel
            {
                Amount = 100M,
                Description = "Test Transaction",
                WalletId = wallet.Id,
                CategoryId = category.Id,
                Type = Shared.Enums.TransactionType.Expense,
                TransactionDate = DateTimeOffset.UtcNow
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);
            var createdId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            createdId.Should().NotBe(Guid.Empty);
            var created = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == createdId);
            created.Should().NotBeNull();
            created!.Amount.Should().Be(upsert.Amount);
        }

        [Fact]
        public async Task Create_CreatesTransferTransaction()
        {
            // Arrange
            var (currency, budget, source, target, _, transaction) = DataFactory.CreateTransferScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(source);
            DbContext.Wallets.Add(target);
            await DbContext.SaveChangesAsync();

            var upsert = new TransferTransactionUpsertApiModel
            {
                Amount = 200M,
                Description = "Test Transfer",
                SourceWalletId = source.Id,
                TargetWalletId = target.Id,
                TransactionDate = DateTimeOffset.UtcNow
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);
            var createdId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            createdId.Should().NotBe(Guid.Empty);
            var created = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == createdId);
            created.Should().NotBeNull();
            created!.Amount.Should().Be(upsert.Amount);
        }

        [Fact]
        public async Task Create_TransferTransaction_SourceAndTargetWalletBalancesAreUpdated()
        {
            // Arrange
            var (currency, budget, source, target, _, transaction) = DataFactory.CreateTransferScenario();
            source.Balance = 1000M;
            target.Balance = 0M;
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(source);
            DbContext.Wallets.Add(target);
            await DbContext.SaveChangesAsync();

            var upsert = new TransferTransactionUpsertApiModel
            {
                Amount = 300M,
                Description = "Test Transfer",
                SourceWalletId = source.Id,
                TargetWalletId = target.Id,
                TransactionDate = DateTimeOffset.UtcNow
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/transaction/transfer/create", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedSource = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == source.Id);
            var updatedTarget = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == target.Id);
            updatedSource!.Balance.Should().Be(700M);
            updatedTarget!.Balance.Should().Be(300M);
        }
        
        [Fact]
        public async Task CreateTransaction_WalletAndBudgetBalancesAreUpdated()
        {
            // Arrange
            var (currency, budget, wallet, category, _) = DataFactory.CreateTransactionScenario();
            wallet.Balance = 500M;
            budget.OverallBalance = 500M;
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            await DbContext.SaveChangesAsync();

            var upsert = new TransactionUpsertApiModel
            {
                Amount = 100M,
                Description = "Test Transaction",
                WalletId = wallet.Id,
                CategoryId = category.Id,
                Type = Shared.Enums.TransactionType.Expense,
                TransactionDate = DateTimeOffset.UtcNow
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == wallet.Id);
            var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == budget.Id);
            updatedWallet!.Balance.Should().Be(400M);
            updatedBudget!.OverallBalance.Should().Be(400M);
        }
        
        [Fact]
        public async Task CreateTransaction_GoalAmountIsUpdated()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, _, _) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            await DbContext.SaveChangesAsync();

            var upsert = new TransactionUpsertApiModel
            {
                Amount = 150M,
                Description = "Goal Transaction",
                WalletId = wallet.Id,
                CategoryId = category.Id,
                Type = Shared.Enums.TransactionType.Income,
                TransactionDate = DateTimeOffset.UtcNow
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/transaction/create", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
            updatedGoal!.ActualMoneyAmount.Should().Be(150M);
        }

        [Fact]
        public async Task Create_WithNullBody_ReturnsBadRequest()
        {
            // Act
            var response = await Client.PostAsJsonAsync("/api/transaction/create", (TransactionUpsertApiModel)null!);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_UpdatesTransactionName()
        {
            // Arrange
            var (currency, budget, wallet, category, transaction) = DataFactory.CreateTransactionScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            DbContext.Transactions.Add(transaction);
            await DbContext.SaveChangesAsync();

            var upsert = new TransactionUpsertApiModel { Description = "Updated Description" };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/transaction/update/{transaction.Id}", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updated = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == transaction.Id);
            updated.Should().NotBeNull();
            updated!.Description.Should().Be(upsert.Description);
        }

        [Fact]
        public async Task Update_UpdateTransferTransaction_ShouldUpdateSourceAndTargetWalletBalances()
        {
            // Arrange
            var (currency, budget, source, target, transfer, transaction) = DataFactory.CreateTransferScenario();
            source.Balance = 600M;
            target.Balance = 400M;
            transfer.Amount = 200M;
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(source);
            DbContext.Wallets.Add(target);
            DbContext.TransferTransactions.Add(transfer);
            await DbContext.SaveChangesAsync();

            var upsert = new TransferTransactionUpsertApiModel { Amount = 400M };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/transaction/transfer/update/{transfer.Id}", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updatedSource = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == source.Id);
            var updatedTarget = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == target.Id);
            updatedSource!.Balance.Should().Be(400M);
            updatedTarget!.Balance.Should().Be(600M);
        }

        [Fact]
        public async Task Update_UpdateTransactionAmount_ShouldUpdateWalletAndBudgetBalances()
        {
            // Arrange
            var (currency, budget, wallet, category, transaction) = DataFactory.CreateTransactionScenario();
            wallet.Balance = 500M;
            budget.OverallBalance = 500M;
            transaction.Amount = 100M;
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            DbContext.Transactions.Add(transaction);
            await DbContext.SaveChangesAsync();

            var upsert = new TransactionUpsertApiModel { Amount = 200M };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/transaction/update/{transaction.Id}", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == wallet.Id);
            var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == budget.Id);
            updatedWallet!.Balance.Should().Be(400M);
            updatedBudget!.OverallBalance.Should().Be(400M);
        }
        
        [Fact]
        public async Task Update_UpdateTransactionAmount_ShouldUpdateGoalAmount()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, _, _) = DataFactory.CreateGoalWithTransactionsScenario();
            var transaction = DataFactory.CreateTransaction(t => { t.WalletId = wallet.Id; t.CategoryId = category.Id; t.Amount = 100M; t.Type = Shared.Enums.TransactionType.Income; });
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            DbContext.Transactions.Add(transaction);
            await DbContext.SaveChangesAsync();

            var upsert = new TransactionUpsertApiModel { Amount = 250M };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/transaction/update/{transaction.Id}", upsert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
            updatedGoal!.ActualMoneyAmount.Should().Be(150M);
        }

        [Fact]
        public async Task Update_WithIncorrectId_ReturnsNotFound()
        {
            // Arrange
            var upsert = new TransactionUpsertApiModel { Description = "Updated" };
            var response = await Client.PutAsJsonAsync($"/api/transaction/update/{Guid.NewGuid()}", upsert);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task HardDelete_DeletesTransactionOnly()
        {
            // Arrange
            var (currency, budget, wallet, category, transaction) = DataFactory.CreateTransactionScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            DbContext.Transactions.Add(transaction);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{transaction.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var deleted = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == transaction.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task HardDelete_WalletAndBudgetBalancesAreUpdated()
        {
            // Arrange
            var (currency, budget, wallet, category, transaction) = DataFactory.CreateTransactionScenario();
            wallet.Balance = 300M;
            budget.OverallBalance = 300M;
            transaction.Amount = 100M;
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(wallet);
            DbContext.Categories.Add(category);
            DbContext.Transactions.Add(transaction);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{transaction.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updatedWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == wallet.Id);
            var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == budget.Id);
            updatedWallet!.Balance.Should().Be(400M);
            updatedBudget!.OverallBalance.Should().Be(400M);
        }

        [Fact]
        public async Task HardDelete_TransferTransaction_SourceAndTargetBalancesAreUpdated()
        {
            // Arrange
            var (currency, budget, source, target, transfer, transaction) = DataFactory.CreateTransferScenario();
            source.Balance = 600M;
            target.Balance = 400M;
            transfer.Amount = 200M;
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Wallets.Add(source);
            DbContext.Wallets.Add(target);
            DbContext.TransferTransactions.Add(transfer);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{transfer.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updatedSource = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == source.Id);
            var updatedTarget = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == target.Id);
            updatedSource!.Balance.Should().Be(800M);
            updatedTarget!.Balance.Should().Be(200M);
        }
        
        [Fact]
        public async Task HardDelete_GoalAmountIsUpdated()
        {
            // Arrange
            var (currency, budget, category, wallet, goal, applicable, _) = DataFactory.CreateGoalWithTransactionsScenario();
            DbContext.Currencies.Add(currency);
            DbContext.Budgets.Add(budget);
            DbContext.Categories.Add(category);
            DbContext.Wallets.Add(wallet);
            DbContext.Goals.Add(goal);
            DbContext.Transactions.AddRange(applicable);
            await DbContext.SaveChangesAsync();
            var plannedResult = applicable[1].Amount;

            // Act
            var response = await Client.DeleteAsync($"/api/transaction/hard_delete/{applicable[0].Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updatedGoal = await DbContext.Goals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == goal.Id);
            updatedGoal!.ActualMoneyAmount.Should().Be(plannedResult);
        }
    }
}