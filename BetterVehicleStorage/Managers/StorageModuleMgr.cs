namespace BetterVehicleStorage.Managers;

using System;
using System.Collections.Generic;
using Nautilus.Crafting;
using UnityEngine;
using static CraftData;

public static class StorageModuleMgr
{
    public static readonly IDictionary<TechType, StorageModule> Modules = new Dictionary<TechType, StorageModule>();

    public static readonly IList<string> NeedMappingMethods = new List<string>()
    {
        "OnHandClick",
        "OpenFromExternal",
        "OpenPDA"
    };

    public static readonly IList<string> NeedFakeTechTypeMethods = new List<string>()
    {
        "RebuildVehicleScreen"
    };

    public static Dictionary<string, Vector2> Sizes = new Dictionary<string, Vector2>()
    {
        {"StorageModuleMk2", new Vector2(8, 4)},
        {"StorageModuleMk3", new Vector2(8, 6)},
        {"StorageModuleMk4", new Vector2(8, 8)},
        {"StorageModuleMk5", new Vector2(8, 10)}
    };

    public static Dictionary<string, TechType> SpecialIngredients = new Dictionary<string, TechType>()
    {
        {"StorageModuleMk2", TechType.Lithium},
        {"StorageModuleMk3", TechType.AluminumOxide},
        {"StorageModuleMk4", TechType.Nickel},
        {"StorageModuleMk5", TechType.Kyanite}
    };

    public static List<string[]> registrationStrings = new List<string[]>()
    {
        new string[] {"StorageModuleMk2", "Storage Module Mk2", "An improved storage module with 32 slots."},
        new string[] {"StorageModuleMk3", "Storage Module Mk3", "An improved storage module with 48 slots."},
        new string[] {"StorageModuleMk4", "Storage Module Mk4", "An improved storage module with 64 slots."},
        new string[] {"StorageModuleMk5", "Storage Module Mk5", "An improved storage module with 80 slots."}
    };

    /// <summary>
    /// Registers modules mk2 through mk5 to the game.
    /// </summary>
    public static void RegisterModules()
    {
        TechType lastRegistered = TechType.VehicleStorageModule;
        foreach (var registrationString in registrationStrings)
        {
            var size = Sizes[registrationString[0]];
            var specialIngredient = SpecialIngredients[registrationString[0]];

            var recipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>
                {
                    new Ingredient(lastRegistered, 1),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(specialIngredient, 3),
                    new Ingredient(TechType.AramidFibers, 1)
                }
            };

            var module = StorageModule.CreateAndRegister(registrationString[0], registrationString[1], registrationString[2], size, recipe);
            if (module.TechType == TechType.None) throw new Exception("Failed to register " + registrationString[0] + " module.");
            lastRegistered = module.TechType;
            Modules.Add(module.TechType, module);
        }

