using TowerDefense.Core;
using TowerDefense.Shared;
using UnityEngine;

namespace TowerDefense.Gameplay.Combat
{
    public class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _speed = 20f;
        
        private Transform _target;
        private float _damage;
        
        private GameObject _prefabContext;
        private ProjectilePool _poolContext;

        public void InitializePoolContext(GameObject prefab, ProjectilePool pool)
        {
            _prefabContext = prefab;
            _poolContext = pool;
        }

        public void Launch(Transform target, float damage)
        {
            _target = target;
            _damage = damage;
        }

        private void Update()
        {
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                ReturnToPool();
                return;
            }

            Vector3 direction = _target.position - transform.position;
            float distanceThisFrame = _speed * Time.deltaTime;

            if (direction.sqrMagnitude <= distanceThisFrame * distanceThisFrame)
            {
                HitTarget();
            }
            else
            {
                transform.Translate(direction.normalized * distanceThisFrame, Space.World);
                transform.LookAt(_target);
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
