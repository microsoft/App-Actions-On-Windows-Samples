// Copyright (C) Microsoft Corporation. All rights reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SampleAgentLauncher.Models;
using System;
using System.Threading.Tasks;

namespace SampleAgentLauncher.ViewModels;

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

    private Func<Task>? _toggleAction;

    public RegistrationStatusItemViewModel(string name, bool hasButton = false, Func<Task>? toggleAction = null)
    {
        _name = name;
        _status = RegistrationStatus.Checking;
        _hasButton = hasButton;
        _toggleAction = toggleAction;
    }

    public string StatusText
    {
        get
        {
            string statusString = Status switch
            {
                RegistrationStatus.Registered => "Registered ✓",
                RegistrationStatus.NotRegistered => "Not Registered",
                RegistrationStatus.Checking => "Checking...",
                RegistrationStatus.Unknown => "Unknown",
                _ => "Unknown"
            };
            return $"{Name}: {statusString}";
        }
    }

    public SolidColorBrush StatusForeground
    {
        get
        {
            return Status switch
            {
                RegistrationStatus.Registered => new SolidColorBrush(Colors.Green),
                RegistrationStatus.NotRegistered => new SolidColorBrush(Colors.Orange),
                RegistrationStatus.Checking => new SolidColorBrush(Colors.Gray),
                RegistrationStatus.Unknown => new SolidColorBrush(Colors.Gray),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
    }

    public string ButtonContent
    {
        get
        {
            return Status switch
            {
                RegistrationStatus.Registered => "Unregister",
                RegistrationStatus.NotRegistered => "Register",
                _ => "..."
            };
        }
    }

    public Visibility ButtonVisibility => HasButton ? Visibility.Visible : Visibility.Collapsed;

    partial void OnStatusChanged(RegistrationStatus value)
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusForeground));
        OnPropertyChanged(nameof(ButtonContent));
    }

    partial void OnHasButtonChanged(bool value)
    {
        OnPropertyChanged(nameof(ButtonVisibility));
    }

    [RelayCommand]
    private async Task ToggleRegistrationAsync()
    {
        if (_toggleAction != null)
        {
            await _toggleAction();
        }
    }
}

