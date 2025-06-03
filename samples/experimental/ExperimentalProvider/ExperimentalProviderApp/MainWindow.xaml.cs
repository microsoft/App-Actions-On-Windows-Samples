// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.AI.Actions;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.Resources;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ExperimentalProviderApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
        private ResourceLoader resourceLoader = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        public async Task<string> AddTableAsync(TableActionEntity table)
        {
            TaskCompletionSource<string> tcs = new();
            DispatcherQueue.TryEnqueue(() =>
            {
                string tableContent = "";
                foreach (string value in table.GetTextContent())
                {
                    tableContent += value + " ";
                }
                Messages.Add(table.RowCount + " x " + table.ColumnCount + " " + resourceLoader.GetString("TableMessage") + " " + tableContent);
                string message = resourceLoader.GetString("TableMessageResult");

                tcs.SetResult(string.Format(message, Messages.Count.ToString()));
            });

            return await tcs.Task;
        }

        public async Task<string> AddContactAsync(ContactActionEntity contactAction)
        {
            TaskCompletionSource<string> tcs = new();
            DispatcherQueue.TryEnqueue(() =>
            {
                Contact contact = contactAction.Contact;
                string messageContent = "";

                if (contact.Name != null)
                {
                    messageContent = resourceLoader.GetString("ContactMessage1") + " " + contact.Name;
                }
                if (contact.Addresses.Any())
                {
                    messageContent += resourceLoader.GetString("ContactMessage2") + " " + contact.Addresses[0];
                }
                Messages.Add(messageContent);
                tcs.SetResult(string.Format(resourceLoader.GetString("ContactMessageResult"), Messages.Count.ToString()));
            });

            return await tcs.Task;
        }
    }
}
