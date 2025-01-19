using AutoMapper;
using WealthTrack.API.ApiModels.Category;
using WealthTrack.API.ApiModels.Currency;
using WealthTrack.API.ApiModels.Transaction;
using WealthTrack.API.ApiModels.Wallet;
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
            CreateMap<CreateCategoryApiModel, CreateCategoryBusinessModel>();
            CreateMap<UpdateCategoryApiModel, UpdateCategoryBusinessModel>();
            CreateMap<CategoryDetailsBusinessModel, CategoryDetailsApiModel>().ReverseMap();
            CreateMap<ParentCategoryDetailsBusinessModel, ParentCategoryDetailsApiModel>().ReverseMap();
            CreateMap<ChildCategoryDetailsBusinessModel, ChildCategoryDetailsApiModel>().ReverseMap();

            // Transactions
            CreateMap<CreateTransactionApiModel, CreateTransactionBusinessModel>();
            CreateMap<UpdateTransactionApiModel, UpdateTransactionBusinessModel>();
            CreateMap<TransactionDetailsApiModel, TransactionDetailsBusinessModel>().ReverseMap();
            CreateMap<CategoryRelatedToTransactionDetailsApiModel, CategoryRelatedToTransactionDetailsBusinessModel>().ReverseMap();
            CreateMap<WalletRelatedToTransactionDetailsApiModel, WalletRelatedToTransactionDetailsBusinessModel>().ReverseMap();

            // Wallets
            CreateMap<CreateWalletApiModel, CreateWalletBusinessModel>();
            CreateMap<UpdateWalletApiModel, UpdateWalletBusinessModel>();
            CreateMap<WalletDetailsApiModel, WalletDetailsBusinessModel>().ReverseMap();
            CreateMap<CurrencyRelatedToWalletDetailsApiModel, CurrencyRelatedToWalletDetailsBusinessModel>().ReverseMap();

            // Currencies
            CreateMap<CurrencyDetailsApiModel, CurrencyDetailsBusinessModel>().ReverseMap();
        }
    }
}
