using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using ShaRPG.Service;

namespace ShaRPG.Entity {
    /// <summary>
    /// The FactionManager keeps track of the various in-game factions and their relationships with eachother
    /// </summary>
    public class FactionManager {
        public class Faction {
            public readonly string Name;

            private readonly int _baseRelationship;
            private readonly Dictionary<Faction, int> _relationships = new Dictionary<Faction, int>();
            private readonly List<GameEntity> _entities = new List<GameEntity>();

            public Faction(string name, int baseRelationship = 0) {
                Name = name;
                _baseRelationship = baseRelationship;
            }

            public void RegisterEntity(GameEntity entity) {
                _entities.Add(entity);
            }

            public int GetRelationship(Faction other) {
                if (_relationships.ContainsKey(other)) return _relationships[other];
                return _baseRelationship;
            }

            public void InfluenceRelationship(Faction other, int amount) {
                if (!_relationships.ContainsKey(other)) _relationships[other] = _baseRelationship;
                _relationships[other] += amount;
            }

            public override string ToString() {
                return $"Faction<{Name}>";
            }
        }

        /// <summary>
        /// The neutral faction represents an entity who has no particular allegiance to any faction.
        /// </summary>
        public const string Neutral = "NEUTRAL";

        /// <summary>
        /// The player faction represents the single player of the game.
        /// </summary>
        public const string Player = "PLAYER";

        /// <summary>
        /// A faction for generically hostile entities, who will fight any entity other than each-other.
        /// </summary>
        public const string Hostile = "HOSTILE";

        public const int HostilityRelationshipValue = -1000;

        private readonly Dictionary<string, Faction> _factions = new Dictionary<string, Faction>();
        private readonly Dictionary<GameEntity, Faction> _userLookup = new Dictionary<GameEntity, Faction>();

        public FactionManager() {
            _factions.Add(Player, new Faction(Player));
            _factions.Add(Neutral, new Faction(Neutral));
            _factions.Add(Hostile, new Faction(Hostile, 10 * HostilityRelationshipValue));
        }

        public void RegisterEntity(GameEntity entity, string factionName) {
            if (!_factions.ContainsKey(factionName)) {
                throw new ArgumentException($"No such faction {factionName} to assign to {entity.Name}");
            }

            _factions[factionName].RegisterEntity(entity);
            _userLookup[entity] = _factions[factionName];
            ServiceLocator.LogService.Log(LogType.Information, $"Registered {entity} to {_factions[factionName]}");
        }

        public Faction GetFaction(GameEntity entity) {
            if (!_userLookup.ContainsKey(entity)) {
                throw new EntityException(entity, "Entity was not registered in the factions manager");
            }

            return _userLookup[entity];
        }
    }
}
