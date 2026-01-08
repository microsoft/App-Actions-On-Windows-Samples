// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SampleAgentLauncher.ViewModels;

namespace SampleAgentLauncher.Controls;

public sealed partial class InvocationInfoPanel : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(InvocationInfoViewModel),
            typeof(InvocationInfoPanel),
            new PropertyMetadata(null));

    public InvocationInfoViewModel? ViewModel
    {
        get => (InvocationInfoViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public InvocationInfoPanel()
    {
        this.InitializeComponent();
    }

    // Converter function for x:Bind
    public Visibility BoolToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;
}

