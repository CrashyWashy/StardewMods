using System;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace MachineAugmentorsExtended.ThirdParty;

/// <summary>
/// Base class for the entire API. Can be used to add custom information providers.<br/>
/// <b>API Version:</b> 1.4.0
/// </summary>
public interface IInformant
{

    /// <summary>
    /// Adds a tooltip generator for the <see cref="TerrainFeature"/>(s) under the mouse position.
    /// </summary>
    void AddTerrainFeatureTooltipGenerator(string id, Func<string> displayName, Func<string> description, Func<TerrainFeature, string> generator);

    /// <summary>
    /// Adds a tooltip generator for the <see cref="Object"/>(s) under the mouse position.
    /// </summary>
    void AddObjectTooltipGenerator(string id, Func<string> displayName, Func<string> description, Func<SObject, string?> generator);

    /// <summary>
    /// Adds a decorator for the <see cref="Item"/>(s) under the mouse position.
    /// </summary>
    void AddItemDecorator(string id, Func<string> displayName, Func<string> description, Func<Item, Texture2D?> decorator);
}