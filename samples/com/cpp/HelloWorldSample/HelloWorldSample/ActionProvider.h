// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma once

namespace winrt
{
    using namespace Windows::Foundation;
    using namespace Windows::Foundation::Collections;
    using namespace Windows::AI::Actions;
    using namespace Windows::AI::Actions::Provider;
}

// Implement Action interfaces
struct HelloWorldCoClass : winrt::implements<HelloWorldCoClass, winrt::IActionProvider>
{
    // IActionProvider implementation

    // InvokeAsync
    // @param context Contains all relevant information about the action
    winrt::IAsyncAction InvokeAsync(const winrt::ActionInvocationContext context) const
    {
        // Check what action to invoke
        if (context.ActionId() == L"Contoso.HelloWorld")
        {
            auto inputs = context.GetInputEntities();
            bool found = false;

            for (winrt::NamedActionEntity entity : inputs)
            {
                if (entity.Name() == L"Name")
                {
                    found = true;

                    auto convertParam = entity.Entity();
                    if (convertParam.Kind() == winrt::ActionEntityKind::Text)
                    {
                        // Get text from Entity
                        auto textEntity = convertParam.as<winrt::TextActionEntity>();
                        auto name = textEntity.Text();

                        // Perform our translation
                        auto hello = L"Hello " + name;

                        // Create output entity, and add it to the results
                        auto outputEntity = context.EntityFactory().CreateTextEntity(hello);
                        context.SetOutputEntity(L"HelloOutput", outputEntity.as<winrt::ActionEntity>());
                    }
                }
            }

            if (!found) {
                context.Result(winrt::ActionInvocationResult::Unsupported);
                context.ExtendedError(E_INVALIDARG);
            }
        }
        else
        {
            // unknown action
            context.Result(winrt::ActionInvocationResult::Unsupported);
            context.ExtendedError(E_UNEXPECTED);
        }
        co_return;
    }
};
