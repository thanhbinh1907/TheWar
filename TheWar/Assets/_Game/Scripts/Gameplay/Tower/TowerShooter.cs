using System.Collections;
using TowerDefense.Core;
using TowerDefense.Gameplay.Combat;
using UnityEngine;

namespace TowerDefense.Gameplay.Tower
{
    [RequireComponent(typeof(TowerDetector))]
    public class TowerShooter : MonoBehaviour
    {
        [SerializeField] private Transform _firePoint;
        
        private TowerDetector _detector;
        private UnitData _currentData;
        private Coroutine _shootingCoroutine;

        private void Awake()
        {
            _detector = GetComponent<TowerDetector>();
            if (_firePoint == null)
            {
                // If firePoint is not assigned, use the tower's transform
                _firePoint = transform;
            }
        }

        public void Initialize(UnitData data)
        {
            _currentData = data;
            
            // Start detector
            _detector.Initialize(data.AttackRange);
            
            // Start shooting
            if (_shootingCoroutine != null)
            {
                StopCoroutine(_shootingCoroutine);
            }
            _shootingCoroutine = StartCoroutine(ShootingRoutine());
        }

        public void StopShooting()
        {
            if (_shootingCoroutine != null)
            {
                StopCoroutine(_shootingCoroutine);
                _shootingCoroutine = null;
            }
            _detector.StopDetecting();
            _currentData = null;
        }

        private IEnumerator ShootingRoutine()
        {
            while (true)
            {
                if (_currentData != null && _detector.CurrentTarget != null)
                {
                    Shoot(_detector.CurrentTarget);
                    
                    // Wait based on attack speed (attacks per second)
                    float fireInterval = 1f / Mathf.Max(_currentData.AttackSpeed, 0.1f);
                    yield return new WaitForSeconds(fireInterval);
                }
                else
                {
                    // Wait a bit before checking again if there's no target
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        private void Shoot(Transform target)
        {
            if (_currentData.ProjectilePrefab != null && ProjectilePool.Instance != null)
            {
                Projectile proj = ProjectilePool.Instance.Get(
                    _currentData.ProjectilePrefab, 
                    _firePoint.position, 
                    Quaternion.identity
                );
                
                if (proj != null)
                {
                    proj.Launch(target, _currentData.Damage);
                }
            }
            else
            {
                Debug.LogWarning("TowerShooter: Missing ProjectilePrefab or ProjectilePool Instance.");
            }
        }
    }
}
