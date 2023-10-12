using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace VFXPatcher.Windows;

public class FixerWindow : Window, IDisposable
{
    private FixerWindow fixer;
    public FixerWindow(Plugin plugin) : base(
        "VFX Patcher: Fixer")
    {
        this.Size = new Vector2(450, 75);
        this.SizeCondition = ImGuiCond.Always;

        this.fixer = plugin.FixerWindow;
    }

    public void Dispose()
    {
        
    }

    public override void Draw()
    {
        ImGui.Text("Soon(TM)");
    }
}

