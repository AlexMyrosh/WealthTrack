using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Seeders
{
    public class CurrencySeeder(IUnitOfWork unitOfWork, HttpClient client, IConfiguration configuration)
    {
        private readonly string? BaseUrl = configuration["fxratesapi:BaseUrl"];
        private readonly string? Api_key = configuration["fxratesapi:ApiKey"];
        public async Task SeedCurrenciesAsync()
        {
            var existedCurrencies = await unitOfWork.CurrencyRepository.GetAllAsync(); 
            var predefinedCurrencies = new List<Currency>
            {
                // Fiat Currencies
                new()
                {
                    Id = new Guid("639c01c2-132a-48cf-8779-efef9358da21"),
                    Code = "USD",
                    Name = "US Dollar",
                    Symbol = "$",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat
                },
                new()
                {
                    Id = new Guid("2e8f06de-10f8-4b32-a382-bb1e5372a77d"),
                    Code = "EUR",
                    Name = "Euro",
                    Symbol = "€",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat
                },
                new()
                {
                    Id = new Guid("3ad62e0d-2a9f-4b6b-a2d8-2c1a93d24889"),
                    Code = "GBP",
                    Name = "British Pound Sterling",
                    Symbol = "£",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat
                },
                new()
                {
                    Id = new Guid("4a98f84d-1a60-4649-80e6-0e358912c17f"),
                    Code = "JPY",
                    Name = "Japanese Yen",
                    Symbol = "¥",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat
                },
                new()
                {
                    Id = new Guid("5412d7c8-8d78-4d82-8b2f-71b3a4bb22d1"),
                    Code = "AUD",
                    Name = "Australian Dollar",
                    Symbol = "A$",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat
                },
                new()
                {
                    Id = new Guid("6280a3de-3411-4ae6-8f95-231b70a72aaf"),
                    Code = "CAD",
                    Name = "Canadian Dollar",
                    Symbol = "C$",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat
                },
                new()
                {
                    Id = new Guid("7f8d72ae-c631-45a5-865b-01984bfb3e01"),
                    Code = "CHF",
                    Name = "Swiss Franc",
                    Symbol = "CHF",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat
                },
                new()
                {
                    Id = new Guid("8a9df4b6-17fa-4d85-8006-8c64d726f15f"),
                    Code = "CNY",
                    Name = "Chinese Yuan Renminbi",
                    Symbol = "¥",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat
                },
                new()
                {
                    Id = new Guid("96c7eaf2-36e1-43c5-b9ad-1c2e23f392c7"),
                    Code = "INR",
                    Name = "Indian Rupee",
                    Symbol = "₹",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat
                },
                new()
                {
                    Id = new Guid("a40f3fd1-8d1f-45b9-8004-9d3fa2927f32"),
                    Code = "UAH",
                    Name = "Ukrainian Hryvnia",
                    Symbol = "₴",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat
                },

                // Cryptocurrencies
                new()
                {
                    Id = new Guid("b60de8e6-daf9-4e2e-bcd6-9c9edc916da8"),
                    Code = "BTC",
                    Name = "Bitcoin",
                    Symbol = "₿",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Crypto
                },
                new()
                {
                    Id = new Guid("c77261fc-7458-4d6d-9f58-74c02e0de7b6"),
                    Code = "ETH",
                    Name = "Ethereum",
                    Symbol = "Ξ",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Crypto
                },
                new()
                {
                    Id = new Guid("d8d2c7f3-3f2a-44e4-bd91-f1089b9e77c7"),
                    Code = "USDT",
                    Name = "Tether",
                    Symbol = "T$",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Crypto
                },
                new()
                {
                    Id = new Guid("e96d7bc2-1e83-4d4d-87cd-124f6f3821c3"),
                    Code = "BNB",
                    Name = "Binance Coin",
                    Symbol = "BNB",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Crypto
                },
                new()
                {
                    Id = new Guid("f9b7a2df-435f-4e2f-b08a-4a09f8cf3f1d"),
                    Code = "XRP",
                    Name = "Ripple",
                    Symbol = "XRP",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Crypto
                },
                new()
                {
                    Id = new Guid("0b3cfc9f-fb67-4e12-bdc7-a8420df27e29"),
                    Code = "ADA",
                    Name = "Cardano",
                    Symbol = "ADA",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Crypto
                },
                new()
                {
                    Id = new Guid("2d4ebc9a-dc6e-40ad-a0fd-38249f4cfb32"),
                    Code = "SOL",
                    Name = "Solana",
                    Symbol = "SOL",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Crypto
                },
                new()
                {
                    Id = new Guid("3e51fc9c-6e4d-4f13-805b-9f482d12b731"),
                    Code = "DOT",
                    Name = "Polkadot",
                    Symbol = "DOT",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Crypto
                },
                new()
                {
                    Id = new Guid("4f62bc9b-7f5e-41cd-80ab-3f483f13c832"),
                    Code = "LTC",
                    Name = "Litecoin",
                    Symbol = "Ł",
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Crypto
                }
            };
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}latest?api-key={Api_key}");
            var response = await client.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();
            var rates = JsonConvert.DeserializeObject<Rates>(content).rates;
            if (rates is null)
            {
                throw new JsonException($"Unable to cast JSON response to {nameof(Rates)} model");
            }

            foreach (var currency in predefinedCurrencies)
            {
                if (!existedCurrencies.Any(c => c.Equals(currency)))
                {
                    var currencyCode = currency.Code == "USDT" ? "USD" : currency.Code;
                    var rateProperty = typeof(ExchangeRates).GetProperty(currencyCode);
                    if (rateProperty is null)
                    {
                        throw new InvalidCastException($"Unable to get exchange rate for {currency.Code}");
                    }

                    var exchangeRate = (decimal)(rateProperty.GetValue(rates) ?? 0);
                    currency.ExchangeRate = exchangeRate;
                    await unitOfWork.CurrencyRepository.CreateAsync(currency);
                }
            }

            await unitOfWork.SaveAsync();
        }
    }

    public class Rates
    {
        public ExchangeRates rates { get; set; }
    }

    public class ExchangeRates
    {
        public decimal AUD { get; set; }

        public decimal CAD { get; set; }

        public decimal CHF { get; set; }

        public decimal CNY { get; set; }

        public decimal EUR { get; set; }

        public decimal GBP { get; set; }

        public decimal INR { get; set; }

        public decimal JPY { get; set; }

        public decimal UAH { get; set; }

        public decimal USD { get; set; }

        public decimal BTC { get; set; }

        public decimal ETH { get; set; }

        public decimal USDT { get; set; }

        public decimal BNB { get; set; }

        public decimal XRP { get; set; }

        public decimal ADA { get; set; }

        public decimal SOL { get; set; }

        public decimal DOT { get; set; }

        public decimal LTC { get; set; }
    }
}
