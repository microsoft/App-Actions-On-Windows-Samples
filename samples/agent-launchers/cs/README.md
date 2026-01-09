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

### MVVM Pattern

This sample follows the **Model-View-ViewModel (MVVM)** architectural pattern using **CommunityToolkit.Mvvm**:

- **Models**: Define data structures (`RegistrationStatus`, `LauncherInfo`)
- **ViewModels**: Handle business logic and state management
  - `MainWindowViewModel`: Central application logic and orchestration
  - `InvocationInfoViewModel`: Manages protocol invocation display data
  - `RegistrationStatusPanelViewModel`: Manages collection of registration status items
  - `RegistrationStatusItemViewModel`: Represents individual agent registration state with toggle commands
- **Views**: XAML UI components with data binding
  - `MainWindow`: Main application window
  - `InvocationInfoPanel`: Displays protocol activation details
  - `RegistrationStatusPanel`: Shows registration status and provides registration controls

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
├── Controls/
│   ├── InvocationInfoPanel.xaml/cs      # Control for displaying protocol activation details
│   └── RegistrationStatusPanel.xaml/cs  # Control for displaying registration status
├── Models/
│   └── RegistrationStatus.cs     # Enum for registration states
├── ViewModels/
│   ├── InvocationInfoViewModel.cs            # ViewModel for invocation info display
│   ├── RegistrationStatusPanelViewModel.cs   # ViewModel for status panel
│   └── RegistrationStatusItemViewModel.cs    # ViewModel for individual status items
├── Properties/                   # Project properties
├── Strings/                      # Localization resources
├── SampleAgentLauncher.csproj
├── Package.appxmanifest          # Main manifest template
├── App.xaml / App.xaml.cs        # Application entry point and protocol handling
├── MainWindow.xaml / MainWindow.xaml.cs      # Main UI shell
├── MainWindowViewModel.cs        # Main ViewModel with business logic
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

## Custom Controls

The application includes reusable custom controls that encapsulate UI functionality:

### InvocationInfoPanel

Displays protocol activation details in a card-based layout:

```csharp
public sealed partial class InvocationInfoPanel : UserControl
{
    public InvocationInfoViewModel ViewModel
    {
        get => (InvocationInfoViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
    
    public static readonly DependencyProperty ViewModelProperty = 
        DependencyProperty.Register(nameof(ViewModel), typeof(InvocationInfoViewModel), 
            typeof(InvocationInfoPanel), new PropertyMetadata(null));
}
```

**Features**:
- Automatically appears when protocol activation occurs
- Displays: Action name, Agent name, Prompt, and Full URI
- Card-style design with theme-aware styling
- Text selection enabled for all fields

### RegistrationStatusPanel

Displays a collection of registration status items with interactive controls:

```csharp
public sealed partial class RegistrationStatusPanel : UserControl
{
    public RegistrationStatusPanelViewModel ViewModel
    {
        get => (RegistrationStatusPanelViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
    
    public static readonly DependencyProperty ViewModelProperty = 
        DependencyProperty.Register(nameof(ViewModel), typeof(RegistrationStatusPanelViewModel), 
            typeof(RegistrationStatusPanel), new PropertyMetadata(null));
}
```

**Features**:
- Displays list of agents with color-coded status
- Each item can have optional Register/Unregister button
- Automatically updates when status changes
- Uses `ItemsControl` with data template for flexible rendering

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

private bool HandleProtocolActivation(ProtocolActivatedEventArgs protocolArgs)
{
    // Ensure window is created and activated
    if (_window == null)
    {
        _window = new MainWindow();
    }

    // Pass the protocol URI to the main window
    if (_window is MainWindow mainWindow)
    {
        mainWindow.HandleProtocolActivation(protocolArgs.Uri);
    }

    _window.Activate();
    return true;
}
```

### MainWindow.xaml.cs - Delegating to ViewModel
The MainWindow acts as a thin shell, delegating protocol handling to the ViewModel:

```csharp
public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow()
    {
        ViewModel = new MainWindowViewModel();
        this.InitializeComponent();
        this.Title = "Sample Agent Launcher";
        
        if (this.AppWindow != null)
        {
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 700));
        }
    }

    public void HandleProtocolActivation(Uri uri)
    {
        ViewModel.HandleProtocolActivation(uri);
    }

    // Converter function for x:Bind
    public Visibility BoolToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;
}
```

### MainWindowViewModel - URI Processing
The ViewModel parses the incoming URI and updates UI state accordingly:

```csharp
public void HandleProtocolActivation(Uri uri)
{
    AppendOutput($"Protocol activation received: {uri}");
    
    string host = uri.Host;
    string query = uri.Query;
        
    if (host.Equals("processAgentPrompt", StringComparison.OrdinalIgnoreCase))
    {
        var queryParams = HttpUtility.ParseQueryString(query);
        string agentName = queryParams["agentName"] ?? "Unknown";
        string prompt = queryParams["prompt"] ?? "No prompt provided";
            
        AppendOutput($"Action: processAgentPrompt");
        AppendOutput($"Agent Name: {agentName}");
        AppendOutput($"Prompt: {prompt}");
            
        ShowInvocationInfo(uri, "processAgentPrompt", agentName, prompt);
        ProcessAgentPrompt(agentName, prompt);
    }
    else
    {
        AppendOutput($"Unknown action: {host}");
        ShowInvocationInfo(uri, host, "N/A", "N/A");
    }
}

