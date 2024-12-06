using EndHighUsageTasks.Business;
using EndHighUsageTasks.Helpers;
using EndHighUsageTasks.Models;

namespace EndHighUsageTasks;

/// <summary>
/// The Worker class is a background service that runs a periodic task based on a configurable interval.
/// It reads configuration settings, processes tasks, and monitors and kills high-memory processes.
/// </summary>
public class Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ReadConfigUtility readConfigUtility) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ReadConfigUtility _readConfigUtility = readConfigUtility;

    /// <summary>
    /// Executes the main task in a loop, running it at intervals defined by configuration.
    /// Reads the task list from configuration, monitors processes, and terminates those exceeding memory limits.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to signal stopping the service.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Read the duration between task executions from configuration, defaulting to 10 minutes if not set.
        long checkDurationInMinutes = _configuration.GetValue("CheckDurationInMinutes", 10);
        TimeSpan delayDuration = TimeSpan.FromMinutes(checkDurationInMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

            try
            {
                // Retrieve the task list from the configuration.
                List<TaskModel> taskList = _readConfigUtility.ReadConfigListByKey<TaskModel>("TaskNameSizeList");

                // Create a scoped service provider to resolve services with a scoped lifetime.
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    // Resolve TaskProcessManagerController from the scoped provider.
                    var taskProcessManagerController = scope.ServiceProvider.GetRequiredService<TaskProcessManagerController>();

                    // Monitor and terminate high-memory processes based on the task list.
                    taskProcessManagerController.MonitorAndKillProcesses(taskList);
                }

                // Log the next run delay duration.
                _logger.LogInformation("Next run in: {Minutes} minutes", delayDuration.Minutes);

                // Wait for the configured delay duration before running the next iteration.
                await Task.Delay(delayDuration, stoppingToken);
            }
            catch (Exception ex)
            {
                // Log any errors encountered during execution.
                _logger.LogError(ex, "An error occurred during execution");
            }
        }
    }
}
