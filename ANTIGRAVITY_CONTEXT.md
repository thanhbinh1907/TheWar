# Tower Defense 3D — Map Editor Architecture (Waypoint Edition) — Full Project Context for Antigravity AI
> Cập nhật: 05/07/2026 | Unity 6 LTS | C# | URP | Synty Polygon Assets
> Bản sửa đổi: Waypoint + Steering, Corner Smoothing, Wall Force đọc trực tiếp từ road mesh (không hardcode độ rộng).

---

## I. MỤC TIÊU

- Game TD 3D có **Map Editor** cho người chơi tự vẽ đường đi (Waypoint) và đặt map.
- Map **chia sẻ được** dưới dạng JSON nhẹ (mảng tọa độ Waypoint + Socket).
- Chịu tải **hàng trăm đến hàng nghìn enemy** cùng lúc.
- Kiến trúc dễ mở rộng, không phải refactor lớn khi thêm tính năng.
- **Không giả định trước kích thước map/đường đi** — mọi hệ thống (di chuyển, validation) phải tự thích ứng theo road mesh thật của từng map người chơi tạo, vì mỗi map rộng hẹp khác nhau và có thể có đoạn rộng đoạn hẹp trong cùng 1 map.

---

## II. TRIẾT LÝ THIẾT KẾ

> **Game không xoay quanh AI tìm đường phức tạp. Game xoay quanh dữ liệu Tuyến đường (Waypoint Path) do người chơi vẽ trong Map Editor.**

```
Người chơi vẽ đường (đặt các Waypoint)
        ↓
Engine liên kết thành Path Network + bo góc cua
        ↓
Enemy đi tuần tự qua các Node bằng Steering (Seek + Separation + Wall Force)
```

Người chơi **không bao giờ** phải:
- Tự cấu hình Grid/Cost Field
- Tính pathfinding thủ công
- Khai báo độ rộng đường bằng số — độ rộng đọc thẳng từ mesh vẽ

Tóm gọn 1 câu: **Game chỉ lưu Map Data + mảng tọa độ Waypoint. Khi Load, Runtime dựng lại path, bo góc, sinh Collider mép đường từ chính road mesh. Enemy không tính pathfinding mỗi frame — chỉ Seek tới waypoint tiếp theo, tự né nhau và tự né mép đường bằng lực vật lý.**

---

## III. KIẾN TRÚC TỔNG THỂ

```
Map Editor (Đặt Terrain/Road mesh, vẽ chuỗi Waypoint, đặt Socket)
    │
    ▼
Map Data JSON (chỉ lưu và chia sẻ chuỗi text này)
    │
    ▼
Load & Reconstruction Pipeline
    │
    ├── Path Linker (nối Waypoint thành tuyến liên tục)
    ├── Path Corner Smoother (bo tròn góc cua bằng Quadratic Bezier)
    ├── Road Edge Baker (quét mép road mesh → sinh Collider layer RoadEdge)
    ├── Socket Spawner (sinh điểm đặt trụ tại vị trí đã lưu)
    └── Validation Pipeline
    │
    ▼
Gameplay Runtime
```

**Save**: chỉ lưu Map Data ở dạng dữ liệu nguyên thủy (`Vector3`, ID Enemy/Tower). Road mesh là 1 phần Map Data (hình dạng do người chơi vẽ), không lưu số liệu độ rộng tách rời.
**Không lưu**: bất kỳ dữ liệu runtime tính toán được (path đã bo góc, Collider mép đường — luôn bake lại khi Load, không cache).
**Load**: đọc JSON → sinh Point object ẩn trong scene → bo góc path → bake Collider mép đường → gán vào WaveManager → sẵn sàng chơi.

---

## IV. AI SYSTEM — WAYPOINT + STEERING

### Không dùng
- ❌ A*/Dijkstra tính theo từng enemy.
- ❌ NavMesh.
- ❌ Flow Field/Cost Field/Distance Field (bake toàn field) — dư thừa cho map dạng lane, tháp luôn đặt ngoài đường đi.
- ❌ Hằng số độ rộng đường cố định (`roadWidth`/`corridorWidth` dạng số tĩnh) — độ rộng phải đọc từ road mesh thật, không giả định trước.
- ❌ Lane cố định theo cột/formation (đã thử, rollback vì không giữ được song song qua khúc cua) — thay bằng để Separation + Wall Force tự nhiên dàn quái trong bề rộng đường thật.
- ❌ Set `transform.position` trực tiếp trong `Update()`, trừ class Steering.

