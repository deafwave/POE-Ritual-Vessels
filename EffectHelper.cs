using System.Collections.Generic;
using System.Numerics;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.Shared.Enums;
using ImGuiNET;

namespace RitualVessels;

public class EffectHelper(
    GameController gameController,
    Graphics graphics
)
{    private void DrawHazard(string text, Vector2 screenPos, Vector3 worldPos, float radius, int segments, int score, SharpDX.Color color = default)
    {
        // Determine color based on score
        if (score > 300)
        {
            color = SharpDX.Color.Orange;
        }
        else
        {
            // Faded out color for scores 300 and below
            color = color == default ? SharpDX.Color.Gray with { A = 120 } : color with { A = 120 };
        }

        ImGui.PushFont(ImGui.GetIO().Fonts.Fonts[0]);
        var textSize = ImGui.CalcTextSize(text);
        ImGui.PopFont();

        var textPosition = screenPos with { Y = screenPos.Y - textSize.Y - 20 };

        var glowColor = color with { A = 100 };
        graphics.DrawTextWithBackground(text, textPosition with { X = textPosition.X + 2, Y = textPosition.Y + 2 }, glowColor, FontAlign.Center, SharpDX.Color.Transparent);
        graphics.DrawTextWithBackground(text, textPosition with { X = textPosition.X - 2, Y = textPosition.Y - 2 }, glowColor, FontAlign.Center, SharpDX.Color.Transparent);

        graphics.DrawTextWithBackground(text, textPosition, color, FontAlign.Center, SharpDX.Color.Black with { A = 240 });        if (score >= 300)
        {
            graphics.DrawFilledCircleInWorld(worldPos, radius * 0.1f, color with { A = 80 }, segments);
        }

        // Apply faded alpha for circles when score is 300 or below
        var circleAlpha = (byte)(score > 300 ? 255 : 60);
        graphics.DrawCircleInWorld(worldPos, radius * 0.4f, color with { A = circleAlpha }, 5.0f, segments);
    }public void DrawRitualSize(Dictionary<long, int> ritualScores)
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
                DrawHazard($"🔥 RITUAL SCORE: {ritualScores[entity.Id]} 🔥", pos, entity.PosNum, 2500.0f, 50, ritualScores[entity.Id], SharpDX.Color.Orange);
            }
        }
    }
}
