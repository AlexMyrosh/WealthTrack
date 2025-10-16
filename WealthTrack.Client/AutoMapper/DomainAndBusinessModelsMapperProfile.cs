using AutoMapper;
using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Business.BusinessModels.Currency;
using WealthTrack.Business.BusinessModels.Goal;
using WealthTrack.Business.BusinessModels.Transaction;
using WealthTrack.Business.BusinessModels.Wallet;
using WealthTrack.Business.Events.Models;
using WealthTrack.Client.Models.Dto;
using WealthTrack.Data.DomainModels;

namespace WealthTrack.Client.AutoMapper;

public class BusinessAndClientModelsMapperProfile : Profile
{
    public BusinessAndClientModelsMapperProfile()
    {
        // Category
        CreateMap<CurrencyDetailsBusinessModel, CurrencyDto>();
        CreateMap<CurrencyRelatedToWalletDetailsBusinessModel, CurrencyDto>();
        
        // Wallet
        CreateMap<WalletDetailsBusinessModel, WalletDto>();
    }
}