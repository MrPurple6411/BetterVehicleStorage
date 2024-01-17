namespace BetterVehicleStorage;

using Common;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using System.IO;
using UnityEngine;

public class StorageModule
{
    public readonly int StorageWidth;
    public readonly int StorageHeight;

    public CustomPrefab CustomPrefab { get; private set; }
    public TechType TechType => CustomPrefab?.Info.TechType ?? TechType.None;

    private StorageModule(Vector2 size)
    {
        StorageWidth = (int)size.x;
        StorageHeight = (int)size.y;
    }

    public static StorageModule CreateAndRegister(string classId, string friendlyName, string description, Vector2 size, RecipeData recipe)
    {
        var storageModule = new StorageModule(size);
        var prefabInfo = PrefabInfo.WithTechType(classId, friendlyName, description, "English", false);

        var iconPath = Path.Combine(Main.AssetsFolder, $"{classId}.png"); 
        if (File.Exists(iconPath))
            prefabInfo.WithIcon(ImageUtils.LoadSpriteFromFile(iconPath));
        else
        {
            QuickLogger.Info($"No icon found for {classId}, using default icon");
            prefabInfo.WithIcon(SpriteManager.Get(TechType.VehicleStorageModule));
        }
        

        var prefab = new CustomPrefab(prefabInfo);

        if(!CraftData.GetBuilderIndex(TechType.VehicleStorageModule, out var group, out var category, out _))
        {
            group = TechGroup.VehicleUpgrades;
            category = TechCategory.VehicleUpgrades;
        }

        // determin what the storage module should be sorted after
        var sortAfter = TechType.VehicleStorageModule;

        string lastChar = classId.Substring(classId.Length - 1);

        if (int.TryParse(lastChar, out int Mk))
        {
            if (Mk > 1)
            {
                var previousMk = $"StorageModuleMk{Mk - 1}";
                if (EnumHandler.TryGetValue(previousMk, out TechType techType))
                {
                    sortAfter = techType;                    
                }
            }
        }

        prefab.SetRecipe(recipe)
            .WithFabricatorType(CraftTree.Type.Workbench)
            .WithStepsToFabricatorTab(Main.WorkBenchTab);

        prefab.SetUnlock(TechType.Workbench)
            .WithPdaGroupCategoryAfter(group, category, sortAfter);
        
        prefab.SetEquipment(EquipmentType.VehicleModule).WithQuickSlotType(QuickSlotType.Instant);

        prefab.SetGameObject(new CloneTemplate(prefabInfo, TechType.VehicleStorageModule));

        storageModule.CustomPrefab = prefab;

        return storageModule;
    }
}