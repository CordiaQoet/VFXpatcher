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

namespace VFXPatcher.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;
    public readonly FileDialogManager _importFolderPicker = new();
    private bool isModSelected = false;
    private string modSelected = "";
    private List<string[]> avfxContent = new List<string[]>();


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

            var files_atex = Directory.GetFiles(modSelected, "*.atex", SearchOption.AllDirectories);
            var files_avfx = Directory.GetFiles(modSelected, "*.avfx", SearchOption.AllDirectories);

            if (ImGui.Button("Unload mod"))
            {
                isModSelected = false;
                files_atex = null;
                files_avfx = null;
                avfxContent = new List<string[]>();
            }
            ImGui.Separator();
            if (files_atex != null)
            {
                ImGui.Text($"atex found: {files_atex.Length}");
                if (files_atex.Length > 0)
                {
                    foreach (var file in files_atex)
                    {
                        ImGui.Text($"{file}");
                    }
                }
                ImGui.Separator();
            }

            if (files_avfx != null)
            {
                ImGui.Text($"avfx found: {files_avfx.Length}"); 

                if (files_avfx.Length > 0)
                {
                    int i = 0;
                    foreach (var file in files_avfx)
                    {
                        ImGui.Text($"{file}"); ImGui.SameLine();
                        if (ImGui.Button($"{i} Parse it"))
                        {
                            avfxContent.Add(ParseVfxFile(file));
                        }
                        if (avfxContent.ElementAtOrDefault(i) != null)
                        {
                            int errorCount = 0;
                            foreach (string s in avfxContent[i])
                            {
                                //to do: check if these paths exist in the gamedata OR in the mod, if not, find the new path and suggest the change
                                if (!Plugin.Data.FileExists(s))
                                {
                                    if (!SearchInJson(modSelected, s))
                                    {
                                        ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),$"{s}"); ImGui.SameLine();
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
                    }
                }
            }

            
        }
        else
        {
            if (ImGui.Button("Choose a mod"))
            {
                OpenFolderDialog("Select a Mod folder", path =>
                {
                });
            }
        }

    }
    private void OpenFolderDialog(string title, Action<string> callback)
    {
        _importFolderPicker.OpenFolderDialog(title, (result, path) => 
        {
            if (!result)
            {
                return;
            }
            else
            {
                PluginLog.Information(path+ " loading");
                PluginLog.Information("Searching for default_mod.json");
                if (File.Exists(path + @"\default_mod.json"))
                {
                    PluginLog.Information("Found!");
                    modSelected = path;
                    isModSelected = true;
                }
                else
                {
                    PluginLog.Error("\"" + path + "\" doesn't seem to be a mod folder");
                }
            }
        });
    }
    private static string[] ParseVfxFile(string path)
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
            return parsedPaths.ToArray();
        }

        var regexAtex = @"00-76-66-78-2F.*?2E-61-74-65-78-00";
        var regexScd = @"00-73-6F-75-6E-64-2F.*?2E-73-63-64-00";
        PluginLog.Information(Regex.Matches(file, regexAtex).Count + " atex match(es) found ");
        foreach (Match match in Regex.Matches(file, regexAtex))
        {
            string a = match.Value[3..^3];
            result = ConvertHex(a);
            parsedPaths.Add(result);
            PluginLog.Information(result);
        }
        PluginLog.Information(Regex.Matches(file, regexScd).Count + " scd match(es) found ");
        foreach (Match match in Regex.Matches(file, regexScd))
        {
            string a = match.Value[3..^3];
            result = ConvertHex(a);
            parsedPaths.Add(result);
            PluginLog.Information(result);
        }
        return parsedPaths.ToArray();
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
