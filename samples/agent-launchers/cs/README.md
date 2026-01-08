# Sample Agent Launcher

This sample demonstrates how to create a packaged WinUI 3 application that registers an Agent Launcher with URI action support. The application includes both static registration (via app manifest) and dynamic agent management capabilities using the ODR (On Device Registry) CLI.

## Features

This sample implements:

1. **App Actions with URI Invocation**: Registers an action that accepts text inputs (`agentName` and `prompt`)
2. **Protocol Activation**: Handles URI scheme activation when the app is launched via the `sampleagentlauncher://` protocol
3. **Agent Management UI**: Provides buttons to dynamically add/remove agents and agent launchers using the ODR CLI (`odr.exe`)
4. **Output Display**: Shows CLI command output and protocol activation details in the application UI with timestamped logging
5. **Static and Dynamic Registration**: Demonstrates both static registration via manifest and dynamic registration via ODR CLI

## Architecture

### Action Definition

The sample registers an action using the v3 action definition format:

- **ID**: `SampleAgentLauncher.ProcessAgentPrompt`
- **URI**: `sampleagentlauncher://processAgentPrompt?agentName=${agentName}&prompt=${prompt}`
- **Inputs**: 
  - `agentName` (Text) - The name of the agent to use
  - `prompt` (Text) - The prompt to process
- **Input Combinations**: Supports the combination of `agentName` and `prompt` inputs

### App Manifest Registrations

The application registers several extensions in its manifest:

1. **Protocol Registration**: Registers the `sampleagentlauncher` URI scheme for protocol activation
2. **Action Definition**: Registers the action definition via `com.microsoft.windows.ai.actions` extension
3. **Static Agent Registration**: Registers a static agent via `com.microsoft.windows.ai.appAgent` extension
4. **App Execution Alias**: Provides `SampleAgentLauncher.exe` as an execution alias

### Files Structure

```
samples/agent-launchers/cs/
├── Assets/
│   ├── actionDefinition.json     # Action definition (v3 format)
│   ├── agent.json                # Agent configuration for dynamic registration
│   ├── agentStatic.json          # Agent configuration for static registration
│   └── *.png                     # Application icons
├── Properties/                   # Project properties
├── Strings/                      # Localization resources
├── SampleAgentLauncher.csproj
├── Package.appxmanifest          # Main manifest template
├── App.xaml / App.xaml.cs        # Application entry point and protocol handling
├── MainWindow.xaml / MainWindow.xaml.cs   # Main UI and agent management
└── app.manifest                  # Application manifest for compatibility
```

## Action Definition Details

The `actionDefinition.json` file uses the v3 action definition format:

```json
{
  "version": 3,
  "actions": [
    {
      "id": "SampleAgentLauncher.ProcessAgentPrompt",
      "icon": "ms-resource://Files/Assets/Square44x44Logo.png",
      "description": "Processes a prompt with an agent",
      "usesGenerativeAI": false,
      "inputs": [
        {
          "name": "agentName",
          "kind": "Text"
        },
        {
          "name": "prompt", 
          "kind": "Text"
        }
      ],
      "outputs": [],
      "allowedAppInvokers": ["*"],
      "invocation": {
        "type": "Uri",
        "uri": "sampleagentlauncher://processAgentPrompt?agentName=${agentName}&prompt=${prompt}"
      },
      "inputCombinations": [
        {
          "inputs": ["agentName", "prompt"],
          "description": "Process a prompt with the specified agent"
        }
      ]
    }
  ]
}
```

## Agent Registration

### Static Agent (`agentStatic.json`)
Registered via the app manifest and installed with the application:

```json
{
  "manifest_version": "0.1.0",
  "version": "1.0.0",
  "name": "static-sample-agent-launcher",
  "display_name": "Sample Agent Launcher (Registered Statically)",
  "description": "A sample agent launcher that demonstrates URI action integration and was registered statically.",
  "icon": "ms-resource://Files/Assets/StoreLogo.png",
  "action_id": "SampleAgentLauncher.ProcessAgentPrompt"
}
```

### Dynamic Agent (`agent.json`)
Used for runtime registration via ODR CLI:

```json
{
  "manifest_version": "0.1.0",
  "version": "1.0.0", 
  "name": "sample-agent-launcher",
  "display_name": "Sample Agent Launcher",
  "description": "A sample agent launcher that demonstrates URI action integration",
  "icon": "ms-resource://Files/Assets/StoreLogo.png",
  "action_id": "SampleAgentLauncher.ProcessAgentPrompt"
}
```

