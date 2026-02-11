// Copyright (C) Microsoft Corporation. All rights reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using SampleAgentLauncher.Helpers;
using SampleAgentLauncher.Models;
using SampleAgentLauncher.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace SampleAgentLauncher;

public partial class MainWindowViewModel : ObservableObject
{
    private const string AgentId = "SampleAgentLauncher_s9y1p3hwd5qda_sample-agent-launcher";
    private const string StaticAgentName = "static-sample-agent-launcher";

    private RegistrationStatusItemViewModel _staticStatusItem;
    private RegistrationStatusItemViewModel _dynamicStatusItem;

    public RegistrationStatusPanelViewModel RegistrationStatusPanel { get; } = new();

    [ObservableProperty]
    private string _outputText = "Application output will appear here...";

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private bool _statusVisible = false;

    public InvocationInfoViewModel InvocationInfo { get; } = new();

    [ObservableProperty]
    private ObservableCollection<LauncherInfo> _launchers = new();

    [ObservableProperty]
    private bool _hasLaunchers = false;

    [ObservableProperty]
    private string _launchersMessage = "Loading...";

    [ObservableProperty]
    private bool _showLaunchersMessage = true;

    public MainWindowViewModel()
    {
        // Create status items
        _staticStatusItem = new RegistrationStatusItemViewModel(StaticAgentName, hasButton: false);
        _dynamicStatusItem = new RegistrationStatusItemViewModel("sample-agent-launcher", hasButton: true, toggleAction: ToggleDynamicLauncherInternalAsync);
        
        RegistrationStatusPanel.AddStatusItem(_staticStatusItem);
        RegistrationStatusPanel.AddStatusItem(_dynamicStatusItem);

        AppendOutput("Application started. Ready to manage agent registration.");
        _ = RefreshStatusAndListAsync();
    }

    private async Task ToggleDynamicLauncherInternalAsync()
    {
        _dynamicStatusItem.ButtonEnabled = false;

        try
        {
            if (_dynamicStatusItem.Status == RegistrationStatus.Registered)
            {
                await RemoveLauncherAsync();
            }
            else
            {
                await AddLauncherAsync();
            }

            await RefreshStatusAndListAsync();
        }
        finally
        {
            _dynamicStatusItem.ButtonEnabled = true;
        }
    }

