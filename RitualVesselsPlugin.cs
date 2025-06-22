using ExileCore;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.PoEMemory.Components;
using System.Numerics;

namespace RitualVessels;

public class RitualVesselsPlugin : BaseSettingsPlugin<RitualVesselsSettings>
{
    private EffectHelper effectHelper;

    public override bool Initialise()
    {
        effectHelper = new EffectHelper(GameController, Graphics);
        return base.Initialise();
    }

    private readonly Dictionary<long, int> uniqueMonsterScores = [];
    private readonly Dictionary<long, Vector3> uniqueMonsterPositions = [];
    private readonly HashSet<long> uniqueMonsterIdentifiers = []; // Track unique monster addresses

    private Dictionary<long, int> ritualScores = [];
    private readonly Dictionary<long, Vector3> ritualPositions = [];
    private readonly HashSet<long> ritualIdentifiers = []; // Track unique ritual addresses
    private readonly HashSet<long> lockedRitualIdentifiers = []; // Track rituals that have been approached and locked

    public override void EntityAdded(Entity entity)
    {
        if (!GameController.Game.IngameState.InGame || GameController.Area.CurrentArea.IsPeaceful || GameController.Area.CurrentArea.RealLevel > 83 || GameController.Area.CurrentArea.RealLevel <= 68)
            return;

        if (entity.Type == EntityType.Monster)
        {
            TrackMonster(entity);
        }
        else if (entity.Type == EntityType.Terrain && entity.Metadata.Contains("Metadata/Terrain/Leagues/Ritual/RitualRuneObject"))
        {
            TrackRitual(entity);
        }

        base.EntityAdded(entity);
    }

    private void TrackMonster(Entity entity)
    {
        string metadata = entity.Metadata;
        int? value = null;

        if (metadata.StartsWith("Metadata/Monsters/Exiles", StringComparison.OrdinalIgnoreCase) && !metadata.Contains("Clone", StringComparison.OrdinalIgnoreCase))
        {
            var hasGigantism = entity.GetComponent<ObjectMagicProperties>().Mods.Contains("MonsterSupporterGigantism1");
            value = hasGigantism ? 100 : 1;
        }
        // else if (metadata == "Metadata/Monsters/Spiders/MapSpiderBossSins")
        // {
        //     value = 0;
        // }

        if (value.HasValue && !uniqueMonsterIdentifiers.Contains(entity.Id))
        {
            uniqueMonsterScores[entity.Id] = value.Value;
            uniqueMonsterPositions[entity.Id] = entity.PosNum;
            uniqueMonsterIdentifiers.Add(entity.Id);
        }
    }

    private void TrackRitual(Entity entity)
    {
        if (!ritualIdentifiers.Contains(entity.Id))
        {
            ritualPositions[entity.Id] = entity.PosNum;
            ritualScores[entity.Id] = 0;
            ritualIdentifiers.Add(entity.Id);
        }
    }

    private readonly Stopwatch _sinceLastReloadStopwatch = Stopwatch.StartNew();
    public override Job Tick()
    {
        if (!GameController.Game.IngameState.InGame || GameController.Area.CurrentArea.IsPeaceful || GameController.Area.CurrentArea.RealLevel > 83 || GameController.Area.CurrentArea.RealLevel <= 68)
            return null;

        if (_sinceLastReloadStopwatch.Elapsed > TimeSpan.FromSeconds(0.5))
        {
            _sinceLastReloadStopwatch.Restart();
            CalculateRitualProximityScores();
        }
        return null;
    }

    public override void Render()
    {
        if (!GameController.Game.IngameState.InGame || GameController.Area.CurrentArea.IsPeaceful || GameController.Area.CurrentArea.RealLevel > 83 || GameController.Area.CurrentArea.RealLevel <= 68)
            return;

        effectHelper.DrawRitualSize(ritualScores);
    }

    public override void AreaChange(AreaInstance area)
    {
        ritualIdentifiers.Clear();
        ritualPositions.Clear();
        ritualScores.Clear();
        lockedRitualIdentifiers.Clear();
        uniqueMonsterIdentifiers.Clear();
        uniqueMonsterPositions.Clear();
        uniqueMonsterScores.Clear();

        base.AreaChange(area);
    }

    private void CalculateRitualProximityScores()
    {
        var temporaryRitualScores = new Dictionary<long, int>();
        var playerPosition = GameController.Player.PosNum;

        // Check if player is within 750 range of any ritual and mark it as locked
        foreach (var ritualId in ritualIdentifiers)
        {
            if (Vector3.Distance(playerPosition, ritualPositions[ritualId]) <= 1150)
            {
                lockedRitualIdentifiers.Add(ritualId);
            }
        }

        foreach (var ritualId in ritualIdentifiers)
        {
            if (lockedRitualIdentifiers.Contains(ritualId))
            {
                // Keep the existing score for this ritual (permanently locked)
                temporaryRitualScores[ritualId] = ritualScores.ContainsKey(ritualId) ? ritualScores[ritualId] : 0;
            }
            else
            {
                // Calculate new score for this ritual
                temporaryRitualScores[ritualId] = 0;
            }
        }

        // Calculate proximity scores for rituals that are not locked
        foreach (var monsterId in uniqueMonsterIdentifiers)
        {
            foreach (var ritualId in ritualIdentifiers)
            {
                // Only update score if ritual is not locked
                if (!lockedRitualIdentifiers.Contains(ritualId) && Vector3.Distance(uniqueMonsterPositions[monsterId], ritualPositions[ritualId]) <= 1050)
                {
                    temporaryRitualScores[ritualId] += uniqueMonsterScores[monsterId];
                }
            }
        }

        ritualScores = temporaryRitualScores;
    }
}
