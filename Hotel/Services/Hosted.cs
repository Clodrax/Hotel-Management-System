using Hangfire;
using Hotel.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class RecurringJobHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public RecurringJobHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
            recurringJobManager.AddOrUpdate(
                "CheckSalaryStatus",
                () => scope.ServiceProvider.GetRequiredService<IEmployeeService>().CheckAndUpdateSalaryStatus().Wait(),
                Cron.Daily);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
