using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ImGuiNET;

namespace SamplePlugin.Windows;

public class MarkdownWindow : Window, IDisposable
{
    private Plugin Plugin;
    private string markdown = "";
    private MarkdownRender render;
    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MarkdownWindow(Plugin plugin)
        : base("Markdown Window")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 450),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        render = new MarkdownRender("MarkdownRenderer");
    }

    public void Dispose() { }

    public override void Draw()
    {
        using (var table = ImRaii.Table("tabletest", 2))
        {
            if (!table) return;
            ImGui.TableNextColumn();
            // Edit area
            ImGui.InputTextMultiline("", ref markdown, 600, new Vector2(600, 400));
            var renderButton = ImGui.Button("Render");
            if (renderButton)
            {
                render.Update(markdown);
            }


            // Render area
            ImGui.TableNextColumn();
            render.Draw();
        }
    }
}
