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

namespace ExperimentalProviderApp
{
    [Guid("403EA403-A3DC-44B0-B1C2-60581E6E265E")]
    public partial class ExperimentalActionProvider : Windows.AI.Actions.Provider.IActionProvider
    {
        public IAsyncAction InvokeAsync(ActionInvocationContext context)
        {
            return InvokeAsyncHelper(context).AsAsyncAction();
        }

        private static async Task InvokeAsyncHelper(ActionInvocationContext context)
        {
            ResourceLoader resourceLoader = new();
            string result = await GetContextResult(context);

            ActionEntity responseEntity = context.EntityFactory.CreateTextEntity(result);
            context.SetOutputEntity("MessageCount", responseEntity);
        }

        private static async Task<string> GetContextResult(ActionInvocationContext context)
        {
            ResourceLoader resourceLoader = new();
            bool found = false;
            NamedActionEntity[] inputs = context.GetInputEntities();
            foreach (NamedActionEntity namedEntity in inputs)
            {
                if(context.ActionId.Equals("ExperimentalProviderApp.Experimental.Table", StringComparison.Ordinal))
                {
                    if (namedEntity.Name.Equals("Table") && namedEntity.Entity.Kind == ActionEntityKind.Table)
                    {
                        found = true;

                        TableActionEntity tableEntity = CastToType<ActionEntity, TableActionEntity>(namedEntity.Entity);

                        await EnsureAppIsInitialized();

                        string value = await ((App)App.Current).m_window.AddTableAsync(tableEntity);
                        return value;
                    }
                }
                else if (context.ActionId.Equals("ExperimentalProviderApp.Experimental.Contact", StringComparison.Ordinal))
                {
                    if (namedEntity.Name.Equals("Contact") && namedEntity.Entity.Kind == ActionEntityKind.Contact)
                    {
                        found = true;

                        ContactActionEntity contactEntity = CastToType<ActionEntity, ContactActionEntity>(namedEntity.Entity);

                        await EnsureAppIsInitialized();

                        return await ((App)App.Current).m_window.AddContactAsync(contactEntity);
                    }
                }
                
            }

            if (!found)
            {
                context.ExtendedError = new KeyNotFoundException();
                context.Result = ActionInvocationResult.Unsupported;
            }

            return resourceLoader.GetString("UnknownResult");
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
