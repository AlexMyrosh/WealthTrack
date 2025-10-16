using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;

namespace WealthTrack.Client.Services.Implementations;

public class SyncService(HttpClient httpClient, AppDbContext dbContext) : ISyncService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);
    private CancellationTokenSource? _cts;

    public void Start()
    {
        if (_cts is { IsCancellationRequested: false })
        {
            return;
        }

        _cts = new CancellationTokenSource();
        Task.Run(async () =>
        {
            await SyncCurrenciesAsync();
            var timer = new PeriodicTimer(_interval);
            try
            {
                while (await timer.WaitForNextTickAsync(_cts.Token))
                {
                    try
                    {
                        await SyncCurrenciesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Currency sync failed: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Currency sync stopped.");
            }
            finally
            {
                timer.Dispose();
            }
        }, _cts.Token);
    }
    
    public void Stop()
    {
        if (_cts is { IsCancellationRequested: false })
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private async Task SyncCurrenciesAsync()
    {
        var response = await httpClient.GetFromJsonAsync<List<Currency>>("/api/sync/currency");
        if (response == null)
        {
            return;
        }

        foreach (var currency in response)
        {
            var existing = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == currency.Id);
            if (existing is null)
            {
                dbContext.Currencies.Add(currency);
            }
            else
            {
                dbContext.Entry(existing).CurrentValues.SetValues(currency);
            }
        }

        await dbContext.SaveChangesAsync();
        Console.WriteLine($"[{DateTime.Now}] Synced {response.Count} currencies.");
    }
}