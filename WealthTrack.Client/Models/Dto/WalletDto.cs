namespace WealthTrack.Client.Models.Dto;

public class WalletDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public decimal Balance { get; set; }

    public bool IsPartOfGeneralBalance { get; set; }

    public CurrencyDto Currency { get; set; }
    
    public override string ToString() => $"{Name} (Balance: {Balance:0:00} {Currency.Symbol})";
}