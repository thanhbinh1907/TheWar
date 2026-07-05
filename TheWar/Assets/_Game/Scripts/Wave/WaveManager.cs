using System.Collections;
using System.Collections.Generic;
using TowerDefense.Enemy;
using TowerDefense.Shared;
using UnityEngine;
using UnityEngine.Pool;

namespace TowerDefense.Wave
{
	public class WaveManager : MonoBehaviour
	{
		[Header("Config")]
		[SerializeField] private LevelDataSO _levelData;
		[SerializeField] private Transform _spawnPoint;
		[SerializeField] private float _timeBetweenWaves = 5f;

		[Header("Waypoint Path")]
		[SerializeField] private PathContainer _pathContainer;
		[SerializeField] private string _pathId = "path_01";

		[Header("Waypoints — Điểm cuối cùng sẽ được làm BasePosition")]
		[SerializeField] private Transform[] _waypointTransforms;

		// Đa Object Pool quản lý riêng biệt cho từng loại quái vật
		private Dictionary<EnemyDataSO, ObjectPool<GameObject>> _enemyPools = new Dictionary<EnemyDataSO, ObjectPool<GameObject>>();
		private int _currentWaveIndex;

		// Static getter để các Enemy truy vấn vị trí Base của màn chơi
		public static Vector3 BasePosition { get; private set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStatics()
		{
			BasePosition = Vector3.zero;
		}

		private void Awake()
		{
			// Gán BasePosition từ phần tử cuối cùng trong list waypoint transforms
			if (_waypointTransforms != null && _waypointTransforms.Length > 0)
			{
				BasePosition = _waypointTransforms[_waypointTransforms.Length - 1].position;
			}
			else
			{
				BasePosition = Vector3.zero;
			}
		}

		private void Start()
		{
			if (_levelData != null)
			{
				StartCoroutine(RunLevel());
			}
			else
			{
				#if UNITY_EDITOR
				Debug.LogWarning("[WaveManager] Chưa kéo dữ liệu LevelDataSO vào WaveManager!");
				#endif
			}
		}

		private IEnumerator RunLevel()
		{
			_currentWaveIndex = 0;

			while (_currentWaveIndex < _levelData.waves.Count)
			{
				WaveDataSO wave = _levelData.waves[_currentWaveIndex];
				if (wave == null)
				{
					_currentWaveIndex++;
					continue;
				}

				#if UNITY_EDITOR
				Debug.Log($"[WaveManager] Bắt đầu đợt {_currentWaveIndex + 1}: {wave.waveName}");
				#endif

				yield return StartCoroutine(SpawnWave(wave));

				// Chờ cho đến khi tất cả quái trên sân bị tiêu diệt hoặc chạm Base (Tất cả các pools đều không có active object)
				yield return new WaitUntil(() => GetTotalActiveEnemies() == 0);

				#if UNITY_EDITOR
				Debug.Log($"[WaveManager] Đã hoàn thành đợt {_currentWaveIndex + 1}: {wave.waveName}");
				#endif

				_currentWaveIndex++;

				if (_currentWaveIndex < _levelData.waves.Count)
				{
					yield return new WaitForSeconds(_timeBetweenWaves);
				}
			}

			#if UNITY_EDITOR
			Debug.Log("Level Complete! Victory!");
			#endif
		}

		private IEnumerator SpawnWave(WaveDataSO wave)
		{
			foreach (var entry in wave.enemies)
			{
				if (entry.enemyData == null) continue;

				if (entry.delayBefore > 0)
				{
					yield return new WaitForSeconds(entry.delayBefore);
				}

				for (int i = 0; i < entry.count; i++)
				{
					SpawnEnemy(entry.enemyData);
					yield return new WaitForSeconds(entry.spawnInterval);
				}
			}
		}

		private void SpawnEnemy(EnemyDataSO enemyData)
		{
			var pool = GetPoolForEnemy(enemyData);
			GameObject obj = pool.Get();
			obj.transform.position = _spawnPoint.position;

			// Init di chuyển bằng Waypoint
			var movement = obj.GetComponent<EnemyMovement>();
			movement.Initialize(enemyData, BasePosition);
			
			if (_pathContainer != null)
			{
				List<BakedSegment> bakedSegments = _pathContainer.GetBakedSegments(_pathId);
				if (bakedSegments == null || bakedSegments.Count == 0)
				{
					Debug.LogError($"[WaveManager] Không thể tạo/tìm thấy BakedSegments cho pathId '{_pathId}'! Vui lòng kiểm tra lại PathContainer.");
				}
				movement.AssignPath(bakedSegments);
			}
			else
			{
				Debug.LogError("[WaveManager] _pathContainer chưa được kéo vào Inspector của WaveManager!");
			}

			movement.OnReachedBaseEvent += () => ReleaseEnemy(pool, obj);

			// Init máu — subscribe event trả về pool khi chết
			var health = obj.GetComponent<EnemyHealth>();
			health.Initialize(enemyData);
			health.OnDied += () => ReleaseEnemy(pool, obj);

			// Init combat
			var combat = obj.GetComponent<EnemyCombat>();
			if (combat != null)
			{
				combat.Initialize(enemyData);
			}
		}

		private void ReleaseEnemy(ObjectPool<GameObject> pool, GameObject enemy)
		{
			if (enemy.activeSelf)
			{
				pool.Release(enemy);
			}
		}

		private ObjectPool<GameObject> GetPoolForEnemy(EnemyDataSO enemyData)
		{
			if (!_enemyPools.TryGetValue(enemyData, out var pool))
			{
				pool = new ObjectPool<GameObject>(
					createFunc: () => InstantiateEnemy(enemyData),
					actionOnGet: obj =>
					{
						obj.SetActive(true);
						var poolables = obj.GetComponents<IPoolable>();
						foreach (var poolable in poolables)
						{
							poolable.OnGetFromPool();
						}
					},
					actionOnRelease: obj =>
					{
						var poolables = obj.GetComponents<IPoolable>();
						foreach (var poolable in poolables)
						{
							poolable.OnReturnToPool();
						}
						obj.SetActive(false);
					},
					defaultCapacity: 20
				);
				_enemyPools.Add(enemyData, pool);
			}
			return pool;
		}

		private GameObject InstantiateEnemy(EnemyDataSO enemyData)
		{
			var obj = Instantiate(enemyData.EnemyPrefab);
			obj.SetActive(false);
			return obj;
		}

		private int GetTotalActiveEnemies()
		{
			int total = 0;
			foreach (var pool in _enemyPools.Values)
			{
				total += pool.CountActive;
			}
			return total;
		}

		// Gọi từ nút UI nếu muốn skip wave
		public void ForceStartNextWave() => StopAllCoroutines();
	}
}