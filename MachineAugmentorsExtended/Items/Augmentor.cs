using System.Runtime.Serialization;
using System.Xml.Serialization;
using MachineAugmentorsExtended.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Logging;
using StardewValley.Objects;

namespace MachineAugmentorsExtended.Items;

[XmlType("Mods_MachineAugmentorsExtended_Augmentor")]
[XmlRoot(ElementName = "Augmentor", Namespace = "")] 
[KnownType(typeof(SpeedAugmentor))]
[XmlInclude(typeof(SpeedAugmentor))]
[KnownType(typeof(EfficiencyAugmentor))]
[XmlInclude(typeof(EfficiencyAugmentor))]
public abstract class Augmentor : SObject
{
    
    
    private static string BaseAugmentorId = ModEntry.Instance.ModManifest.UniqueID; 
    public AugmentorType AugmentorType { get; }
    public abstract Augmentor CreateOne();
    
    protected Augmentor(AugmentorType type) : base($"crashy.{type.ToString().ToLower()}_augmentor", 1, false, -1, 0)
    {
        this.AugmentorType = type;
        this.Price = 2200;
    }
    public Augmentor(string itemId, int quant) : this(AugmentorType.Speed) {}
    
    public static Augmentor Create(AugmentorType type, int amt = 1)
    {
        switch (type)
        {
            case (AugmentorType.Speed): return new SpeedAugmentor() { Stack = amt };
            case (AugmentorType.Efficiency): return new EfficiencyAugmentor() { Stack = amt };
            default: return new SpeedAugmentor() {Stack = amt};
        }
    }
    
    public static void drawIcon(AugmentorType type,SpriteBatch spriteBatch, Vector2 offset, float baseScale, float layerDepth,
        float opacity)
    {
        Texture2D tex;
        Rectangle sourceRect;
        SpriteEffects effects;
        
        switch (type)
        {
            case (AugmentorType.Speed):
            {
                tex = Game1.mouseCursors;
                sourceRect = new Rectangle(130, 430, 10, 7);
                effects = SpriteEffects.FlipHorizontally;
                break;
            }
            case (AugmentorType.Efficiency):
            {
                tex = Game1.mouseCursors;
                sourceRect = new Rectangle(80, 428, 10, 10);
                effects = SpriteEffects.None;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        spriteBatch.Draw(tex, offset, sourceRect, Color.White * opacity,0f,new Vector2(0,0), baseScale, effects, layerDepth);
        
    }
    
    public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
    {
        ParsedItemData itemDataOrErrorItem = ItemRegistry.GetDataOrErrorItem(this.QualifiedItemId);
        Texture2D texture = itemDataOrErrorItem.GetTexture();
        float layerDepth = Math.Max(0.0f, (float) (f.StandingPixel.Y + 3) / 10000f);
        float layerDepthAndSome = Math.Max(0.0f, (float) (f.StandingPixel.Y + 4) / 10000f);
        var offsetPosition = new Vector2((objectPosition.X + 44), (objectPosition.Y + 40f));
        
        spriteBatch.Draw(texture, objectPosition,itemDataOrErrorItem.GetSourceRect(0, this.parentSheetIndex), 
            Color.White,0.0f, Vector2.Zero,4f, SpriteEffects.None, layerDepth);
         drawIcon(this.AugmentorType,spriteBatch, offsetPosition, 2.2f, layerDepthAndSome, 1f);
    }

    public override void drawInMenu(
        SpriteBatch spriteBatch,
        Vector2 location,
        float scaleSize,
        float transparency,
        float layerDepth,
        StackDrawType drawStackNumber,
        Color color,
        bool drawShadow)
    {
        ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(this.QualifiedItemId);
        var sourceRect = itemData.GetSourceRect();
        float num = scaleSize;
        var offsetPosition = new Vector2((location.X + 44), (location.Y + 40f));
        
        spriteBatch.Draw(
            itemData.GetTexture(), location + new Vector2(32f, 32f),
            (sourceRect), color * transparency, 0.0f, 
            new Vector2((float) (sourceRect.Width / 2), (float) (sourceRect.Height / 2)), 
            4f * num, SpriteEffects.None, layerDepth
            );
        drawIcon(AugmentorType,spriteBatch, offsetPosition, 2.2f, layerDepth + 0.1f, 1f);
        this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth,drawStackNumber, color);

    }
    
    public override bool isPlaceable() { return false; }
    public override bool isPassable() { return true; }
    protected override Item GetOneNew() => CreateOne();
    

}