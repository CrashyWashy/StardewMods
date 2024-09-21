using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace MachineProgressionSystem
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);
            ObjectPatches.Init(Monitor);

            // Patches for Custom Incubators
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), 
                    nameof(StardewValley.Object.DayUpdate)),
                
                postfix: new HarmonyMethod(typeof(ObjectPatches),
                    nameof(ObjectPatches.DayUpdate_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), 
                    nameof(StardewValley.Object.performToolAction)),
                
                prefix: new HarmonyMethod(typeof(ObjectPatches),
                    nameof(ObjectPatches.PerformToolAction_Prefix))
            );
            
            // Patches for Custom Lightning Rods
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Utility), 
                    nameof(StardewValley.Utility.performLightningUpdate)),
                
                prefix: new HarmonyMethod(typeof(ObjectPatches), 
                    nameof(ObjectPatches.performLightningUpdate_Prefix))
            );
        }

        
    }
}