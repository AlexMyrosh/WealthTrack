using AutoMapper;
using WealthTrack.API.ApiModels.Budget;
using WealthTrack.API.ApiModels.Category;
using WealthTrack.API.ApiModels.Currency;
using WealthTrack.API.ApiModels.Transaction;
using WealthTrack.API.ApiModels.Wallet;
using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Business.BusinessModels.Currency;
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
            CreateMap<CategoryDetailsBusinessModel, CategoryDetailsApiModel>().ReverseMap();
            CreateMap<ParentCategoryDetailsBusinessModel, ParentCategoryDetailsApiModel>().ReverseMap();
            CreateMap<ChildCategoryDetailsBusinessModel, ChildCategoryDetailsApiModel>().ReverseMap();

            // Transactions
            CreateMap<TransactionUpsertApiModel, TransactionUpsertBusinessModel>();
            CreateMap<TransactionDetailsApiModel, TransactionDetailsBusinessModel>().ReverseMap();
            CreateMap<CategoryRelatedToTransactionDetailsApiModel, CategoryRelatedToTransactionDetailsBusinessModel>().ReverseMap();
            CreateMap<WalletRelatedToTransactionDetailsApiModel, WalletRelatedToTransactionDetailsBusinessModel>().ReverseMap();

            // Wallets
            CreateMap<WalletUpsertApiModel, WalletUpsertBusinessModel>();
            CreateMap<WalletDetailsApiModel, WalletDetailsBusinessModel>().ReverseMap();
            CreateMap<CurrencyRelatedToWalletDetailsApiModel, CurrencyRelatedToWalletDetailsBusinessModel>().ReverseMap();

            // Currencies
            CreateMap<CurrencyDetailsApiModel, CurrencyDetailsBusinessModel>().ReverseMap();

            // Budgets
            CreateMap<BudgetUpsertApiModel, BudgetUpsertBusinessModel>();
            CreateMap<BudgetDetailsApiModel, BudgetDetailsBusinessModel>().ReverseMap();
            CreateMap<CurrencyRelatedToBudgetDetailsApiModel, CurrencyRelatedToBudgetDetailsBusinessModel>().ReverseMap();
            CreateMap<WalletRelatedToBudgetDetailsApiModel, WalletRelatedToBudgetDetailsBusinessModel>().ReverseMap();
        }
    }
}
