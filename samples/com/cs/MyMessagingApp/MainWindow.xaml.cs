// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace MyMessagingApp
{
    public sealed partial class MainWindow
    {
        private ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
        private ResourceLoader resourceLoader = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        public async Task<string> AddMessageAsync(string message)
        {
            TaskCompletionSource<string> tcs = new();
            DispatcherQueue.TryEnqueue(() =>
            {
                Messages.Add(message);
                tcs.SetResult(resourceLoader.GetString("MessageResult_1") + Messages.Count.ToString() + resourceLoader.GetString("MessageResult_2"));
            });

            return await tcs.Task;
        }
    }
}
