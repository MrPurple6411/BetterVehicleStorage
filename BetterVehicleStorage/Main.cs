namespace BetterVehicleStorage;

using System.IO;
using System.Reflection;
using BepInEx;
using Common;
using HarmonyLib;
using Managers;
using Nautilus.Handlers;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID, Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInIncompatibility("com.ahk1221.smlhelper")]
#if SUBNAUTICA
[BepInProcess("Subnautica.exe")]
#elif BELOWZERO
[BepInProcess("SubnauticaZero.exe")]
#endif
public class Main: BaseUnityPlugin
{
    internal const string WorkBenchTab = "Storage";
    internal static string AssetsFolder { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

    public void Awake()
    {
        QuickLogger.Info("Started patching v" + MyPluginInfo.PLUGIN_VERSION);

        CraftTreeHandler.AddTabNode(
            CraftTree.Type.Workbench,
            WorkBenchTab,
            "Storage Modules",
            SpriteManager.Get(TechType.VehicleStorageModule));

        StorageModuleMgr.RegisterModules();
        
        CraftDataHandler.SetQuickSlotType(TechType.VehicleStorageModule, QuickSlotType.Instant);

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);

        QuickLogger.Info("Finished patching");
    }
}