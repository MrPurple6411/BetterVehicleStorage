namespace BetterVehicleStorage.Patchers;

using Common;
using HarmonyLib;
using Managers;

[HarmonyPatch(typeof(Exosuit))]
internal class Exosuit_Patcher
{
    [HarmonyPatch(nameof(Exosuit.OnUpgradeModuleChange)), HarmonyPostfix]
    internal static void Exosuit_OnUpgradeModuleChange(ref Exosuit __instance, int slotID, TechType techType, bool added)
    {
        StorageModuleMgr.UpdateExosuitStorage(ref __instance, slotID, techType, added);
    }

    [HarmonyPatch(nameof(Exosuit.IsAllowedToRemove)), HarmonyPrefix]
    internal static bool Exosuit_IsAllowedToRemove(Exosuit __instance, Pickupable pickupable, bool verbose, ref bool __result)
    {
        __result = StorageModuleMgr.IsAllowedToRemoveFromExosuit(__instance, pickupable, verbose);
        return false;
    }

    [HarmonyPatch(nameof(Exosuit.UpdateStorageSize)), HarmonyPostfix]
    internal static void Exosuit_UpdateStorageSize(Exosuit __instance)
    {
        StorageModuleMgr.UpdateExosuitStorageSize(ref __instance);
    }
}