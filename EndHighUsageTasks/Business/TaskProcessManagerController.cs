using EndHighUsageTasks.Helper;
using EndHighUsageTasks.Models;
using System.ComponentModel;
using System.Diagnostics;

namespace EndHighUsageTasks.Business;

/// <summary>
/// Manages and monitors task-related processes. 
/// Provides functionality to retrieve application pool names and terminate processes exceeding memory limits.
/// </summary>
public class TaskProcessManagerController(ILogger<TaskProcessManagerController> logger, CommunicationService communicationService)
{
    private readonly ILogger<TaskProcessManagerController> _logger = logger;
    private readonly CommunicationService _communicationService = communicationService;

    /// <summary>
    /// Monitors and terminates processes that exceed their defined memory limits.
    /// </summary>
    /// <param name="tasks">A collection of task definitions, including process names and memory limits.</param>
    /// <remarks>
    /// This method retrieves all running processes and compares their memory usage with the defined limits.
    /// If a process exceeds the limit, it is terminated, and an email notification is sent.
    /// </remarks>
    public void MonitorAndKillProcesses(IEnumerable<TaskModel> tasks)
    {
        try
        {
            // Get a list of all currently running processes.
            var processes = Process.GetProcesses();

            // Iterate over each task definition to monitor processes
            foreach (var task in tasks)
            {
                if (task == null) continue;

                // Find processes that match the task name (ignoring case and spaces)
                var matchingProcesses = processes
                    .Where(x => x.ProcessName.ToLower().Replace(" ", "").Contains(task.Name?.ToLower() ?? ""))
                    .ToList();

                // Iterate over matching processes to check memory usage
                foreach (var process in matchingProcesses)
                {
                    try
                    {
                        if (!process.HasExited) // Ensure the process is still running
                        {
                            // Calculate memory usage in megabytes
                            double workingSetMB = Math.Round(process.WorkingSet64 / 1048576.0, 2); // Physical memory usage
                            double privateMemoryMB = Math.Round(process.PrivateMemorySize64 / 1048576.0, 2); // Private memory usage
                            double totalMemoryMB = workingSetMB + privateMemoryMB; // Total memory usage

                            // Convert the task's max size limit from MB to bytes
                            long maxSizeBytes = task.MaxSizeInMB * 1024 * 1024;

                            // Check if the process exceeds the memory limit
                            if (process.WorkingSet64 + process.PrivateMemorySize64 > maxSizeBytes)
                            {
                                _logger.LogWarning("Task {ProcessName} exceeded memory limit: {totalMemoryMB} MB. Terminating...", process.ProcessName, totalMemoryMB);

                                // Terminate the process
                                process.Kill();

                                // Email notification details
                                string emailHtmlBody = @"
                                    <!DOCTYPE html>
                                    <html lang='en'>
                                    <head>
                                        <meta charset='UTF-8'>
                                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                        <title>Process Termination Notification</title>
                                        <style>
                                            body {
                                                font-family: Arial, sans-serif;
                                                line-height: 1.6;
                                                color: #333;
                                                margin: 0;
                                                padding: 0;
                                                background-color: #f4f4f9;
                                            }
                                            .email-container {
                                                max-width: 600px;
                                                margin: 20px auto;
                                                background: #fff;
                                                padding: 20px;
                                                border: 1px solid #ddd;
                                                border-radius: 5px;
                                                box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                                            }
                                            .email-header {
                                                background: #007bff;
                                                color: #fff;
                                                padding: 10px;
                                                text-align: center;
                                                border-radius: 5px 5px 0 0;
                                            }
                                            .email-body {
                                                padding: 20px;
                                            }
                                            .email-footer {
                                                font-size: 0.9em;
                                                color: #555;
                                                margin-top: 20px;
                                                border-top: 1px solid #ddd;
                                                padding-top: 10px;
                                            }
                                            .highlight {
                                                font-weight: bold;
                                                color: #d9534f;
                                            }
                                            a {
                                                color: #007bff;
                                                text-decoration: none;
                                            }
                                        </style>
                                    </head>
                                    <body>
                                        <div class='email-container'>
                                            <div class='email-header'>
                                                <h1>Process Termination Notification</h1>
                                            </div>
                                            <div class='email-body'>
                                                <p>Dear User,</p>
                                                <p>The process <span class='highlight'>{{ProcessName}}</span> was terminated because it exceeded the defined memory limit of <span class='highlight'>{{MemoryLimit}} MB</span>.</p>
                                                <p><strong>Details:</strong></p>
                                                <ul>
                                                    <li><strong>Process Name:</strong> {{ProcessName}}</li>
                                                    <li><strong>Termination Time:</strong> {{TerminationTime}}</li>
                                                </ul>
                                                <p>If you believe this action was taken in error or need further assistance, please contact the IT support team.</p>
                                            </div>
                                            <div class='email-footer'>
                                                <p>Best regards,<br>The Monitoring System Team</p>
                                            </div>
                                        </div>
                                    </body>
                                    </html>";

                                //string body = $"Process {process.ProcessName} was terminated for exceeding the memory limit of {task.MaxSizeInMB} MB.";
                                emailHtmlBody = emailHtmlBody
                                    .Replace("{{ProcessName}}", process.ProcessName)
                                    .Replace("{{MemoryLimit}}", task.MaxSizeInMB.ToString())
                                    .Replace("{{TerminationTime}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                                // Send a notification email
                                bool isEmailSent = _communicationService.SendEmail(emailHtmlBody);
                                if (isEmailSent)
                                    _logger.LogInformation("Email sent successfully");
                                else
                                    _logger.LogInformation("Failed to send email");
                            }
                        }
                    }
                    catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
                    {
                        // Handle cases where the process cannot be accessed due to insufficient permissions
                        _logger.LogError(ex, "Access denied for process {ProcessName} (ID: {ProcessId})", process.ProcessName, process.Id);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Handle cases where the process has already exited
                        _logger.LogError(ex, "Process {ProcessName} (ID: {ProcessId}) has already exited", process.ProcessName, process.Id);
                    }
                    catch (Exception ex)
                    {
                        // Log unexpected errors
                        _logger.LogError(ex, "Error monitoring process {ProcessName} (ID: {ProcessId})", process.ProcessName, process.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log any errors encountered during execution.
            _logger.LogError(ex, "An error occurred during execution");
        }
    }

    /// <summary>
    /// Retrieves the application pool name associated with a process ID using the `appcmd` command-line tool.
    /// </summary>
    /// <param name="processId">The ID of the process to retrieve the application pool name for.</param>
    /// <returns>The application pool name as a string, or null if the retrieval fails.</returns>
    /// <remarks>
    /// This method uses the `appcmd` command available in IIS to list the worker process details.
    /// It is specifically designed for applications hosted on IIS.
    /// </remarks>
    public string? GetAppPoolNameFromAppCmd(int processId)
    {
        try
        {
            // Prepare the command to retrieve app pool name for the given process ID
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c %windir%\\system32\\inetsrv\\appcmd.exe list wp /PID:{processId}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Start the process to execute the command
            using var process = Process.Start(startInfo);
            if (process == null)
                return null;

            // Read the standard output of the process to retrieve the app pool name
            string result = process.StandardOutput.ReadToEnd();
            string? appPoolName = result.Split(["APPPOOL"], StringSplitOptions.None).LastOrDefault()?.Trim();

            return appPoolName;
        }
        catch (Exception ex)
        {
            // Log any error that occurs during the operation
            _logger.LogError(ex, "Error retrieving application pool name for process ID {ProcessId}", processId);
            return null;
        }
    }
}
