// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma once

#include <stdint.h>
#include <combaseapi.h>

// In .exe local servers the class object must not contribute to the module ref count, and use
// winrt::no_module_lock, the other objects must and this is the hook into the C++ WinRT ref
// counting system that enables this.

void SignalLocalServerShutdown();

namespace winrt
{
    inline auto get_module_lock() noexcept
    {
        struct service_lock
        {
            uint32_t operator++() noexcept
            {
                return ::CoAddRefServerProcess();
            }

            uint32_t operator--() noexcept
            {
                const auto ref = ::CoReleaseServerProcess();

                if (ref == 0)
                {
                    SignalLocalServerShutdown();
                }
                return ref;
            }
        };

        return service_lock{};
    }
}

#define WINRT_CUSTOM_MODULE_LOCK

#include <wil/cppwinrt.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Windows.AI.Actions.h>
#include <winrt/Windows.AI.Actions.Provider.h>

#include <wil/resource.h>

#include <shellapi.h>

#include <algorithm>
#include <cctype>
#include <iostream>
#include <mutex>
#include <random>
#include <sstream>
#include <unordered_map>
#include <vector>

// {43F4D8CF-001D-4E91-BEB0-9D01B9575793}
static const GUID CLSID_HelloWorld =
{ 0x43f4d8cf, 0x1d, 0x4e91, { 0xbe, 0xb0, 0x9d, 0x1, 0xb9, 0x57, 0x57, 0x93 } };