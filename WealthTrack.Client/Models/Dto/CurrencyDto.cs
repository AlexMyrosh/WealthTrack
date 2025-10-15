namespace WealthTrack.Client.Models.Dto;

public class CurrencyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public override string ToString() => $"{Name} ({Code})";
}