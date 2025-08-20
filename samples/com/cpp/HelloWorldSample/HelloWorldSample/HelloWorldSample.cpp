// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#include "pch.h"
#include "ActionProvider.h"

using namespace winrt::Windows::Foundation;

// Event to track when to shut down this process
wil::unique_event g_shutdownEvent(wil::EventOptions::None);

void SignalLocalServerShutdown()
{
    g_shutdownEvent.SetEvent();
}

// Helper class that implements IClassFactory for the given type T
template <typename T>
struct SingletonClassFactory : winrt::implements<SingletonClassFactory<T>, IClassFactory, winrt::no_module_lock>
{
    STDMETHODIMP CreateInstance(
        ::IUnknown* outer,
        GUID const& iid,
        void** result) noexcept final
    {
        *result = nullptr;

        std::unique_lock lock(mutex);

        if (outer)
        {
            return CLASS_E_NOAGGREGATION;
        }

        return winrt::make<T>().as(iid, result);
    }

    STDMETHODIMP LockServer(BOOL) noexcept final
    {
        return S_OK;
    }

private:
    std::mutex mutex;
};

// Main executable entrypoint
int APIENTRY wWinMain(_In_ HINSTANCE, _In_opt_ HINSTANCE, _In_ LPWSTR, _In_ int)
{
    // Check to see if we're being invoked to serve COM objects
    int argc;
    PWSTR* argv = CommandLineToArgvW(GetCommandLineW(), &argc);
    if (argc == 2 && lstrcmp(argv[1], L"-Embedding") == 0)
    {
        // COM server scenario
        winrt::init_apartment();

        // Create our class factory and register our coclass
        auto cf = winrt::make<SingletonClassFactory<HelloWorldCoClass>>();

        wil::unique_com_class_object_cookie cookie;
        winrt::check_hresult(CoRegisterClassObject(
            CLSID_HelloWorld,
            cf.get(),
            CLSCTX_LOCAL_SERVER,
            REGCLS_MULTIPLEUSE,
            cookie.put()));

        // Wait for outstanding references to be released before exiting process
        DWORD index{};
        HANDLE events[] = { g_shutdownEvent.get() };
        winrt::check_hresult(CoWaitForMultipleObjects(CWMO_DISPATCH_CALLS | CWMO_DISPATCH_WINDOW_MESSAGES, INFINITE,
            static_cast<ULONG>(std::size(events)), events, &index));
    }
    else
    {
        // Not serving COM, show a message and exit
        MessageBox(nullptr, L"Hello world!", L"Hello world sample", MB_OK);
    }
}