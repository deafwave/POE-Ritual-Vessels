using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Enums;
using ImGuiNET;

namespace RitualVessels;

public class EffectHelper(
    GameController gameController,
    Graphics graphics
)
{    private void DrawHazard(string text, Vector2 screenPos, Vector3 worldPos, float radius, int segments, SharpDX.Color color = default)
    {
        if (color == default)
        {
            color = SharpDX.Color.Red;
        }

        // Make text much larger and more prominent
        ImGui.PushFont(ImGui.GetIO().Fonts.Fonts[0]);
        var textSize = ImGui.CalcTextSize(text);
        ImGui.PopFont();
        
        // Position text higher above the circle
        var textPosition = screenPos with { Y = screenPos.Y - textSize.Y - 20 };

        // Draw multiple layers for glow effect
        var glowColor = color with { A = 100 };
        graphics.DrawTextWithBackground(text, textPosition with { X = textPosition.X + 2, Y = textPosition.Y + 2 }, glowColor, FontAlign.Center, SharpDX.Color.Transparent);
        graphics.DrawTextWithBackground(text, textPosition with { X = textPosition.X - 2, Y = textPosition.Y - 2 }, glowColor, FontAlign.Center, SharpDX.Color.Transparent);
        
        // Main text with stronger background
        graphics.DrawTextWithBackground(text, textPosition, color, FontAlign.Center, SharpDX.Color.Black with { A = 240 });
        
        // Draw multiple circle layers for enhanced visibility
        graphics.DrawFilledCircleInWorld(worldPos, radius * 0.1f, color with { A = 80 }, segments);
        
        // Draw bright outline circle
        graphics.DrawCircleInWorld(worldPos, radius * 0.4f, color with { A = 255 }, 5.0f, segments);
    }    public void DrawRitualSize(Dictionary<long, int> ritualScores)
    {
        var terrainEntityList = gameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Terrain] ?? [];

        foreach (var entity in terrainEntityList)
        {
            if (entity.DistancePlayer >= 100)
                continue;
            var pos = RemoteMemoryObject.pTheGame.IngameState.Camera.WorldToScreen(entity.PosNum);

            if (entity.Metadata.Contains("Metadata/Terrain/Leagues/Ritual/RitualRuneObject"))
            {
                // Much larger radius and more vibrant color
                DrawHazard($"🔥 RITUAL SCORE: {ritualScores[entity.Id]} 🔥", pos, entity.PosNum, 2500.0f, 50, SharpDX.Color.Orange);
            }
        }
    }
}
