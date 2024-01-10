namespace BetterVehicleStorage.Patchers;

using System.Diagnostics;
using HarmonyLib;
using Managers;

[HarmonyPatch(typeof(Vehicle))]
internal class Vehicle_Patcher
{
    [HarmonyPatch(nameof(Vehicle.OnUpgradeModuleUse)), HarmonyPostfix]
    internal static void Vehicle_OnUpgradeModuleUse(ref Vehicle __instance, TechType techType, int slotID)
    {
        if (__instance is Exosuit exosuit)
        StorageModuleMgr.OnUpgradeModuleUseFromExosuit(exosuit, techType, slotID);
    }

    [HarmonyPatch(nameof(Vehicle.GetStorageInSlot)), HarmonyPrefix]
    internal static bool Vehicle_GetStorageInSlot(Vehicle __instance, int slotID, TechType techType, ref ItemsContainer __result)
    {
        if (!StorageModuleMgr.IsStorageModule(techType)) return true;
        var callingMethodName = new StackFrame(2).GetMethod().Name;
        __result = StorageModuleMgr.GetStorageInSlot(ref __instance, slotID, techType, callingMethodName);
        return false;
    }
}