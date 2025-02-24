# Vault Debug

Vault Debug is a comprehensive logging solution for Unity projects. It provides a robust runtime logging system with a lightweight dependency injection container, efficient log pooling, and an interactive Unity Editor Console for real-time log management.

---

## Features

- **Robust Logging System:**  
  Log messages at various levels (Info, Debug, Warn, Error, Exception) to help you monitor and debug your application.
  
- **Lightweight Dependency Injection:**  
  Uses a simple DI container (via the `DIBootstrapper`) to register and resolve core components, making it easy to extend or replace dependencies.
  
- **Efficient Log Pooling:**  
  Recycles log objects using a log pool to reduce garbage collection overhead and improve performance.
  
- **Editor Console Integration:**  
  Includes a custom Unity Editor window that displays logs, supports filtering by log level, provides search capabilities, and allows log exporting.
  
- **Persistent Log Storage:**  
  Saves and loads logs using JSON serialization, enabling you to persist log history between sessions.
  
- **Main Thread Dispatcher:**  
  Ensures that log processing and UI updates occur on Unity's main thread.

---

## Installation

1. **Download the Package:**  
   Download the Vault Debug Unity package from your preferred source or repository.

2. **Import the Package:**  
   In Unity, go to **Assets > Import Package > Custom Package…** and select the Vault Debug package file.

3. **Setup Complete:**  
   Once imported, Vault Debug will automatically initialize its core systems through the `DIBootstrapper`.

---

## Getting Started

### Initializing the Logger

Vault Debug uses a lightweight DI container to automatically register core dependencies. To start logging, simply create a new logger instance with your desired context:

```csharp
using VaultDebug.Runtime.Logger;

public class ExampleUsage : MonoBehaviour
{
    void Start()
    {
        // Create a logger for the current context.
        var logger = new VaultLogger("ExampleContext");
        
        // Log an informational message.
        logger.Info("Vault Debug initialized successfully.");
    }
}
```
## Using the Editor Console
Vault Debug includes an interactive console for viewing and managing logs within the Unity Editor:

- **Open the Console:**
Navigate to Vault Debug > Console > Open Window in the Unity Editor menu.

- **Filtering & Searching:**
Use the filter buttons to display logs by level (Info, Debug, Warn, Error). The search field supports text filtering and context-specific searches.
   - **Currently supported filters**
      - `@context:` followed by a string, will show only logs pertaining to a specific context. Example:  `@context:"TestContext"`

- **Exporting Logs:**
Click the Export button to save the current logs to a text file.

- **Clearing Logs:**
Use the Clear button to remove all logs from the current session.

## Editor Settings
Access additional settings via Vault Debug > Settings to configure options such as:

- Automatically opening the console on startup.
- Specifying the log export path.
- Resetting editor preferences.

## API Overview
- **DIBootstrapper:**
Initializes the dependency injection container and registers core dependencies (e.g., log pool, dispatcher, logger provider).

- **VaultLogger:**
The primary class used for logging messages. Offers methods like Info(), Debug(), Warn(), and Error().

- **VaultLogPool:**
Manages a pool of reusable log objects to optimize memory usage.

- **VaultLogDispatcher:**
Dispatches log messages to registered handlers, ensuring that logs are processed by all interested components.

- **VaultDebugLoggerMainThreadDispatcher:**
Facilitates the execution of log processing actions on Unity's main thread.

- **LoggerProvider:**
Provides logger instances for various contexts.

- **Vault Console Editor:**
A custom Unity Editor window that displays logs, supports filtering, and offers features like log export and clearing.

- **Persistent Log Storage:**
Uses JSON serialization (powered by Newtonsoft.Json) to save and load logs via the EditorFileLogStorageService.

## Customization
Vault Debug is designed to be extensible:

- **Custom Log Handlers:**
Implement the IVaultLogHandler interface to create custom log processing behaviors.

- **Extend the DI Container:**
Register additional dependencies or replace existing ones using the built-in DI container.

- **Customize the Editor Console:**
Modify the UI and behavior of the Vault Console Editor to suit your project's needs.

## Known  Issues
- `Assertion failed` generates aVisual issues in the Vault Console when displaying logs while UIToolkit windows are open

## Dependencies
- **Newtonsoft.Json:**
For JSON serialization and deserialization of log data.

- **UnityEngine & UnityEditor:**
Core Unity modules used for runtime functionality and editor integration.

## Contributing
Contributions are welcome! If you have ideas, improvements, or bug fixes, please submit a pull request or open an issue in the project's repository.

## License
MIT License