### Cơ chế di chuyển của Enemy — Steering 3 lực

```
Target Waypoint tiếp theo (đã bo góc)
        ↓
Seek Force (kéo về waypoint) + Separation Force (né quái khác)
        + Wall Force (né mép đường, đọc từ Collider RoadEdge bake sẵn)
        ↓
Velocity cuối cùng
        ↓
Xoay mượt bằng Slerp (_rotationSpeed) + Di chuyển
```

- **Seek**: hướng tới `waypointPositions[index]`, chuyển waypoint sớm theo `_arriveRadius` (look-ahead) để bo cua mượt, không đợi chạm sát điểm.
- **Separation**: `Physics.OverlapSphere` bán kính nhỏ, layer Enemy, throttle qua `InvokeRepeating` mỗi 0.1s.
- **Wall Force**: bắn 2 tia ngắn vuông góc velocity sang 2 bên, layer `RoadEdge`; nếu trúng, đẩy ngược vào tâm đường, cường độ tỉ lệ nghịch khoảng cách. Đường hẹp → đẩy sớm; đường rộng → không đẩy, enemy tự do dàn theo Separation. **Không cần biết trước độ rộng bao nhiêu.**

### Path Corner Smoother — bo góc cua

Waypoint người chơi vẽ theo lưới road thường tạo góc vuông 90°, làm enemy đi "vuông". Xử lý **1 lần lúc Bake/Load map** (không phải mỗi frame), chỉ bo tại các góc, giữ nguyên đoạn thẳng:

```csharp
// Pure static class — Gameplay/AI/PathCornerSmoother.cs
public static List<Vector3> RoundCorners(List<Vector3> rawWaypoints, float cornerRadius, int segmentsPerCorner)
```
Dùng Quadratic Bezier tại mỗi waypoint trung gian, control point = chính góc cua gốc. `cornerRadius` giới hạn không vượt nửa đoạn liền kề (tránh 2 góc gần nhau đè lên nhau).

### Cấu trúc dữ liệu tuyến đường

```csharp
[System.Serializable]
public class PathData
{
    public string pathId;
    public List<Vector3> waypointPositions; // đã qua PathCornerSmoother lúc bake
}
```

### Enemy đọc Waypoint mỗi frame

```
Mỗi frame:
1. Enemy biết đang nhắm Waypoint[index] nào (int _currentWaypointIndex)
2. Seek Force hướng tới waypoint đó
3. Cộng Separation Force + Wall Force (throttle 0.1s)
4. Slerp xoay theo velocity, move theo velocity tổng hợp
5. Nếu khoảng cách < _arriveRadius → _currentWaypointIndex++
```

Enemy chỉ giữ 1 số index, không giữ path riêng — path là dữ liệu dùng chung (reference).

---

## V. MAP EDITOR & VALIDATION

Người chơi ở chế độ Editor chỉ tương tác với:
- **Spawn Point**: Waypoint đầu tiên.
- **Goal Point**: Waypoint cuối cùng (nhà chính).
- **Waypoints**: điểm mốc trung gian nối Spawn → Goal (được bo góc tự động khi bake).
- **Tower Socket**: vị trí đặt trụ cố định, nằm ngoài đường đi.
- **Obstacle**: vật cản trang trí/tầm nhìn.

### Validation trước khi Publish/Share
- **Path Connectivity**: `waypointPositions` không rỗng, điểm đầu trùng Spawn, điểm cuối dẫn tới Goal.
- **Overlap Check (Socket/Obstacle vs Road)**: dùng `Physics.CheckSphere` tại vị trí Socket/Obstacle với layer `Road` — nếu chạm road thật → fail. **Không dùng khoảng cách so với hằng số** — kiểm tra trực tiếp trên hình dạng mesh thật, đúng với mọi map rộng/hẹp khác nhau.
- **Wave Check**: mọi Wave có ít nhất 1 enemy, không có giá trị âm.

**Không hợp lệ → khóa Export/Publish.**

