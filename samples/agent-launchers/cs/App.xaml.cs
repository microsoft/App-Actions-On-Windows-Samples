// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;

namespace SampleAgentLauncher;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        bool protocolHandled = TryHandleProtocolActivation();

        if (!protocolHandled)
        {
            // Normal application launch
            _window = new MainWindow();
            _window.Activate();
        }
    }

    private bool TryHandleProtocolActivation()
    {
        try
        {
            AppActivationArguments activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

            if (activationArgs.Kind == ExtendedActivationKind.Protocol)
            {
                if (activationArgs.Data is ProtocolActivatedEventArgs protocolArgs)
                {
                    return HandleProtocolActivation(protocolArgs);
                }
            }
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // If protocol activation fails, fall back to normal launch
        }

        return false;
    }

    private bool HandleProtocolActivation(ProtocolActivatedEventArgs protocolArgs)
    {
        // Ensure window is created and activated
        if (_window == null)
        {
            _window = new MainWindow();
        }

        // Pass the protocol URI to the main window
        if (_window is MainWindow mainWindow)
        {
            mainWindow.HandleProtocolActivation(protocolArgs.Uri);
        }

        _window.Activate();
        return true;
    }
}
