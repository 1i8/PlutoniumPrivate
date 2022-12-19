using ImGuiNET;
using Plutonium.Common;

namespace Plutonium.Framework;

public class Style
{
    public static void ApplyStyle()
    {
        var style = ImGui.GetStyle();
        style.WindowRounding = 0;
        style.Colors[(int)ImGuiCol.Border] = Utils.Vec_Color(30, 30, 30);
        style.Colors[(int)ImGuiCol.TitleBg] = Utils.Vec_Color(17, 17, 17);
        style.Colors[(int)ImGuiCol.TitleBgActive] = Utils.Vec_Color(20, 20, 20);
        style.Colors[(int)ImGuiCol.WindowBg] = Utils.Vec_Color(15, 15, 15);
        style.Colors[(int)ImGuiCol.ChildBg] = Utils.Vec_Color(15, 15, 15);
        style.Colors[(int)ImGuiCol.FrameBg] = Utils.Vec_Color(20, 20, 20);
        style.Colors[(int)ImGuiCol.Button] = Utils.Vec_Color(20, 20, 20);
        style.Colors[(int)ImGuiCol.ButtonHovered] = Utils.Vec_Color(25, 25, 25);
        style.Colors[(int)ImGuiCol.ButtonActive] = Utils.Vec_Color(30, 30, 30);
        style.Colors[(int)ImGuiCol.Tab] = Utils.Vec_Color(20, 20, 20);
        style.Colors[(int)ImGuiCol.TabHovered] = Utils.Vec_Color(25, 25, 25);
        style.Colors[(int)ImGuiCol.TabActive] = Utils.Vec_Color(30, 30, 30);
        style.Colors[(int)ImGuiCol.Separator] = Utils.Vec_Color(30, 30, 30);
    }
}