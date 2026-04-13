# SlimePool Refactoring Design

## Summary

Split `SlimeSpawner`'s pooling responsibilities into a separate `Pool<T>` class following SOLID Single Responsibility Principle.

## Problem

- `SlimeSpawner` handles both spawning logic AND pooling (SRP violation)
- `SlimePool.cs` is missing (referenced in scene but file doesn't exist)
- Inconsistent pooling pattern across project (BulletPool, HitTextPool are separate classes)

## Solution

Create a generic `Pool<T>` base class that can be reused across all poolable MonoBehaviour types.

## Architecture

```
Pool<T> (Base MonoBehaviour)
├── prefab: GameObject (SerializeField)
├── poolSize: int
├── Get() -> T
├── Release(T item)
├── Prewarm()

SlimePool : Pool<Slime>
├── (inherits all from Pool<T>)
└── Inspector configures prefab

SlimeSpawner (Spawn only)
├── slimePool: SlimePool (reference)
├── SpawnLoop()
├── SpawnOne()
└── (pool logic removed)
```

## Components

### Pool<T>

Generic object pool for any MonoBehaviour type.

**Properties:**
- `prefab: GameObject` - prefab to instantiate (SerializeField)
- `poolSize: int` - initial pool size (SerializeField, default: 10)
- `poolParent: Transform` - parent for pooled objects (optional)

**Methods:**
- `Get(): T` - get object from pool or create new
- `Release(T item): void` - return object to pool
- `Prewarm(int count): void` - pre-populate pool

**Internal:**
- `Queue<T> pooledItems` - available items
- `List<T> allItems` - all created items (for cleanup)

### SlimePool

Inherits from `Pool<Slime>` with no additional logic. Unity Inspector handles prefab assignment.

### SlimeSpawner (Modified)

Remove pooling logic, reference `SlimePool` instead.

**Removed:**
- `pooledSlimes` Queue
- `allSlimes` List
- `InitializePool()`
- `GetPooledSlime()`
- `CreatePooledSlime()`
- `Release(Slime)` (move to Slime/Pool)

**Added:**
- `slimePool: SlimePool` reference (SerializeField)

**Modified:**
- `SpawnOne()` uses `slimePool.Get()` instead of `GetPooledSlime()`

## Data Flow

```
Spawn Flow:
SlimeSpawner.SpawnOne()
  -> slimePool.Get()
  -> returns Slime (from pool or newly created)
  -> Slime.InitializeSpawn(position, id, hp)
  -> gameObject.SetActive(true)

Release Flow:
Slime.OnDeath() (or external trigger)
  -> slimePool.Release(this)
  -> gameObject.SetActive(false)
  -> enters pooledItems queue
```

## Error Handling

- Pool.Get() returns null if prefab not assigned (logs error)
- Pool.Release() ignores null items
- Pool handles prefab component missing gracefully

## Testing

- Pool<T> can be tested with any MonoBehaviour prefab
- SlimeSpawner can be tested independently from pool
- Pool behavior: Get returns deactivated objects, Release deactivates

## Migration Steps

1. Create `Pool<T>.cs` in `Assets/Scripts/Core/`
2. Create `SlimePool.cs` in `Assets/Scripts/Monsters/`
3. Modify `SlimeSpawner.cs` to use `SlimePool`
4. Modify `Slime.cs` to call pool.Release()
5. Update scene: remove old SlimePool GameObject, add new one
6. Assign prefab in Inspector

## Files Changed

| File | Action |
|------|--------|
| `Assets/Scripts/Core/Pool.cs` | Create (new) |
| `Assets/Scripts/Monsters/SlimePool.cs` | Create (new) |
| `Assets/Scripts/Monsters/SlimeSpawner.cs` | Modify |
| `Assets/Scripts/Monsters/Slime.cs` | Modify |
| `Assets/Scenes/SampleScene.unity` | Update SlimePool GameObject |