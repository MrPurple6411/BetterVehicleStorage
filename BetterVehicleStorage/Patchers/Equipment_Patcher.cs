namespace BetterVehicleStorage.Patchers;

using System.Diagnostics;
using Common;
using HarmonyLib;
using Managers;

[HarmonyPatch(typeof(Equipment))]
public class Equipment_Patcher
{
    [HarmonyPatch(nameof(Equipment.AllowedToAdd)), HarmonyPrefix]
    internal static bool Prefix(Equipment __instance, string slot, Pickupable pickupable, bool verbose,
        ref bool __result)
    { 
        return StorageModuleMgr.AllowedToAdd(__instance, slot, pickupable, verbose, ref __result);
    }

    [HarmonyPatch(nameof(Equipment.GetTechTypeInSlot)), HarmonyPrefix]
    internal static bool Prefix(Equipment __instance, string slot, ref TechType __result)
    {
        var callingMethodName = new StackFrame(2).GetMethod().Name;
        return StorageModuleMgr.GetTechTypeInSlot(__instance, slot, ref __result, callingMethodName);
    }
}
