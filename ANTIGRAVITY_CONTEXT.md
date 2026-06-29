
# Tower Defense — Full Project Context for Antigravity AI

## Stack

- Unity 6 (6000.x LTS), Universal Render Pipeline (URP)
- C# 10, Composition-over-Inheritance (KHÔNG dùng class kế thừa sâu)
- Asset: Synty Polygon Fantasy Kingdom Pack (low poly, GPU Instancing enabled)
- Target: PC + WebGL (itch.io)

---

## Game Design Document

### Bối cảnh

Thế giới gồm 5 Vùng Đất (5 bộ Synty asset). Thế lực hắc ám đánh cắp Nguyên Tố cốt lõi của từng vùng khiến mỗi vùng **mất hoàn toàn 1 Class quân đội**. Người chơi (giữ Ấn Cổ) đi qua các vùng đất, dùng thẻ bài của vùng này để "vá" lỗ hổng chiến thuật cho vùng khác.

### Ma trận 5 Vùng Đất × Khuyết Tật

| Vùng Đất                | Asset Synty | Mất Nguyên Tố | Class bị khóa                 |
| -------------------------- | ----------- | ---------------- | ------------------------------- |
| Vương Quốc Thần Thoại | Kingdom     | 🔥 Hỏa          | ❌ Pháo Binh (AoE)             |
| Thung Lũng Hoang Dã      | Adventure   | 💨 Phong         | ❌ Cung Thủ (tầm xa)          |
| Vùng Biển Thất Lạc     | Pirate      | 🌊 Thủy         | ❌ Pháp Sư (phép)            |
| Thánh Địa Samurai       | Samurai     | ⛰️ Thổ        | ❌ Đấu Sĩ (block/chặn)      |
| Hầm Ngục U Tối          | Dungeons    | ⚡ Lôi          | ❌ Sát Thủ (blink/cơ động) |

### Core Gameplay Loop (PvZ × TFT)

1. Trước trận: Người chơi chọn thẻ Quân Cờ từ bộ sưu tập (PvZ style)
2. Trong trận: Click thẻ → Click vào **Khung Tháp cố định** trên map → Tháp hấp thụ lõi, đổi hành vi theo Class
3. Kích hoạt Tộc/Hệ (TFT style) khi đủ số lượng cùng Faction/Class

### Cơ chế Socket Tower (QUAN TRỌNG)

- **KHÔNG spawn tháp tự do khi click đất trống**
- Map được đặt sẵn Khung Tháp cố định theo level design
- GridManager chỉ dùng để: quản lý trạng thái ô + tính A* cho enemy
- Flow đúng: Chọn thẻ (PlacementManager) → Click Khung Tháp (TowerSocket) → Tháp đổi màu/stat

---

## Cấu Trúc Thư Mục

```
Assets/
├── _Game/
│   ├── Scripts/
│   │   ├── Core/          ← GridManager, GridNode, PlacementManager, TowerSocket, UnitData
│   │   ├── Enemy/         ← EnemyHealth, EnemyMovement, EnemyDataSO
│   │   ├── Tower/         ← TowerDetector, TowerShooter, TowerUpgrade
│   │   ├── Wave/          ← WaveManager
│   │   ├── UI/            ← CardUI, HandUI
│   │   └── Shared/        ← IDamageable, IPoolable
│   ├── Prefabs/
│   │   ├── Enemies/
│   │   ├── Towers/
│   │   └── Projectiles/
│   └── ScriptableObjects/
│       ├── UnitData/
│       └── EnemyData/
└── Plugins/
    └── PolygonFantasy/    ← Synty asset, KHÔNG sửa file trong này
```

---

## Code Hiện Có (Đã Chạy Thành Công)

### GridNode.cs

