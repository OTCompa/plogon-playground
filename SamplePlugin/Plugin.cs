using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using Lumina.Data.Files.Excel;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; set; } = null!;

    private const string CommandName = "/pmycommand";
    private const string TestCommandName = "/testcommand";
    private const int AttributeGathering = 72;
    private const int AttributePerception = 73;
    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        CommandManager.AddHandler(TestCommandName, new CommandInfo(TestCommand)
        {
            HelpMessage = "Test command"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private unsafe void TestCommand(string command, string args)
    {
        var gatherWindow = (AddonGathering*)GameGui.GetAddonByName("Gathering");
        var ilvlSheet = DataManager.GetExcelSheet<ItemLevel>();
        var uiState = UIState.Instance();
        for (int i = 0; i < gatherWindow->ItemIds.Length; i++)
        {
            var itemId = gatherWindow->ItemIds[i];
            if (itemId == 0)
            {
                continue;
            }
            var item = DataManager.GetExcelSheet<Item>().GetRow(itemId);
            var itemName = item.Name;
            
            var itemGatherLevel = item.LevelItem.Value;
            var reqGathering = itemGatherLevel!.Gathering;
            var reqPerception = itemGatherLevel!.Perception;

            var playerGathering = uiState->PlayerState.Attributes[72];
            //var playerPerception = uiState->PlayerState.Attributes[73];
            var bonusBountiful = 0;
            if (playerGathering < Math.Floor(reqGathering * 0.9)) {
                bonusBountiful = 1;
            } else if (playerGathering > Math.Floor(reqGathering * 1.1))
            {
                bonusBountiful = 3;
            } else
            {
                bonusBountiful = 2;
            }
            var test = GatheredItemComponentCheckboxHelper(gatherWindow, i + 1);
            var test2 = test->UldManager.SearchNodeById(16)->GetAsAtkTextNode();
            var chanceOk = Int32.TryParse(test2->NodeText.ToString(), out var chance);
            var bonusBlessedYield = gatherWindow->IntegrityLeftover->NodeText.ToInteger() * 2.0 / 5;
            Log.Debug(i.ToString() + ": " + gatherWindow->ItemIds[i].ToString() + ", " + itemName);
            Log.Debug("\tGatherer's Boon chance: " + test2->NodeText + "%");
            Log.Debug("\tRequired gathering: " + reqGathering);
            Log.Debug("\tPlayer gathering: " + playerGathering);
            Log.Debug("\tPer 100 GP: ");
            Log.Debug("\t\tBonus yield from bountiful: " + bonusBountiful);
            Log.Debug("\t\tBonus yield from blessed: " + bonusBlessedYield);
            Log.Debug(gatherWindow->IntegrityLeftover->NodeText.ToString());
            
        }
    }

    private unsafe AtkComponentCheckBox* GatheredItemComponentCheckboxHelper(AddonGathering* g, int i)
    {
        return i switch
        {
            1 => g->GatheredItemComponentCheckBox1,
            2 => g->GatheredItemComponentCheckBox2,
            3 => g->GatheredItemComponentCheckBox3,
            4 => g->GatheredItemComponentCheckBox4,
            5 => g->GatheredItemComponentCheckBox5,
            6 => g->GatheredItemComponentCheckBox6,
            7 => g->GatheredItemComponentCheckBox7,
            8 => g->GatheredItemComponentCheckBox8,
            _ => null,
        };
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }


    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
