using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using Lumina.Data.Parsing.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SamplePlugin;

internal class MarkdownRender(string id)
{
    private string id { get; } = id;
    private List<Action> elements = new List<Action>();
    public bool Ready { get; set; } = false;
    public void Draw()
    {
        if (Ready)
        {
            execAll(elements);
        } else
        {
            ImGui.TextUnformatted("Processing...");
        }
    }

    public void Update(string md)
    {
        Ready = false;
        elements.Clear();
        var lines = md.Split("\n");
        Update(lines, 0, elements);
        Ready = true;
    }

    private uint Update(string[]? lines, uint index, List<Action> elementList, string tag = "")
    {
        var sb = new StringBuilder();

        uint i = index;
        while (i < lines?.Length)
        {
            var line = lines[i];

            // close collapsing header
            if (!tag.IsNullOrEmpty() && line == $"</{tag}>")
            {
                FlushBuffer(sb, elementList);
                return i + 1;
            }

            if (line == "===")
            {
                FlushBuffer(sb, elementList);
                elementList.Add(ImGui.Separator);
                i++;
                continue;
            }
            else if (line.StartsWith("- "))
            {
                FlushBuffer(sb, elementList);
                elementList.Add(() => {ImGui.BulletText(line.Substring(2));});
                i++;
                continue;
            }
            else if (line == "")
            {
                FlushBuffer(sb, elementList);
                elementList.Add(ImGui.NewLine);
                i++;
                continue;
            }
            else if (line.StartsWith('<') && line.EndsWith('>') && line[1] != '/')
            {
                FlushBuffer(sb, elementList);
                var innerTag = line.Substring(1, line.Length - 2);
                List<Action> subElements = new List<Action>();

                subElements.Add(ImGui.Indent);
                i = Update(lines, ++i, subElements, innerTag);
                subElements.Add(ImGui.Unindent);

                elementList.Add(() =>
                {
                    if (ImGui.CollapsingHeader(innerTag))
                    {
                        execAll(subElements);
                    }
                });

                continue;
            }

            sb.Append(line + "\n");
            i++;
        }

        FlushBuffer(sb, elementList);
        return i;
    }

    private static void FlushBuffer(StringBuilder sb, List<Action> elementList)
    {
        if (sb.Length > 0)
        {
            var str = sb.ToString();
            elementList.Add(() => {ImGui.TextWrapped(str);});
        }
        sb.Clear();
    }

    private static void execAll(List<Action> elementList)
    {
        foreach (var element in elementList)
        {
            element();
        }
    }


}
