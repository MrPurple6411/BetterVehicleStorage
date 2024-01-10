namespace BetterVehicleStorage;

using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Managers;
using Nautilus.Handlers;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID, Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInIncompatibility("com.ahk1221.smlhelper")]
public class Main: BaseUnityPlugin
{
    internal const string WorkBenchTab = "Storage";
    internal static new ManualLogSource Logger { get; private set; }
    internal static string AssetsFolder { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

    public void Awake()
    {
        if (Main.Logger != null)
        {
            Logger.LogError("Only one instance of this plugin can be active at a time");
            Destroy(this);
            return;
        }

        Main.Logger = base.Logger;
        Logger.LogInfo("Started patching v" + MyPluginInfo.PLUGIN_VERSION);

        CraftTreeHandler.AddTabNode(
            CraftTree.Type.Workbench,
            WorkBenchTab,
            "Storage Modules",
            SpriteManager.Get(TechType.VehicleStorageModule));

        StorageModuleMgr.RegisterModules();
        
        CraftDataHandler.SetQuickSlotType(TechType.VehicleStorageModule, QuickSlotType.Instant);

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);

        Logger.LogInfo("Finished patching");
    }
}