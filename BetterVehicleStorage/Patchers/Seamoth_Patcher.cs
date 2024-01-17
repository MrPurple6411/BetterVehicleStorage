namespace BetterVehicleStorage.Patchers;

using Common;
using HarmonyLib;
using Managers;

[HarmonyPatch(typeof(SeaMoth))]
internal class Seamoth_Patcher
{
    [HarmonyPatch(nameof(SeaMoth.OnUpgradeModuleChange)), HarmonyPostfix]
    internal static void Seamoth_OnUpgradeModuleChange(ref SeaMoth __instance, int slotID, TechType techType, bool added)
    {
        StorageModuleMgr.UpdateSeamothStorage(ref __instance, slotID, techType, added);
    }

    [HarmonyPatch(nameof(SeaMoth.OnUpgradeModuleUse)), HarmonyPostfix]
    internal static void Seamoth_OnUpgradeModuleUse(ref SeaMoth __instance, TechType techType, int slotID)
    {
        StorageModuleMgr.OnUpgradeModuleUse(__instance, techType, slotID);
    }

    [HarmonyPatch(nameof(SeaMoth.IsAllowedToRemove)), HarmonyPrefix]
    internal static bool Seamoth_IsAllowedToRemove(SeaMoth __instance, Pickupable pickupable, bool verbose, ref bool __result)
    {
        __result = StorageModuleMgr.IsAllowedToRemove(__instance, pickupable, verbose);
        return false;
    }
}