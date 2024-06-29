using System.Diagnostics;
using MachineAugmentorsExtended.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Objects;

namespace MachineAugmentorsExtended.Data;

public class MachinesData
{
    public static MachinesData Instance { get; internal set; }

    private static readonly Random rand = Utility.CreateRandom(Game1.stats.StepsTaken, Game1.stats.DaysPlayed);
    
    public Dictionary<string, WorldLocationInstance> Locations { get; set; } = new();

    internal void StartupCheck()
    {
        foreach (var location in Locations.Values)
        {
            // check if there is a machine here
            var loc = Game1.getLocationFromName(location.LocationName);
            foreach (var tile in location.Tiles)
            {
                if (!loc.isObjectAtTile(tile.Value._x, tile.Value._y))
                {
                    location.Tiles.Remove(tile.Key);
                }
            }
        }
    }
    
    internal void OnAugmentorPlaced(SObject obj, Farmer who, AugmentorType type)
    {
        if (!Instance.AddAugmentorToData(obj, type)) {Log.Debug($"this location has reached its max augmentor capacity"); return;}
        who.reduceActiveItemByOne();
        Log.Debug($"Successfully added augmentor to {obj.TileLocation}");

    }

    private bool AddAugmentorToData(SObject obj, AugmentorType type)
    {
        Instance.Locations.TryAdd(
            obj.Location.NameOrUniqueName,
            new WorldLocationInstance( obj.Location.NameOrUniqueName)
        );
        Instance.Locations[obj.Location.NameOrUniqueName].Tiles.TryAdd(
            obj.TileLocation.ToString(),
            new MachineTileInstance(obj.TileLocation));
        Instance.Locations[obj.Location.NameOrUniqueName].Tiles[$"{obj.TileLocation.ToString()}"].
            AugmentorsDict.TryAdd(type, new int());
        
        if (Instance.Locations[obj.Location.NameOrUniqueName].Tiles[$"{obj.TileLocation}"].AugmentorsDict[type] > 9)
        {
            return false;
        }
        
        Instance.Locations[obj.Location.NameOrUniqueName].Tiles[$"{obj.TileLocation}"].AugmentorsDict[type] += 1;
        return true;
    }

    public static int GetAugCount(SObject obj, AugmentorType type)
    { 
        var tile = Instance.Locations[obj.Location.NameOrUniqueName].Tiles[$"{obj.TileLocation}"].AugmentorsDict;
        switch (type)
        {
            case AugmentorType.Speed: return tile[type]; 
            default: return -1; 
        }
    }
    
    
    public AugmentorType AugmentorIds(Item item)
    {
        switch (item.ItemId)
        {
            case "crashy.speed_augmentor": return AugmentorType.Speed;
            case "crashy.efficiency_augmentor": return AugmentorType.Efficiency;
            default: return AugmentorType.None;
        }
    }

    public static bool PerformEffiAugRNG(SObject obj)
    {
        int count = CurrentTile(obj).AugmentorsDict[AugmentorType.Efficiency];
        return rand.Next(0, 50) < count;
    }

    public static MachineTileInstance CurrentTile(SObject obj)
    {
        return Instance.Locations[obj.Location.NameOrUniqueName].Tiles[obj.TileLocation.ToString()];
    }
    
    public static int CalculateAugmentedTime(int originalMinutesUntilReady, int augCount)
    {
        int newMinutesUntilReady = (int)Math.Round(originalMinutesUntilReady * (1f / (1f + 0.9f * Math.Log( 1f + augCount, 10))) / 10) * 10;
        return newMinutesUntilReady;
    }
    public static bool DoesThisContainAnyAugmentor(SObject obj)
    {
        if (Instance.Locations.TryGetValue(obj.Location.NameOrUniqueName, out var location)
            && location.Tiles.TryGetValue($"{obj.TileLocation}", out var tile))
            return tile.AugmentorsDict.Any();
        return false;
    }
    public static bool DoesThisContainAugmentor(SObject obj, AugmentorType type)
    {
        if (!Instance.Locations.TryGetValue(obj.Location.NameOrUniqueName, out var location)
            || !location.Tiles.TryGetValue($"{obj.TileLocation}", out var tile))
            return false;
        return tile.AugmentorsDict.ContainsKey(type);
    }

    public static void DestroyAugmentor(Vector2 tile, GameLocation location)
    {
        var currentTile = Instance.Locations[location.NameOrUniqueName].Tiles[tile.ToString()];
        var augmentorsDict = currentTile.AugmentorsDict;
        var tileToPixels = new Vector2(tile.X * Game1.tileSize, tile.Y * Game1.tileSize);
        foreach (var augType in augmentorsDict)
        {
            var augmentor = Augmentor.Create(augType.Key, augType.Value);
            Game1.createMultipleItemDebris(augmentor, tileToPixels, 0,location);
        }

        Instance.Locations[location.NameOrUniqueName].Tiles.Remove(tile.ToString());
    }
}
public class MachineTileInstance
{ 
    public Vector2 MachineTile { get; set; }
    internal int _x { get; set; }
    internal int _y { get; set; }
    public Dictionary<AugmentorType,int> AugmentorsDict { get; set; }
    public MachineTileInstance(Vector2 machineTile)
    { 
        MachineTile = machineTile;
        AugmentorsDict = new Dictionary<AugmentorType, int>();
        _x = (int)MachineTile.X;
        _y = (int)MachineTile.Y;
    }
}
public class WorldLocationInstance
{
    public string LocationName { get; set; }
    public Dictionary<string, MachineTileInstance> Tiles { get; set; }

public WorldLocationInstance(string location)
    {
        this.LocationName = location;
        this.Tiles = new Dictionary<string,MachineTileInstance>();
    }
}