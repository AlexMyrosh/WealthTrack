using AutoMapper;
using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Business.BusinessModels.Currency;
using WealthTrack.Business.BusinessModels.Goal;
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
            CreateMap<CategoryUpsertBusinessModel, Category>()
                .ForMember(dest => dest.IconName, opt => opt.Condition(src => src.IconName != null))
                .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
                .ForMember(dest => dest.Type, opt => opt.Condition(src => src.Type.HasValue))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.Condition(src => src.ParentCategoryId.HasValue));
            CreateMap<Category, CategoryDetailsBusinessModel>();
            CreateMap<Category, ParentCategoryDetailsBusinessModel>();
            CreateMap<Category, ChildCategoryDetailsBusinessModel>();

            // Transaction
            CreateMap<TransactionUpsertBusinessModel, Transaction>()
                .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null))
                .ForMember(dest => dest.Type, opt => opt.Condition(src => src.Type.HasValue))
                .ForMember(dest => dest.Amount, opt => opt.Condition(src => src.Amount.HasValue))
                .ForMember(dest => dest.CategoryId, opt => opt.Condition(src => src.CategoryId.HasValue))
                .ForMember(dest => dest.WalletId, opt => opt.Condition(src => src.WalletId.HasValue));
            CreateMap<Transaction, TransactionDetailsBusinessModel>();
            CreateMap<Category, CategoryRelatedToTransactionDetailsBusinessModel>();
            CreateMap<Wallet, WalletRelatedToTransactionDetailsBusinessModel>();

            // Wallet
            CreateMap<WalletUpsertBusinessModel, Wallet>()
                .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
                .ForMember(dest => dest.Type, opt => opt.Condition(src => src.Type.HasValue))
                .ForMember(dest => dest.Balance, opt => opt.Condition(src => src.Balance.HasValue))
                .ForMember(dest => dest.IsPartOfGeneralBalance, opt => opt.Condition(src => src.IsPartOfGeneralBalance.HasValue))
                .ForMember(dest => dest.CurrencyId, opt => opt.Condition(src => src.CurrencyId.HasValue))
                .ForMember(dest => dest.BudgetId, opt => opt.Condition(src => src.BudgetId.HasValue));
            CreateMap<Wallet, WalletDetailsBusinessModel>();
            CreateMap<Currency, CurrencyRelatedToWalletDetailsBusinessModel>();
            CreateMap<Budget, BudgetRelatedToWalletDetailsBusinessModel>();
            CreateMap<Transaction, TransactionRelatedToWalletDetailsBusinessModel>();

            // Currencies
            CreateMap<Currency, CurrencyDetailsBusinessModel>();

            // Budgets
            CreateMap<BudgetUpsertBusinessModel, Budget>()
                .ForMember(dest => dest.CurrencyId, opt => opt.Condition(src => src.CurrencyId.HasValue))
                .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null));
            CreateMap<Budget, BudgetDetailsBusinessModel>();
            CreateMap<Currency, CurrencyRelatedToBudgetDetailsBusinessModel>();
            CreateMap<Wallet, WalletRelatedToBudgetDetailsBusinessModel>();

            // Goal
            CreateMap<GoalUpsertBusinessModel, Goal>()
                .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
                .ForMember(dest => dest.Type, opt => opt.Condition(src => src.Type.HasValue))
                .ForMember(dest => dest.PlannedMoneyAmount, opt => opt.Condition(src => src.PlannedMoneyAmount.HasValue))
                .ForMember(dest => dest.StartDate, opt => opt.Condition(src => src.StartDate.HasValue))
                .ForMember(dest => dest.EndDate, opt => opt.Condition(src => src.EndDate.HasValue))
                .ForMember(dest => dest.Categories, opt =>
                {
                    opt.PreCondition(src => src.CategoryIds != null && src.CategoryIds.Count > 0);
                    opt.MapFrom(src => new List<Category>(src.CategoryIds!.Count));
                })
                .ForMember(dest => dest.Wallets, opt =>
                {
                    opt.PreCondition(src => src.WalletIds != null && src.WalletIds.Count > 0);
                    opt.MapFrom(src => new List<Wallet>(src.WalletIds!.Count));
                });
            CreateMap<Goal, GoalDetailsBusinessModel>();
            CreateMap<Category, CategoryRelatedToGoalDetailsBusinessModel>();
            CreateMap<Wallet, WalletRelatedToGoalDetailsBusinessModel>();
        }
    }
}
