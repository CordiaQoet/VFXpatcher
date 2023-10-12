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

namespace VFXPatcher.Windows;

public class VfxFileContent
{
    public required string[] ParsedPaths { get; set; }
    public required string Origin { get; set; }
    public bool HaveError { get; set; }
}


public class MainWindow : Window, IDisposable
{
    private Plugin plugin;
    public readonly FileDialogManager _folderPicker = new();
    private bool isModSelected = false;
    private string modSelected = "";
    //private List<string[]> avfxContent = new List<string[]>();
    //private List<string[]> papContent = new List<string[]>();
    //private List<string[]> tmbContent = new List<string[]>();
    private List<VfxFileContent> avfxContent = new List<VfxFileContent>();
    private List<VfxFileContent> papContent = new List<VfxFileContent>();
    private List<VfxFileContent> tmbContent = new List<VfxFileContent>();
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
    private bool avfxParsed = false;
    private bool papParsed = false;
    private bool tmbParsed = false;


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

    }

    public override void Draw()
    {
        /*if (ImGui.Button("Show Settings"))
        {
            this.Plugin.DrawConfigUI();
        }*/

        if (isModSelected == true)
        {
            ImGui.Text($"Mod selected is {modSelected}");

            if (ImGui.Button("Unload mod"))
            {
                isModSelected = false;
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
                avfxParsed = false;
                papParsed = false;
                tmbParsed = false;
                avfxContent = new List<VfxFileContent>();
                papContent = new List<VfxFileContent>();
                tmbContent = new List<VfxFileContent>();
            }

            if (scdExist)
            {
                ImGui.SameLine();
                using (ImRaii.Disabled(!(tmbExist == tmbParsed && papExist == papParsed && avfxExist == avfxParsed)))
                {
                    if (ImGui.Button("Check scd"))
                    {
                        //do stuff
                    }
                    if (!(tmbExist == tmbParsed && papExist == papParsed && avfxExist == avfxParsed))
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                            ImGui.SetTooltip("First parse avfx, pap and tmp files");
                }
            }

            if (atexExist)
            {
                ImGui.SameLine();
                using (ImRaii.Disabled(!(tmbExist == tmbParsed && papExist == papParsed && avfxExist == avfxParsed)))
                {
                    if (ImGui.Button("Check atex"))
                    {
                        //do stuff
                    }
                    if (!(tmbExist == tmbParsed && papExist == papParsed && avfxExist == avfxParsed))
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                            ImGui.SetTooltip("First parse avfx, pap and tmp files");
                }
            }

            if (avfxExist)
            {
                ImGui.SameLine();
                using (ImRaii.Disabled(avfxParsed = true))
                {
                    if (ImGui.Button("Parse avfx"))
                    {
                        foreach (var file in files_avfx)
                        {
                            var a = ParseVfxFile(file);
                            avfxContent.Add(new VfxFileContent() { ParsedPaths = a.Item1.ToArray(), Origin = a.Item2 });
                            //avfxContent.ParsedPaths.Origin = a.Item2;
                        }
                        avfxParsed = true;
                    }
                }
            }

            if (papExist)
            {
                ImGui.SameLine();
                using (ImRaii.Disabled(papParsed = true))
                {
                    if (ImGui.Button("Parse pap"))
                    {
                        foreach (var file in files_pap)
                        {
                            var a = ParseVfxFile(file);
                            papContent.Add(new VfxFileContent() { ParsedPaths = a.Item1.ToArray(), Origin = a.Item2 });
                        }
                        papParsed = true;
                    }
                }
            }

            if (tmbExist)
            {
                ImGui.SameLine();
                using (ImRaii.Disabled(tmbParsed = true))
                {
                    if (ImGui.Button("Parse tmb"))
                    {
                        foreach (var file in files_tmb)
                        {
                            var a = ParseVfxFile(file);
                            tmbContent.Add(new VfxFileContent() { ParsedPaths = a.Item1.ToArray(), Origin = a.Item2 });
                        }
                        tmbParsed = true;
                    }
                }
            }

            ImGui.Separator();
            if (scdExist)
            {
                ImGui.Text($"scd found: {files_scd.Length}");
                foreach (var file in files_scd)
                {
                    ImGui.Text($"{file.Replace(modSelected, ".")}");
                }
                ImGui.Separator();
            }

            if (atexExist)
            {
                ImGui.Text($"atex found: {files_atex.Length}");
                foreach (var file in files_atex)
                {
                    ImGui.Text($"{file.Replace(modSelected, ".")}");
                }
                ImGui.Separator();
            }

            if (avfxExist)
            {
                ImGui.Text($"avfx found: {files_avfx.Length}");
                int i = 0;
                foreach (var file in files_avfx)
                {
                    ImGui.Text($"{file.Replace(modSelected, ".")}");
                    /*
                    if (avfxContent.Exists(x => x.Origin == file))
                    {
                        int errorCount = 0;
                        avfxContent.ForEach(x => x.Origin == file);
                        {
                            //to do: check if these paths exist in the gamedata OR in the mod, if not, find the new path and suggest the change
                            if (!Plugin.Data.FileExists(s))
                            {
                                if (!SearchInJson(modSelected, s))
                                {
                                    ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), $"{s}"); ImGui.SameLine();
                                    if (ImGui.Button($"{i}-{errorCount} Do something"))
                                    {
                                        // do stuff...
                                    }
                                    errorCount++;
                                }
                            }
                        }
                        if (errorCount == 0)
                        {
                            ImGui.Text("No error found");
                        }
                        else ImGui.Text($"{errorCount} error(s) found");
                    }
                    i++;
                    */
                }
                ImGui.Separator();
            }

            if (papExist)
            {
                ImGui.Text($"pap found: {files_pap.Length}");
                foreach (var file in files_pap)
                {
                    ImGui.Text($"{file.Replace(modSelected, ".")}");
                }
                ImGui.Separator();
            }

            if (tmbExist)
            {
                ImGui.Text($"tmb found: {files_tmb.Length}");
                foreach (var file in files_tmb)
                {
                    ImGui.Text($"{file.Replace(modSelected, ".")}");
                }
                ImGui.Separator();
            }
        }
        else
        {
            if (ImGui.Button("Choose a mod"))
            {
                OpenFolderDialog("Select a Mod folder");
            }
        }
    }
    private void OpenFolderDialog(string title)
    {
        _folderPicker.OpenFolderDialog(title, (result, path) => 
        {
            if (!result) return;

            PluginLog.Information(path + " loading");
            PluginLog.Information("Searching for default_mod.json");

            if (File.Exists(path + @"\default_mod.json"))
            {
                PluginLog.Information("Found!");
                modSelected = path;
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
            else
            {
                PluginLog.Error("\"" + path + "\" doesn't seem to be a mod folder");
            }
        });
    }
    private static (List<string>, string) ParseVfxFile(string path)
    {
        PluginLog.Information("Parsing " + Path.GetFileName(path));
        string file;
        string result;
        List<string> parsedPaths = new List<string>();

        if (new FileInfo(path).Length != 0)
            file = BitConverter.ToString(File.ReadAllBytes(path));
        else
        {
            PluginLog.Error("Error parsing the file...");
            return (parsedPaths, string.Empty);
        }

        var regexScd = @"00-73-6F-75-6E-64-2F.*?2E-73-63-64-00";
        var regexAtex = @"00-76-66-78-2F.*?2E-61-74-65-78-00";
        var regexAvfx = @"00-76-66-78-2F.*?2E-61-76-66-78-00";
        var regexPap = @"00-76-66-78-2F.*?2E-70-61-70-00";

        PluginLog.Information(Regex.Matches(file, regexScd).Count + " scd link(s) found ");
        foreach (Match match in Regex.Matches(file, regexScd))
        {
            var a = match.Value[3..^3];
            result = ConvertHex(a);
            parsedPaths.Add(result);
            PluginLog.Information(result);
        }
        PluginLog.Information(Regex.Matches(file, regexAtex).Count + " atex link(s) found ");
        foreach (Match match in Regex.Matches(file, regexAtex))
        {
            var a = match.Value[3..^3];
            result = ConvertHex(a);
            parsedPaths.Add(result);
            PluginLog.Information(result);
        }
        PluginLog.Information(Regex.Matches(file, regexAvfx).Count + " avfx link(s) found ");
        foreach (Match match in Regex.Matches(file, regexAvfx))
        {
            var a = match.Value[3..^3];
            result = ConvertHex(a);
            parsedPaths.Add(result);
            PluginLog.Information(result);
        }
        PluginLog.Information(Regex.Matches(file, regexPap).Count + " pap link(s) found ");
        foreach (Match match in Regex.Matches(file, regexPap))
        {
            var a = match.Value[3..^3];
            result = ConvertHex(a);
            parsedPaths.Add(result);
            PluginLog.Information(result);
        }
        return (parsedPaths, path);
    }
    public static string ConvertHex(String hexString)
    {
        string[] hexArray = hexString.Split('-');
        try
        {
            string ascii = string.Empty;

            foreach (string hex in hexArray)
            {
                // Convert the number expressed in base-16 to an integer.
                int value = Convert.ToInt32(hex, 16);
                // Get the character corresponding to the integral value.
                string stringValue = Char.ConvertFromUtf32(value);
                char charValue = (char)value;
                ascii += stringValue;
            }

            return ascii;
        }
        catch (Exception ex) { PluginLog.Error(ex.Message); }

        return string.Empty;
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