    public void HandleProtocolActivation(Uri uri)
    {
        AppendOutput($"Protocol activation received: {uri}");

        string host = uri.Host;
        string query = uri.Query;

        if (host.Equals("processAgentPrompt", StringComparison.OrdinalIgnoreCase))
        {
            var queryParams = HttpUtility.ParseQueryString(query);

            // multiple agents can be registered within a single app. the agentName parameter
            // is how to determine which agent is being invoked
            string agentName = queryParams["agentName"] ?? "Unknown";

            // This is the prompt provided by the user
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

    private void ProcessAgentPrompt(string agentName, string prompt)
    {
        AppendOutput($"Processing prompt '{prompt}' with agent '{agentName}'");
        AppendOutput("Processing complete. Response: Hello from the sample agent launcher!");
    }

    private async Task RefreshStatusAndListAsync()
    {
        await CheckRegistrationStatusAsync();
        await ListAndDisplayLaunchersAsync();
    }

    private async Task CheckRegistrationStatusAsync()
    {
        _dynamicStatusItem.ButtonEnabled = false;
        _staticStatusItem.Status = RegistrationStatus.Checking;
        _dynamicStatusItem.Status = RegistrationStatus.Checking;

        AppendOutput("Checking registration status...");

        OdrCommandResult result = await RunOdrCommandAsync("agent-info list", string.Empty);

        if (result.ExitCode == 0)
        {
            bool isRegistered = !string.IsNullOrEmpty(result.Output) && result.Output.Contains(AgentId);
            bool isStaticRegistered = !string.IsNullOrEmpty(result.Output) && result.Output.Contains(StaticAgentName);

            _dynamicStatusItem.Status = isRegistered ? RegistrationStatus.Registered : RegistrationStatus.NotRegistered;
            _staticStatusItem.Status = isStaticRegistered ? RegistrationStatus.Registered : RegistrationStatus.NotRegistered;
        }
        else
        {
            AppendOutput($"Error checking status: {result.Error}");
            _dynamicStatusItem.Status = RegistrationStatus.Unknown;
            _staticStatusItem.Status = RegistrationStatus.Unknown;
        }

        _dynamicStatusItem.ButtonEnabled = true;
    }

    private async Task AddLauncherAsync()
    {
        ClearStatus();

        string? agentJsonPath = GetAgentJsonPath();
        if (agentJsonPath == null)
        {
            AppendOutput("Error: Could not locate agent.json file");
            SetStatus("Error: agent.json not found");
            return;
        }

        AppendOutput($"Registering agent launcher using: {agentJsonPath}");

        OdrCommandResult result = await RunOdrCommandAsync("agent-info add", agentJsonPath);

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
    }

    private async Task RemoveLauncherAsync()
    {
        ClearStatus();

        string? agentJsonPath = GetAgentJsonPath();
        if (agentJsonPath == null)
        {
            AppendOutput("Error: Could not locate agent.json file");
            SetStatus("Error: agent.json not found");
            return;
        }

        AppendOutput($"Unregistering agent launcher {agentJsonPath}");

        OdrCommandResult result = await RunOdrCommandAsync("agent-info remove", agentJsonPath);

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
    }

    private async Task ListAndDisplayLaunchersAsync()
    {
        AppendOutput("Retrieving list of agent launchers...");

        OdrCommandResult result = await RunOdrCommandAsync("agent-info list", string.Empty);

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
        Launchers.Clear();

        try
        {
            using JsonDocument doc = JsonDocument.Parse(output);
            JsonElement root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement launcher in root.EnumerateArray())
                {
                    string name = launcher.TryGetProperty("name", out JsonElement nameElem) ? nameElem.GetString() ?? "N/A" : "N/A";
                    string displayName = launcher.TryGetProperty("display_name", out JsonElement displayNameElem) ? displayNameElem.GetString() ?? "N/A" : "N/A";
                    string actionId = launcher.TryGetProperty("action_id", out JsonElement actionIdElem) ? actionIdElem.GetString() ?? "N/A" : "N/A";

                    Launchers.Add(new LauncherInfo(name, displayName, actionId));
                }

                if (Launchers.Count == 0)
                {
                    DisplayNoLaunchers();
                }
                else
                {
                    HasLaunchers = true;
                    ShowLaunchersMessage = false;
                }
            }
            else
            {
                DisplayNoLaunchers();
            }
        }
        catch (JsonException)
        {
            DisplayPlainTextLaunchers(output);
        }
    }

    private void DisplayNoLaunchers()
    {
        Launchers.Clear();
        HasLaunchers = false;
        ShowLaunchersMessage = true;
        LaunchersMessage = "No agent launchers registered";
    }

    private void DisplayPlainTextLaunchers(string output)
    {
        Launchers.Clear();
        HasLaunchers = false;
        ShowLaunchersMessage = true;
        LaunchersMessage = output;
    }

    private void DisplayError(string errorMessage)
    {
        Launchers.Clear();
        HasLaunchers = false;
        ShowLaunchersMessage = true;
        LaunchersMessage = errorMessage;
    }

    private string? GetAgentJsonPath()
    {
        string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets", "agent.json");
        if (File.Exists(assetsPath))
        {
            return assetsPath;
        }

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

    private async Task<OdrCommandResult> RunOdrCommandAsync(string command, string argument)
    {
        AppendOutput($"Executing: {OdrCommandHelper.GetFullCommandString(command, string.IsNullOrEmpty(argument) ? null : argument)}");
        return await OdrCommandHelper.RunCommandAsync(command, string.IsNullOrEmpty(argument) ? null : argument);
    }

    private void SetStatus(string status)
    {
        StatusText = status;
        StatusVisible = true;
    }

    private void ClearStatus()
    {
        StatusText = string.Empty;
        StatusVisible = false;
    }

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
}

public record LauncherInfo(string Name, string DisplayName, string ActionId);