private void ShowInvocationInfo(Uri uri, string action, string agentName, string prompt)
{
    InvocationInfo.ShowInvocation(uri, action, agentName, prompt);
}
```

## Dynamic Agent Management

The application provides a modern, reactive UI for managing agent registrations using the MVVM pattern:

### Registration Status Panel

The application displays real-time registration status for both static and dynamic agents:

- **Static Agent** (`static-sample-agent-launcher`): Read-only status display registered via app manifest
- **Dynamic Agent** (`sample-agent-launcher`): Interactive status with Register/Unregister button

### RegistrationStatusItemViewModel

Each agent's registration state is managed by a dedicated ViewModel:

```csharp
public partial class RegistrationStatusItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private RegistrationStatus _status;

    [ObservableProperty]
    private bool _hasButton;

    [ObservableProperty]
    private bool _buttonEnabled = false;

    // Status Text: Shows formatted status like "sample-agent-launcher: Registered ✓"
    public string StatusText { get; }
    
    // Status Foreground: Green for Registered, Orange for Not Registered, Gray for Checking/Unknown
    public SolidColorBrush StatusForeground { get; }
    
    // Button Content: "Register" or "Unregister" based on current status
    public string ButtonContent { get; }

    [RelayCommand]
    private async Task ToggleRegistrationAsync() { ... }
}
```

### Registration States

The `RegistrationStatus` enum defines possible states:

- **Checking**: Status is being queried
- **Registered**: Agent is registered in ODR
- **NotRegistered**: Agent is not registered
- **Unknown**: Status could not be determined

### Agent Launcher Management

The `MainWindowViewModel` manages agent operations through the ODR CLI:

#### Add Agent Launcher
```csharp
private async Task AddLauncherAsync()
{
    string? agentJsonPath = GetAgentJsonPath();
    CommandResult result = await RunOdrCommandAsync("agent-launchers add", agentJsonPath);
    // Updates status and refreshes UI
}
```

#### Remove Agent Launcher
```csharp
private async Task RemoveLauncherAsync()
{
    CommandResult result = await RunOdrCommandAsync("agent-launchers remove", AgentId);
    // Updates status and refreshes UI
}
```

#### Check Registration Status
```csharp
private async Task CheckRegistrationStatusAsync()
{
    CommandResult result = await RunOdrCommandAsync("agent-launchers list", string.Empty);
    
    // Parses output to determine registration state
    bool isRegistered = result.Output.Contains(AgentId);
    bool isStaticRegistered = result.Output.Contains(StaticAgentName);
    
    _dynamicStatusItem.Status = isRegistered ? RegistrationStatus.Registered : RegistrationStatus.NotRegistered;
    _staticStatusItem.Status = isStaticRegistered ? RegistrationStatus.Registered : RegistrationStatus.NotRegistered;
}
```

### Command Execution
Commands are executed asynchronously using `Process.Start()` with proper output/error redirection and logging:

```csharp
private async Task<CommandResult> RunOdrCommandAsync(string command, string argument)
{
    ProcessStartInfo startInfo = new ProcessStartInfo
    {
        FileName = GetODRCommand(), // Retrieved from registry
        Arguments = string.IsNullOrEmpty(argument) ? command : $"{command} \"{argument}\"",
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

### ODR Command Discovery

The ODR command path is retrieved dynamically from the Windows Registry:

```csharp
private static string GetODRCommand()
{
    using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Mcp", false);
    OdrCommand = key?.GetValue("Command") as string;
    
    if (string.IsNullOrEmpty(OdrCommand))
    {
        throw new InvalidOperationException("ODR command not found in registry.");
    }
    
    return OdrCommand;
}
```

## Logging and Output Management

The application implements a comprehensive logging system through the `AppendOutput` method in the `MainWindowViewModel`:

### Output Features
- **Timestamping**: All log entries include `HH:mm:ss` timestamps for easy debugging
- **Smart Text Management**: Automatically replaces placeholder text with actual log content
- **Persistent Output**: Log entries accumulate in the ViewModel's `OutputText` property
- **MVVM Binding**: UI updates automatically through data binding
- **Formatted Display**: Uses Cascadia Code monospace font for consistent formatting

### Usage Pattern
```csharp
public void AppendOutput(string text)
{
    string timestamp = DateTime.Now.ToString("HH:mm:ss");
    string currentOutput = OutputText;
    
    if (currentOutput == "Application output will appear here...")
    {
        currentOutput = string.Empty;
    }
    
    OutputText = currentOutput + (string.IsNullOrEmpty(currentOutput) ? "" : "\n") + 
                 $"[{timestamp}] {text}";
}
```

All major operations (protocol activation, CLI commands, status checks, errors) are logged through this system for comprehensive traceability.

## Agent Launcher List Display

The application displays registered agent launchers in a table format with live updates:

### LauncherInfo Model
```csharp
public record LauncherInfo(string Name, string DisplayName, string ActionId);
```

### List Management in MainWindowViewModel

```csharp
[ObservableProperty]
private ObservableCollection<LauncherInfo> _launchers = new();

[ObservableProperty]
private bool _hasLaunchers = false;

[ObservableProperty]
private string _launchersMessage = "Loading...";

[ObservableProperty]
private bool _showLaunchersMessage = true;

private async Task ListAndDisplayLaunchersAsync()
{
    CommandResult result = await RunOdrCommandAsync("agent-launchers list", string.Empty);
    
    if (result.ExitCode == 0 && !string.IsNullOrEmpty(result.Output))
    {
        ParseAndDisplayLaunchers(result.Output);
    }
    else
    {
        DisplayNoLaunchers();
    }
}

private void ParseAndDisplayLaunchers(string output)
{
    Launchers.Clear();
    
    using JsonDocument doc = JsonDocument.Parse(output);
    JsonElement root = doc.RootElement;
    
    if (root.ValueKind == JsonValueKind.Array)
    {
        foreach (JsonElement launcher in root.EnumerateArray())
        {
            string name = launcher.TryGetProperty("name", out JsonElement nameElem) 
                ? nameElem.GetString() ?? "N/A" : "N/A";
            string displayName = launcher.TryGetProperty("display_name", out JsonElement displayNameElem) 
                ? displayNameElem.GetString() ?? "N/A" : "N/A";
            string actionId = launcher.TryGetProperty("action_id", out JsonElement actionIdElem) 
                ? actionIdElem.GetString() ?? "N/A" : "N/A";
            
            Launchers.Add(new LauncherInfo(name, displayName, actionId));
        }
        
        HasLaunchers = Launchers.Count > 0;
        ShowLaunchersMessage = !HasLaunchers;
    }
}
```

### Table Display in XAML

The launcher list is displayed in an expandable table using `ItemsControl`:

```xaml
<Expander Header="Registered Agent Launchers" IsExpanded="True">
    <ScrollViewer MaxHeight="200">
        <Grid>
            <!-- Header Row with Name, Display Name, Action ID columns -->
            <ItemsControl ItemsSource="{x:Bind ViewModel.Launchers, Mode=OneWay}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate x:DataType="local:LauncherInfo">
                        <Grid>
                            <TextBlock Text="{x:Bind Name}" />
                            <TextBlock Text="{x:Bind DisplayName}" />
                            <TextBlock Text="{x:Bind ActionId}" FontFamily="Cascadia Code" />
                        </Grid>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </Grid>
    </ScrollViewer>
</Expander>
```

## User Interface

The application uses WinUI 3 with a modern, clean design following Fluent Design principles:

### Layout Structure
The UI is organized in a vertical grid layout with the following sections:

1. **Header** (Row 0): Large title "Sample Agent Launcher" (24pt, bold, centered)
2. **Invocation Info Panel** (Row 1): Card-based display showing details of the most recent protocol activation
   - Displayed via `InvocationInfoPanel` custom control
   - Shows: Action, Agent Name, Prompt, and Full URI
   - Bound to `InvocationInfoViewModel`
3. **Registration Status Panel** (Row 2): Shows registration status for static and dynamic agents
   - Displayed via `RegistrationStatusPanel` custom control
   - Each item shows colored status indicator and action button (for dynamic agent)
   - Bound to `RegistrationStatusPanelViewModel`
4. **Status Bar** (Row 3): Error/status messages displayed as needed
5. **Launcher List** (Row 3): Expandable table showing all registered agent launchers
   - Three columns: Name, Display Name, Action ID
   - Scrollable view with max height of 200px
   - Shows "Loading..." or "No agent launchers registered" when appropriate
6. **Debug Output** (Row 5): Expandable console with timestamped debug output
   - Collapsible expander control
   - Fixed height (300px) with scrolling
   - Monospace font (Cascadia Code) for log clarity

### Visual Design Features
- **Mica Backdrop**: Modern translucent window background
- **Card-based Layout**: Information panels use card styling with borders and rounded corners
- **Theme Resources**: Uses system theme colors for consistent appearance
- **Fluent Controls**: Uses WinUI 3 Expander, ScrollViewer, and other Fluent controls
- **Color-coded Status**: Registration status uses semantic colors (Green = Registered, Orange = Not Registered, Gray = Checking/Unknown)

### Window Properties
- **Initial Size**: 800x700 pixels
- **Title**: "Sample Agent Launcher"
- **Responsive Design**: Uses Grid layout with auto-sizing rows and flexible content areas

### Data Binding
All UI elements use `x:Bind` for compiled bindings to ViewModels:

```xaml
<controls:InvocationInfoPanel ViewModel="{x:Bind ViewModel.InvocationInfo, Mode=OneWay}"/>
<controls:RegistrationStatusPanel ViewModel="{x:Bind ViewModel.RegistrationStatusPanel, Mode=OneWay}"/>
<ItemsControl ItemsSource="{x:Bind ViewModel.Launchers, Mode=OneWay}"/>
<TextBlock Text="{x:Bind ViewModel.OutputText, Mode=OneWay}"/>
```

## Testing the Sample

1. **Build and Deploy**: Build the project and deploy the MSIX package
2. **Verify Initial State**: 
   - Launch the app normally to see the management UI
   - Static agent should show as "Registered ✓" (green)
   - Dynamic agent should show status with Register/Unregister button
3. **Test Agent Registration**:
   - Click the Register/Unregister button on the dynamic agent
   - Observe real-time status updates in the Registration Status Panel
   - Check the "Registered Agent Launchers" table for changes
4. **Test Protocol Activation**: Use URIs like:
```
sampleagentlauncher://processAgentPrompt?agentName=TestAgent&prompt=Hello%20World
sampleagentlauncher://processAgentPrompt?agentName=MyAgent&prompt=Process%20this%20text
```
5. **Verify Invocation Display**: 
   - The Invocation Info Panel should appear at the top showing action details
   - Debug output should log the activation with timestamps
6. **Verify Static Registration**: The static agent (`static-sample-agent-launcher`) is automatically registered when the app is installed

## Requirements

- Windows 10/11 with App Actions support (minimum version 10.0.26100.0)
- ODR (On Device Registry) installed and registered in Windows Registry at `HKLM\Software\Microsoft\Windows\CurrentVersion\Mcp`
- WinUI 3 runtime
- .NET 8
- Microsoft WindowsAppSDK
- CommunityToolkit.Mvvm for MVVM pattern support

## Implementation Notes

- **MVVM Architecture**: Uses CommunityToolkit.Mvvm with `ObservableObject`, `ObservableProperty`, and `RelayCommand` attributes for clean separation of concerns
- **Reactive UI**: All UI updates are driven by data binding to observable ViewModels
- **Custom Controls**: Reusable `InvocationInfoPanel` and `RegistrationStatusPanel` controls encapsulate UI logic
- **AOT Compatibility**: The sample follows AOT-compatible patterns as specified in the coding guidelines
- **Explicit Types**: Uses explicit types instead of `var` throughout the codebase
- **Error Handling**: Implements comprehensive error handling with proper try-catch blocks and CLI output validation
- **Timestamped Logging**: All output includes `HH:mm:ss` timestamps via the `AppendOutput` method
- **Protocol Safety**: Uses `System.Web.HttpUtility.ParseQueryString` for safe URI parameter parsing
- **Modern Activation**: Uses `Microsoft.Windows.AppLifecycle` for modern WinUI 3 activation handling with fallback support
- **COM Exception Handling**: Includes proper COM exception handling for protocol activation scenarios
- **Dual Registration**: Demonstrates both static (manifest-based) and dynamic (CLI-based) agent registration
- **Registry-based ODR Discovery**: Dynamically retrieves ODR command path from Windows Registry instead of hardcoding
- **Async/Await Pattern**: All I/O operations use proper async/await for responsive UI
- **Resource Management**: Uses proper `using` statements for process disposal and async resource cleanup
- **JSON Parsing**: Uses `System.Text.Json` for parsing ODR command output
- **Record Types**: Uses C# record types for immutable data models (`CommandResult`, `LauncherInfo`)
- **Compiled Bindings**: Uses `x:Bind` throughout XAML for better performance than traditional `Binding`
- **Theme Integration**: Uses theme resources for consistent appearance across light/dark themes

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
- **CommunityToolkit.Mvvm**: Provides MVVM infrastructure (`ObservableObject`, `ObservableProperty`, `RelayCommand`)
- **System.CommandLine**: For CLI operations (follows AOT-compatible project guidelines)
- **System.Text.Json**: For parsing ODR JSON output

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