```csharp
namespace TowerDefense.Core {
    public class GridNode {
        public int X { get; }
        public int Y { get; }
        public bool Walkable { get; set; }
        public bool HasTower { get; set; }
        // A* data
        public float GCost { get; set; }
        public float HCost { get; set; }
        public float FCost => GCost + HCost;
        public GridNode Parent { get; set; }

        public GridNode(int x, int y, bool walkable) {
            X = x; Y = y; Walkable = walkable;
        }
    }
}
```

### GridManager.cs (Singleton)

- Width, Height, CellSize (=2), OriginPosition configurable từ Inspector
- `GetNode(int x, int y)` — trả về GridNode hoặc null nếu out of bounds
- `GetWorldPosition(int x, int y)` — tâm ô theo world space
- `GetXY(Vector3 worldPos, out int x, out int y)` — world → grid index
- `SetWalkable(int x, int y, bool walkable)`
- `OnDrawGizmos`: ô trắng = walkable, đỏ = blocked
- Road ở cột 4-6 đã bị SetWalkable(false) trong Start()

### UnitData.cs (ScriptableObject)

```csharp
using UnityEngine;
namespace TowerDefense.Core {
    public enum UnitClass { Bruiser, Ranger, Mage, Assassin, Artillery }
    public enum FactionVoundDat { Kingdom, Adventure, Pirate, Samurai, Dungeons }

    [CreateAssetMenu(fileName = "NewUnitData", menuName = "TowerDefense/Unit Data")]
    public class UnitData : ScriptableObject {
        public string unitName;
        public UnitClass unitClass;
        public FactionVoundDat faction;
        public float damage = 10f;
        public float attackRange = 5f;
        public float attackSpeed = 1f;
        public Color unitColor = Color.white;
    }
}
```

### PlacementManager.cs (Singleton)

- Ghi nhớ `selectedUnit` (UnitData) người chơi đang chọn
- `SelectUnitCard(UnitData data)` — chọn thẻ
- `ClearSelection()` — bỏ chọn sau khi cắm xong

### TowerSocket.cs (Gắn vào Khung Tháp prefab)

```csharp
using UnityEngine;
namespace TowerDefense.Core {
    public class TowerSocket : MonoBehaviour {
        public bool IsOccupied { get; private set; }
        public UnitData CurrentUnitData { get; private set; }
        private MeshRenderer _meshRenderer;

        private void Awake() => _meshRenderer = GetComponent<MeshRenderer>();

        private void OnMouseDown() {
            if (PlacementManager.Instance?.selectedUnit == null) return;
            if (IsOccupied) return;
            PlugUnit(PlacementManager.Instance.selectedUnit);
        }

        public void PlugUnit(UnitData data) {
            CurrentUnitData = data;
            IsOccupied = true;
            // MaterialPropertyBlock — KHÔNG tạo material instance, giữ GPU Instancing
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", data.unitColor);
            _meshRenderer.SetPropertyBlock(block);
            PlacementManager.Instance.ClearSelection();
        }
    }
}
```

---

## Quy Tắc Kiến Trúc (BẮT BUỘC TUÂN THEO)

### Composition, không kế thừa

```csharp
// ❌ KHÔNG BAO GIỜ làm thế này
class FlyingBossEnemy : BossEnemy : Enemy { }

// ✅ LUÔN làm thế này — thêm component
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyMovement))]
public class Enemy : MonoBehaviour { }
public class FlyAbility : MonoBehaviour { }   // gắn thêm nếu cần
public class ShieldAbility : MonoBehaviour { } // gắn thêm nếu cần
```

- Max 1 cấp kế thừa MonoBehaviour
- Hành vi mới = component mới, KHÔNG phải subclass mới

### Performance — Bắt buộc

