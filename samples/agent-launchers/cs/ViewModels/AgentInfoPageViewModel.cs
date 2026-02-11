// Copyright (C) Microsoft Corporation. All rights reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SampleAgentLauncher.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SampleAgentLauncher.ViewModels;

public partial class AgentInfoCommandResult : ObservableObject
{
    [ObservableProperty]
    private string _commandDescription = string.Empty;

    [ObservableProperty]
    private string _command = string.Empty;

    [ObservableProperty]
    private string _output = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSuccess))]
    private bool _isLoading = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSuccess))]
    private bool _isError = false;

    public bool IsSuccess => !IsLoading && !IsError;

    public AgentInfoCommandResult(string description, string command)
    {
        CommandDescription = description;
        Command = command;
    }
}

public partial class AgentInfoPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<AgentInfoCommandResult> _commandResults = new();

    [ObservableProperty]
    private bool _isRefreshing = false;

    public AgentInfoPageViewModel()
    {
        InitializeCommands();
        _ = ExecuteAllCommandsAsync();
    }

    private void InitializeCommands()
    {
        CommandResults.Clear();

        // Theme variations
        CommandResults.Add(new AgentInfoCommandResult(
            "Dark Theme",
            "agent-info list --theme dark"));

        CommandResults.Add(new AgentInfoCommandResult(
            "Light Theme",
            "agent-info list --theme light"));

        // Contrast variations
        CommandResults.Add(new AgentInfoCommandResult(
            "High Contrast",
            "agent-info list --contrast high"));

        CommandResults.Add(new AgentInfoCommandResult(
            "Standard Contrast",
            "agent-info list --contrast standard"));

        // Target size with theme variations
        CommandResults.Add(new AgentInfoCommandResult(
            "Target Size 16 - Dark Theme",
            "agent-info list --targetsize 16 --theme dark"));

        CommandResults.Add(new AgentInfoCommandResult(
            "Target Size 16 - Light Theme",
            "agent-info list --targetsize 16 --theme light"));

        // Scale with multiple themes
        CommandResults.Add(new AgentInfoCommandResult(
            "Scale 200 - Dark and Light Themes",
            "agent-info list --theme dark,light --scale 200"));
    }

    [RelayCommand]
    private async Task RefreshAllAsync()
    {
        if (IsRefreshing)
            return;

        IsRefreshing = true;

        try
        {
            foreach (var result in CommandResults)
            {
                result.IsLoading = true;
                result.IsError = false;
                result.Output = string.Empty;
            }

            await ExecuteAllCommandsAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task ExecuteAllCommandsAsync()
    {
        foreach (var commandResult in CommandResults)
        {
            await ExecuteCommandAsync(commandResult);
        }
    }

    private async Task ExecuteCommandAsync(AgentInfoCommandResult commandResult)
    {
        commandResult.IsLoading = true;
        commandResult.IsError = false;

        try
        {
            var result = await OdrCommandHelper.RunCommandAsync(commandResult.Command);

            if (result.IsSuccess)
            {
                commandResult.Output = string.IsNullOrWhiteSpace(result.Output)
                    ? "(No output)"
                    : result.Output;
            }
            else
            {
                commandResult.IsError = true;
                commandResult.Output = $"Exit Code: {result.ExitCode}\n";
                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    commandResult.Output += $"Error: {result.Error}";
                }
                else if (!string.IsNullOrWhiteSpace(result.Output))
                {
                    commandResult.Output += result.Output;
                }
            }
        }
        catch (Exception ex)
        {
            commandResult.IsError = true;
            commandResult.Output = $"Exception: {ex.Message}";
        }
        finally
        {
            commandResult.IsLoading = false;
        }
    }
}
