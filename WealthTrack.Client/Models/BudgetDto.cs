namespace WealthTrack.Client.Models;

public class BudgetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal OverallBalance { get; set; }
    public Guid CurrencyId { get; set; }
    public string Status { get; set; } = string.Empty; // или enum, если удобно
}