// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.UI.Xaml;
using System;

namespace SampleAgentLauncher;

public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow()
    {
        ViewModel = new MainWindowViewModel();
        
        this.InitializeComponent();
        this.Title = "Sample Agent Launcher";
        
        // Set initial window size
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


