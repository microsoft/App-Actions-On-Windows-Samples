// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.AI.Actions;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using WinRT;

namespace ExperimentalProviderApp
{
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

        public async Task<string> AddEntityArrayAsync(ArrayActionEntity arrayAction)
        {
            TaskCompletionSource<string> tcs = new();
            DispatcherQueue.TryEnqueue(() =>
            {
                ActionEntity[] texts = arrayAction.GetAll();
                string messageContent = "";

                foreach (ActionEntity entity in texts)
                {
                    TextActionEntity text = entity.As<TextActionEntity>();
                    if(text.Text != null)
                    {
                        messageContent += text.Text + " ";
                    }
                }
                Messages.Add(messageContent);
                tcs.SetResult(string.Format(resourceLoader.GetString("ArrayMessageResult"), arrayAction.GetAll().Length.ToString()));
            });

            return await tcs.Task;
        }

        public async Task<string> AddUriAsync(UriActionEntity uriAction)
        {
            TaskCompletionSource<string> tcs = new();
            DispatcherQueue.TryEnqueue(() =>
            {
                Uri uri = uriAction.Uri;
                string messageContent = "";

                if (uri.AbsoluteUri != null)
                {
                    messageContent += uri.AbsoluteUri;
                }
                Messages.Add(messageContent);
                tcs.SetResult(string.Format(resourceLoader.GetString("UriMessageResult"), Messages.Count.ToString()));
            });

            return await tcs.Task;
        }

        public async Task<string> AddCustomTextAsync(CustomTextActionEntity customTextAction)
        {
            TaskCompletionSource<string> tcs = new();
            DispatcherQueue.TryEnqueue(() =>
            {
                string messageContent = "";

                if(customTextAction.KeyPhrase != null)
                {
                    messageContent += customTextAction.KeyPhrase;
                }
                Messages.Add(messageContent);
                tcs.SetResult(string.Format(resourceLoader.GetString("CustomTextMessageResult"), Messages.Count.ToString()));
            });

            return await tcs.Task;
        }

        public async Task<string> AddStreamingTextAsync(StreamingTextActionEntity streamingTextAction)
        {
            TaskCompletionSource<string> tcs = new();
            DispatcherQueue.TryEnqueue(() =>
            {
                string messageContent = "";

                if(streamingTextAction.GetText() != null)
                {
                    messageContent += streamingTextAction.GetText();
                }
                Messages.Add(messageContent);
                tcs.SetResult(string.Format(resourceLoader.GetString("CustomTextMessageResult"), Messages.Count.ToString()));
            });

            return await tcs.Task;
        }
    }
}
