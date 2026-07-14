using TowerDefense.Core;
using TowerDefense.Tower;
using UnityEngine;

namespace TowerDefense.Gameplay.Tower
{
    /// <summary>
    /// Serves as the central brain for any unit placed on a tower (Mage, Archer, Barracks).
    /// Fetches all its components and injects the UnitData into them.
    /// </summary>
    public class TowerUnit : MonoBehaviour
    {
        private UnitData _data;
        private TowerSpawner _spawner;
        private TowerUnitAnimation _animation;
        private TowerDetector _detector;

        public void Initialize(UnitData data)
        {
            _data = data;
            
            // Lấy các component (nếu có) trên chính prefab của Lính
            _spawner = GetComponent<TowerSpawner>();
            _animation = GetComponent<TowerUnitAnimation>();
            _detector = GetComponent<TowerDetector>();

            // Bơm data vào Animation
            if (_animation != null)
            {
                _animation.Initialize(data);
                _animation.SetAttackSpeed(data.AttackSpeed);
            }

            // Khởi tạo Detector
            if (_detector != null)
            {
                _detector.Initialize(data.AttackRange);
            }

            // Khởi tạo Combat (dùng UnitController thay vì TowerShooter)
            var unitController = GetComponent<TowerDefense.Gameplay.Combat.UnitController>();
            if (unitController != null)
            {
                unitController.SetAttackRange(data.AttackRange);
                unitController.SetAttackSpeed(data.AttackSpeed);
            }

            var simpleAttack = GetComponent<TowerDefense.Gameplay.Combat.SimpleAttack>();
            if (simpleAttack != null)
            {
                simpleAttack.SetDamage(data.Damage);
            }
            
            var chargedAttack = GetComponent<TowerDefense.Gameplay.Combat.ChargedAttack>();
            if (chargedAttack != null)
            {
                // Giả sử lấy damage từ UnitData, cho min=half, max=full
                chargedAttack.SetDamageRange(data.Damage * 0.5f, data.Damage);
            }

            var simpleRanged = GetComponent<TowerDefense.Gameplay.Combat.SimpleRangedAttack>();
            if (simpleRanged != null)
            {
                simpleRanged.SetDamage(data.Damage);
            }

            // Bơm data vào Spawner (dùng cho nhà lính)
            if (_spawner != null)
            {
                _spawner.Initialize(data);
            }
        }

        private void OnDestroy()
        {
            // Dừng Detector
            if (_detector != null)
            {
                _detector.StopDetecting();
            }

            // Dọn dẹp lính chặn đường nếu bị tiêu diệt
            if (_spawner != null)
            {
                _spawner.DespawnGuards();
            }
        }
    }
}
