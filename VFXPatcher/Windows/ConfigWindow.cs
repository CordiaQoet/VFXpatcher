using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace VFXPatcher.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;
    private bool penumbraDirExists = false;

    public ConfigWindow(Plugin plugin) : base(
        "VFX Patcher Config",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(450, 75);
        this.SizeCondition = ImGuiCond.Always;

        this.configuration = plugin.Configuration;
    }
    public void Init()
    {
        penumbraDirExists = !string.IsNullOrEmpty(this.configuration.ModDirectory);
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        /*var configValue = this.Configuration.SomePropertyToBeSavedAndWithADefault;
        if (ImGui.Checkbox("Random Config Bool", ref configValue))
        {
            this.Configuration.SomePropertyToBeSavedAndWithADefault = configValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.Configuration.Save();
        }*/
        var penumbraDir = this.configuration.ModDirectory;
        if (ImGui.InputTextWithHint("Penumbra Root Directory", "Enter your Penumbra Root Directory and press enter...", ref penumbraDir, 64, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            this.configuration.ModDirectory = penumbraDir;
            this.configuration.Save();
            penumbraDirExists = true;
        }
    }
}
