using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using Lumina.Data.Files.Excel;
using Lumina.Data.Parsing.Layer;
using SamplePlugin.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SamplePlugin;

internal partial class MarkdownRender(string id)
{
    private string id { get; } = id;
    private string markdown;
    private bool sameLine = false;
    private readonly List<Action> elements = [];

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

    public void Draw2()
    {
        if(Ready)
        {
            var lines = markdown.Split("\n");
            ContinuousDraw(lines, 0);
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
            if (!tag.IsNullOrEmpty() && line == $"::/{tag}::")
            {
                return i + 1;
            }

            if (line == "===")
            {
                elementList.Add(ImGui.Separator);
                i++;
                continue;
            }
            else if (line.StartsWith("- "))
            {
                elementList.Add(() => {ImGui.BulletText(line.Substring(2));});
                i++;
                continue;
            }
            else if (line == "")
            {
                elementList.Add(ImGui.NewLine);
                i++;
                continue;
            }
            else if (line.StartsWith("::") && line.EndsWith("::") && line[2] != '/')
            {
                var innerTag = line.Substring(2, line.Length - 4);
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

            var tempIndex = 0;
            foreach (Match match in LinkRegex().Matches(line))
            {
                if (match.Index != 0)
                {
                    if (match.Index != tempIndex)
                    {
                        sb.Append(line.Substring(tempIndex, match.Index));
                        FlushBuffer(sb, elementList);
                    }

                    elementList.Add(ImGui.SameLine);
                    elementList.Add(() => {
                        ImGui.SameLine();
                        Windows.Util.Hyperlink(match.Value);
                    });
                } else
                {
                    elementList.Add(() => {
                        Windows.Util.Hyperlink(match.Value);
                    });
                }

                tempIndex = match.Index + match.Length;
                sameLine = true;
            }
            
            sb.Append(line.Substring(tempIndex));
            FlushBuffer(sb, elementList);
            i++;
        }

        return i;
    }

    public void ContinuousDraw(string md)
    {
        markdown = md;
        Ready = true;
    }

    private uint ContinuousDraw(string[]? lines, uint index, string tag = "")
    {
        var sb = new StringBuilder();

        uint i = index;

        while (i < lines?.Length)
        {
            var line = lines[i];

            // close collapsing header
            if (!tag.IsNullOrEmpty() && line == $"::/{tag}::")
            {
                FlushBuffer(sb);
                return i + 1;
            }

            if (line == "===")
            {
                FlushBuffer(sb);
                ImGui.Separator();
                i++;
                continue;
            }
            else if (line.StartsWith("- "))
            {
                FlushBuffer(sb);
                ImGui.BulletText(line.Substring(2));
                i++;
                continue;
            }
            else if (line == "")
            {
                FlushBuffer(sb);
                ImGui.NewLine();
                i++;
                continue;
            }
            else if (line.StartsWith("::") && line.EndsWith("::") && line[2] != '/')
            {
                FlushBuffer(sb);
                var innerTag = line.Substring(2, line.Length - 4);

                if (ImGui.CollapsingHeader(innerTag))
                {
                    ImGui.Indent();
                    i = ContinuousDraw(lines, ++i, innerTag);
                    ImGui.Unindent();
                }

                continue;
            }

            var tempIndex = 0;
            foreach (Match match in LinkRegex().Matches(line))
            {
                if (match.Index != 0)
                {
                    if (match.Index != tempIndex)
                    {
                        sb.Append(line.Substring(tempIndex, match.Index));
                        FlushBuffer(sb);
                    }

                    ImGui.SameLine();
                    Windows.Util.Hyperlink(match.Value);
                }
                else
                {
                    Windows.Util.Hyperlink(match.Value);
                }

                tempIndex = match.Index + match.Length;
                sameLine = true;
            }

            sb.Append(line.Substring(tempIndex) + "\n");
            i++;
        }

        FlushBuffer(sb);
        return i;
    }
    private void FlushBuffer(StringBuilder sb)
    {
        if (sb.Length > 0)
        {
            if (sameLine)
                ImGui.SameLine();
            var str = sb.ToString();
            ImGui.TextWrapped(str);
        }
        if (sameLine)
            sameLine = false;
        sb.Clear();
    }

    private void FlushBuffer(StringBuilder sb, List<Action> elementList)
    {
        if (sb.Length > 0)
        {
            if (sameLine)
            {
                elementList.Add(ImGui.SameLine);
            }
            var str = sb.ToString();
            elementList.Add(() => {ImGui.TextWrapped(str);});
        }
        if (sameLine)
            sameLine = false;
        sb.Clear();
    }

    private static void execAll(List<Action> elementList)
    {
        foreach (var element in elementList)
        {
            element();
        }
    }

    [GeneratedRegex(@"(http|https)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*")]
    private static partial Regex LinkRegex();
}
