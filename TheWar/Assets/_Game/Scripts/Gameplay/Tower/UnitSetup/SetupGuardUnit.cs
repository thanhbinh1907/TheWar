using UnityEngine;

namespace TowerDefense.Gameplay.Tower.UnitSetup
{
    /// <summary>
    /// Kéo script này vào mô hình Lính cận chiến (Guard/Bruiser) chặn đường.
    /// Nó sẽ tự động gắn GuardUnit, TowerUnitAnimation, Animator và Collider.
    /// </summary>
    [RequireComponent(typeof(GuardUnit))]
    [RequireComponent(typeof(TowerDefense.Gameplay.Combat.UnitController))]
    [RequireComponent(typeof(TowerDefense.Gameplay.Combat.SimpleAttack))]
    [RequireComponent(typeof(GuardDetector))]
    [RequireComponent(typeof(GuardMovement))]
    [RequireComponent(typeof(TowerDefense.Tower.TowerUnitAnimation))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class SetupGuardUnit : MonoBehaviour
    {
        // Class này chỉ dùng để kích hoạt tính năng tự động gắn Component của Unity
    }
}
