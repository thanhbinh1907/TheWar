using UnityEngine;
using System.Collections.Generic;
using TowerDefense.Shared;

namespace TowerDefense.Enemy
{
	public class EnemyMovement : MonoBehaviour, IPoolable
	{
		private EnemyDataSO _data;
		private List<Vector3> _waypoints;
		private int _currentWaypointIndex;
		private bool _isMoving;

		public void Initialize(EnemyDataSO data, List<Vector3> waypoints)
		{
			_data = data;
			_waypoints = waypoints;
			_currentWaypointIndex = 0;
			_isMoving = true;
		}

		private void Update()
		{
			if (!_isMoving || _waypoints == null || _waypoints.Count == 0) return;
			MoveToNextWaypoint();
		}

		private void MoveToNextWaypoint()
		{
			if (_currentWaypointIndex >= _waypoints.Count)
			{
				OnReachedBase();
				return;
			}

			Vector3 target = _waypoints[_currentWaypointIndex];
			transform.position = Vector3.MoveTowards(
				transform.position, target, _data.MoveSpeed * Time.deltaTime);

			// Rotate về phía đang đi
			Vector3 dir = (target - transform.position);
			if (dir.sqrMagnitude > 0.001f)
				transform.rotation = Quaternion.LookRotation(dir);

			if (Vector3.Distance(transform.position, target) < 0.05f)
				_currentWaypointIndex++;
		}

		private void OnReachedBase()
		{
			_isMoving = false;
			//GameManager.Instance?.OnEnemyReachedBase();
			gameObject.SetActive(false);
			OnReturnToPool();
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