Vì Socket luôn bị validate nằm ngoài road thật → enemy **không cần** và **không thể** tấn công tháp, không cần tính va chạm vật lý Tower-Enemy ở runtime.

---

## VI. COMPONENT RULES

### Mỗi Component chỉ 1 nhiệm vụ
```
Enemy/
├── EnemyMovement   (quản lý _currentWaypointIndex, nhận Steering output để di chuyển)
├── EnemySteering   (Seek + Separation + Wall Force, Slerp xoay)
├── EnemyHealth
├── EnemyCombat     (trừ máu người chơi khi tới Goal)
└── EnemyAnimation  (đồng bộ tốc độ Steering vào Animator)
```
❌ Không có `EnemyController` ôm hết logic.

### Event System — decouple bằng Event Bus
```
❌ Tower → gọi trực tiếp Enemy.TakeDamage()
✅ Tower → Damage Event → Event Bus → Enemy subscribe
```

### Data — ScriptableObject cho mọi thứ
Enemy, Tower, Projectile, Buff, Debuff, Wave, Map Rule — tất cả là ScriptableObject, không hard-code số liệu.

### Naming Convention
| Type | Convention | Ví dụ |
|------|-----------|-------|
| Private field | `_camelCase` | `_currentWaypointIndex` |
| SerializeField | `private` | `[SerializeField] private EnemyDataSO _data` |
| Interface | `I` prefix | `IDamageable` |
| ScriptableObject | `SO` suffix | `EnemyDataSO` |
| Event | `On` prefix | `OnEnemyDied` |

### File Limits
- 1 class = 1 file, tên file = tên class
- Method tối đa 50 dòng, Class tối đa 200 dòng
- `Debug.Log` chỉ trong `#if UNITY_EDITOR`
- Không Singleton bừa bãi — chỉ chấp nhận cho hệ thống Core tĩnh (`EventBus`, `ObjectPool`) nếu thực sự cần
- Không dùng `Resources.Load()` — tài nguyên tham chiếu qua ScriptableObject
- Không Manager gọi Manager, không God Object

---

## VII. CẤU TRÚC THƯ MỤC

```
Assets/
├── _Game/
│   ├── Scripts/
│   │   ├── Core/          ← Event Bus, Object Pool, base interfaces
│   │   ├── Gameplay/
│   │   │   ├── AI/        ← PathData, PathContainer, PathValidator, PathCornerSmoother
│   │   │   ├── Combat/    ← IDamageable, DamageEvent, ProjectilePool
│   │   │   ├── Enemy/     ← EnemyMovement, EnemySteering, EnemyHealth, EnemyDataSO
│   │   │   ├── Tower/     ← TowerSocket, TowerDetector, TowerShooter, TowerDataSO
│   │   │   ├── Wave/      ← WaveManager, WaveDataSO, EnemySpawnEntry
│   │   │   └── Map/       ← MapData, SpawnPoint, GoalPoint, SocketData, RoadEdgeBaker
│   │   ├── Editor/        ← Map Editor tools, Waypoint draw UI, Validation Tool
│   │   ├── UI/            ← CardUI, EditorHUD, GameplayHUD
│   │   ├── Save/          ← MapSerializer (Map Data ↔ JSON)
│   │   └── Debug/
│   ├── Prefabs/
│   └── ScriptableObjects/
└── Plugins/
    └── PolygonFantasy/    ← KHÔNG sửa file trong này
```

---

## VIII. ROADMAP

1. Bootstrap & Core Framework (Event Bus, Object Pool)
2. Waypoint Data Structure (`PathData`, `PathContainer`, `PathValidator`)
3. Enemy Steering (Seek + Separation qua Waypoint)
4. Path Corner Smoother (bo góc cua)
5. Road Edge Baker + Wall Force (né mép đường không hardcode độ rộng)
6. Combat Layer (`IDamageable`, Projectile Pool, Tower dò mục tiêu)
7. Tower Socket System (load Socket từ Map Data, cắm thẻ runtime)
8. Wave Manager (đọc config Wave từ Map Data, spawn qua Object Pool)
9. Map Editor UI (click chấm điểm tạo Waypoint, đặt Socket)
10. Map Serializer & Validation (JSON, lọc lỗi logic trước khi Publish)
11. Save/Load
12. Optimization

