using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using HarmonyLib;
using MachineAugmentorsExtended.Data;
using MachineAugmentorsExtended.Items;
using MachineAugmentorsExtended.Patches;
//using MachineAugmentorsExtended.Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using xTile.Tiles;
using MachineAugmentorsExtended.ThirdParty;
using StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;
using StardewValley.Objects;
using StardewValley.GameData;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Objects;
using StardewValley.Mods;
using Vector2 = System.Numerics.Vector2;

namespace MachineAugmentorsExtended
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        private ModConfig Config;
        public static string texSource;
        public static ModEntry Instance;

        public ModData ModData;
        private GamePatches GamePatches;
        public static Texture2D SpeedAugmentorTexture { get; set; }

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Log.Monitor = this.Monitor;
            this.Config = helper.ReadConfig<ModConfig>();
            
            // Harmony stuff
            var harmony = new Harmony(this.ModManifest.UniqueID);
            // harmony.Patch(
            //     original: AccessTools.Method(typeof(StardewValley.Object),
            //         nameof(StardewValley.Object.performObjectDropInAction)),
            //     prefix: new HarmonyMethod(typeof(GamePatches), nameof(GamePatches.performObjectDropInAction_Prefix))
            // );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object),
                    nameof(StardewValley.Object.performObjectDropInAction)),
                postfix: new HarmonyMethod(typeof(GamePatches), nameof(GamePatches.performObjectDropInAction_Postfix))
            );
            // harmony.Patch(
            //     original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.checkForAction)),
            //     prefix: new HarmonyMethod(typeof(GamePatches), nameof(GamePatches.checkForAction_Prefix))
            // );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.draw), [typeof(SpriteBatch),typeof(int), typeof(int), typeof(float)]),
                postfix: new HarmonyMethod(typeof(GamePatches), nameof(GamePatches.draw_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.OutputMachine)),
                prefix: new HarmonyMethod(typeof(GamePatches), nameof(GamePatches.OutputMachine_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.OutputMachine)),
                postfix: new HarmonyMethod(typeof(GamePatches), nameof(GamePatches.OutputMachine_Postfix))
            );
            // end of harmony stuff

            // AugData = helper.Data.ReadJsonFile<AugmentorData>("assets/data.json") ??
            //           throw new InvalidOperationException();
            // Log.Debug($"{AugData.Id}");

            ModData = new ModData(new List<AugmentorData>());
            foreach (AugmentorType aug in Enum.GetValues(typeof(AugmentorType)))
            {
                if (aug != AugmentorType.None)
                {
                    AugmentorData dat = new AugmentorData
                    {
                        Id = $"crashy.{aug.ToString()}_augmentor".ToLower(),
                        Object = new ObjectData()
                        {
                            Name = $"{aug.ToString()} Augmentor",
                            DisplayName = $"{aug.ToString()} Augmentor",
                            Description = $"{aug.ToString()} Augmentor. Attach to machines.",
                            Type = "Crafting",
                            Category = -8,
                            Texture = "LooseSprites\\Cursors",
                            SpriteIndex = 890
                        }
                    };
                    ModData.AugmentorDataList.Add(dat);
                }

            }
            helper.Data.WriteJsonFile($"assets/data.json", ModData);

            // load assets
            SpeedAugmentorTexture = helper.GameContent.Load<Texture2D>("LooseSprites/Cursors");

            helper.Events.GameLoop.GameLaunched += this.GameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saved += this.OnGameSaved;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTick;
            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Display.MenuChanged += this.DisplayMenuChanged;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            if (e.IsCurrentLocation)
            {
                if (e.Removed.Any())
                {
                    foreach(KeyValuePair<Microsoft.Xna.Framework.Vector2,SObject> entry in e.Removed)
                    {
                        if (MachinesData.Instance.Locations[e.Location.NameOrUniqueName].Tiles
                            .ContainsKey(entry.Key.ToString()))
                        {
                            MachinesData.DestroyAugmentor(entry.Key, e.Location);
                        }
                    }
                }
            }
        }

        private void OnUpdateTick(object? sender, UpdateTickedEventArgs e)
        {
          //  if(Context.IsWorldReady)

        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            MachinesData.Instance = new MachinesData();
            MachinesData.Instance = this.Helper.Data.ReadJsonFile<MachinesData>("augmentedlocations.json") ?? 
                                    new MachinesData();
        }

        private void OnGameSaved(object? sender, SavedEventArgs e)
        {
            this.Helper.Data.WriteJsonFile("augmentedlocations.json", MachinesData.Instance);
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (e.Button == SButton.B)
            {
                Augmentor speed = Augmentor.Create(AugmentorType.Speed);
                Game1.player.addItemToInventory(speed);
                Augmentor effy = Augmentor.Create(AugmentorType.Efficiency);
                Game1.player.addItemToInventory(effy);
            }

            if (e.Button == SButton.C)
            {
                Utility.TryOpenShopMenu("Traveler",null,true);
            }
        }

        private void GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            ISpaceCore spaceCore = Helper.ModRegistry.GetApi<ISpaceCore>("spacechase0.SpaceCore");
            if (spaceCore == null)
            {
                Instance.Monitor.Log($"Couldn't access SpaceCore for " + Instance.ModManifest.Name, LogLevel.Error);
                return;
            }

            spaceCore.RegisterSerializerType(typeof(Augmentor));

            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu == null)
            {
                Instance.Monitor.Log($"Couldn't access GenericModConfigMenu for " + Instance.ModManifest.Name,
                    LogLevel.Error);
                return;
            }

            var informant = Helper.ModRegistry.GetApi<IInformant>("Slothsoft.Informant");
            if (informant == null)
            {
                Instance.Monitor.Log($"Couldn't access Informant for " + Instance.ModManifest.Name, LogLevel.Error);
                return;
            }

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                getValue: () => this.Config.EnablePriceVariation,
                setValue: value => this.Config.EnablePriceVariation = value,
                name: () => "Enable Price Variation",
                tooltip: () => "Enable random price variation for the augmentors sold at the travelling merchant."
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                getValue: () => this.Config.BasePrice,
                setValue: value => this.Config.BasePrice = (int)value,
                name: () => "Base Price",
                tooltip: () => "The base price for augmentors sold by the travelling merchant.",
                min: 100,
                max: 999999
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                getValue: () => this.Config.PriceVariation,
                setValue: value => this.Config.PriceVariation = value,
                name: () => "Price Variation",
                tooltip: () => "The maximum percentage price variation for items sold by the travelling merchant.",
                min: 0.0f,
                max: 1.0f
            );
            spaceCore.RegisterSerializerType(typeof(Augmentor));

        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, ObjectData>().Data;
                    foreach (var aug in ModData.AugmentorDataList)
                    {
                        aug.Object.DisplayName = aug.Object.Name;
                        data[aug.Id] = aug.Object;
                    }
                });
            }

        }

        private Dictionary<ISalable, ItemStockInformation>? DailyStock = null;

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            DailyStock = null;
        }

        private void DisplayMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            Random random = new Random();
            // ignore if the menu isn't the travelling merchant
            if (e.NewMenu is ShopMenu shop && shop.ShopId == "Traveler")
            {
                if (DailyStock == null)
                {
                    DailyStock = new Dictionary<ISalable, ItemStockInformation>();

                    float priceVariation = Config.PriceVariation;
                    int basePrice = Config.BasePrice;
                    int priceRange = (int)(priceVariation * 10); // Scale up float to work with integers
                    List<float> floatList = Enumerable.Range(-priceRange, 2 * priceRange + 1).Select(x => 1 + x / 10f)
                        .ToList();

                    float listChoice = floatList[random.Next(floatList.Count)];
                    int todaysPrice = (int)(basePrice * listChoice);
                    

                    DailyStock.Add(new SpeedAugmentor().GetSalableInstance(), new ItemStockInformation(todaysPrice, 10));
                }

                foreach (KeyValuePair<ISalable, ItemStockInformation> item in DailyStock)
                {
                    shop.AddForSale(item.Key, item.Value);
                }
            }

        }
    }
}