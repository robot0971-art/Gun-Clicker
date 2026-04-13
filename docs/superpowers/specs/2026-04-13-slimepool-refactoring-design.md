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
- `Get(): T` - get object from pool, activates it, or create new if empty
- `Release(T item): void` - deactivate and return object to pool, parent to poolParent
- `Prewarm(int count): void` - pre-populate pool (creates deactivated objects under poolParent)

**Internal:**
- `Queue<T> pooledItems` - available items (all deactivated)
- `List<T> allItems` - all created items (for cleanup and duplicate check)
- Objects are parented to `poolParent` on creation and stay there throughout lifecycle

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

## Reference Mechanism

Slime needs pool reference to call Release. Replace `ownerSpawner` pattern:

**Current:**
- `Slime.ownerSpawner: SlimeSpawner` field
- `Slime.SetOwnerSpawner(SlimeSpawner)` method
- `ReturnToPoolAfterDelay()` calls `ownerSpawner.Release(this)`

**New:**
- Replace `ownerSpawner` with `ownerPool: SlimePool` field
- Rename `SetOwnerSpawner()` to `SetOwnerPool(SlimePool)`
- `ReturnToPoolAfterDelay()` calls `ownerPool.Release(this)`
- SlimeSpawner calls `slime.SetOwnerPool(slimePool)` after Get()

## Data Flow

```
Spawn Flow:
SlimeSpawner.SpawnOne()
  -> slimePool.Get()
  -> returns Slime (from pool or newly created)
  -> slime.SetOwnerPool(slimePool)
  -> Slime.InitializeSpawn(position, id, hp)
  -> gameObject.SetActive(true)

Release Flow:
Slime.StartDeathSequence()
  -> StartCoroutine(ReturnToPoolAfterDelay())
  -> yield WaitForSeconds(destroyDelay)
  -> ownerPool.Release(this)
  -> gameObject.SetActive(false)
  -> enters pooledItems queue
```

## Error Handling

**Pool:**
- Pool.Get() returns null if prefab not assigned (logs error)
- Pool.Get() creates new instance if pool is empty (no null return)
- Pool.Release() ignores null items (logs warning)
- Pool.Release() ignores items not from this pool (checks against allItems list)
- Pool.Release() handles duplicate release gracefully (item already inactive, logs warning)
- Pool handles prefab component missing gracefully (logs error, returns null)
- Pool.OnDestroy() cleanup prevents memory leaks

**Slime:**
- ReturnToPoolAfterDelay() checks if ownerPool is null (logs warning, does nothing)
- Slime remains deactivated if pool reference lost

## Testing

**Pool<T> Test Cases:**
1. Get() returns deactivated object
2. Get() on empty pool creates new instance
3. Release() deactivates object
4. Release() returns object to queue
5. Rapid Get/Release cycle (stress test)
6. Prewarm() creates correct count
7. Pool cleanup on scene change (OnDestroy)

**Integration Tests:**
- SlimeSpawner spawn loop with pool
- Slime death returns to pool
- Pool exhaustion recovery
- Slime death with null pool reference (warning logged, object deactivated)

## Migration Steps

1. Create `Pool.cs` in `Assets/Scripts/Core/` (generic Pool<T>)
2. Create `SlimePool.cs` in `Assets/Scripts/Monsters/` (inherits Pool<Slime>)
3. Modify `Slime.cs`:
   - Replace `ownerSpawner: SlimeSpawner` with `ownerPool: SlimePool`
   - Rename `SetOwnerSpawner()` to `SetOwnerPool()`
   - Update `ReturnToPoolAfterDelay()` to call `ownerPool.Release(this)`
4. Modify `SlimeSpawner.cs`:
   - Remove pooling logic (pooledSlimes, allSlimes, InitializePool, GetPooledSlime, CreatePooledSlime, Release)
   - Add `slimePool: SlimePool` reference (SerializeField)
   - Update `SpawnOne()` to use `slimePool.Get()` and `slime.SetOwnerPool()`
5. Update scene:
   - Remove old SlimePool GameObject (with missing script)
   - Create new SlimePool GameObject
   - Assign Slime prefab in Inspector
6. Verify: Run game, check spawn/death/pooling works correctly

## Files Changed

| File | Action | Details |
|------|--------|---------|
| `Assets/Scripts/Core/Pool.cs` | Create | Generic Pool<T> class |
| `Assets/Scripts/Monsters/SlimePool.cs` | Create | Inherits Pool<Slime> |
| `Assets/Scripts/Monsters/SlimeSpawner.cs` | Modify | Remove pooling, add pool reference |
| `Assets/Scripts/Monsters/Slime.cs` | Modify | Replace ownerSpawner with ownerPool |
| `Assets/Scenes/SampleScene.unity` | Modify | Update SlimePool GameObject |