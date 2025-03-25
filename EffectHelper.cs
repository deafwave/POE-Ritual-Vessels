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
{

    private void DrawHazard(string text, Vector2 screenPos, Vector3 worldPos, float radius, int segments, SharpDX.Color color = default)
    {
        if (color == default)
        {
            color = SharpDX.Color.Red;
        }

        var textSize = ImGui.CalcTextSize(text);
        var textPosition = screenPos with { Y = screenPos.Y - textSize.Y / 2 };

        graphics.DrawTextWithBackground(text, textPosition, color, FontAlign.Center, SharpDX.Color.Black with { A = 200 });
        // graphics.DrawFilledCircleInWorld(worldPos, radius, color with { A = 150 }, segments);
    }

    public void DrawRitualSize(Dictionary<long, int> ritualScores)
    {
        var terrainEntityList = gameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Terrain] ?? [];

        foreach (var entity in terrainEntityList)
        {
            if (entity.DistancePlayer >= 100)
                continue;
            var pos = RemoteMemoryObject.pTheGame.IngameState.Camera.WorldToScreen(entity.PosNum);

            if (entity.Metadata.Contains("Metadata/Terrain/Leagues/Ritual/RitualRuneObject"))
            {
                DrawHazard($"Ritual SCORE: {ritualScores[entity.Id]}", pos, entity.PosNum, 1050.0f, 30, SharpDX.Color.LightBlue);
            }
        }
    }
}
