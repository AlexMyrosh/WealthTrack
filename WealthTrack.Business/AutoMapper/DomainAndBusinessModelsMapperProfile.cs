using AutoMapper;
using WealthTrack.Business.BusinessModels;
using WealthTrack.Data.DomainModels;

namespace WealthTrack.Business.AutoMapper
{
    public class DomainAndBusinessModelsMapperProfile : Profile
    {
        public DomainAndBusinessModelsMapperProfile()
        {
            CreateMap<CategoryBusinessModel, Category>().ReverseMap();
            CreateMap<CurrencyBusinessModel, Currency>().ReverseMap();
            CreateMap<TransactionBusinessModel, Transaction>().ReverseMap();
            CreateMap<WalletBusinessModel, Wallet>().ReverseMap();
        }
    }
}
