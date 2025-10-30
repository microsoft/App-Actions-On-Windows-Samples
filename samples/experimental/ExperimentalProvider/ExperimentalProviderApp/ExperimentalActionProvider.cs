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
using Windows.System;
using WinRT;

namespace ExperimentalProviderApp
{
    [Guid("403EA403-A3DC-44B0-B1C2-60581E6E265E")]
    public partial class ExperimentalActionProvider : Windows.AI.Actions.Provider.IActionProvider
    {
        private static readonly string[] wordsForStreamingStory = new string[]
        {
        "This ",
        "is ",
        "a ",
        "story ",
        "about a superhero ",
        "named {first_name} ",
        "that defended the city of {city} ",
        "with courage and honor. ",
        "{first_name} was known for their ability to fly " +
        "and see through solid objects. {first_name} was beloved " +
        "by everyone in {city} and {first_name}'s journey still continues to this day.",
        };

        private static bool hasChanged = false;

        public IAsyncAction InvokeAsync(ActionInvocationContext context)
        {
            context.HelpDetails.Changed += (sender, args) =>
            {
                hasChanged = true;
            };
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

                    ArrayActionEntity? inputTextEntities = GetActionEntityFromNamedActionArray("Texts", entities) as ArrayActionEntity;

                    ActionEntity[] texts = inputTextEntities.GetAll();

                    List<ActionEntity> updatedText = new List<ActionEntity>();

                    for (int i = 0; i < texts.Length; i++)
                    {
                        string updatedTextContent = prefixEntity.Text + i;
                        ActionEntity updatedTextEntity = context.EntityFactory.CreateTextEntity(updatedTextContent);
                        updatedText.Add(updatedTextEntity);
                    }

                    ArrayActionEntity arrayEntity = context.EntityFactory.CreateArrayEntity(ActionEntityKind.Text, updatedText.ToArray());
                    await EnsureAppIsInitialized();

                    return await ((App)App.Current).m_window.AddEntityArrayAsync(arrayEntity);
                }
                else if (context.ActionId.Equals("ExperimentalProviderApp.Experimental.Uri", StringComparison.Ordinal))
                {
                    if (namedEntity.Name.Equals("Link") && namedEntity.Entity.Kind == ActionEntityKind.Uri)
                    {
                        found = true;

                        UriActionEntity uriEntity = CastToType<ActionEntity, UriActionEntity>(namedEntity.Entity);

                        await EnsureAppIsInitialized();

                        return await ((App)App.Current).m_window.AddUriAsync(uriEntity);
                    }
                }
                else if (context.ActionId.Equals("ExperimentalProviderApp.Experimental.CustomText", StringComparison.Ordinal))
                {
                    if (namedEntity.Name.Equals("CustomText") && namedEntity.Entity.Kind == ActionEntityKind.CustomText)
                    {
                        found = true;
                        CustomTextActionEntity customEntity = CastToType<ActionEntity, CustomTextActionEntity>(namedEntity.Entity);

                        string movie = customEntity.KeyPhrase;
                        IReadOnlyDictionary<string, object> movieProperties = customEntity.Properties;

                        Launcher.LaunchUriAsync(
                            new Uri($"ms-windows-store://search/?query={movie}")).Get();

                        await EnsureAppIsInitialized();

                        return await ((App)App.Current).m_window.AddCustomTextAsync(customEntity);
                    }
                }
                else if(context.ActionId.Equals("ExperimentalProviderApp.Experimental.StreamingText", StringComparison.Ordinal))
                {
                    found = true;

                    await EnsureAppIsInitialized();

                    await InvokeStreamingActionAsyncHelper(context).AsAsyncAction();

                    return await ((App)App.Current).m_window.AddStreamingTextAsync(hasChanged);
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

        private static ActionEntity? GetActionEntityFromNamedActionArray(string name, NamedActionEntity[] array)
        {
            ActionEntity? entity = null;
            foreach (NamedActionEntity namedActionEntity in array)
            {
                if (namedActionEntity.Name.Equals(name, StringComparison.Ordinal))
                {
                    entity = namedActionEntity.Entity;
                    break;
                }
            }

            return entity;
        }

        // A helper that updates the HelpDetails and raises the event 

        private static  void UpdateHelpDetails(
            ActionInvocationContext context,
            string title,
            string description,
            string helpDescription)
        {
            // Update HelpDetails of the context, e.g., context.HelpDetails.Title = title. elided... 
            // context.HelpDetails internally raises the event upon each property update. 
            context.HelpDetails.Title = title;
            context.HelpDetails.Description = description;
            context.HelpDetails.HelpUriDescription = helpDescription;
            // The Changed event is raised after each property is updated 
            ((App)App.Current).m_window.AddStreamingTextAsync(hasChanged);
        }

        private static async Task InvokeStreamingActionAsyncHelper(ActionInvocationContext context)
        {
            string firstName = string.Empty;
            string cityOfAction = string.Empty;
            foreach (NamedActionEntity inputEntity in context.GetInputEntities())
            {
                if (inputEntity.Name.Equals("FirstName", StringComparison.Ordinal))
                {
                    TextActionEntity entity = CastToType<ActionEntity, TextActionEntity>(inputEntity.Entity);
                    firstName = entity.Text;
                }
                if (inputEntity.Name.Equals("City", StringComparison.Ordinal))
                {
                    TextActionEntity entity = CastToType<ActionEntity, TextActionEntity>(inputEntity.Entity);
                    cityOfAction = entity.Text;
                }
            }
            var streamingTextWriter = context.EntityFactory.CreateStreamingTextActionEntityWriter(ActionEntityTextFormat.Plain);

            // Feed fake LLM output to the host in LLM fashion.
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                string storyText = string.Empty;
                foreach (string word in wordsForStreamingStory)
                {
                    string formattedWord = word;
                    if (formattedWord.Contains("{first_name}", StringComparison.Ordinal))
                    {
                        formattedWord = formattedWord.Replace("{first_name}", firstName);
                    }
                    if (formattedWord.Contains("{city}", StringComparison.Ordinal))
                    {
                        formattedWord = formattedWord.Replace("{city}", cityOfAction);
                    }

                    UpdateHelpDetails(
                        context,
                        "Generating Story...",
                        $"Current length: {storyText.Length + formattedWord.Length} characters",
                        "This story is being generated using a simulated LLM output.");

                    storyText += formattedWord;
                    streamingTextWriter.SetText(storyText);
                    await Task.Delay(500);
                }
                streamingTextWriter.Dispose();
            });

            context.Result = ActionInvocationResult.Success;
            context.SetOutputEntity("Story", streamingTextWriter.ReaderEntity);
        }
    }
}
