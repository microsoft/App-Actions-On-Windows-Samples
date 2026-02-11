// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace SampleAgentLauncher.Pages;

public sealed partial class HomePage : Page
{
    public MainWindowViewModel ViewModel { get; private set; }

    public HomePage()
    {
        ViewModel = new MainWindowViewModel();
        this.InitializeComponent();
    }

    public void SetViewModel(MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        Bindings.Update();
    }

    public void HandleProtocolActivation(Uri uri)
    {
        ViewModel.HandleProtocolActivation(uri);
    }

    private void NavigateToAgentInfoPage_Click(object sender, RoutedEventArgs e)
    {
        this.Frame.Navigate(typeof(AgentInfoPage));
    }

    // Converter function for x:Bind
    public Visibility BoolToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;
}
