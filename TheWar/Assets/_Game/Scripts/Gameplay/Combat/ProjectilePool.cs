using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace TowerDefense.Gameplay.Combat
{
    public class ProjectilePool : MonoBehaviour
    {
        private static ProjectilePool _instance;
        public static ProjectilePool Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("ProjectilePool");
                    _instance = go.AddComponent<ProjectilePool>();
                }
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        private Dictionary<GameObject, ObjectPool<Projectile>> _pools = new Dictionary<GameObject, ObjectPool<Projectile>>();
        private Transform _poolContainer;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            _poolContainer = transform;
        }

        public Projectile Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                Debug.LogError("ProjectilePool: Prefab is null!");
                return null;
            }

            if (!_pools.ContainsKey(prefab))
            {
                _pools[prefab] = CreatePoolForPrefab(prefab);
            }

            Projectile projectile = _pools[prefab].Get();
            projectile.transform.position = position;
            projectile.transform.rotation = rotation;
            return projectile;
        }

        public void Release(GameObject prefab, Projectile projectile)
        {
            if (_pools.TryGetValue(prefab, out var pool))
            {
                pool.Release(projectile);
            }
            else
            {
                Debug.LogWarning($"ProjectilePool: Trying to release a projectile but its pool doesn't exist. Destroying instead.");
                Destroy(projectile.gameObject);
            }
        }

        private ObjectPool<Projectile> CreatePoolForPrefab(GameObject prefab)
        {
            return new ObjectPool<Projectile>(
                createFunc: () =>
                {
                    GameObject go = Instantiate(prefab, _poolContainer);
                    Projectile proj = go.GetComponent<Projectile>();
                    if (proj == null)
                    {
                        proj = go.AddComponent<Projectile>();
                    }
                    proj.InitializePoolContext(prefab, this);
                    return proj;
                },
                actionOnGet: (proj) => proj.gameObject.SetActive(true),
                actionOnRelease: (proj) => proj.gameObject.SetActive(false),
                actionOnDestroy: (proj) => 
                {
                    if (proj != null && proj.gameObject != null)
                    {
                        Destroy(proj.gameObject);
                    }
                },
                collectionCheck: false,
                defaultCapacity: 20,
                maxSize: 200
            );
        }
    }
}