## Protocol Activation Handling

The application uses the modern WinUI 3 activation pattern with `Microsoft.Windows.AppLifecycle`:

### App.xaml.cs - Activation Handling
```csharp
protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
{
    bool protocolHandled = TryHandleProtocolActivation();
    
    if (!protocolHandled)
    {
        // Normal application launch
        _window = new MainWindow();
        _window.Activate();
    }
}

[DynamicWindowsRuntimeCast(typeof(ProtocolActivatedEventArgs))]
private bool TryHandleProtocolActivation()
{
    try
    {
        AppActivationArguments activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

        if (activationArgs.Kind == ExtendedActivationKind.Protocol)
        {
            if (activationArgs.Data is ProtocolActivatedEventArgs protocolArgs)
            {
                return HandleProtocolActivation(protocolArgs);
            }
        }
    }
    catch (System.Runtime.InteropServices.COMException)
    {
        // If protocol activation fails, fall back to normal launch
    }

    return false;
}
```

### MainWindow.xaml.cs - URI Processing
The `MainWindow.HandleProtocolActivation` method parses the incoming URI and extracts parameters using `HttpUtility.ParseQueryString()`:

```csharp
public void HandleProtocolActivation(Uri uri)
{
    AppendOutput($"Protocol activation received: {uri}");
    
    // Parse the URI and extract parameters
    string scheme = uri.Scheme; // should be "sampleagentlauncher"
    string host = uri.Host; // should be "processAgentPrompt"
    string query = uri.Query;
        
    if (host.Equals("processAgentPrompt", StringComparison.OrdinalIgnoreCase))
    {
        // Parse query parameters
        System.Collections.Specialized.NameValueCollection queryParams = 
            HttpUtility.ParseQueryString(query);
            
        string agentName = queryParams["agentName"] ?? "Unknown";
        string prompt = queryParams["prompt"] ?? "No prompt provided";
            
        AppendOutput($"Action: processAgentPrompt");
        AppendOutput($"Agent Name: {agentName}");
        AppendOutput($"Prompt: {prompt}");
            
        // Process the action
        ProcessAgentPrompt(agentName, prompt);
    }
    else
    {
        AppendOutput($"Unknown action: {host}");
    }
}
```

## Dynamic Agent Management

The application provides buttons for managing agent registrations:

### Agent Launcher Management
- **Add Agent Launcher**: Calls `odr.exe agent-launchers add "path\to\agent.json"`
- **Remove Agent Launcher**: Calls `odr.exe agent-launchers remove` with the full ID
- **List Agent Launchers**: Calls `odr.exe agent-launchers list` to show registered launchers

### App Agent Management (Legacy)
- **Add App Agent**: Calls `odr.exe add-app-agent "path\to\agent.json"`
- **Remove App Agent**: Calls `odr.exe remove-app-agent`
- **List App Agents**: Calls `odr.exe list-app-agents`

All commands use a common execution helper:

### Command Execution
Commands are executed asynchronously using `Process.Start()` with proper output/error redirection and logging:

```csharp
private async Task<CommandResult> RunOdrCommandAsync(string command, string argument)
{
    ProcessStartInfo startInfo = new ProcessStartInfo
    {
        FileName = "odr.exe",
        Arguments = $"{command} \"{argument}\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };

    AppendOutput($"Executing: {startInfo.FileName} {startInfo.Arguments}");

    using Process process = new Process { StartInfo = startInfo };
    process.Start();

    string output = await process.StandardOutput.ReadToEndAsync();
    string error = await process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync();

    return new CommandResult(process.ExitCode, output, error);
}

private record CommandResult(int ExitCode, string Output, string Error);
```

## Logging and Output Management

The application implements a comprehensive logging system through the `AppendOutput` method:

### Output Features
- **Timestamping**: All log entries include `HH:mm:ss` timestamps for easy debugging
- **Smart Text Management**: Automatically replaces placeholder text with actual log content
- **Persistent Output**: Log entries accumulate in the UI for session-long visibility
- **Formatted Display**: Uses Consolas monospace font for consistent formatting

### Usage Pattern
```csharp
private void AppendOutput(string text)
{
    string timestamp = DateTime.Now.ToString("HH:mm:ss");
    string currentOutput = OutputTextBlock.Text;
    
    if (currentOutput == "Application output will appear here...")
    {
        currentOutput = string.Empty;
    }
    
    OutputTextBlock.Text = currentOutput + (string.IsNullOrEmpty(currentOutput) ? "" : "\n") + 
                          $"[{timestamp}] {text}";
}
```

