// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Shmuelie.WinRTServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shmuelie.WinRTServer.CsWinRT;

namespace ExperimentalProviderApp
{
    public static class Program
    {
        [global::System.STAThreadAttribute]
        static async Task Main(string[] args)
        {
            global::WinRT.ComWrappersSupport.InitializeComWrappers();

            await using (ComServer server = new())
            {
                ExperimentalActionProvider sampleActionProvider = new ExperimentalActionProvider();
                server.RegisterClass<ExperimentalActionProvider, Windows.AI.Actions.Provider.IActionProvider>(() => sampleActionProvider);
                server.Start();

                global::Microsoft.UI.Xaml.Application.Start((p) => {
                    var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
                    global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                    new App();
                });

                // Regular disposing of the ComServer is broken (https://github.com/shmuelie/Shmuelie.WinRTServer/issues/28).
                // We instead call the UnsafeDispose method to dispose it.
                server.UnsafeDispose();
            }
        }
    }
}
