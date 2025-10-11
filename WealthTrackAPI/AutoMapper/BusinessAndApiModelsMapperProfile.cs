using AutoMapper;
using WealthTrack.API.ApiModels.Category;
using WealthTrack.API.ApiModels.Currency;
using WealthTrack.API.ApiModels.Goal;
using WealthTrack.API.ApiModels.Transaction;
using WealthTrack.API.ApiModels.Wallet;
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
            // Category
            CreateMap<CategoryUpsertApiModel, CategoryUpsertBusinessModel>();
            CreateMap<CategoryDetailsBusinessModel, CategoryDetailsApiModel>();
            CreateMap<CategoryRelatedToCategoryDetailsBusinessModel, CategoryRelatedToCategoryDetailsApiModel>();

            // Currency
            CreateMap<CurrencyDetailsBusinessModel, CurrencyDetailsApiModel>();
            
            // Goal
            CreateMap<GoalUpsertApiModel, GoalUpsertBusinessModel>();
            CreateMap<GoalDetailsBusinessModel, GoalDetailsApiModel>();
            CreateMap<CategoryRelatedToGoalDetailsBusinessModel, CategoryRelatedToGoalDetailsApiModel>();
            
            // Transactions
            CreateMap<TransactionUpsertApiModel, TransactionUpsertBusinessModel>();
            CreateMap<TransferTransactionUpsertApiModel, TransferTransactionUpsertBusinessModel>();
            CreateMap<TransactionDetailsBusinessModel, TransactionDetailsApiModel>();
            CreateMap<CategoryRelatedToTransactionDetailsBusinessModel, CategoryRelatedToTransactionDetailsApiModel>();
            CreateMap<WalletRelatedToTransactionDetailsBusinessModel, WalletRelatedToTransactionDetailsApiModel>();

            // Wallets
            CreateMap<WalletUpsertApiModel, WalletUpsertBusinessModel>();
            CreateMap<WalletDetailsBusinessModel, WalletDetailsApiModel>();
            CreateMap<CurrencyRelatedToWalletDetailsBusinessModel, CurrencyRelatedToWalletDetailsApiModel>();
            CreateMap<TransactionRelatedToWalletDetailsBusinessModel, TransactionRelatedToWalletDetailsApiModel>();
        }
    }
}
