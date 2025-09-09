using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using WealthTrack.API.ApiModels.Budget;
using WealthTrack.Data.DomainModels;
using WealthTrack.IntegrationTests.TestData;
using WealthTrack.IntegrationTests.WebAppFactories;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Tests
{
    [Collection("BudgetTests")]
    public class BudgetControllerTests(EmptyWebAppFactory factory) : IntegrationTestBase(factory)
    {
        // GET ALL tests

        [Fact]
        public async Task GetAll_WithoutInclude_ReturnsAllBudgetsWithoutRelatedEntities()
        {
            // Arrange
            var scenario = DataFactory.CreateMultiBudgetsScenario(budgetCount: 2, walletCount: 2);
            DbContext.Currencies.Add(scenario.currency);
            DbContext.Budgets.AddRange(scenario.budgets);
            DbContext.Wallets.AddRange(scenario.wallets);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/budget");
            var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allBudgets.Should().NotBeNullOrEmpty();
            allBudgets.Should().HaveCount(2);
            allBudgets.Should().Contain(budget =>budget.Id == scenario.budgets[0].Id);
            allBudgets.Should().Contain(budget => budget.Id == scenario.budgets[1].Id);
            allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().BeNull());
            allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().BeNullOrEmpty());
        }

        [Fact]
        public async Task GetAll_WithIncludeCurrency_ReturnsAllBudgetsWithCurrencyOnly()
        {
            // Arrange
            var scenario = DataFactory.CreateMultiBudgetsScenario(budgetCount: 2, walletCount: 2);
            DbContext.Currencies.Add(scenario.currency);
            DbContext.Budgets.AddRange(scenario.budgets);
            DbContext.Wallets.AddRange(scenario.wallets);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/budget?include={nameof(Budget.Currency)}");
            var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allBudgets.Should().NotBeNullOrEmpty();
            allBudgets.Should().HaveCount(2);
            allBudgets.Should().Contain(budget =>budget.Id == scenario.budgets[0].Id);
            allBudgets.Should().Contain(budget => budget.Id == scenario.budgets[1].Id);
            allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().NotBeNull());
            allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().BeNullOrEmpty());
        }

        [Fact]
        public async Task GetAll_WithIncludeAllRelatedEntities_ReturnsAllBudgetsWithAllRelatedEntities()
        {
            // Arrange
            var scenario = DataFactory.CreateMultiBudgetsScenario(budgetCount: 2, walletCount: 2);
            DbContext.Currencies.Add(scenario.currency);
            DbContext.Budgets.AddRange(scenario.budgets);
            DbContext.Wallets.AddRange(scenario.wallets);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/budget?include={nameof(Budget.Currency)},{nameof(Budget.Wallets)}");
            var allBudgets = await response.Content.ReadFromJsonAsync<List<BudgetDetailsApiModel>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            allBudgets.Should().NotBeNullOrEmpty();
            allBudgets.Should().HaveCount(2);
            allBudgets.Should().Contain(budget =>budget.Id == scenario.budgets[0].Id);
            allBudgets.Should().Contain(budget => budget.Id == scenario.budgets[1].Id);
            allBudgets.Should().AllSatisfy(budget => budget.Currency.Should().NotBeNull());
            allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().NotBeNullOrEmpty());
            allBudgets.Should().AllSatisfy(budget => budget.Wallets.Should().HaveCount(2));
        }

        // GET BY ID tests

        [Fact]
        public async Task GetById_WithoutInclude_ReturnsBudgetWithoutRelatedEntities()
        {
            // Arrange
            var scenario = DataFactory.CreateSingleBudgetScenario();
            DbContext.Currencies.Add(scenario.currency);
            DbContext.Budgets.Add(scenario.budget);
            DbContext.Wallets.Add(scenario.wallet);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/budget/{scenario.budget.Id}");
            var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            budget.Should().NotBeNull();
            budget.Id.Should().Be(scenario.budget.Id);
            budget.Currency.Should().BeNull();
            budget.Wallets.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetById_WithIncludeCurrency_ReturnsBudgetWithCurrencyOnly()
        {
            // Arrange
            var scenario = DataFactory.CreateSingleBudgetScenario();
            DbContext.Currencies.Add(scenario.currency);
            DbContext.Budgets.Add(scenario.budget);
            DbContext.Wallets.Add(scenario.wallet);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/budget/{scenario.budget.Id}?include={nameof(Budget.Currency)}");
            var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            budget.Should().NotBeNull();
            budget.Id.Should().Be(scenario.budget.Id);
            budget.Currency.Should().NotBeNull();
            budget.Currency.Id.Should().Be(scenario.budget.CurrencyId);
            budget.Wallets.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task GetById_WithIncludeAllRelatedEntities_ReturnsBudgetWithAllRelatedEntities()
        {
            // Arrange
            var scenario = DataFactory.CreateMultiBudgetsScenario(budgetCount: 1, walletCount: 2);
            DbContext.Currencies.Add(scenario.currency);
            DbContext.Budgets.Add(scenario.budgets[0]);
            DbContext.Wallets.AddRange(scenario.wallets);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/budget/{scenario.budgets[0].Id}?include={nameof(Budget.Currency)},{nameof(Budget.Wallets)}");
            var budget = await response.Content.ReadFromJsonAsync<BudgetDetailsApiModel>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            budget.Should().NotBeNull();
            budget.Id.Should().Be(scenario.budgets[0].Id);
            budget.Currency.Should().NotBeNull();
            budget.Currency.Id.Should().Be(scenario.budgets[0].CurrencyId);
            budget.Wallets.Should().NotBeEmpty();
            budget.Wallets.Should().HaveCount(2);
            budget.Wallets.Should().AllSatisfy(wallet => scenario.wallets.Any(w => w.Id == wallet.Id));
        }

        [Fact]
        public async Task GetById_WithWrongId_ReturnsNotFoundResult()
        {
            // Act
            var response = await Client.GetAsync($"/api/budget/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // CREATE tests

        [Fact]
        public async Task Create_CreatesBudget()
        {
            // Arrange
            var currency = DataFactory.CreateCurrency();
            DbContext.Currencies.Add(currency);
            await DbContext.SaveChangesAsync();

            var budgetToCreate = new BudgetUpsertApiModel
            {
                Name = "Test Budget",
                CurrencyId = currency.Id
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/budget/create", budgetToCreate);
            var createdId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            createdId.Should().NotBe(Guid.Empty);
            var createdBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == createdId);
            createdBudget.Should().NotBeNull();
            createdBudget.Name.Should().Be(budgetToCreate.Name);
            createdBudget.CurrencyId.Should().Be(budgetToCreate.CurrencyId!.Value);
        }

        [Fact]
        public async Task Create_WithNullBody_ReturnsBadRequest()
        {
            // Act
            var response = await Client.PostAsJsonAsync("/api/budget/create", (BudgetUpsertApiModel)null!);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // UPDATE tests

        [Fact]
        public async Task Update_UpdatesBudgetName()
        {
            // Arrange
            var scenario = DataFactory.CreateSingleBudgetScenario();
            DbContext.Currencies.Add(scenario.currency);
            DbContext.Budgets.Add(scenario.budget);
            DbContext.Wallets.Add(scenario.wallet);
            await DbContext.SaveChangesAsync();
            var modelToUpdate = new BudgetUpsertApiModel
            {
                Name = "Updated Budget Name"
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/budget/update/{scenario.budget.Id}", modelToUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var updatedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == scenario.budget.Id);
            updatedBudget.Should().NotBeNull();
            updatedBudget.Name.Should().Be(modelToUpdate.Name);
            updatedBudget.CurrencyId.Should().Be(scenario.budget.CurrencyId);
        }

        [Fact]
        public async Task Update_WithIncorrectId_ReturnsNotFoundResult()
        {
            // Arrange
            var modelToUpdate = new BudgetUpsertApiModel
            {
                Name = "Updated Budget Name"
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/budget/update/{Guid.NewGuid()}", modelToUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // DELETE tests

        [Fact]
        public async Task HardDelete_DeletesBudgetAndAllRelatedData()
        {
            // Arrange - Create a complex scenario with wallets, transactions, and transfers
            var scenario = DataFactory.CreateBudgetWithTransferScenario();
            DbContext.Currencies.Add(scenario.currency);
            DbContext.Budgets.Add(scenario.budget);
            DbContext.Wallets.AddRange([scenario.sourceWallet, scenario.targetWallet]);
            DbContext.TransferTransactions.Add(scenario.transfer);
            await DbContext.SaveChangesAsync();
            
            // Add some additional transactions to the source wallet
            var additionalTransaction = DataFactory.CreateTransaction(t => 
            {
                t.WalletId = scenario.sourceWallet.Id;
                t.Amount = 100M;
                t.Type = TransactionType.Income;
            });
            DbContext.Transactions.Add(additionalTransaction);
            await DbContext.SaveChangesAsync();

            // Act
            var deleteResponse = await Client.DeleteAsync($"/api/budget/hard_delete/{scenario.budget.Id}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            
            // Verify budget and related data are deleted
            var deletedBudget = await DbContext.Budgets.AsNoTracking().FirstOrDefaultAsync(budget => budget.Id == scenario.budget.Id);
            var deletedSourceWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == scenario.sourceWallet.Id);
            var deletedTargetWallet = await DbContext.Wallets.AsNoTracking().FirstOrDefaultAsync(wallet => wallet.Id == scenario.targetWallet.Id);
            var deletedTransferTransaction = await DbContext.TransferTransactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == scenario.transfer.Id);
            var deletedTransaction = await DbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(transaction => transaction.Id == additionalTransaction.Id);
            
            // Verify currency still exists (not deleted)
            var existingCurrency = await DbContext.Currencies.AsNoTracking().FirstOrDefaultAsync(currency => currency.Id == scenario.currency.Id);

            deletedBudget.Should().BeNull();
            deletedSourceWallet.Should().BeNull();
            deletedTargetWallet.Should().BeNull();
            deletedTransferTransaction.Should().BeNull();
            deletedTransaction.Should().BeNull();
            existingCurrency.Should().NotBeNull();
        }
    }
}