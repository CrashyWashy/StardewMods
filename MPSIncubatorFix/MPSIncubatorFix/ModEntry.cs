
using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace MPSIncubatorFix
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        internal static IMonitor? monitor;
        
        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
            
            var harmony = new Harmony(this.ModManifest.UniqueID);
                
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.DayUpdate)),
                postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.DayUpdate_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.performToolAction)),
                prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.PerformToolAction_Prefix))
            );
            
            
            Monitor.Log($"Completed all operations for {ModManifest.UniqueID}");
        }
    }
}