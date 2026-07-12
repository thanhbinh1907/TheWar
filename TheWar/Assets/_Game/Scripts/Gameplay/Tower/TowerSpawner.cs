using System.Collections.Generic;
using TowerDefense.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace TowerDefense.Gameplay.Tower
{
	public class TowerSpawner : MonoBehaviour
	{
		[SerializeField] private float _guardSpacing = 1.5f;
		[SerializeField] private int _guardCount = 3;
		[SerializeField] private float _spawnYOffset = 0f; // Chỉnh cao độ bằng 0 để lính đứng chạm đất (chỉnh lại trong Inspector nếu cần)

		private UnitData _currentData;
		private List<GameObject> _activeGuards = new List<GameObject>();
		private ObjectPool<GameObject> _guardPool;
		private bool _isSpawned = false;

		public void Initialize(UnitData data)
		{
			_currentData = data;
			if (_currentData.SpawnedGuardPrefab == null)
			{
				Debug.LogError($"[TowerSpawner] {_currentData.UnitName} has DeployMode.SocketSpawner but SpawnedGuardPrefab is NULL!");
				return;
			}

			InitializePool();
			SpawnGuards();
		}

		private void InitializePool()
		{
			if (_guardPool == null)
			{
				_guardPool = new ObjectPool<GameObject>(
					createFunc: () =>
					{
						var go = Instantiate(_currentData.SpawnedGuardPrefab);
						go.SetActive(false);
						return go;
					},
					actionOnGet: obj =>
					{
						obj.SetActive(true);
						var poolables = obj.GetComponents<Shared.IPoolable>();
						foreach (var p in poolables) p.OnGetFromPool();
					},
					actionOnRelease: obj =>
					{
						var poolables = obj.GetComponents<Shared.IPoolable>();
						foreach (var p in poolables) p.OnReturnToPool();
						obj.SetActive(false);
					},
					actionOnDestroy: obj =>
					{
						if (obj != null) Destroy(obj);
					}
				);
			}
		}

		private void SpawnGuards()
		{
			if (_isSpawned) return;

			var pathContainer = Object.FindFirstObjectByType<PathContainer>();
			if (pathContainer == null)
			{
				Debug.LogError("[TowerSpawner] Cannot find PathContainer in the scene!");
				return;
			}

			// We need to find the closest segment
			var segments = pathContainer.GetBakedSegments("path_01");
			if (segments == null || segments.Count == 0) return;

			BakedSegment closestSegment = segments[0];
			float minDistance = float.MaxValue;
			Vector3 closestPoint = Vector3.zero;

			foreach (var segment in segments)
			{
				// Find closest point on line segment
				Vector3 segmentDir = segment.EndNode - segment.StartNode;
				float length = segmentDir.magnitude;
				Vector3 dirNorm = segmentDir / length;
				
				Vector3 toTower = transform.position - segment.StartNode;
				float dot = Vector3.Dot(toTower, dirNorm);
				dot = Mathf.Clamp(dot, 0f, length);
				
				Vector3 pointOnSegment = segment.StartNode + dirNorm * dot;
				float dist = Vector3.Distance(transform.position, pointOnSegment);

				if (dist < minDistance)
				{
					minDistance = dist;
					closestSegment = segment;
					closestPoint = pointOnSegment;
				}
			}

			Vector3 pathDir = (closestSegment.EndNode - closestSegment.StartNode).normalized;
			Vector3 rightDir = Vector3.Cross(Vector3.up, pathDir).normalized;

			// Spawn guards perpendicular to path
			float startOffset = -(_guardCount - 1) * _guardSpacing * 0.5f;

			for (int i = 0; i < _guardCount; i++)
			{
				Vector3 spawnPos = closestPoint + rightDir * (startOffset + i * _guardSpacing);
				spawnPos.y += _spawnYOffset;

				GameObject guard = _guardPool.Get();
				guard.transform.position = spawnPos;
				guard.transform.rotation = Quaternion.LookRotation(pathDir);
				
				// Set up GuardUnit component if it exists
				var guardUnit = guard.GetComponent<GuardUnit>();
				if (guardUnit != null)
				{
					guardUnit.Initialize(_currentData, this, spawnPos);
				}

				_activeGuards.Add(guard);
			}

			_isSpawned = true;
		}

		public void DespawnGuards()
		{
			if (!_isSpawned) return;

			foreach (var guard in _activeGuards)
			{
				if (guard != null && guard.activeSelf)
				{
					_guardPool.Release(guard);
				}
			}
			_activeGuards.Clear();
			_isSpawned = false;
		}

		public void OnGuardDied(GameObject guard)
		{
			if (_activeGuards.Contains(guard))
			{
				_activeGuards.Remove(guard);
				_guardPool.Release(guard);
			}
		}
	}
}
