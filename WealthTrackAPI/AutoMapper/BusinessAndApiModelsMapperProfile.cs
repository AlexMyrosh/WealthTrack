using AutoMapper;
using WealthTrack.API.ApiModels.Budget;
using WealthTrack.API.ApiModels.Category;
using WealthTrack.API.ApiModels.Currency;
using WealthTrack.API.ApiModels.Goal;
using WealthTrack.API.ApiModels.Transaction;
using WealthTrack.API.ApiModels.Wallet;
using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Business.BusinessModels.Currency;
using WealthTrack.Business.BusinessModels.Goal;
using WealthTrack.Business.BusinessModels.Transaction;
using WealthTrack.Business.BusinessModels.Wallet;

namespace WealthTrack.API.AutoMapper
{
    public class BusinessAndApiModelsMapperProfile : Profile
    {
        public BusinessAndApiModelsMapperProfile()
        {
            // Categories
            CreateMap<CategoryUpsertApiModel, CategoryUpsertBusinessModel>();
            CreateMap<CategoryDetailsBusinessModel, CategoryDetailsApiModel>();
            CreateMap<ParentCategoryDetailsBusinessModel, ParentCategoryDetailsApiModel>();
            CreateMap<ChildCategoryDetailsBusinessModel, ChildCategoryDetailsApiModel>();

            // Transactions
            CreateMap<TransactionUpsertApiModel, TransactionUpsertBusinessModel>();
            CreateMap<TransactionDetailsBusinessModel, TransactionDetailsApiModel>();
            CreateMap<CategoryRelatedToTransactionDetailsBusinessModel, CategoryRelatedToTransactionDetailsApiModel>();
            CreateMap<WalletRelatedToTransactionDetailsBusinessModel, WalletRelatedToTransactionDetailsApiModel>();

            // Wallets
            CreateMap<WalletUpsertApiModel, WalletUpsertBusinessModel>();
            CreateMap<WalletDetailsBusinessModel, WalletDetailsApiModel>();
            CreateMap<CurrencyRelatedToWalletDetailsBusinessModel, CurrencyRelatedToWalletDetailsApiModel>();
            CreateMap<BudgetRelatedToWalletDetailsBusinessModel, BudgetRelatedToWalletDetailsApiModel>();
            CreateMap<TransactionRelatedToWalletDetailsBusinessModel, TransactionRelatedToWalletDetailsApiModel>();

            // Currencies
            CreateMap<CurrencyDetailsBusinessModel, CurrencyDetailsApiModel>();

            // Budgets
            CreateMap<BudgetUpsertApiModel, BudgetUpsertBusinessModel>();
            CreateMap<BudgetDetailsBusinessModel, BudgetDetailsApiModel>();
            CreateMap<CurrencyRelatedToBudgetDetailsBusinessModel, CurrencyRelatedToBudgetDetailsApiModel>();
            CreateMap<WalletRelatedToBudgetDetailsBusinessModel, WalletRelatedToBudgetDetailsApiModel>();

            // Goal
            CreateMap<GoalUpsertApiModel, GoalUpsertBusinessModel>();
            CreateMap<GoalDetailsBusinessModel, GoalDetailsApiModel>();
            CreateMap<CategoryRelatedToGoalDetailsBusinessModel, CategoryRelatedToGoalDetailsApiModel>();
            CreateMap<WalletRelatedToGoalDetailsBusinessModel, WalletRelatedToGoalDetailsApiModel>();
        }
    }
}
