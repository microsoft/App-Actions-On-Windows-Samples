// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SampleAgentLauncher.ViewModels;

namespace SampleAgentLauncher.Controls;

public sealed partial class RegistrationStatusPanel : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(RegistrationStatusPanelViewModel),
            typeof(RegistrationStatusPanel),
            new PropertyMetadata(null));

    public RegistrationStatusPanelViewModel? ViewModel
    {
        get => (RegistrationStatusPanelViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public RegistrationStatusPanel()
    {
        this.InitializeComponent();
    }

    // Converter function for x:Bind
    public Visibility BoolToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;
}

