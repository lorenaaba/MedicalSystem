using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace SimpleMedicalORM.Core
{
    public enum EntityState
    {
        Unchanged,
        Added,
        Modified,
        Deleted
    }

    public class EntityEntry
    {
        public object Entity { get; set; } = null!;
        public EntityState State { get; set; }

        public Dictionary<string, object?> OriginalValues { get; } = new();
        public Dictionary<string, object?> CurrentValues { get; } = new();
    }

    public class ChangeTracker
    {
        private readonly Dictionary<object, EntityEntry> _trackedEntities = new();

        public void Track(object entity, EntityState state = EntityState.Unchanged)
        {
            if (_trackedEntities.ContainsKey(entity))
                return;

            var entry = new EntityEntry
            {
                Entity = entity,
                State = state
            };

            if (state == EntityState.Unchanged)
            {
                SnapshotEntity(entity, entry.OriginalValues);
            }

            _trackedEntities[entity] = entry;
        }

        public void DetectChanges()
        {
            foreach (var entry in _trackedEntities.Values)
            {
                if (entry.State != EntityState.Unchanged)
                    continue;

                SnapshotEntity(entry.Entity, entry.CurrentValues);

                if (HasChanges(entry.OriginalValues, entry.CurrentValues))
                {
                    entry.State = EntityState.Modified;
                }
            }
        }

        public void AcceptChanges(object entity)
        {
            if (!_trackedEntities.TryGetValue(entity, out var entry))
                return;

            entry.State = EntityState.Unchanged;
            SnapshotEntity(entity, entry.OriginalValues);
            entry.CurrentValues.Clear();
        }

        public void Detach(object entity)
        {
            _trackedEntities.Remove(entity);
        }

        public IEnumerable<EntityEntry> GetEntries(EntityState state)
        {
            return _trackedEntities.Values.Where(e => e.State == state);
        }

        public EntityState GetState(object entity)
        {
            return _trackedEntities.TryGetValue(entity, out var entry)
                ? entry.State
                : EntityState.Unchanged;
        }

        public void SetState(object entity, EntityState state)
        {
            if (_trackedEntities.TryGetValue(entity, out var entry))
            {
                entry.State = state;

                if (state == EntityState.Unchanged)
                {
                    SnapshotEntity(entity, entry.OriginalValues);
                    entry.CurrentValues.Clear();
                }
            }
            else
            {
                Track(entity, state);
            }
        }

        public void Clear()
        {
            _trackedEntities.Clear();
        }

        private static void SnapshotEntity(object entity, Dictionary<string, object?> snapshot)
        {
            snapshot.Clear();

            var properties = entity.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (prop.CanRead)
                {
                    snapshot[prop.Name] = prop.GetValue(entity);
                }
            }
        }

        private static bool HasChanges(
            Dictionary<string, object?> original,
            Dictionary<string, object?> current)
        {
            foreach (var key in original.Keys)
            {
                if (!Equals(original[key], current[key]))
                    return true;
            }

            return false;
        }
    }
}
