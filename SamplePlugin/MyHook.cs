using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using JohnFinalfantasy;
using System;
using Dalamud.Plugin;
using System;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;

namespace SamplePlugin
{
    internal class MyHook : IDisposable
    {
        private delegate void UpdatePartyFinderDelegate(long a1, long a2, long a3);

        [Signature("48 89 5C 24 ?? 56 57 41 55 48 83 EC 30", DetourName = nameof(DetourTest))]
        private readonly Hook<UpdatePartyFinderDelegate> _partyFinderUpdateHook;
        public MyHook()
        {
            Services.gameInteropProvider.InitializeFromAttributes(this);
            this._partyFinderUpdateHook?.Enable();
        }

        public void Dispose()
        {
            this._partyFinderUpdateHook.Dispose();
        }

        private void DetourTest(long a1, long a2, long a3)
        {
            Services.PluginLog.Debug("Updated pf!\n" + a1.ToString("X") + "\n" + a2.ToString("X") + "\n" + a3.ToString("X"));
            unsafe
            {
                long r13 = *((long*)(a2 + 568));
                int* dc = (int*)(1608 + *(long*)(r13 + 32));
                int* world = dc + 1;
                int* priv = dc + 2;
                Services.PluginLog.Debug(dc->ToString() + " " + world->ToString() + " " + priv->ToString());
            }
            this._partyFinderUpdateHook.Original(a1, a2, a3);
        }

        

    }
}
