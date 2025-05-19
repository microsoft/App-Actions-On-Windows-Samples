// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppLifecycle;
using System;
using Windows.ApplicationModel.Activation;

namespace DisplayMessageApp
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched. Utilized to parse protocol args.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            bool success = false;

            // Parse for protocol args
            AppActivationArguments activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

            if (activationArgs.Kind == ExtendedActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs? protocolArgs = activationArgs.Data as ProtocolActivatedEventArgs;
                if (protocolArgs != null)
                {
                    success = true;

                    string query = protocolArgs.Uri.Query;
                    string message = query.Substring("?message=".Length);

                    Window = new MainWindow(Uri.UnescapeDataString(message));
                    Window.Activate();
                }
            }

            if (!success)
            {
                ResourceLoader resourceLoader = new();
                Window = new MainWindow(resourceLoader.GetString("AppLaunchError"));
                Window.Activate();
            }
        }

        public static MainWindow? Window { get; private set; }
    }
}
