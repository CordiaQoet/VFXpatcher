using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using System.Linq;
using Lumina;
using Lumina.Data;
using Newtonsoft.Json.Linq;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using Dalamud.IoC;
using Dalamud.Interface.Utility.Raii;
using System.Globalization;
using Lumina.Data.Files;
using System.Text.Json;
using Lumina.Excel.GeneratedSheets;
using FFXIVClientStructs.Havok;
using System.Text;
using static FFXIVClientStructs.FFXIV.Client.UI.UI3DModule;
using Dalamud.Utility;

namespace VFXPatcher.Windows;

public class VfxFileContent
{
    public string[]? ParsedPaths { get; set; }
    public bool[]? HaveError { get; set; }
    public string[]? TargetPath { get; set; }
}

public class ModInfo
{
    public string? Name { get; set; }
    public string? Author { get; set; }
}

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;
    public readonly FileDialogManager _folderPicker = new();
    private bool isModSelected = false;
    private string modSelected = "";
    private Dictionary<string, VfxFileContent> vfxFileContent = new Dictionary<string, VfxFileContent>();
    public ModInfo modInfo = new();
    private string[]? files_scd;
    private string[]? files_atex;
    private string[]? files_avfx;
    private string[]? files_pap;
    private string[]? files_tmb;
    private bool scdExist = false;
    private bool atexExist = false;
    private bool avfxExist = false;
    private bool papExist = false;
    private bool tmbExist = false;
    private bool scdChecked = false;
    private bool atexChecked = false;
    private bool avfxParsed = false;
    private bool papParsed = false;
    private bool tmbParsed = false;
    private bool errorOnly = true;


    public MainWindow(Plugin plugin) : base(
        "VFX Patcher", ImGuiWindowFlags.HorizontalScrollbar)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public override void Draw()
    {
        /*if (ImGui.Button("Show Settings"))
        {
            this.plugin.DrawConfigUI();
        }*/


        if (isModSelected == true)
        {
            ImGui.Text($"Mod selected: {modSelected}");
            ImGui.Text($"Name: {modInfo.Name}");
            ImGui.Text($"Author: {modInfo.Author}");

            if (ImGui.Button("Unload mod"))
            {
                isModSelected = false;
                modSelected = "";
                modInfo = new();
                scdExist = false;
                atexExist = false;
                avfxExist = false;
                papExist = false;
                tmbExist = false;
                files_scd = null;
                files_atex = null;
                files_avfx = null;
                files_pap = null;
                files_tmb = null;
                scdChecked = false;
                atexChecked = false;
                avfxParsed = false;
                papParsed = false;
                tmbParsed = false;
                vfxFileContent = new Dictionary<string, VfxFileContent>();
            }

            if (scdExist)
            {
                ImGui.SameLine();
                using (ImRaii.Disabled(!(tmbExist == tmbParsed && papExist == papParsed && avfxExist == avfxParsed) || scdChecked))
                {
                    if (ImGui.Button("Check scd"))
                    {
                        foreach (string file in files_scd)
                        {
                            PluginLog.Information($"{file.Replace(modSelected, "")[1..]}");
                            //if (!Plugin.Data.FileExists(s.Replace(modSelected, "")[1..]))
                            //PluginLog.Information($"{s} not replacing vanilla file.");
                        }
                        scdChecked = true;
                    }
                    if (!(tmbExist == tmbParsed && papExist == papParsed && avfxExist == avfxParsed))
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                            ImGui.SetTooltip("First parse avfx, pap and tmp files");
                }
            }

            if (atexExist)
            {
                ImGui.SameLine();
                using (ImRaii.Disabled(!(tmbExist == tmbParsed && papExist == papParsed && avfxExist == avfxParsed) || atexChecked))
                {
                    if (ImGui.Button("Check atex"))
                    {
                        // do stuff...
                        foreach (string file in files_atex)
                        {
                            PluginLog.Information($"{file.Replace(modSelected, "")[1..]}");
                            //if (!Plugin.Data.FileExists(s.Replace(modSelected, "")[1..]))
                            //PluginLog.Information($"{s} not replacing vanilla file.");
                        }
                        atexChecked = true;
                    }
                    if (!(tmbExist == tmbParsed && papExist == papParsed && avfxExist == avfxParsed))
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                            ImGui.SetTooltip("First parse avfx, pap and tmp files");
                }
            }

            if (avfxExist)
            {
                ImGui.SameLine();
                using (ImRaii.Disabled(avfxParsed))
                {
                    if (ImGui.Button("Parse avfx"))
                    {
                        foreach (var file in files_avfx)
                        {
                            var a = ParseVfxFile(file, modSelected);
                            vfxFileContent.Add(a.Item2, new VfxFileContent() { ParsedPaths = a.Item1.ToArray(), HaveError = a.Item3.ToArray(), TargetPath = a.Item4.ToArray() });
                        }
                        avfxParsed = true;
                    }
                }
            }

            if (papExist)
            {
                ImGui.SameLine();
                using (ImRaii.Disabled(papParsed))
                {
                    if (ImGui.Button("Parse pap"))
                    {
                        foreach (var file in files_pap)
                        {
                            var a = ParseVfxFile(file, modSelected);
                            vfxFileContent.Add(a.Item2, new VfxFileContent() { ParsedPaths = a.Item1.ToArray(), HaveError = a.Item3.ToArray(), TargetPath = a.Item4.ToArray() });
                        }
                        papParsed = true;
                    }
                }
            }

            if (tmbExist)
            {
                ImGui.SameLine();
                using (ImRaii.Disabled(tmbParsed))
                {
                    if (ImGui.Button("Parse tmb"))
                    {
                        foreach (var file in files_tmb)
                        {
                            var a = ParseVfxFile(file, modSelected);
                            vfxFileContent.Add(a.Item2, new VfxFileContent() { ParsedPaths = a.Item1.ToArray(), HaveError = a.Item3.ToArray(), TargetPath = a.Item4.ToArray() });
                        }
                        tmbParsed = true;
                    }
                }
            }
            ImGui.SameLine();
            ImGui.Checkbox("Show only errors", ref errorOnly);

            if (tmbExist == tmbParsed && papExist == papParsed && avfxExist == avfxParsed && scdExist == scdChecked && atexExist == atexChecked)
            {
                if (vfxFileContent.Values.Count(x => x.HaveError.Contains(true)) > 0)
                {
                    ImGui.SameLine();
                    if (ImGui.Button("Try to fix it"))
                    {
                        // Do something...
                        this.plugin.FixerWindow.Fixer(modSelected, vfxFileContent);
                    }
                }
            }

            ImGui.Separator();
            if (scdExist)
            {
                ImGui.Text($"scd found: {files_scd.Length}");
                if (scdChecked)
                {
                    foreach (var file in files_scd)
                    {
                        ImGui.Text($"{file.Replace(modSelected, ".")}");
                    }
                } else ImGui.Text("Not checked yet.");
                ImGui.Separator();
            }

            if (atexExist)
            {
                ImGui.Text($"atex found: {files_atex.Length}");
                if (atexChecked)
                {
                    foreach (var file in files_atex)
                    {
                        ImGui.Text($"{file.Replace(modSelected, ".")}");
                    }
                } else ImGui.Text("Not checked yet.");
                ImGui.Separator();
            }

            // List all avfx files, the paths included in them when parsed and check for errors.
            if (avfxExist)
            {
                ImGui.Text($"avfx found: {files_avfx.Length}");
                if (avfxParsed)
                {
                    foreach (var file in files_avfx)
                    {
                        var e = file.Replace(modSelected, ".");
                        if (vfxFileContent[file].HaveError.Contains(true))
                        {
                            ImGui.SetNextItemOpen(true, ImGuiCond.Once);
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                        }
                        if (ImGui.TreeNode($"{e}"))
                        {
                            ImGui.Indent();
                            var content = vfxFileContent[file].ParsedPaths;
                            if (content != null)
                            {
                                for (int i = 0; i < content.Length; i++ )
                                {
                                    if (vfxFileContent[file].HaveError[i])
                                        ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), $"{content[i]}");
                                    else if (!errorOnly) ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 0.5f), content[i]);
                                }
                            }
                            if (!vfxFileContent[file].HaveError.Contains(true)) ImGui.Text("No error found.");
                            else ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), $"{vfxFileContent[file].HaveError.Count(x => x == true)} error(s) found.");
                            ImGui.Unindent();
                            ImGui.TreePop();
                        }
                        if (vfxFileContent[file].HaveError.Contains(true)) ImGui.PopStyleColor();
                    }
                } else ImGui.Text("Not parsed yet.");
                ImGui.Separator();
            }

            if (papExist)
            {
                ImGui.Text($"pap found: {files_pap.Length}");
                if (papParsed)
                {
                    foreach (var file in files_pap)
                    {
                        var e = file.Replace(modSelected, ".");
                        if (vfxFileContent[file].HaveError.Contains(true))
                        {
                            ImGui.SetNextItemOpen(true, ImGuiCond.Once);
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                        }
                        if (ImGui.TreeNode($"{e}"))
                        {
                            ImGui.Indent();
                            var content = vfxFileContent[file].ParsedPaths;
                            if (content != null)
                            {
                                for (int i = 0; i < content.Length; i++)
                                {
                                    if (vfxFileContent[file].HaveError[i])
                                        ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), $"{content[i]}");
                                    else if (!errorOnly) ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 0.5f), content[i]);
                                }
                            }
                            if (!vfxFileContent[file].HaveError.Contains(true)) ImGui.Text("No error found.");
                            else ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), $"{vfxFileContent[file].HaveError.Count(x => x == true)} error(s) found.");
                            ImGui.Unindent();
                            ImGui.TreePop();
                        }
                        if (vfxFileContent[file].HaveError.Contains(true)) ImGui.PopStyleColor();
                    }
                }
                else ImGui.Text("Not parsed yet.");
                ImGui.Separator();
            }

            if (tmbExist)
            {
                ImGui.Text($"tmb found: {files_tmb.Length}");
                if (tmbParsed)
                {
                    foreach (var file in files_tmb)
                    {
                        var e = file.Replace(modSelected, ".");
                        if (vfxFileContent[file].HaveError.Contains(true))
                        {
                            ImGui.SetNextItemOpen(true, ImGuiCond.Once);
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                        }
                        if (ImGui.TreeNode($"{e}"))
                        {
                            ImGui.Indent();
                            var content = vfxFileContent[file].ParsedPaths;
                            if (content != null)
                            {
                                for (int i = 0; i < content.Length; i++)
                                {
                                    if (vfxFileContent[file].HaveError[i])
                                        ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), $"{content[i]}");
                                    else if (!errorOnly) ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 0.5f), content[i]);
                                }
                            }
                            if (!vfxFileContent[file].HaveError.Contains(true)) ImGui.Text("No error found.");
                            else ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), $"{vfxFileContent[file].HaveError.Count(x => x == true)} error(s) found.");
                            ImGui.Unindent();
                            ImGui.TreePop();
                        }
                        if (vfxFileContent[file].HaveError.Contains(true)) ImGui.PopStyleColor();
                    }
                }
                else ImGui.Text("Not parsed yet.");
                ImGui.Separator();
            }
        }
        else
        {
            if (ImGui.Button("Choose a mod"))
            {
                SelectMod("Select a Mod folder");
            }
            if (!modSelected.IsNullOrEmpty()) ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), $"{modSelected} isn't a valid mod folder.");
        }
    }
    private void SelectMod(string title)
    {
        _folderPicker.OpenFolderDialog(title, (result, path) => 
        {
            if (!result) return;

            modSelected = path;
            PluginLog.Information(path + " loading");
            PluginLog.Information("Searching for meta.json");
            var metaPath = Path.Combine(path, "meta.json");

            if (File.Exists(metaPath))
            {
                PluginLog.Information("Found!");

                modInfo = JsonSerializer.Deserialize<ModInfo>(File.ReadAllText(metaPath));

                isModSelected = true;

                files_scd = Directory.GetFiles(modSelected, "*.scd", SearchOption.AllDirectories);
                if (files_scd.Length > 0) scdExist = true;
                files_atex = Directory.GetFiles(modSelected, "*.atex", SearchOption.AllDirectories);
                if (files_atex.Length > 0) atexExist = true;
                files_avfx = Directory.GetFiles(modSelected, "*.avfx", SearchOption.AllDirectories);
                if (files_avfx.Length > 0) avfxExist = true;
                files_pap = Directory.GetFiles(modSelected, "*.pap", SearchOption.AllDirectories);
                if (files_pap.Length > 0) papExist = true;
                files_tmb = Directory.GetFiles(modSelected, "*.tmb", SearchOption.AllDirectories);
                if (files_tmb.Length > 0) tmbExist = true;
            }
            else PluginLog.Warning($"\"{path}\" doesn't seem to be a mod folder.");
        });
    }

    private static (List<string>, string, List<bool>, List<string>) ParseVfxFile(string path, string modSelected)
    {
        PluginLog.Information("Parsing " + Path.GetFileName(path));
        var file = File.ReadAllBytes(path);
        List<bool> haveError = new List<bool>();
        List<string> parsedPaths = new List<string>();
        List<string> targetPath = new List<string>();

        if (!(Path.GetExtension(path) == ".atex" || Path.GetExtension(path) == ".scd"))
        {
            var scdStart = "soun"u8;
            var scdEnd = ".scd"u8;
            var vfxStart = "vfx/"u8;
            var atexEnd = ".atex"u8;
            var avfxEnd = ".avfx"u8;
            var papEnd = ".pap"u8;

            int maxFirstCharSlot = file.Length - 4;
            for (int i = 0; i < maxFirstCharSlot; i += 4)
            {
                if (file[i] != scdStart[0] && file[i] != vfxStart[0])
                    continue;
                if (file.AsSpan().Slice(i,4).SequenceEqual(scdStart))
                {
                    // found the start of a scd path
                    for (int j = 0; j < maxFirstCharSlot - i; j++)
                    {
                        // search for the next dot
                        if (file[i + j] != 46)
                            continue;
                        if (file.AsSpan().Slice(i + j, scdEnd.Length).SequenceEqual(scdEnd))
                        {
                            parsedPaths.Add(Encoding.ASCII.GetString(file.AsSpan().Slice(i, j + scdEnd.Length)));
                            break;
                        }
                    }
                }
                if (file.AsSpan().Slice(i, 4).SequenceEqual(vfxStart))
                {
                    // found the start of an atex, avfx or pap path
                    for (int j = 0; j < maxFirstCharSlot - i; j++)
                    {
                        // search for the next dot
                        if (file[i + j] != 46)
                            continue;
                        if (file.AsSpan().Slice(i + j, atexEnd.Length).SequenceEqual(atexEnd))
                        {
                            parsedPaths.Add(Encoding.ASCII.GetString(file.AsSpan().Slice(i, j + atexEnd.Length)));
                            break;
                        }
                        if (file.AsSpan().Slice(i + j, avfxEnd.Length).SequenceEqual(avfxEnd))
                        {
                            parsedPaths.Add(Encoding.ASCII.GetString(file.AsSpan().Slice(i, j + avfxEnd.Length)));
                            break;
                        }
                        if (file.AsSpan().Slice(i + j, papEnd.Length).SequenceEqual(papEnd))
                        {
                            parsedPaths.Add(Encoding.ASCII.GetString(file.AsSpan().Slice(i, j + papEnd.Length)));
                            break;
                        }
                    }
                }
            }

            foreach (string s in parsedPaths)
            {
                // Check if the found paths are either bundled in the mod or exist in the game data.
                if (!Plugin.Data.FileExists(s) && !SearchInJson(modSelected, s))
                {
                    haveError.Add(true);
                    //PluginLog.Information($"Error detected: gamedata :{Plugin.Data.FileExists(s)} json: {SearchInJson(modSelected, s)}");
                }
                else
                {
                    haveError.Add(false);
                    //PluginLog.Information("Looks good");
                }
            }
        }

        // todo: get the target redirection path(s) from the json

        PluginLog.Information($"Error(s) detected: {haveError.Count(x => x == true)}");
        return (parsedPaths, path, haveError, targetPath);
    }

    public static bool SearchInJson(String modSelected, String path)
    {
        bool exists = false;
        string jsonContents;
        string[] jsonFile = Directory.GetFiles(modSelected, "*.json");
        foreach (string file in jsonFile)
        {
            jsonContents = File.ReadAllText(file);
            if (jsonContents.Contains(path)) exists = true;
        }
        return exists;
    }
}