        foreach (StorageModule storageModule in Modules.Values)
        {
            storageModule.CustomPrefab.Register();
        }
    }

    public static bool IsStorageModule(TechType techType)
    {
        return techType == TechType.VehicleStorageModule || Modules.ContainsKey(techType);
    }

    public static bool IsTorpedoModule(TechType techType)
    {
        return techType == TechType.ExosuitTorpedoArmModule || techType == TechType.SeamothTorpedoModule;
    }

    public static void UpdateSeamothStorage(ref SeaMoth seaMoth, int slotId, TechType techType, bool added)
    {
        var physicalStorageModuleAmount = CalculateStorageModuleAmount(seaMoth.modules);
        for (int i = 0; i < 4; i++)
        {
            //Storage activation
            var enabled = i < physicalStorageModuleAmount;
            seaMoth.storageInputs[i].SetEnabled(enabled);
        }
    }

    public static void UpdateExosuitStorage(ref Exosuit exosuit, int slotId, TechType techType, bool added)
    {
        if (!IsStorageModule(techType)) return;
        UpdateExosuitStorageSize(ref exosuit);
    }

    public static void UpdateExosuitStorageSize(ref Exosuit exosuit)
    {
        var allModules = exosuit.GetSlotBinding();
        TechType storageTechType = TechType.None;
        for (int i = 0; i < allModules.Length; i++)
        {
            if (!IsStorageModule(allModules[i])) continue;
            storageTechType = allModules[i];
            break;
        }

        if (storageTechType == TechType.None)
        {
            exosuit.storageContainer.Resize(4, 4);
            return;
        }

        ;
        var module = Modules.ContainsKey(storageTechType) ? Modules[storageTechType] : null;
        exosuit.storageContainer.Resize(module?.StorageWidth ?? 6, module?.StorageHeight ?? 4);
    }

    internal static ItemsContainer GetStorageInSlot(ref Vehicle vehicle, int slotId, TechType techType,
        string callingMethodName)
    {
        var allModules = vehicle.GetSlotBinding();
        int mappedStorageSlot = slotId;
        if (NeedMappingMethods.Contains(callingMethodName))
        {
            List<int> allStorageModules = new List<int>();
            for (int i = 0; i < allModules.Length; i++)
            {
                if (!IsStorageModule(allModules[i])) continue;
                allStorageModules.Add(i);
            }

            mappedStorageSlot = allStorageModules[slotId];
        }

        InventoryItem inventoryItem = vehicle.GetSlotItem(mappedStorageSlot);
        return GetItemsContainerFromIventoryItem(inventoryItem, allModules[mappedStorageSlot]);
    }

    public static void fixOnResize(ref uGUI_ItemsContainer itemsContainer, int width, int height)
    {
        if (height == 10)
            itemsContainer.rectTransform.anchoredPosition =
                new Vector2(itemsContainer.rectTransform.anchoredPosition.x, -55f);
    }

    private static ItemsContainer GetItemsContainerFromIventoryItem(InventoryItem inventoryItem, TechType techType)
    {
        if (inventoryItem == null) return null;
        var module = Modules.ContainsKey(techType) ? Modules[techType] : null;
        Pickupable pickupable = inventoryItem.item;
        SeamothStorageContainer storageContainer = pickupable.GetComponent<SeamothStorageContainer>();
        if (storageContainer == null) return null;
        ItemsContainer itemsContainer = storageContainer.container;
        itemsContainer.Resize(module?.StorageWidth ?? 4, module?.StorageHeight ?? 4);
        return itemsContainer;
    }

    internal static bool IsAllowedToRemove(SeaMoth seaMoth, Pickupable pickupable, bool verbose)
    {
        if (!IsStorageModule(pickupable.GetTechType())) return true;
        SeamothStorageContainer component = pickupable.GetComponent<SeamothStorageContainer>();
        if (component == null) return true;
        var flag = component.container.count == 0;
        if (verbose && !flag)
        {
            ErrorMessage.AddDebug(Language.main.Get("SeamothStorageNotEmpty"));
        }

        return flag;
    }

    internal static bool IsAllowedToRemoveFromExosuit(Exosuit exosuit, Pickupable pickupable, bool verbose)
    {
        if (!IsStorageModule(pickupable.GetTechType())) return true;
        var flag = exosuit.storageContainer.container.count == 0;
        if (verbose && !flag)
        {
            ErrorMessage.AddDebug("Storage must be empty in order to upgrade it.");
        }

        return flag;
    }

    internal static bool AllowedToAdd(Equipment equipment, string slot, Pickupable pickupable, bool verbose,
        ref bool __result)
    {
        var isSeaMoth = equipment.owner.GetComponent<SeaMoth>() != null;
        var isExosuit = equipment.owner.GetComponent<Exosuit>() != null;
        var isOtherVehicle = equipment.owner.GetComponent<Vehicle>() != null;

        if (!IsStorageModule(pickupable.GetTechType())) return true;
        if (isSeaMoth && CalculateStorageModuleAmount(equipment) < 4) return true;
        if (isExosuit && CalculateStorageModuleAmount(equipment) == 0) return true;
        if (isOtherVehicle && CalculateStorageModuleAmount(equipment) < 4) return true;
        __result = false;

        if (verbose)
        {
            if (isSeaMoth || isOtherVehicle)
            {
                ErrorMessage.AddError(
                                       "You can only equip up to 4 storage modules.");
            }
            else if (isExosuit)
            {
                ErrorMessage.AddError(
                                       "You can only equip 1 storage module.");
            }
        }

        return false;
    }

    internal static bool GetTechTypeInSlot(Equipment equipment, string slot, ref TechType __result,
        string callingMethodName)
    {
        if (!NeedFakeTechTypeMethods.Contains(callingMethodName)) return true;
        var isExosuit = equipment.owner.GetComponent<Exosuit>() != null;
        if (isExosuit) return true;
        var allModules = equipment.GetEquipment();
        List<string> slotNames = new List<string>()
        {
            "SeamothModule1",
            "SeamothModule2",
            "SeamothModule3",
            "SeamothModule4"
        };
        List<string> allStorageModules = new List<string>();
        while (allModules.MoveNext())
        {
            var module = allModules.Current;
            if (module.Value == null || module.Value.item == null ||
                !IsStorageModule(module.Value.item.GetTechType())) continue;
            allStorageModules.Add(module.Key);
        }

        if (slotNames.IndexOf(slot) >= allStorageModules.Count)
        {
            __result = TechType.None;
            return false;
        }

        string mappedStorageSlot = allStorageModules[slotNames.IndexOf(slot)];
        InventoryItem itemInSlot = equipment.GetItemInSlot(mappedStorageSlot);
        if (itemInSlot == null || itemInSlot.item == null)
        {
            __result = TechType.None;
            return false;
        }

        Pickupable item = itemInSlot.item;
        if (IsStorageModule(item.GetTechType()))
        {
            __result = TechType.VehicleStorageModule;
            return false;
        }

        __result = TechType.None;
        return false;
    }

    internal static void OnUpgradeModuleUse(Vehicle vehicle, TechType techType, int slotId)
    {
        if (!IsStorageModule(techType)) return;
        var slotItem = vehicle.GetSlotItem(slotId);
        var itemsContainer = GetItemsContainerFromIventoryItem(slotItem, techType);
        PDA pda = Player.main.GetPDA();
        Inventory.main.SetUsedStorage(itemsContainer);
        pda.Open(PDATab.Inventory);
    }

    internal static void OnUpgradeModuleUseFromExosuit(Exosuit exosuit, TechType techType, int slotId)
    {
        if (!IsStorageModule(techType)) return;
        var slotItem = exosuit.GetSlotItem(slotId);
        var itemsContainer = exosuit.storageContainer.container;
        PDA pda = Player.main.GetPDA();
        Inventory.main.SetUsedStorage(itemsContainer);
        pda.Open(PDATab.Inventory);
    }

    private static int CalculateStorageModuleAmount(Equipment equipment)
    {
        var amount = equipment.GetCount(TechType.VehicleStorageModule);
        foreach (var module in Modules.Keys)
        {
            amount += equipment.GetCount(module);
        }

        return amount;
    }
}