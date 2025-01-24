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
            CreateMap<CategoryUpsertBusinessModel, Category>();
            CreateMap<Category, CategoryDetailsBusinessModel>();
            CreateMap<Category, ParentCategoryDetailsBusinessModel>();
            CreateMap<Category, ChildCategoryDetailsBusinessModel>();

            // Transaction
            CreateMap<TransactionUpsertBusinessModel, Transaction>();
            CreateMap<Transaction, TransactionDetailsBusinessModel>();
            CreateMap<Category, CategoryRelatedToTransactionDetailsBusinessModel>();
            CreateMap<Wallet, WalletRelatedToTransactionDetailsBusinessModel>();

            // Wallet
            CreateMap<WalletUpsertBusinessModel, Wallet>();
            CreateMap<Wallet, WalletDetailsBusinessModel>();
            CreateMap<Currency, CurrencyRelatedToWalletDetailsBusinessModel>();

            // Currencies
            CreateMap<Currency, CurrencyDetailsBusinessModel>();

            // Budgets
            CreateMap<BudgetUpsertBusinessModel, Budget>();
            CreateMap<Budget, BudgetDetailsBusinessModel>();
            CreateMap<Currency, CurrencyRelatedToBudgetDetailsBusinessModel>();
            CreateMap<Wallet, WalletRelatedToBudgetDetailsBusinessModel>();
        }
    }
}
