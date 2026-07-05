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
		
		private List<BakedSegment> _bakedSegments;
		private int _currentSegmentIndex;

		// Event triggered when the enemy successfully reaches the final waypoint (base)
		public event Action OnReachedBaseEvent;

		public void Initialize(EnemyDataSO data, Vector3 targetPosition)
		{
			_data = data;
			_targetPosition = targetPosition;
			_isMoving = true;
		}

		/// <summary>
		/// Nhận danh sách các đoạn đường đã được phân tích độ rộng an toàn từ Map
		/// </summary>
		public void AssignPath(List<BakedSegment> bakedSegments)
		{
			_bakedSegments = bakedSegments;
			_currentSegmentIndex = 0;
		}

		private void OnDisable()
		{
			// Clear event subscriptions to prevent memory leaks in ObjectPool
			OnReachedBaseEvent = null;
		}

		private void Update()
		{
			if (!_isMoving) return;

			MoveAlongAdaptivePath();
		}

		private void MoveAlongAdaptivePath()
		{
			if (_bakedSegments == null || _bakedSegments.Count == 0)
			{
				Debug.LogError($"[EnemyMovement] _bakedSegments is empty or null! Cannot move.");
				return;
			}
			
			if (_steering == null)
			{
				Debug.LogError($"[EnemyMovement] _steering is NULL! Cannot move.");
				return;
			}

			// Nếu đã chạy hết các mảng đường -> tới đích
			if (_currentSegmentIndex >= _bakedSegments.Count)
			{
				OnReachedBase();
				return;
			}

			BakedSegment currentSegment = _bakedSegments[_currentSegmentIndex];
			Vector3 currentPos = transform.position;

			// Lấy mục tiêu ảo di động và tiến độ (progress)
			Vector3 adaptiveTarget = _steering.CalculateAdaptivePathTarget(currentPos, currentSegment, out float progress);
			
			// Tính vận tốc Seek tới mục tiêu ảo + Lực đẩy Separation né nhau
			Vector3 velocity = _steering.CalculateVelocity(currentPos, adaptiveTarget, _data.MoveSpeed);
			
			// Cập nhật vị trí thông qua Velocity thay vì thay đổi transform.position cục súc
			transform.position += velocity * Time.deltaTime;

			// Slerp mượt mà mặt nhìn theo hướng vận tốc
			if (velocity.sqrMagnitude > 0.001f)
			{
				Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
			}

			// Tính khoảng cách chiếu từ vị trí quái lên trục đường hướng tới mặt phẳng EndNode
			float distanceToTargetPlane = Vector3.Dot(currentSegment.EndNode - currentPos, (currentSegment.EndNode - currentSegment.StartNode).normalized);

			// Chuyển sang đoạn tiếp theo nếu tiến trình gần xong, HOẶC cách mặt phẳng EndNode dưới 0.5f (giúp bẻ cua sớm, chống kẹt vật lý)
			if (progress >= 0.98f || distanceToTargetPlane < 0.5f)
			{
				_currentSegmentIndex++;
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
			_currentSegmentIndex = 0;
		}

		public void OnReturnToPool()
		{
			_isMoving = false;
			gameObject.SetActive(false);
		}
	}
}