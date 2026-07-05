# Tower Defense Game — Agent Rules
Unity 6 (URP) · C# · Synty Polygon Fantasy Pack

---

## Stack
- **Engine**: Unity 6 (6000.x LTS), Universal Render Pipeline (URP)
- **Language**: C# 10
- **Asset**: Synty Polygon Fantasy Kingdom Pack
- **Target**: PC + WebGL (itch.io)

---

## Architecture — Composition over Inheritance

**Do NOT use deep inheritance chains.** Unity is a component system — respect that.

```
// ❌ NEVER do this
class FlyingBossEnemy : BossEnemy : Enemy : Character { }

// ✅ Always do this — compose behaviours as components
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyMovement))]
public class Enemy : MonoBehaviour { }

public class FlyAbility : MonoBehaviour { }   // attach to flying enemies only
public class BossAbility : MonoBehaviour { }  // attach to boss only
```

**Rule**: Max 1 level of MonoBehaviour inheritance. If you need shared logic, use:
1. Interfaces (`IDamageable`, `IPoolable`)
2. Composition (add component)
3. ScriptableObject for shared data

---

## File & Folder Structure

```
Assets/
├── _Game/                    ← all project code lives here
│   ├── Scripts/
│   │   ├── Core/             ← managers, singletons, events
│   │   ├── Enemy/            ← enemy behaviours
│   │   ├── Tower/            ← tower behaviours
│   │   ├── Wave/             ← wave system
│   │   ├── UI/               ← UI controllers only
│   │   └── Shared/           ← interfaces, utilities, SO base classes
│   ├── Prefabs/
│   │   ├── Enemies/
│   │   ├── Towers/
│   │   └── Projectiles/
│   ├── ScriptableObjects/
│   │   ├── EnemyData/
│   │   └── TowerData/
│   └── Scenes/
└── Plugins/                  ← Synty assets only, never modify
    └── PolygonFantasy/
```

**Rules**:
- Never put scripts inside `Plugins/`
- One script per file, filename = class name
- No script in root `Assets/` folder

---

## Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Class | PascalCase | `EnemyHealth` |
| Interface | I + PascalCase | `IDamageable` |
| ScriptableObject | SO suffix | `EnemyDataSO` |
| MonoBehaviour | No suffix | `TowerShooter` |
| Private field | `_camelCase` | `_currentHealth` |
| Public property | PascalCase | `CurrentHealth` |
| Const | UPPER_SNAKE | `MAX_ENEMY_COUNT` |
| Event | On + PascalCase | `OnEnemyDied` |

---

## Performance Rules — Non-negotiable

### Object Pooling
```csharp
// NEVER use Instantiate/Destroy for enemies, projectiles, or VFX
// ALWAYS use ObjectPool<T>

// ✅ Correct
_pool.Get();
_pool.Release(obj);

// ❌ Wrong
Instantiate(prefab);
Destroy(gameObject);
```

### GetComponent
```csharp
// NEVER call GetComponent in Update/FixedUpdate
// Cache in Awake or use [SerializeField]

// ✅ Correct
private EnemyHealth _health;
void Awake() => _health = GetComponent<EnemyHealth>();

// ❌ Wrong
void Update() { GetComponent<EnemyHealth>().TakeDamage(1); }
```

### Animator
```csharp
// Always use integer hashes, never string names
private static readonly int _walkHash = Animator.StringToHash("Walk");
_animator.SetBool(_walkHash, true);

// Set culling mode on all enemy animators
_animator.cullingMode = AnimatorCullingMode.CullCompletely;
```

### Find methods
```csharp
// NEVER use Find, FindObjectOfType in Update or frequently called methods
// Use dependency injection, events, or serialize references instead

// ❌ Wrong
void Update() { FindObjectOfType<GameManager>().AddScore(10); }

// ✅ Correct — inject via SerializeField or singleton
[SerializeField] private GameManager _gameManager;
```

---

## Event System — Decouple Everything

Use C# events or UnityEvents to decouple systems. Tower should not know about Enemy class directly.

```csharp
// Enemy publishes
public event Action<float> OnDamageTaken;
public event Action OnDied;

// Tower subscribes — never calls enemy methods directly
enemy.OnDied += HandleEnemyDied;
```

---

## ScriptableObject for All Data

Every tunable value lives in a ScriptableObject. No hardcoded stats.

```csharp
[CreateAssetMenu(menuName = "Game/Enemy Data")]
public class EnemyDataSO : ScriptableObject {
    public string enemyName;
    public float maxHealth;
    public float moveSpeed;
    public float goldReward;
    public GameObject prefab;       // link to Synty prefab
    public Sprite icon;
}

[CreateAssetMenu(menuName = "Game/Tower Data")]
public class TowerDataSO : ScriptableObject {
    public string towerName;
    public float damage;
    public float range;
    public float fireRate;
    public int cost;
    public int upgradeCost;
}
```

---

## Code Quality Rules

- Max **50 lines per method** — split if longer
- Max **200 lines per class** — split into components if longer
- No magic numbers: `const float TOWER_BASE_RANGE = 3f;` not just `3f`
- No `Debug.Log` in production code — use `#if UNITY_EDITOR` guard
- Always use `[SerializeField] private` instead of `public` for Inspector fields
- `null` check before using any optional component reference

```csharp
// ✅ Inspector-exposed but encapsulated
[SerializeField] private TowerDataSO _data;

// ❌ Exposes internals unnecessarily
public TowerDataSO data;
```

---

## Pathfinding

This game uses custom A* (not Unity NavMesh) for learning purposes.

- Grid stored in `GridManager` singleton
- Node struct: `GridNode { Vector2Int pos; bool walkable; float gCost; float hCost; }`
- Pathfinding runs on path change only — NOT every frame
- Enemy caches its path and follows waypoints
- Recalculate path only when a tower is placed/removed

---

## When Adding New Enemy Type

1. Create `EnemyDataSO` asset in `ScriptableObjects/EnemyData/`
2. Create prefab from Synty asset in `Prefabs/Enemies/`
3. Attach `Enemy`, `EnemyHealth`, `EnemyMovement` components
4. Attach ability components only if needed (`FlyAbility`, `ShieldAbility`)
5. Link `EnemyDataSO` in the prefab's `Enemy` component
6. Register prefab in `EnemyPool`

Do NOT subclass `Enemy` to create new enemy types.

---

## When Adding New Tower Type

1. Create `TowerDataSO` asset in `ScriptableObjects/TowerData/`
2. Create prefab from Synty asset in `Prefabs/Towers/`
3. Attach `Tower`, `TowerShooter`, `TowerDetector` components
4. For special behaviour, attach additional components (`SlowEffect`, `SplashDamage`)
5. Link `TowerDataSO` in the prefab's `Tower` component

Do NOT subclass `Tower` to create new tower types.

---

## Fast Play Mode (No Domain Reload) Rules

This project runs with **Domain Reload disabled** for fast Play Mode iteration.
- If you declare a `static` variable, `Action`, or `Singleton`, it will NOT automatically reset when restarting Play Mode.
- You MUST manually reset all static variables using `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.

```csharp
// ✅ Correct — manual reset for No Domain Reload
private static ProjectilePool _instance;

[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetStatics()
{
    _instance = null;
}
```

---

## Commit Convention

```
feat: add flying enemy type
fix: enemy not recalculating path after tower placed
perf: pool projectiles to reduce GC alloc
refactor: extract damage logic into DamageHandler component
chore: import Synty pack v1.2
```
