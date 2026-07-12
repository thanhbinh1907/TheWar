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
        private TowerShooter _shooter;
        private TowerSpawner _spawner;
        private TowerUnitAnimation _animation;

        public void Initialize(UnitData data)
        {
            _data = data;
            
            // Lấy các component (nếu có) trên chính prefab của Lính
            _shooter = GetComponent<TowerShooter>();
            _spawner = GetComponent<TowerSpawner>();
            _animation = GetComponent<TowerUnitAnimation>();

            // Bơm data vào Animation
            if (_animation != null)
            {
                _animation.Initialize(data);
                _animation.SetAttackSpeed(data.AttackSpeed);
            }

            // Bơm data vào Shooter và nối Event
            if (_shooter != null)
            {
                _shooter.Initialize(data);
                
                if (_animation != null)
                {
                    _shooter.OnShoot += _animation.TriggerAttackAnimation;
                    _animation.OnHitEvent += _shooter.SpawnProjectile;
                }
            }

            // Bơm data vào Spawner (dùng cho nhà lính)
            if (_spawner != null)
            {
                _spawner.Initialize(data);
            }
        }

        private void OnDestroy()
        {
            // Dọn dẹp event và tắt súng
            if (_shooter != null)
            {
                if (_animation != null)
                {
                    _shooter.OnShoot -= _animation.TriggerAttackAnimation;
                    _animation.OnHitEvent -= _shooter.SpawnProjectile;
                }
                _shooter.StopShooting();
            }

            // Dọn dẹp lính chặn đường nếu bị tiêu diệt
            if (_spawner != null)
            {
                _spawner.DespawnGuards();
            }
        }
    }
}
