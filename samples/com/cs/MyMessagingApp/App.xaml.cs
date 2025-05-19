// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace MyMessagingApp
{
    public partial class App : Application
    {
        internal static TaskCompletionSource loaded = new();

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();

            loaded.SetResult();
        }

        internal MainWindow m_window;
    }
}
