// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.AI.Actions;
using Windows.Foundation;
using Windows.Storage;
using WinRT;

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
                if (context.ActionId.Equals("ExperimentalProviderApp.Experimental.Table", StringComparison.Ordinal))
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
                else if (context.ActionId.Equals("ExperimentalProviderApp.Experimental.EntityArray", StringComparison.Ordinal))
                {
                    found = true;

                    NamedActionEntity[] entities = context.GetInputEntities();

                    TextActionEntity? prefixEntity = GetActionEntityFromNamedActionArray("Prefix", entities) as TextActionEntity;

                    ArrayActionEntity? inputPhotoEntities = GetActionEntityFromNamedActionArray("Photos", entities) as ArrayActionEntity;

                    ActionEntity[] photos = inputPhotoEntities.GetItems();

                    List<ActionEntity> outputPhotos = new List<ActionEntity>();

                    for (int i = 0; i < photos.Length; i++)
                    {
                        string photoPath = (photos[i] as PhotoActionEntity)!.FullPath;
                        StorageFile photoFile = await StorageFile.GetFileFromPathAsync(photoPath);
                        StorageFolder parentFolder = await photoFile.GetParentAsync();
                        string newFileName = $"{prefixEntity.Text}_{i}";
                        await photoFile.RenameAsync(newFileName);
                        ActionEntity newPhotoEntity = context.EntityFactory.CreatePhotoEntity(photoFile.Path);
                        outputPhotos.Add(newPhotoEntity);
                    }

                    ArrayActionEntity arrayEntity = context.EntityFactory.CreateArrayEntity(ActionEntityKind.Photo, outputPhotos.ToArray());
                    await EnsureAppIsInitialized();

                    return await ((App)App.Current).m_window.AddArrayActionAsync(arrayEntity);
                }
                else if (context.ActionId.Equals("ExperiementalProviderApp.Experimental.EntityArray", StringComparison.Ordinal))
                {
                    if (namedEntity.Name.Equals("Uri") && namedEntity.Entity.Kind == ActionEntityKind.Uri)
                    {
                        found = true;

                        UriActionEntity contactEntity = CastToType<ActionEntity, UriActionEntity>(namedEntity.Entity);

                        await EnsureAppIsInitialized();

                        return await ((App)App.Current).m_window.AddUriAsync(contactEntity);
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
