using UnityEngine;

namespace TowerDefense.Gameplay.Tower.UnitSetup
{
    /// <summary>
    /// Kéo script này vào mô hình Lính Bắn Xa (Pháp Sư, Cung Thủ).
    /// Nó sẽ tự động gắn tất cả các Script cần thiết như TowerUnit, TowerShooter, TowerDetector, TowerUnitAnimation.
    /// </summary>
    [RequireComponent(typeof(TowerUnit))]
    [RequireComponent(typeof(TowerDetector))]
    [RequireComponent(typeof(TowerDefense.Gameplay.Combat.UnitController))]
    [RequireComponent(typeof(TowerDefense.Gameplay.Combat.ChargedAttack))]
    [RequireComponent(typeof(TowerDefense.Tower.TowerUnitAnimation))]
    [RequireComponent(typeof(Animator))]
    public class SetupRangedTower : MonoBehaviour
    {
        // Class này chỉ dùng để kích hoạt tính năng tự động gắn Component của Unity
    }
}
