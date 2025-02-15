using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SamplePlugin.Windows;

internal class Util
{
    public static void Hyperlink(string url, string label = "")
    {
        var clicked = false;
        if (label.IsNullOrEmpty())
            clicked = HyperlinkStyle(url);
        else
            clicked = HyperlinkStyle(label);

        if (clicked)
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
    private static bool HyperlinkStyle(string label, bool underlineWhenHoveredOnly = false)
    {
        var linkColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.3f, 0.8f, 1f));
        var linkHoverColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.4f, 0.6f, 0.8f, 1));
        var linkFocusColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.6f, 0.4f, 0.8f, 1));

        var id = ImGui.GetID(label);

        var draw = ImGui.GetWindowDrawList();

        var pos = ImGui.GetCursorScreenPos();
        var size = ImGui.CalcTextSize(label);
        var bb = new Vector4(pos.X, pos.Y, pos.X + size.X, pos.Y + size.Y);


        using (ImRaii.PushColor(ImGuiCol.Text, 0))
        {
            ImGui.InvisibleButton(label, size);
        }

        bool isHovered = ImGui.IsItemHovered();
        bool isClicked = ImGui.IsItemClicked();
        bool isFocused = ImGui.IsItemFocused();

        var color = isHovered ? linkHoverColor : isFocused ? linkFocusColor : linkColor;

        draw.AddText(new Vector2(bb.X, bb.Y), color, label);

        if (isHovered)
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        if (isFocused)
            draw.AddRect(new Vector2(bb.X, bb.Y), new Vector2(bb.Z, bb.W), color);
        else if (!underlineWhenHoveredOnly || isHovered)
            draw.AddLine(new Vector2(bb.X, bb.W), new Vector2(bb.Z, bb.W), color);

        return isClicked;
    }
}
