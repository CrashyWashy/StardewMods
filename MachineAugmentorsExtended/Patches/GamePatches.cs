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

    public class OutputMachineState(SObject obj, MachineData data, MachineOutputRule outputRule)
    {
        public SObject Obj { get; set; } = obj;
        public MachineData Data { get; set; } = data;
        public MachineOutputRule OutputRule { get; set; } = outputRule;

        public Dictionary<string, int> ACItems { get; set; } = new();
        public int OriginalMinutesUntilReady { get; set; } = new();
    }

    private static OutputMachineState _outputMachineState { get; set; }

    public static void OutputMachine_Prefix(
        SObject __instance,
        MachineData machine, 
        MachineOutputRule outputRule, 
        Item inputItem, Farmer who, 
        GameLocation location, 
        bool probe
    )
    {
        if (probe) _outputMachineState = null;
        
        if (!probe 
            && MachinesData.DoesThisContainAnyAugmentor(__instance) 
            && MachinesData.Instance.AugmentorIds(inputItem) == AugmentorType.None
        )
        { 
            _outputMachineState = new OutputMachineState(__instance, machine, outputRule);
            
            
            // speed
            if (MachinesData.DoesThisContainAugmentor(__instance, AugmentorType.Speed))
            {
                _outputMachineState.OriginalMinutesUntilReady = outputRule.MinutesUntilReady;
                outputRule.MinutesUntilReady =
                    MachinesData.CalculateAugmentedTime(outputRule.MinutesUntilReady,
                        MachinesData.GetAugCount(__instance, 0));
            }
            // efficiency
            if (MachinesData.DoesThisContainAugmentor(__instance, AugmentorType.Efficiency))
            {
                outputRule.MinutesUntilReady = 10;
                if (machine.AdditionalConsumedItems.Count > 0)
                {
                    foreach (var i in machine.AdditionalConsumedItems)
                    {
                        // if (MachinesData.PerformEffiAugRNG(__instance))
                        // {
                        //     i.RequiredCount = 0;
                        // }
                        _outputMachineState.ACItems.Add(i.ItemId, i.RequiredCount);
                        i.RequiredCount = 0;
                        Log.Debug($"{_outputMachineState.ACItems[i.ItemId]}");
                    }
                }
            }
        }
        
    }
    public static void OutputMachine_Postfix(
        SObject __instance,
        MachineData machine,
        MachineOutputRule outputRule,
        bool probe
    )
    {
        if (_outputMachineState is null) return;
        
        if(!probe && MachinesData.DoesThisContainAugmentor(__instance, AugmentorType.Speed))
        {
            outputRule.MinutesUntilReady = _outputMachineState.OriginalMinutesUntilReady;
        }
        if (!probe && MachinesData.DoesThisContainAugmentor(__instance, AugmentorType.Efficiency))
        {
            foreach (var i in machine.AdditionalConsumedItems)
            {
                i.RequiredCount = _outputMachineState.ACItems[i.ItemId];
            }
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
    
