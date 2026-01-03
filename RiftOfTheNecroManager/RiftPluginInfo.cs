using BepInEx;
using System;

namespace RiftOfTheNecroManager;


public class RiftPluginInfo : State<PluginInfo, RiftPluginInfo> {
    public string Name => Instance.Metadata.Name;
    public string Version => Instance.Metadata.Version.ToString();
    public string GUID => Instance.Metadata.GUID;
    
    public NecroManagerInfoAttribute Attribute => attribute ??= NecroManagerInfoAttribute.GetAttribute(Instance.Instance.GetType());
    private NecroManagerInfoAttribute? attribute;
    
    public string GetMenuName() {
        var pluginName = Util.PascalToSpaced(Instance.Metadata.Name);
        var menuName = Attribute.MenuNameOverride;
        if(string.IsNullOrWhiteSpace(menuName)) {
            return pluginName;
        }
        
        var isOverrideValid = string.Equals(pluginName.Replace(" ", ""), menuName.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase);
        if(isOverrideValid) {
            return string.Join(" ", menuName.Split(" ", StringSplitOptions.RemoveEmptyEntries));
        } else {
            Log.Warning($"The menu name override \"{menuName}\" does not match the plugin name \"{pluginName}\". The override will be ignored.");
            return pluginName;
        }
    }
}
