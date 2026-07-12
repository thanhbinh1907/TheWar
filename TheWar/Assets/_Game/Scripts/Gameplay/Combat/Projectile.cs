using TowerDefense.Core;
using TowerDefense.Shared;
using UnityEngine;

namespace TowerDefense.Gameplay.Combat
{
    public class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _speed = 20f;
        [SerializeField] private float _arcHeight = 0f; // Bằng 0 thì đạn bay thẳng, > 0 thì đạn bay vòng cung
        
        private Transform _target;
        private float _damage;
        
        private GameObject _prefabContext;
        private ProjectilePool _poolContext;

        private Vector3 _currentLogicalPosition;
        private float _initialDistance;

        public void InitializePoolContext(GameObject prefab, ProjectilePool pool)
        {
            _prefabContext = prefab;
            _poolContext = pool;
        }

        public void Launch(Transform target, float damage)
        {
            _target = target;
            _damage = damage;
            
            _currentLogicalPosition = transform.position;
            _initialDistance = Vector3.Distance(_currentLogicalPosition, _target.position);
        }

        private void Update()
        {
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                ReturnToPool();
                return;
            }

            Vector3 direction = _target.position - _currentLogicalPosition;
            float distanceThisFrame = _speed * Time.deltaTime;

            if (direction.sqrMagnitude <= distanceThisFrame * distanceThisFrame)
            {
                HitTarget();
            }
            else
            {
                // Di chuyển vị trí logic (thẳng tắp) tới mục tiêu
                _currentLogicalPosition += direction.normalized * distanceThisFrame;
                
                // Tính phần trăm chặng đường (progress)
                float progress = 1f - (direction.magnitude / (_initialDistance > 0 ? _initialDistance : 1f));
                progress = Mathf.Clamp01(progress);
                
                // Tính chiều cao vòng cung bằng công thức Parabol: 4 * h * x * (1 - x)
                float arcOffset = 4f * _arcHeight * progress * (1f - progress);

                // Cập nhật vị trí thực tế = Vị trí logic + Chiều cao vòng cung
                Vector3 newPosition = _currentLogicalPosition + Vector3.up * arcOffset;
                
                // Hướng đầu mũi tên / viên đá theo hướng di chuyển tiếp theo
                Vector3 lookDirection = newPosition - transform.position;
                if (lookDirection.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }

                transform.position = newPosition;
            }
        }

        private void HitTarget()
        {
            if (_target.TryGetComponent<IDamageable>(out var damageable))
            {
                EventBus.Publish(new DamageEvent(damageable, _damage));
            }
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            _target = null;
            if (_poolContext != null && _prefabContext != null)
            {
                _poolContext.Release(_prefabContext, this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OnGetFromPool()
        {
            // IPoolable method, called by pool if we used a standard IPoolable implementation.
            // But since we use Unity's ObjectPool, we handle state via actionOnGet/actionOnRelease.
            // Leaving it here for IPoolable conformity.
        }

        public void OnReturnToPool()
        {
            // Left for IPoolable conformity
        }
    }
}