```csharp
// 1. Cache GetComponent trong Awake, KHÔNG gọi trong Update
private EnemyHealth _health;
void Awake() => _health = GetComponent<EnemyHealth>();

// 2. Object Pool cho enemy, đạn, VFX — KHÔNG Instantiate/Destroy
using UnityEngine.Pool;
private ObjectPool<GameObject> _pool;

// 3. Animator hash — KHÔNG dùng string
private static readonly int _walkHash = Animator.StringToHash("Walk");
_animator.cullingMode = AnimatorCullingMode.CullCompletely;

// 4. MaterialPropertyBlock — KHÔNG material.color (đã áp dụng trong TowerSocket)

// 5. Tất cả prop tĩnh trên map: đánh Static trong Inspector
```

### ScriptableObject cho mọi data

- Mọi số liệu (stats, cost, prefab ref) → ScriptableObject, KHÔNG hardcode
- Naming: `EnemyDataSO`, `TowerDataSO`, `WaveDataSO`

### Event system — decouple

```csharp
// Tower KHÔNG gọi enemy trực tiếp
// Enemy publish event, Tower/Manager subscribe
public event Action<float> OnDamageTaken;
public event Action OnDied;
// Tower dùng interface IDamageable, không import class Enemy
```

### Naming convention

| Type             | Convention                         | Ví dụ                                     |
| ---------------- | ---------------------------------- | ------------------------------------------- |
| Private field    | `_camelCase`                     | `_currentHealth`                          |
| SerializeField   | `private` + `[SerializeField]` | `[SerializeField] private UnitData _data` |
| Interface        | `I` prefix                       | `IDamageable`                             |
| ScriptableObject | `SO` suffix                      | `EnemyDataSO`                             |
| Event            | `On` prefix                      | `OnEnemyDied`                             |

### File limits

- 1 class = 1 file, tên file = tên class
- Method tối đa 50 dòng
- Class tối đa 200 dòng
- `Debug.Log` chỉ trong `#if UNITY_EDITOR`

---

## Roadmap Còn Lại

### Milestone 2 — Enemy chạy được (Đang làm)

- `EnemyDataSO`: maxHealth, moveSpeed, goldReward, prefab
- `EnemyHealth` component: máu, nhận IDamageable.TakeDamage(), event OnDied
- `EnemyMovement` component: đi theo waypoint tĩnh trước, sau thay bằng A*
- `EnemyPool`: UnityEngine.Pool.ObjectPool, size mặc định 20
- `WaveManager`: spawn enemy từ pool theo thời gian

### Milestone 3 — A* Pathfinding

- `AStarPathfinder`: class thuần (KHÔNG MonoBehaviour), dùng MinHeap tự code
- `EnemyMovement` gọi FindPath() khi spawn
- Khi tower cắm → GridManager.SetWalkable(false) → trigger recalculate path toàn bộ enemy đang sống

### Milestone 4 — Tower bắn

- `TowerDetector`: Physics.OverlapSphere tìm enemy trong attackRange
- `TowerShooter`: spawn đạn từ pool, rotate về target, gọi IDamageable
- `ProjectilePool`: pool đạn riêng
- Stats lấy từ `CurrentUnitData` trong TowerSocket (damage, attackRange, attackSpeed)

### Milestone 5 — UI & Economy

- `CardUI`: hiện thẻ Unit, click → PlacementManager.SelectUnitCard()
- `HandUI`: quản lý danh sách thẻ được chọn trước trận
- `GameManager`: gold, base HP, win/lose condition
- Faction synergy system (TFT style — kích hoạt khi đủ X quân cùng Faction)

---

## Cách Dùng File Này

**Trả lời bằng tiếng Việt, code và comment bằng tiếng Anh.**

Trước khi viết code mới, hãy:

1. Nêu component sẽ tạo
2. Interface nào nó implement
3. Event nào nó publish/subscribe
4. File đặt ở thư mục nào

Không tạo subclass để làm loại enemy/tower mới.
Không dùng NavMesh — dự án tự code A* để học thuật toán.
Không dùng Resources.Load — dùng SerializeField reference trực tiếp.
Không thêm package bên ngoài khi chưa hỏi.
