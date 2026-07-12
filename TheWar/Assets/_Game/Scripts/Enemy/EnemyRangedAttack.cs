using TowerDefense.Gameplay.Combat;
using TowerDefense.Gameplay.Tower;
using UnityEngine;

namespace TowerDefense.Enemy
{
    public class EnemyRangedAttack : MonoBehaviour, IEnemyAttackBehavior
    {
        [Header("Ranged Settings")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;

        public void ExecuteAttack(GuardUnit target, float damage)
        {
            if (_projectilePrefab == null)
            {
                Debug.LogError($"[{gameObject.name}] Missing Projectile Prefab in EnemyRangedAttack!");
                return;
            }

            Vector3 spawnPosition = _firePoint != null ? _firePoint.position : transform.position + Vector3.up;
            
            // Get projectile from pool and launch it
            var projectile = ProjectilePool.Instance.Get(_projectilePrefab, spawnPosition, Quaternion.identity);
            if (projectile != null)
            {
                projectile.Launch(target.transform, damage);
            }
        }
    }
}
