using BepInEx;
using HarmonyLib;

namespace RiftOfTheNecroManager;

public abstract partial class RiftPluginInternal : BaseUnityPlugin {
    // contains functionality shared by all rift plugins
    // see RiftPluginInternal.cs for static functionality used by this assembly
    
    public RiftPluginInfo Metadata => RiftPluginInfo.Of(Info);
    
    internal RiftPluginInternal() {
        // internal prevents direct inheritance outside this assembly
        // do not change access modifier
        LoadedPlugins[Info.Metadata.GUID] = this;
    }
    
    protected virtual void Initialize() {
        var assembly = GetType().Assembly;
        var harmony = new Harmony(Metadata.GUID);
        harmony.PatchAll(assembly);
        Log.SetLog(assembly, Logger);
        Setting.BindAssembly(Config, assembly);
    }
}


[BepInDependency(NECROMANAGER_GUID)]
public abstract class RiftPlugin : RiftPluginInternal {
}
