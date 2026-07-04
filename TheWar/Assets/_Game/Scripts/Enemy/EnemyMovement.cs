using UnityEngine;
using System;
using System.Collections.Generic;
using TowerDefense.Shared;
using TowerDefense.Core;

namespace TowerDefense.Enemy
{
	public class EnemyMovement : MonoBehaviour, IPoolable
	{
		private EnemyDataSO _data;
		private Vector3 _targetPosition;
		private bool _isMoving;

		[SerializeField] private EnemySteering _steering;
		private int _currentWaypointIndex;
		private PathData _assignedPath;



		// Event triggered when the enemy successfully reaches the final waypoint (base)
		public event Action OnReachedBaseEvent;

		public void Initialize(EnemyDataSO data, Vector3 targetPosition)
		{
			_data = data;
			_targetPosition = targetPosition;
			_isMoving = true;


		}

		private void OnDisable()
		{
			// Clear event subscriptions to prevent memory leaks in ObjectPool
			OnReachedBaseEvent = null;
		}

		private void Update()
		{
			if (!_isMoving) return;

			MoveAlongWaypoint();
		}

		public void AssignPath(PathData path)
		{
			_assignedPath = path;
			_currentWaypointIndex = 0;
		}

		private void MoveAlongWaypoint()
		{
			if (_assignedPath == null)
			{
				Debug.LogError($"[EnemyMovement] _assignedPath is NULL! Cannot move.");
				return;
			}
			if (_assignedPath.waypointPositions == null || _assignedPath.waypointPositions.Count == 0)
			{
				Debug.LogError($"[EnemyMovement] waypointPositions is empty! Cannot move.");
				return;
			}
			if (_steering == null)
			{
				Debug.LogError($"[EnemyMovement] _steering is NULL! Cannot move.");
				return;
			}

			if (_currentWaypointIndex >= _assignedPath.waypointPositions.Count)
			{
				OnReachedBase();
				return;
			}

			Vector3 targetWaypoint = _assignedPath.waypointPositions[_currentWaypointIndex];
			Vector3 currentPos = transform.position;

			Vector3 velocity = _steering.CalculateSeekForce(currentPos, targetWaypoint, _data.MoveSpeed);
			transform.position += velocity * Time.deltaTime;

			if (velocity.sqrMagnitude > 0.001f)
			{
				transform.rotation = Quaternion.LookRotation(velocity.normalized);
			}

			if (Vector3.Distance(currentPos, targetWaypoint) < 0.3f)
			{
				_currentWaypointIndex++;
			}
		}



		private void OnReachedBase()
		{
			_isMoving = false;
			// Invoke event so WaveManager (or GameManager) can release this object back to pool
			OnReachedBaseEvent?.Invoke();
		}

		// IPoolable
		public void OnGetFromPool()
		{
			_isMoving = false;
			_currentWaypointIndex = 0;


		}

		public void OnReturnToPool()
		{
			_isMoving = false;
			gameObject.SetActive(false);
		}


	}
}