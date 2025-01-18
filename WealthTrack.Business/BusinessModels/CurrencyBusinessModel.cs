using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels
{
    public class CurrencyBusinessModel
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public CurrencyStatus Status { get; set; }
    }
}