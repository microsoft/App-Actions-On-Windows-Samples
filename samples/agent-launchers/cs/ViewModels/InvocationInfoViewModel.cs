// Copyright (C) Microsoft Corporation. All rights reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace SampleAgentLauncher.ViewModels;

public partial class InvocationInfoViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isVisible = false;

    [ObservableProperty]
    private string _uriText = string.Empty;

    [ObservableProperty]
    private string _actionText = string.Empty;

    [ObservableProperty]
    private string _agentName = string.Empty;

    [ObservableProperty]
    private string _prompt = string.Empty;

    public void ShowInvocation(Uri uri, string action, string agentName, string prompt)
    {
        IsVisible = true;
        UriText = uri.ToString();
        ActionText = action;
        AgentName = agentName;
        Prompt = prompt;
    }

    public void Hide()
    {
        IsVisible = false;
    }
}

