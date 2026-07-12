using UnityEngine;

namespace TowerDefense.Enemy.EnemySetup
{
    /// <summary>
    /// Kéo script này vào mô hình Quái Đánh Xa (Goblin Ném Đá, Pháp Sư...).
    /// Nó sẽ tự động gắn tất cả các Script cần thiết như Enemy (bao gồm Movement, Health, Combat) 
    /// và đặc biệt là EnemyRangedAttack.
    /// </summary>
    [RequireComponent(typeof(TowerDefense.Enemy.Enemy))]
    [RequireComponent(typeof(TowerDefense.Enemy.EnemyRangedAttack))]
    public class SetupRangedEnemy : MonoBehaviour
    {
        // Class này chỉ dùng để kích hoạt tính năng tự động gắn Component của Unity
    }
}
