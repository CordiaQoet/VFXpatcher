using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace VFXPatcher.Windows;

public class FixerWindow : Window, IDisposable
{
    private Plugin plugin;
    private string modSelected;
    private Dictionary<string, VfxFileContent> vfxFileContent = new Dictionary<string, VfxFileContent>();

    public FixerWindow(Plugin plugin) : base(
        "VFX Patcher: Fixer")
    {
        this.Size = new Vector2(450, 75);
        this.SizeCondition = ImGuiCond.Always;

        this.plugin = plugin;
    }

    public void Dispose()
    {
        
    }

    public void Fixer(string _modSelected, Dictionary<string, VfxFileContent> _vfxFileContent)
    {
        modSelected = _modSelected;
        vfxFileContent = _vfxFileContent;
        plugin.DrawFixerUI();
    }
    public override void Draw()
    {
        ImGui.Text("Soon(TM)");
        ImGui.Text($"{modSelected}");
    }
}

