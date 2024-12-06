using EndHighUsageTasks;
using EndHighUsageTasks.Business;
using EndHighUsageTasks.Helper;
using EndHighUsageTasks.Helpers;
using Serilog;
using Serilog.Events;

internal class Program
{
    /// <summary>
    /// The entry point of the application.
    /// Initializes logging and configuration, builds the host, and runs the service.
    /// </summary>
    private static async Task Main(string[] args)
    {
        // Initial bootstrap logger for logging errors before appsettings is loaded
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console() // Logs to the console
            .WriteTo.File("D:\\Logs\\EndHighUsageTasks\\InitialLogs\\Logs-.txt", rollingInterval: RollingInterval.Day) // Logs to a bootstrap file
            .CreateBootstrapLogger();

        try
        {
            // Initialize configuration from appsettings files and environment variables
            var configuration = BuildConfiguration();

            // Reconfigure the logger with the loaded configuration
            InitializeLogger(configuration);

            Log.Information("Starting application...");

            // Build and run the host
            var host = CreateHostBuilder(args, configuration).Build();
            Log.Information("Service successfully started at {Timestamp}", DateTime.Now);

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            // Log critical error and terminate the application
            Log.Fatal(ex, "There was a problem starting the service");
        }
        finally
        {
            // Ensure all logs are flushed before application shutdown
            Log.Information("Service stopped at {Timestamp}", DateTime.Now);
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Builds the application configuration by reading from multiple sources, 
    /// including appsettings.json, environment-specific settings, machine-specific settings, and environment variables.
    /// </summary>
    /// <returns>The built <see cref="IConfiguration"/> instance.</returns>
    private static IConfiguration BuildConfiguration()
    {
        // Determine the environment (default to "Production" if not set)
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        // Build the configuration from multiple sources
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) // Set the base path for configuration files
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Load base appsettings
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true) // Load environment-specific appsettings
            .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true, reloadOnChange: true) // Load machine-specific appsettings
            .AddEnvironmentVariables() // Add environment variables
            .Build();

        Console.WriteLine($"Environment detected: {environment}"); // Log detected environment
        return configuration;
    }

    /// <summary>
    /// Initializes the logger using the provided configuration.
    /// Configures the logging system to respect settings in the appsettings files.
    /// </summary>
    /// <param name="configuration">The configuration to use for logger setup.</param>
    private static void InitializeLogger(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration) // Read Serilog configuration from appsettings
            .CreateLogger();

        Log.Information("Logger initialized from configuration.");
    }

    /// <summary>
    /// Creates the host builder and configures the application.
    /// Registers services, logging, and application-specific configurations.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="configuration">The configuration object.</param>
    /// <returns>An <see cref="IHostBuilder"/> instance.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
        Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = "EndHighUsageTasks"; // Set the service name
            })
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                // Add configuration to the host context
                config.AddConfiguration(configuration);
            })
            .UseSerilog((context, services, serilogConfiguration) =>
            {
                // Configure Serilog for the host
                serilogConfiguration
                    .ReadFrom.Configuration(configuration) // Use the provided configuration
                    .ReadFrom.Services(services) // Include services for context enrichment
                    .Enrich.FromLogContext(); // Add log context enrichment
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Log environment and machine details for troubleshooting
                Log.Information("Environment: {EnvironmentName} / Machine: {MachineName}",
                                hostContext.HostingEnvironment.EnvironmentName,
                                Environment.MachineName);

                // Register application-specific services
                services.AddSingleton<CommunicationService>();
                services.AddSingleton<ReadConfigUtility>();
                services.AddScoped<TaskProcessManagerController>();
                services.AddHostedService<Worker>(); // Add the background worker as a hosted service
            });
}
