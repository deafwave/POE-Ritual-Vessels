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

        // TODO: Ignore gigantism splits (don't see anything in the code for this)
        // TODO: Ignore totems
        if (entity.Type == EntityType.Monster && entity.Rarity == MonsterRarity.Unique && entity.RenderName != "Volatile")
        {
            if (!uniqueMonsterIdentifiers.Contains(entity.Id))
            {
                int value = 1;
                if (entity.GetComponent<ObjectMagicProperties>().Mods.Contains("MonsterSupporterGigantism1")) // TODO: Validate there isn't other Gigantism
                {
                    value = 100;
                }

                uniqueMonsterScores[entity.Id] = value;
                uniqueMonsterPositions[entity.Id] = entity.PosNum;
                uniqueMonsterIdentifiers.Add(entity.Id);
            }
        }

        if (entity.Type == EntityType.Terrain && entity.Metadata.Contains("Metadata/Terrain/Leagues/Ritual/RitualRuneObject"))
        {
            if (!ritualIdentifiers.Contains(entity.Id))
            {
                ritualPositions[entity.Id] = entity.PosNum;
                ritualScores[entity.Id] = 0;
                ritualIdentifiers.Add(entity.Id);
            }
        }

        base.EntityAdded(entity);
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
            if (Vector3.Distance(playerPosition, ritualPositions[ritualId]) <= 750)
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
