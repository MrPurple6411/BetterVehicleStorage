namespace BetterVehicleStorage.Patchers;

using HarmonyLib;
using Managers;

[HarmonyPatch(typeof(uGUI_ItemsContainer))]
public class ItemsContainerGUI_Patcher
{
    [HarmonyPatch(nameof(uGUI_ItemsContainer.OnResize)), HarmonyPostfix]
    internal static void Postfix(ref uGUI_ItemsContainer __instance, int width, int height)
    {
        StorageModuleMgr.fixOnResize(ref __instance, width, height);
    }
}