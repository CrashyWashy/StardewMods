using System.Diagnostics;
using System.Text;
using MachineAugmentorsExtended.Data;
using MachineAugmentorsExtended.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Weapons;
using StardewValley.Menus;

namespace MachineAugmentorsExtended.Patches;

internal class GamePatches
{
    private static IMonitor Monitor;

    internal static void Initialize(IMonitor monitor)
    {
        Monitor = monitor;
    }
    
    public static void performObjectDropInAction_Postfix(
        SObject __instance,
        Item dropInItem,
        bool probe,
        Farmer who,
        ref bool __result
    )
    {
        try
        {
            AugmentorType getType = MachinesData.Instance.AugmentorIds(dropInItem);
            
            if (!__instance.Location.Equals(who.currentLocation)) return;
            if (probe || getType == AugmentorType.None) return;
            Log.Debug($"placed augmentor on {__instance.DisplayName}");

            if (__instance.GetMachineData() == null) 
                return;
            
            MachinesData.Instance.OnAugmentorPlaced(__instance, who, getType);
        }
        catch (Exception e)
        {
            Log.Error($"{e}");
        }
    }
    public static void OutputMachine_Prefix(
        SObject __instance,
        MachineData machine, 
        MachineOutputRule outputRule, 
        Item inputItem, Farmer who, 
        GameLocation location, 
        bool probe,
        out Dictionary<AugmentorType, dynamic> __state
        
    )
    { 
        __state = new Dictionary<AugmentorType, dynamic>();
        
        if (!probe 
            && MachinesData.DoesThisContainAnyAugmentor(__instance) 
            && MachinesData.Instance.AugmentorIds(inputItem) == AugmentorType.None
        )
        {
            __state.TryAdd(AugmentorType.Speed, outputRule.MinutesUntilReady);
            __state.TryAdd(AugmentorType.Efficiency, machine.AdditionalConsumedItems);
            
            // speed
            if (MachinesData.DoesThisContainAugmentor(__instance, AugmentorType.Speed))
            {
                outputRule.MinutesUntilReady =
                    MachinesData.CalculateAugmentedTime(outputRule.MinutesUntilReady,
                        MachinesData.GetAugCount(__instance, 0));
            }
            // efficiency
            if (MachinesData.DoesThisContainAugmentor(__instance, AugmentorType.Efficiency))
            {
                outputRule.MinutesUntilReady = 0;
                if (machine.AdditionalConsumedItems.Count > 0)
                {
                    foreach (var i in machine.AdditionalConsumedItems)
                    {
                        var x = i;
                        if (MachinesData.PerformEffiAugRNG(__instance))
                        {
                            i.RequiredCount = 0;
                        }
                        
                        Log.Debug($"old: {x.RequiredCount} new: {i.RequiredCount}");
                    }
                } 
            }
            Log.Debug($"original: {__state[AugmentorType.Speed]} | augmented: {outputRule.MinutesUntilReady}");
        }
    }
    public static void OutputMachine_Postfix(
        SObject __instance,
        MachineData machine,
        MachineOutputRule outputRule,
        bool probe, 
        Dictionary<AugmentorType, dynamic> __state
    )
    {
        if(!probe && MachinesData.DoesThisContainAugmentor(__instance, AugmentorType.Speed))
        {
            outputRule.MinutesUntilReady = (int)__state[AugmentorType.Speed];
        }
        if (!probe && MachinesData.DoesThisContainAugmentor(__instance, AugmentorType.Efficiency))
        {
            machine.AdditionalConsumedItems = (List<MachineItemAdditionalConsumedItems>)
                __state[AugmentorType.Efficiency];
        }
    }

    public static void draw_Postfix(SObject __instance, SpriteBatch spriteBatch, int x, int y)
    {
        try{
            if (Context.IsWorldReady && Game1.currentLocation.objects.ContainsKey(__instance.TileLocation))
            {
                if (!MachinesData.Instance.Locations.ContainsKey(__instance.Location.NameOrUniqueName) || !MachinesData.Instance.Locations[$"{__instance.Location.NameOrUniqueName}"].LocationName.Any())
                    return;
                if (MachinesData.Instance.Locations[__instance.Location.NameOrUniqueName].Tiles.ContainsKey($"{__instance.TileLocation}"))
                {
                    var augmentors = MachinesData.Instance.Locations[__instance.Location.NameOrUniqueName]
                        .Tiles[$"{__instance.TileLocation}"].AugmentorsDict;
                    var sortedAugmentors = augmentors.Keys.OrderBy(key => (int)key).ToList();
                    
                    float size = 3f;
                    float layerDepth = Math.Max(0.0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-04f;
                    var pos = (new Vector2((x * Game1.tileSize) - Game1.viewport.X,
                        (y * Game1.tileSize + 32) - Game1.viewport.Y));
                    // TODO : Make icons of the correct augmentor show
                    var offset = new Vector2(0,0);
                    foreach (var aug in sortedAugmentors)
                    {
                        Augmentor.drawIcon(aug, spriteBatch, pos + offset, size, layerDepth, 0.75f);
                        offset.X += 32;
                    }
                }
            }
        } catch(Exception e) { Log.Error($"{e}");}
    }
}
    