All major operations (protocol activation, CLI commands, errors) are logged through this system for comprehensive traceability.

## User Interface

The application uses WinUI 3 with a clean, functional design organized in a vertical grid layout:

### Layout Structure
- **Header** (Grid Row 0): Large title text "Sample Agent Launcher" (24pt, bold, centered)
- **Button Panel** (Grid Row 1): Stack panels with action buttons:
  - "App Agents" section with Add/Remove/List buttons
  - "Agent Launchers" section with Add/Remove/List buttons
- **Status Bar** (Grid Row 2): Displays current operation status with real-time updates (semi-bold text)
- **Output Console** (Grid Row 3): Scrollable text area with timestamped output

### Window Properties
- **Initial Size**: Automatically resizes to 800x600 pixels
- **Title**: "Sample Agent Launcher"
- **Responsive Design**: Uses Grid layout with auto-sizing and flexible content area

## Testing the Sample

1. **Build and Deploy**: Build the project and deploy the MSIX package
2. **Test Regular Launch**: Launch the app normally to see the management UI
3. **Test Protocol Activation**: Use URIs like:
```
sampleagentlauncher://processAgentPrompt?agentName=TestAgent&prompt=Hello%20World
sampleagentlauncher://processAgentPrompt?agentName=MyAgent&prompt=Process%20this%20text
```
4. **Test Agent Management**: Use the "Add Agent Launcher" and "Remove Agent Launcher" buttons to interact with ODR
5. **Verify Static Registration**: The static agent is automatically registered when the app is installed

## Requirements

- Windows 10/11 with App Actions support (minimum version 10.0.26100.0)
- ODR (On Device Registry) installed and accessible via `odr.exe`
- WinUI 3 runtime
- .NET 8
- Microsoft WindowsAppSDK

## Implementation Notes

- **AOT Compatibility**: The sample follows AOT-compatible patterns as specified in the coding guidelines
- **Explicit Types**: Uses explicit types instead of `var` throughout the codebase
- **Error Handling**: Implements comprehensive error handling with proper try-catch blocks and CLI output validation
- **Timestamped Logging**: All output includes `HH:mm:ss` timestamps via the `AppendOutput` method
- **Protocol Safety**: Uses `System.Web.HttpUtility.ParseQueryString` for safe URI parameter parsing
- **Modern Activation**: Uses `Microsoft.Windows.AppLifecycle` for modern WinUI 3 activation handling with fallback support
- **COM Exception Handling**: Includes proper COM exception handling for protocol activation scenarios
- **Dual Registration**: Demonstrates both static (manifest-based) and dynamic (CLI-based) agent registration
- **UI State Management**: Properly manages button states during async operations to prevent concurrent executions
- **Resource Management**: Uses proper `using` statements for process disposal and async resource cleanup
- **WinRT Attributes**: Uses `[DynamicWindowsRuntimeCast]` attribute for proper COM interop with protocol activation

## Project Configuration

The project is configured as:
- **Output Type**: WinExe (Windows executable)
- **Framework**: .NET 8
- **UseWinUI**: Enabled for WinUI 3 support
- **AOT Enabled**: `<PublishAot>true</PublishAot>` for Native AOT compilation
- **Self-Contained WindowsAppSDK**: `<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>`
- **MSIX Tooling**: `<EnableMsixTooling>true</EnableMsixTooling>` for proper packaging
- **UES Project Type**: ComponentTestApp for integration with the build system
- **App Fragment**: Uses UES_AppxFragmentTemplate for manifest generation

## Key Dependencies

- **Microsoft.WindowsAppSDK**: Core WinUI 3 and Windows runtime functionality
- **System.CommandLine**: For CLI operations (follows AOT-compatible project guidelines)
- **Microsoft.Web.WebView2**: WebView2 runtime support

## Asset Files

The project includes several key asset files that are copied to the output directory:
- `actionDefinition.json` - Defines the app action specification
- `agent.json` - Configuration for dynamic agent registration
- `agentStatic.json` - Configuration for static agent registration

## Related Documentation

- [Windows App Actions Documentation](https://learn.microsoft.com/en-us/windows/ai/app-actions/)
- [App Extension Registration](https://learn.microsoft.com/en-us/uwp/schemas/appxpackage/uapmanifestschema/element-uap3-appextension-manual)
- [Protocol Activation](https://learn.microsoft.com/en-us/windows/uwp/launch-resume/handle-uri-activation)
- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [Microsoft.Windows.AppLifecycle](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/applifecycle/applifecycle-instancing)
