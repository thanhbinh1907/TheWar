using System.Collections;
using System.Collections.Generic;
using TowerDefense.Enemy;
using UnityEngine;
using UnityEngine.Pool;

namespace TowerDefense.Wave
{
	public class WaveManager : MonoBehaviour
	{
		[Header("Config")]
		[SerializeField] private EnemyDataSO _enemyData;
		[SerializeField] private Transform _spawnPoint;
		[SerializeField] private float _spawnInterval = 1.5f;
		[SerializeField] private int _enemiesPerWave = 5;
		[SerializeField] private float _timeBetweenWaves = 5f;

		[Header("Waypoints — kéo các Transform dọc cột 4-6 vào đây")]
		[SerializeField] private Transform[] _waypointTransforms;

		private ObjectPool<GameObject> _enemyPool;
		private List<Vector3> _waypointPositions;
		private int _currentWave;

		private void Awake()
		{
			_enemyPool = new ObjectPool<GameObject>(
				createFunc: () => CreateEnemy(),
				actionOnGet: obj => obj.SetActive(true),
				actionOnRelease: obj => obj.SetActive(false),
				defaultCapacity: 20
			);

			// Cache waypoint positions một lần — không gọi .position trong Update
			_waypointPositions = new List<Vector3>();
			foreach (var wp in _waypointTransforms)
				_waypointPositions.Add(wp.position);
		}

		private void Start() => StartCoroutine(RunWaves());

		private IEnumerator RunWaves()
		{
			while (true)
			{
				_currentWave++;
				yield return StartCoroutine(SpawnWave());
				yield return new WaitForSeconds(_timeBetweenWaves);
			}
		}

		private IEnumerator SpawnWave()
		{
			for (int i = 0; i < _enemiesPerWave; i++)
			{
				SpawnEnemy();
				yield return new WaitForSeconds(_spawnInterval);
			}
		}

		private void SpawnEnemy()
		{
			GameObject obj = _enemyPool.Get();
			obj.transform.position = _spawnPoint.position;

			// Init movement
			var movement = obj.GetComponent<EnemyMovement>();
			movement.Initialize(_enemyData, _waypointPositions);

			// Init health — subscribe event trả về pool khi chết
			var health = obj.GetComponent<EnemyHealth>();
			health.Initialize(_enemyData);
			health.OnDied += () => _enemyPool.Release(obj);
		}

		private GameObject CreateEnemy()
		{
			// Instantiate chỉ chạy 1 lần lúc khởi tạo pool — OK
			var obj = Instantiate(_enemyData.EnemyPrefab);
			obj.SetActive(false);
			return obj;
		}

		// Gọi từ nút UI nếu muốn skip wave
		public void ForceStartNextWave() => StopAllCoroutines();
	}
}