using AutoMapper;
using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Business.BusinessModels.Currency;
using WealthTrack.Business.BusinessModels.Transaction;
using WealthTrack.Business.BusinessModels.Wallet;
using WealthTrack.Data.DomainModels;

namespace WealthTrack.Business.AutoMapper
{
    public class DomainAndBusinessModelsMapperProfile : Profile
    {
        public DomainAndBusinessModelsMapperProfile()
        {
            // Category
            CreateMap<CreateCategoryBusinessModel, Category>();
            CreateMap<UpdateCategoryBusinessModel, Category>();
            CreateMap<Category, CategoryDetailsBusinessModel>();
            CreateMap<Category, ParentCategoryDetailsBusinessModel>();
            CreateMap<Category, ChildCategoryDetailsBusinessModel>();

            // Transaction
            CreateMap<CreateTransactionBusinessModel, Transaction>();
            CreateMap<UpdateTransactionBusinessModel, Transaction>();
            CreateMap<Transaction, TransactionDetailsBusinessModel>();
            CreateMap<Category, CategoryRelatedToTransactionDetailsBusinessModel>();
            CreateMap<Wallet, WalletRelatedToTransactionDetailsBusinessModel>();

            // Wallet
            CreateMap<CreateWalletBusinessModel, Wallet>();
            CreateMap<UpdateWalletBusinessModel, Wallet>();
            CreateMap<Wallet, WalletDetailsBusinessModel>();
            CreateMap<Currency, CurrencyRelatedToWalletDetailsBusinessModel>();

            // Currencies
            CreateMap<Currency, CurrencyDetailsBusinessModel>();

            // Budgets
            CreateMap<CreateBudgetBusinessModel, Budget>();
            CreateMap<UpdateBudgetBusinessModel, Budget>();
            CreateMap<Budget, BudgetDetailsBusinessModel>();
            CreateMap<Currency, CurrencyRelatedToBudgetDetailsBusinessModel>();
            CreateMap<Wallet, WalletRelatedToBudgetDetailsBusinessModel>();
        }
    }
}