---

## IX. NHỮNG GÌ ĐÃ THỬ VÀ BỎ

- ❌ Flow Field / Cost Field / Distance Field / Grid-based A* / NavMesh — bỏ khi chuyển sang Waypoint.
- ❌ **Grid Formation Spawn + Fixed Lane Offset theo cột** — thử để enemy đi thành nhiều làn song song, nhưng lane cố định không giữ được song song qua khúc cua (hướng vuông góc đổi theo từng đoạn, cột formation lúc spawn không khớp lane lúc di chuyển). Đã rollback.
- ❌ **Corridor Width dạng hằng số cố định** — thử random lane trong 1 bề rộng cố định, nhưng nhận ra mỗi map người chơi vẽ rộng/hẹp khác nhau, không thể hardcode 1 con số đúng cho mọi map. Thay bằng Wall Force đọc trực tiếp từ Collider mép đường (mục IV).

**Giải pháp hiện tại cho "quái dàn hàng"**: không ép lane bằng code — để `Separation Force` + `Wall Force` (tự thích ứng theo road mesh thật) tự nhiên dàn quái trong bề rộng đường, đường rộng dàn nhiều, đường hẹp tự nén lại.

---

## X. GAMEPLAY LAYER (áp dụng trên Core Waypoint mới)

> Thiết kế nội dung, không phải kiến trúc engine — vẫn chạy được trên nền Waypoint.

- **5 Vùng Đất × Khuyết Tật**: mỗi theme map khóa 1 Class quân đội (Pháo Binh/Cung Thủ/Pháp Sư/Đấu Sĩ/Sát Thủ).
- **Deck 6 thẻ kiểu TFT**: thuần chủng (buff tối thượng) vs lai (vá lỗ hổng Class).
- **Thủ Tĩnh (Socket)** vs **Thả Động (Free Deploy)**: 2 luồng đặt quân theo `PlacementMode`.

### Socket System — giữ nguyên quyết định đã chốt

```
Map Editor (Build-time)              Gameplay (Runtime)
────────────────────────             ──────────────────
Người chơi đặt tọa độ Socket    →    Người chơi cắm thẻ Unit
trong danh sách towerSockets          vào Socket (PlugUnit
của Map Data.                         qua PlacementManager)
```

- **Build-time**: tọa độ Socket lưu trong `List<Vector3> towerSockets` của Map Data, cùng lúc với Waypoint.
- **Runtime**: `MapSerializer` đọc JSON → sinh prefab `TowerSocket` tại các tọa độ đó → người chơi chọn thẻ, click Socket trống → `PlugUnit()`.
- **Validation**: Socket bắt buộc không chạm layer `Road` thật (mục V) → không cần enemy target/attack tháp, không cần physics phá tháp.

---

## XI. CÁCH DÙNG FILE NÀY

**Trả lời bằng tiếng Việt, code và comment bằng tiếng Anh.**

Trước khi viết code mới, luôn nêu:
1. Component nào sẽ tạo (tên Class, mục đích)
2. Interface nào implement (`IDamageable`, `IPoolable`...)
3. Event nào publish/subscribe qua Event Bus
4. File đặt ở thư mục nào (theo mục VII)

**Nghiêm cấm:**
- Set `transform.position` trực tiếp trong `Update()`, trừ class Steering
- Tạo subclass để làm loại enemy/tower mới (composition, không inheritance)
- Dùng NavMesh / A* / Flow Field / Grid-based pathfinding cho bất kỳ enemy nào
- Hardcode độ rộng đường bằng hằng số — mọi kiểm tra độ rộng/mép đường phải đọc từ road mesh/Collider thật
- Dùng `Resources.Load()`
- Singleton bừa bãi — chỉ Core tĩnh (`EventBus`, `ObjectPool`) mới được phép
- Thêm package bên ngoài khi chưa hỏi
- Gọi `GetComponent` trong `Update`
- Dùng `Instantiate`/`Destroy` cho enemy/đạn/VFX — luôn qua Object Pool
- Lưu path đã bo góc/Collider mép đường đã bake vào Save — chỉ lưu Map Data gốc, luôn dựng lại khi Load
