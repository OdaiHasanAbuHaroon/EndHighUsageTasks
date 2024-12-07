# EndHighUsageTasks

1- Process Monitoring:
  - Retrieves a list of system processes and evaluates their memory usage against predefined thresholds.
  - Matches processes by name and checks both physical and private memory usage.
  
2- Automatic Process Termination:
  - Terminates processes that exceed the specified memory limits.
  - Provides detailed logging to capture termination events and their reasons.
  
3- Email Notifications:
  - Sends email alerts for terminated processes, ensuring administrators are informed about critical actions.
  - Utilizes an SMTP-based email service with configurable settings, such as sender email, recipient email, subject, and server details.
  
4- Configuration Management:
  - Supports dynamic configuration through appsettings files, environment variables, and machine-specific overrides.
  - Utilizes caching to optimize the retrieval of configuration data, improving performance.
  
5- Background Worker Task:
  - Runs as a scheduled task with configurable intervals, ensuring regular process monitoring and resource management.
  
6- Application Pool Identification:
  - Retrieves IIS application pool names associated with process IDs for enhanced tracking, specifically for IIS-hosted environments.
  
7- Logging and Error Handling:
  - Comprehensive logging for all operations using Serilog, including process information, errors, and email status.
  - Implements robust error handling to manage permission issues, unexpected exits, and exceptions.
