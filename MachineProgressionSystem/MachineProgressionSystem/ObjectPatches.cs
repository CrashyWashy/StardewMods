using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace MachineProgressionSystem;

public static class ObjectPatches
{
    private static IMonitor Monitor;
    internal static void Init(IMonitor monitor)
    {
        Monitor = monitor;
    }
    
    // Patches for Custom Incubators
        public static void PerformToolAction_Prefix(Object __instance, Tool t)
    {
        try 
        {
            if (__instance.name.Contains("SlimeIncubator") && __instance.name.Contains("nachonline.mps"))
            {
                __instance.ResetParentSheetIndex();
                __instance.heldObject.Value = (Object)null;
                __instance.minutesUntilReady.Value = -1;
            }
            else if (__instance.name.Contains("Crystalarium") && __instance.name.Contains("nachonline.mps") && __instance.heldObject.Value != null)
            {
                __instance.Location.debris.Add(new Debris((Item) __instance.heldObject.Value, __instance.tileLocation.Value * 64f + new Vector2(32f, 32f)));
                __instance.heldObject.Value = (Object) null;
            }
        }
        catch (Exception e)
        {
            Monitor.Log($"exception {e}", LogLevel.Error);
        }
    }
    
    public static void DayUpdate_Postfix(Object __instance)
    {
        try
        {

            GameLocation location1 = __instance.Location;
            if (!__instance.ItemId.Equals("nachonline.mps_PrismaticSlimeIncubator") &&
                !__instance.ItemId.Equals("nachonline.mps_RadioactiveSlimeIncubator")) return;

            if (__instance.MinutesUntilReady > 0 || __instance.heldObject.Value == null)
            {
                return;
            }

            if (location1.canSlimeHatchHere())
            {
                GreenSlime slime = null;
                Vector2 v = new Vector2((int)__instance.tileLocation.X, (int)__instance.tileLocation.Y + 1) * 64f;
                switch (__instance.heldObject.Value.QualifiedItemId)
                {
                    case "(O)680":
                        slime = new GreenSlime(v, 0);
                        break;
                    case "(O)413":
                        slime = new GreenSlime(v, 40);
                        break;
                    case "(O)437":
                        slime = new GreenSlime(v, 80);
                        break;
                    case "(O)439":
                        slime = new GreenSlime(v, 121);
                        break;
                    case "(O)857":
                        slime = new GreenSlime(v, 121);
                        slime.makeTigerSlime();
                        break;
                }

                if (slime != null)
                {
                    Game1.showGlobalMessage(slime.cute
                        ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12689")
                        : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12691"));
                    Vector2 openSpot = Utility.recursiveFindOpenTileForCharacter(slime, location1,
                        __instance.tileLocation.Value + new Vector2(0f, 1f), 10, false);
                    slime.setTilePosition((int)openSpot.X, (int)openSpot.Y);
                    location1.characters.Add(slime);
                    __instance.ResetParentSheetIndex();
                    __instance.heldObject.Value = null;
                    __instance.minutesUntilReady.Value = -1;
                }
            }
            else
            {
                __instance.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
                __instance.readyForHarvest.Value = false;
            }
        }
        catch (Exception e)
        {
            Monitor.Log($"exception {e}", LogLevel.Error);
        }
    }
    
    // Patches for Custom Lightning Rods
    public static void performLightningUpdate_Prefix(object __instance)
    {
        Monitor.LogOnce("Prefix working I think.", LogLevel.Trace);
        Farm farm = Game1.getFarm();
        List<Vector2> options = new List<Vector2>();
        foreach (KeyValuePair<Vector2, Object> pair in farm.objects.Pairs)
        {
            if(CustomLightningRod.LightningRodId(pair.Value.QualifiedItemId) != 0)
            {
                Monitor.Log($"Found {pair.Value.QualifiedItemId} Rod at {pair.Key}", LogLevel.Trace);
                options.Add(pair.Key);
            }
        }
        if (options.Count > 0)
        {
            Random random = Utility.CreateRandom((double)Game1.uniqueIDForThisGame, (double)Game1.stats.DaysPlayed,
                (double)Game1.timeOfDay);
            for (int index = 0; index < 2; ++index)
            {
                Vector2 key = random.ChooseFrom<Vector2>((IList<Vector2>)options);

                var obj = farm.objects[key];
                if (obj.heldObject.Value == null)
                {
                    obj.heldObject.Value = ItemRegistry.Create<Object>("(O)787", CustomLightningRod.LightningRodId(obj.QualifiedItemId));
                    obj.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
                    obj.shakeTimer = 1000;
                    Farm.LightningStrikeEvent lightningStrikeEvent = new Farm.LightningStrikeEvent
                    {
                        createBolt = true,
                        boltPosition = key * 64f + new Vector2(32f, 0.0f)
                    };
                    farm.lightningStrikeEvent.Fire(lightningStrikeEvent);
                    return;
                }
            }
        }
    }
}