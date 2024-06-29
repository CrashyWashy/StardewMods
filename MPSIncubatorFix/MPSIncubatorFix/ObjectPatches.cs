using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace MPSIncubatorFix;

using Object = StardewValley.Object;

public static class ObjectPatches
{
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
        }
        catch (Exception e)
        {
            ModEntry.monitor.Log($"exception {e}", LogLevel.Error);
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
            ModEntry.monitor.Log($"exception {e}", LogLevel.Error);
        }
    }
}