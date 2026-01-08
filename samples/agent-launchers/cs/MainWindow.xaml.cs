// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace SampleAgentLauncher;

public sealed partial class MainWindow : Window
{
    private const string AgentId = "AppAssociatedAgent_SampleAgentLauncher_s9y1p3hwd5qda_sample-agent-launcher";
    private const string StaticAgentName = "static-sample-agent-launcher";
    private bool _isRegistered = false;
    private bool _isStaticRegistered = false;

    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "Sample Agent Launcher";

        // Set initial window size
        if (this.AppWindow != null)
        {
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 700));
        }

        AppendOutput("Application started. Ready to manage agent registration.");
        
        // Check registration status and list launchers on startup
        _ = RefreshStatusAndListAsync();
    }

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

            // Display invocation info in the UI
            ShowInvocationInfo(uri, "processAgentPrompt", agentName, prompt);

            // Process the action
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
        InvocationInfoPanel.Visibility = Visibility.Visible;
        InvocationUriText.Text = uri.ToString();
        InvocationActionText.Text = action;
        InvocationAgentNameText.Text = agentName;
        InvocationPromptText.Text = prompt;
    }

    private void ProcessAgentPrompt(string agentName, string prompt)
    {
        AppendOutput($"Processing prompt '{prompt}' with agent '{agentName}'");

        // Simulate processing
        AppendOutput("Processing complete. Response: Hello from the sample agent launcher!");
    }

    private async Task RefreshStatusAndListAsync()
    {
        await CheckRegistrationStatusAsync();
        await ListAndDisplayLaunchersAsync();
    }

    private async Task CheckRegistrationStatusAsync()
    {
        ToggleDynamicLauncherButton.IsEnabled = false;
        DynamicRegistrationStatusText.Text = "sample-agent-launcher: Checking...";
        StaticRegistrationStatusText.Text = "static-sample-agent-launcher: Checking...";
        
        AppendOutput("Checking registration status...");
        
        CommandResult result = await RunOdrCommandAsync("agent-launchers list", string.Empty);
        
        if (result.ExitCode == 0)
        {
            // Check if our agents are in the list
            _isRegistered = !string.IsNullOrEmpty(result.Output) && result.Output.Contains(AgentId);
            _isStaticRegistered = !string.IsNullOrEmpty(result.Output) && result.Output.Contains(StaticAgentName);
            
            UpdateRegistrationUI();
        }
        else
        {
            AppendOutput($"Error checking status: {result.Error}");
            DynamicRegistrationStatusText.Text = "sample-agent-launcher: Unknown";
            StaticRegistrationStatusText.Text = "static-sample-agent-launcher: Unknown";
        }
        
        ToggleDynamicLauncherButton.IsEnabled = true;
    }

    private void UpdateRegistrationUI()
    {
        if (_isRegistered)
        {
            DynamicRegistrationStatusText.Text = "sample-agent-launcher: Registered ✓";
            DynamicRegistrationStatusText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
            ToggleDynamicLauncherButton.Content = "Unregister";
        }
        else
        {
            DynamicRegistrationStatusText.Text = "sample-agent-launcher: Not Registered";
            DynamicRegistrationStatusText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Orange);
            ToggleDynamicLauncherButton.Content = "Register";
        }

        if (_isStaticRegistered)
        {
            StaticRegistrationStatusText.Text = "static-sample-agent-launcher: Registered ✓";
            StaticRegistrationStatusText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
        }
        else
        {
            StaticRegistrationStatusText.Text = "static-sample-agent-launcher: Not Registered";
            StaticRegistrationStatusText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Orange);
        }
    }

    private async void ToggleDynamicLauncherButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isRegistered)
        {
            await RemoveLauncherAsync();
        }
        else
        {
            await AddLauncherAsync();
        }
        
        await RefreshStatusAndListAsync();
    }

    private async Task AddLauncherAsync()
    {
        ToggleDynamicLauncherButton.IsEnabled = false;
        ClearStatus();

        string? agentJsonPath = GetAgentJsonPath();
        if (agentJsonPath == null)
        {
            AppendOutput("Error: Could not locate agent.json file");
            SetStatus("Error: agent.json not found");
            ToggleDynamicLauncherButton.IsEnabled = true;
            return;
        }

        AppendOutput($"Registering agent launcher using: {agentJsonPath}");

        CommandResult result = await RunOdrCommandAsync("agent-launchers add", agentJsonPath);

        AppendOutput($"Add Agent Launcher Command Output:");
        AppendOutput($"Exit Code: {result.ExitCode}");
        if (!string.IsNullOrEmpty(result.Output))
        {
            AppendOutput($"Output: {result.Output}");
        }

        if (!string.IsNullOrEmpty(result.Error))
        {
            AppendOutput($"Error: {result.Error}");
        }

        if (result.ExitCode != 0)
        {
            SetStatus($"Failed to add agent launcher (exit code: {result.ExitCode})");
        }

        ToggleDynamicLauncherButton.IsEnabled = true;
    }

    private async Task RemoveLauncherAsync()
    {
        ToggleDynamicLauncherButton.IsEnabled = false;
        ClearStatus();

        AppendOutput($"Unregistering agent launcher with ID: {AgentId}");

        CommandResult result = await RunOdrCommandAsync("agent-launchers remove", AgentId);

        AppendOutput($"Remove Agent Launcher Command Output:");
        AppendOutput($"Exit Code: {result.ExitCode}");
        if (!string.IsNullOrEmpty(result.Output))
        {
            AppendOutput($"Output: {result.Output}");
        }

        if (!string.IsNullOrEmpty(result.Error))
        {
            AppendOutput($"Error: {result.Error}");
        }

        if (result.ExitCode != 0)
        {
            SetStatus($"Failed to remove agent launcher (exit code: {result.ExitCode})");
        }

        ToggleDynamicLauncherButton.IsEnabled = true;
    }

    private async Task ListAndDisplayLaunchersAsync()
    {
        AppendOutput("Retrieving list of agent launchers...");

        CommandResult result = await RunOdrCommandAsync("agent-launchers list", string.Empty);

        AppendOutput("List Agent Launchers Command Output:");
        AppendOutput($"Exit Code: {result.ExitCode}");

        if (result.ExitCode == 0)
        {
            if (!string.IsNullOrEmpty(result.Output))
            {
                AppendOutput("--- Registered Agent Launchers ---");
                AppendOutput(result.Output);
                
                ParseAndDisplayLaunchers(result.Output);
            }
            else
            {
                AppendOutput("No agent launchers registered");
                DisplayNoLaunchers();
            }
        }
        else
        {
            AppendOutput($"Error: {result.Error}");
            DisplayError("Failed to retrieve launcher information");
        }
    }

    private void ParseAndDisplayLaunchers(string output)
    {
        LaunchersDataPanel.Children.Clear();

        try
        {
            // The output is JSON - parse it
            using JsonDocument doc = JsonDocument.Parse(output);
            JsonElement root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                int rowIndex = 0;
                foreach (JsonElement launcher in root.EnumerateArray())
                {
                    string name = launcher.TryGetProperty("name", out JsonElement nameElem) ? nameElem.GetString() ?? "N/A" : "N/A";
                    string displayName = launcher.TryGetProperty("display_name", out JsonElement displayNameElem) ? displayNameElem.GetString() ?? "N/A" : "N/A";
                    string actionId = launcher.TryGetProperty("action_id", out JsonElement actionIdElem) ? actionIdElem.GetString() ?? "N/A" : "N/A";

                    AddLauncherRow(name, displayName, actionId, rowIndex % 2 == 0);
                    rowIndex++;
                }

                if (rowIndex == 0)
                {
                    DisplayNoLaunchers();
                }
            }
            else
            {
                DisplayNoLaunchers();
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, try to display as plain text
            DisplayPlainTextLaunchers(output);
        }
    }

    private void AddLauncherRow(string name, string displayName, string actionId, bool isEvenRow)
    {
        Grid rowGrid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 4)
        };

        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

        if (isEvenRow)
        {
            if (Application.Current.Resources.TryGetValue("SubtleFillColorSecondaryBrush", out object subtleBackground))
            {
                rowGrid.Background = subtleBackground as SolidColorBrush;
            }
        }

        TextBlock nameText = new TextBlock
        {
            Text = name,
            Padding = new Thickness(8, 4, 8, 4),
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true
        };
        if (Application.Current.Resources.TryGetValue("TextFillColorPrimaryBrush", out object primaryForeground))
        {
            nameText.Foreground = primaryForeground as SolidColorBrush;
        }
        Grid.SetColumn(nameText, 0);

        TextBlock displayNameText = new TextBlock
        {
            Text = displayName,
            Padding = new Thickness(8, 4, 8, 4),
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true
        };
        if (Application.Current.Resources.TryGetValue("TextFillColorPrimaryBrush", out object primaryForeground2))
        {
            displayNameText.Foreground = primaryForeground2 as SolidColorBrush;
        }
        Grid.SetColumn(displayNameText, 1);

        TextBlock actionIdText = new TextBlock
        {
            Text = actionId,
            Padding = new Thickness(8, 4, 8, 4),
            TextWrapping = TextWrapping.Wrap,
            FontFamily = new FontFamily("Cascadia Code"),
            FontSize = 11,
            IsTextSelectionEnabled = true
        };
        if (Application.Current.Resources.TryGetValue("TextFillColorSecondaryBrush", out object secondaryForeground))
        {
            actionIdText.Foreground = secondaryForeground as SolidColorBrush;
        }
        Grid.SetColumn(actionIdText, 2);

        rowGrid.Children.Add(nameText);
        rowGrid.Children.Add(displayNameText);
        rowGrid.Children.Add(actionIdText);

        LaunchersDataPanel.Children.Add(rowGrid);
    }

    private void DisplayNoLaunchers()
    {
        LaunchersDataPanel.Children.Clear();
        TextBlock noDataText = new TextBlock
        {
            Text = "No agent launchers registered",
            FontStyle = Windows.UI.Text.FontStyle.Italic,
            Padding = new Thickness(8)
        };
        if (Application.Current.Resources.TryGetValue("TextFillColorSecondaryBrush", out object secondaryForeground))
        {
            noDataText.Foreground = secondaryForeground as SolidColorBrush;
        }
        LaunchersDataPanel.Children.Add(noDataText);
    }

    private void DisplayPlainTextLaunchers(string output)
    {
        LaunchersDataPanel.Children.Clear();
        TextBlock plainTextBlock = new TextBlock
        {
            Text = output,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = new FontFamily("Cascadia Code"),
            FontSize = 11,
            Padding = new Thickness(8),
            IsTextSelectionEnabled = true
        };
        if (Application.Current.Resources.TryGetValue("TextFillColorPrimaryBrush", out object primaryForeground))
        {
            plainTextBlock.Foreground = primaryForeground as SolidColorBrush;
        }
        LaunchersDataPanel.Children.Add(plainTextBlock);
    }

    private void DisplayError(string errorMessage)
    {
        LaunchersDataPanel.Children.Clear();
        TextBlock errorText = new TextBlock
        {
            Text = errorMessage,
            FontStyle = Windows.UI.Text.FontStyle.Italic,
            Padding = new Thickness(8)
        };
        if (Application.Current.Resources.TryGetValue("SystemFillColorCriticalBrush", out object criticalForeground))
        {
            errorText.Foreground = criticalForeground as SolidColorBrush;
        }
        LaunchersDataPanel.Children.Add(errorText);
    }

    private string? GetAgentJsonPath()
    {
        // First try the Assets folder relative to the executable
        string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets", "agent.json");
        if (File.Exists(assetsPath))
        {
            return assetsPath;
        }

        // Try the package installation folder
        Windows.ApplicationModel.Package package = Windows.ApplicationModel.Package.Current;
        string packagePath = Path.Combine(package.InstalledLocation.Path, "Assets", "agent.json");
        if (File.Exists(packagePath))
        {
            return packagePath;
        }

        AppendOutput($"Checked paths:");
        AppendOutput($"  - {assetsPath}");
        AppendOutput($"  - {packagePath}");

        return null;
    }


    private static string? OdrCommand;

    private static string GetODRCommand()
    {
        if (string.IsNullOrEmpty(OdrCommand))
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Mcp", false);
            OdrCommand = key?.GetValue("Command") as string;
        }

        if (string.IsNullOrEmpty(OdrCommand))
        {
            throw new InvalidOperationException("ODR command not found in registry.");
        }

        return OdrCommand;
    }

    private async Task<CommandResult> RunOdrCommandAsync(string command, string argument)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = GetODRCommand(),
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

    private void SetStatus(string status)
    {
        StatusTextBlock.Text = status;
        StatusTextBlock.Visibility = Visibility.Visible;
    }

    private void ClearStatus()
    {
        StatusTextBlock.Text = string.Empty;
        StatusTextBlock.Visibility = Visibility.Collapsed;
    }

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

        // Force layout update before scrolling
        OutputTextBlock.UpdateLayout();

        // Scroll to the bottom to show the latest output
        // Use Dispatcher to ensure the scroll happens after the text has been fully rendered
        DispatcherQueue.TryEnqueue(() =>
        {
            OutputScrollViewer.ChangeView(null, OutputScrollViewer.ScrollableHeight, null, disableAnimation: false);
        });
    }

    private record CommandResult(int ExitCode, string Output, string Error);
}
