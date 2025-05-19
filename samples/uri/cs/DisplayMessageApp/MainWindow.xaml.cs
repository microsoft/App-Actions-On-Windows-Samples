// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

namespace DisplayMessageApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow(string text)
        {
            this.InitializeComponent();

            this.MyTextBlock.Text = text;
        }
    }
}
