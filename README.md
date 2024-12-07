# 🛠️ EndHighUsageTasks Windows Service

## 📋 Table of Contents
- [📖 Overview](#-overview)
- [✨ Key Features](#-key-features)
- [🧩 Design Patterns Used](#-design-patterns-used)
- [🏗️ Project Architecture](#-project-architecture)
- [🚀 How to Use](#-how-to-use)

---

## 📖 Overview
The **EndHighUsageTasks** Windows Service is built using .NET Core 8 and is designed to monitor and manage system processes to prevent memory overutilization. It runs as a background service, periodically evaluating processes and taking actions to ensure system stability and resource efficiency.

---

## ✨ Key Features
- **🖥️ Process Monitoring**: Tracks system processes and evaluates their memory usage against predefined thresholds.
- **⛔ Automatic Termination**: Stops processes exceeding the allowed memory limits and logs the event.
- **📧 Email Notifications**: Sends alerts to administrators when a process is terminated.
- **⚙️ Dynamic Configuration**: Reads configurations from `appsettings.json`, environment variables, and machine-specific settings, with caching for better performance.
- **🔁 Background Worker**: Executes tasks at regular intervals, with the duration configurable in the appsettings.
- **📂 Application Pool Management**: Identifies IIS application pools associated with process IDs for enhanced tracking.
- **📜 Robust Logging**: Uses Serilog for detailed logging, including errors, warnings, and informational messages.

---

## 🧩 Design Patterns Used

This project implements several design patterns to ensure maintainability, scalability, and optimized performance:

### 💡 Dependency Injection
Used throughout the service to inject dependencies like `ILogger` and `IConfiguration` into constructors, promoting modularity and testability.

### 🔒 Singleton Pattern
Applied to services like `ReadConfigUtility` and `CommunicationService` to ensure a single instance exists during the application's lifecycle, reducing memory usage.

### 🏭 Factory Pattern
Used in the `Worker` class via `IServiceScopeFactory` to create scoped service instances dynamically for each task execution.

### 🖇️ Template Method Pattern
Implemented by extending the `BackgroundService` base class, allowing the `Worker` class to customize the logic while reusing the core lifecycle management.

### 🛠️ Builder Pattern
Used in `Program.cs` to configure the host and logger using fluent APIs, providing a clear and modular setup for services.

### 👀 Observer Pattern
Used with `ILogger` to capture log events. The logger observes and handles log messages without coupling tightly with the business logic.

### 🔌 Adapter Pattern
Implemented in classes like `CommunicationService` and `ReadConfigUtility` to adapt external dependencies (e.g., SMTP email, configuration) to the service's needs.

### 🗂️ Caching Pattern
Employed in `ReadConfigUtility` to cache frequently accessed configuration values, minimizing redundant reads and improving performance.

### 🎯 Strategy Pattern
Encapsulates email-sending logic in the `CommunicationService`, allowing easy extension or modification of communication methods without affecting other components.

---

## 🏗️ Project Architecture

### Core Components
- **🔄 `Worker.cs`**: The background service that periodically runs tasks.
- **⚡ `TaskProcessManagerController.cs`**: Manages process monitoring and termination.
- **📬 `CommunicationService.cs`**: Handles email notifications.
- **📂 `ReadConfigUtility.cs`**: Reads configuration data and caches it for better performance.
- **🛠️ `Utility.cs`**: Provides helper methods for common tasks like file handling and JSON serialization.
- **📋 `TaskModel.cs`**: Represents the configuration for each task (process name and memory limit).
- **📧 `EmailSettings.cs`**: Stores SMTP settings for email communication.

---

## 🚀 How to Use

Follow these steps to set up and run the **EndHighUsageTasks** Windows Service:

1. **📂 Clone the Repository**
   - Clone the repository to your local machine:
     ```bash
     git clone https://github.com/YourUsername/EndHighUsageTasks.git
     cd EndHighUsageTasks
     ```

2. **⚙️ Configure the Settings**
   - Open the `appsettings.json` file.
   - Define the tasks by specifying process names and memory limits under the `TaskNameSizeList` key.
   - Configure the email settings for notifications, such as SMTP server, sender, and recipient details.

3. **🛠️ Build the Service**
   - Build the service using Visual Studio or the .NET CLI:
     ```bash
     dotnet publish -c Release
     ```

4. **🔧 Install the Service**
   - Open a terminal as Administrator.
   - Use the `sc` command to install the service:
     ```bash
     sc create "EndHighUsageTasks" binPath= "<path-to-published-executable>"
     ```
   - Start the service:
     ```bash
     sc start "EndHighUsageTasks"
     ```

5. **📜 Monitor Logs**
   - Logs are generated in the directory configured in the `appsettings.json` file (e.g., `D:\Logs\EndHighUsageTasks\`).
   - Use the logs to monitor process management activities and troubleshoot issues.

6. **🔁 Test the Service**
   - Verify the service is running and correctly monitoring processes.
   - Confirm that email notifications are sent for terminated processes and all configurations are correctly applied.

---

Feel free to modify the content to suit your project's unique setup and requirements!
