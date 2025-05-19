// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Windows.AI.Actions;
using System.Collections.Generic;
using Windows.Foundation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System;
using WinRT;
using Microsoft.Windows.ApplicationModel.Resources;

namespace MyMessagingApp
{
    // Class is declared partial to allow cswinrt insert generated code to enable marshalling without using reflection
    [Guid("15271F46-7E72-4703-B511-6C22CDD17A47")]
    public partial class SampleActionProvider : Windows.AI.Actions.Provider.IActionProvider
    {
        public IAsyncAction InvokeAsync(ActionInvocationContext context)
        {
            return InvokeAsyncHelper(context).AsAsyncAction();
        }

        private static async Task InvokeAsyncHelper(ActionInvocationContext context)
        {
            ResourceLoader resourceLoader = new();
            string result = resourceLoader.GetString("UnknownResult");

            if (context.ActionId.Equals("MyMessagingApp.SendMessage", StringComparison.Ordinal))
            {
                bool found = false;
                NamedActionEntity[] inputs = context.GetInputEntities();
                foreach (NamedActionEntity namedEntity in inputs)
                {
                    if ((namedEntity.Name.Equals("Message") || namedEntity.Name.Equals("Contact")) && namedEntity.Entity.Kind == ActionEntityKind.Text)
                    {
                        found = true;

                        TextActionEntity textEntity = CastToType<ActionEntity, TextActionEntity>(namedEntity.Entity);
                        string message = textEntity.Text;

                        await EnsureAppIsInitialized();

                        result = await ((App)App.Current).m_window.AddMessageAsync(message);
                    }
                }

                if (!found)
                {
                    context.ExtendedError = new KeyNotFoundException();
                    context.Result = ActionInvocationResult.Unsupported;
                }
            }
            else
            {
                context.ExtendedError = new NotImplementedException();
                context.Result = ActionInvocationResult.Unsupported;
            }

            ActionEntity responseEntity = context.EntityFactory.CreateTextEntity(result);
            context.SetOutputEntity("MessageCount", responseEntity);
        }

        private static async Task EnsureAppIsInitialized()
        {
            if (App.loaded.Task.IsCompleted)
            {
                return;
            }

            await App.loaded.Task;
        }

        public static TTo CastToType<TFrom, TTo>(TFrom obj)
        {
            IntPtr abiPtr = default;
            try
            {
                abiPtr = MarshalInspectable<TFrom>.FromManaged(obj);
                return MarshalInspectable<TTo>.FromAbi(abiPtr);
            }
            finally
            {
                MarshalInspectable<object>.DisposeAbi(abiPtr);
            }
        }
    }
}