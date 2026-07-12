using UnityEngine;

namespace TowerDefense.Gameplay.Tower.UnitSetup
{
    /// <summary>
    /// Kéo script này vào mô hình Lều Trại (Nhà lính).
    /// Nó sẽ tự động gắn TowerUnit và TowerSpawner.
    /// </summary>
    [RequireComponent(typeof(TowerUnit))]
    [RequireComponent(typeof(TowerSpawner))]
    public class SetupSpawnerTower : MonoBehaviour
    {
        // Class này chỉ dùng để kích hoạt tính năng tự động gắn Component của Unity
    }
}
