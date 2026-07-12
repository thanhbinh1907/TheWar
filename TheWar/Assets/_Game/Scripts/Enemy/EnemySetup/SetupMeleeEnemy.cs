using UnityEngine;

namespace TowerDefense.Enemy.EnemySetup
{
    /// <summary>
    /// Kéo script này vào mô hình Quái Đánh Gần (Goblin Cận Chiến, Orc...).
    /// Nó sẽ tự động gắn tất cả các Script cần thiết như Enemy, EnemyHealth, EnemyMovement, EnemyCombat, v.v.
    /// </summary>
    [RequireComponent(typeof(TowerDefense.Enemy.Enemy))]
    public class SetupMeleeEnemy : MonoBehaviour
    {
        // Class này chỉ dùng để kích hoạt tính năng tự động gắn Component của Unity
    }
}
