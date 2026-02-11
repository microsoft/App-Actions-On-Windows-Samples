// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SampleAgentLauncher.ViewModels;

namespace SampleAgentLauncher.Pages;

public sealed partial class AgentInfoPage : Page
{
    public AgentInfoPageViewModel ViewModel { get; }

    public AgentInfoPage()
    {
        ViewModel = new AgentInfoPageViewModel();
        this.InitializeComponent();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.Frame.CanGoBack)
        {
            this.Frame.GoBack();
        }
    }

    // Converter function for x:Bind
    public bool InvertBool(bool value) => !value;
}
