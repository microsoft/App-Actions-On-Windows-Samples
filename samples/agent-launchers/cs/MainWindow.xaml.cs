// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.UI.Xaml;
using SampleAgentLauncher.Pages;
using System;

namespace SampleAgentLauncher;

public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }
    private HomePage? _homePage;

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

        // Navigate to HomePage and pass the ViewModel
        RootFrame.Navigate(typeof(HomePage));
        if (RootFrame.Content is HomePage homePage)
        {
            _homePage = homePage;
            _homePage.SetViewModel(ViewModel);
        }
    }

    public void HandleProtocolActivation(Uri uri)
    {
        // Navigate back to HomePage if on another page
        if (RootFrame.Content is not HomePage)
        {
            RootFrame.Navigate(typeof(HomePage));
            if (RootFrame.Content is HomePage homePage)
            {
                _homePage = homePage;
                _homePage.SetViewModel(ViewModel);
            }
        }

        _homePage?.HandleProtocolActivation(uri);
    }
}


