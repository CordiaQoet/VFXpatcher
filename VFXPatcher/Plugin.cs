using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Lumina;
using Lumina.Data;
using Dalamud.Data;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using VFXPatcher.Windows;

namespace VFXPatcher
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] public static IDataManager Data { get; private set; } = null!;
        public string Name => "VFXpatcher";
        private const string CommandName = "/vfxpatcher";

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("VFXPatcher");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }
        public FixerWindow FixerWindow { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            /* you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            */

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);
            FixerWindow = new FixerWindow(this);
            
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(FixerWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "No argument opens the main window, /vfxpatcher cfg opens the config."
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenMainUi += OpenMainUI;
            //this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            
            ConfigWindow.Dispose();
            MainWindow.Dispose();
            FixerWindow.Dispose();
            
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {

            if (args == "cfg")
            {
                ConfigWindow.IsOpen = true;
                return;
            }
            // in response to the slash command, just display our main ui
            MainWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
            MainWindow._folderPicker.Draw();
        }
        private void OpenMainUI()
        {
            MainWindow.IsOpen = true;
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
        public void DrawFixerUI()
        {
            FixerWindow.IsOpen = true;
        }
    }
}
