using System.Collections.Generic;
using System.Numerics;
using System.Linq;
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
    private void DrawHazard(string text, Vector2 screenPos, Vector3 worldPos, float radius, int segments, int score, SharpDX.Color color = default)
    {
        ImGui.PushFont(ImGui.GetIO().Fonts.Fonts[0]);
        var textSize = ImGui.CalcTextSize(text);
        ImGui.PopFont();

        var textPosition = screenPos with { Y = screenPos.Y - textSize.Y - 20 };

        var glowColor = color with { A = 100 };
        graphics.DrawTextWithBackground(text, textPosition with { X = textPosition.X + 2, Y = textPosition.Y + 2 }, glowColor, FontAlign.Center, SharpDX.Color.Transparent);
        graphics.DrawTextWithBackground(text, textPosition with { X = textPosition.X - 2, Y = textPosition.Y - 2 }, glowColor, FontAlign.Center, SharpDX.Color.Transparent);

        graphics.DrawTextWithBackground(text, textPosition, color, FontAlign.Center, SharpDX.Color.Black with { A = 240 });

        var circleAlpha = (byte)60;
        if (score >= 300)
        {
            circleAlpha = (byte)255;
            graphics.DrawFilledCircleInWorld(worldPos, radius * 0.1f, color with { A = 80 }, segments);
        }

        graphics.DrawCircleInWorld(worldPos, radius * 0.4f, color with { A = circleAlpha }, 5.0f, segments);
    }
    public void DrawRitualSize(Dictionary<long, int> ritualScores)
    {
        var terrainEntityList = gameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Terrain] ?? [];
        var ingameIconEntityList = gameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.IngameIcon] ?? [];

        var hasNamelessAo = ingameIconEntityList.Any(entity => 
            entity.Metadata.Contains("Metadata/Terrain/Leagues/Ritual/RitualRuneInteractable") &&
            entity.GetComponent<Animated>()?.BaseAnimatedObjectEntity?.Path?.Contains("Metadata/Effects/Spells/monsters_effects/League_Ritual/ritual_rune/runetypes/ritual_rune_nameless1.ao") == true);
        
        var color = hasNamelessAo ? SharpDX.Color.Green : SharpDX.Color.Gray with { A = 120 };

        foreach (var entity in terrainEntityList)
        {
            if (entity.DistancePlayer >= 150)
                continue;

            var pos = RemoteMemoryObject.pTheGame.IngameState.Camera.WorldToScreen(entity.PosNum);

            if (entity.Metadata.Contains("Metadata/Terrain/Leagues/Ritual/RitualRuneObject"))
            {
                if (ritualScores[entity.Id] >= 300)
                {
                    color = SharpDX.Color.Orange;
                }

                DrawHazard($" RITUAL SCORE: {ritualScores[entity.Id]}", pos, entity.PosNum, 2500.0f, 50, ritualScores[entity.Id], color);
            }
        }
    }
}
