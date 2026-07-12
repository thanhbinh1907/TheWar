using UnityEngine;
using System;
using System.Collections.Generic;
using TowerDefense.Shared;
using TowerDefense.Core;
using TowerDefense.Core.Events;

namespace TowerDefense.Enemy
{
	public class EnemyMovement : MonoBehaviour, IPoolable
	{
		private EnemyDataSO _data;
		private Vector3 _targetPosition;
		private bool _isMoving;
		private bool _isMovementPaused;

		private EnemySteering _steering;
		private EnemyAnimation _animation;
		
		private void Awake()
		{
			_steering = GetComponent<EnemySteering>();
			_animation = GetComponent<EnemyAnimation>();
		}
		
		private List<BakedSegment> _bakedSegments;
		private int _currentSegmentIndex;

		public Transform TargetOverride { get; set; }

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

		public void SetMovementPaused(bool paused)
		{
			_isMovementPaused = paused;
			if (paused)
			{
				var rb = GetComponent<Rigidbody>();
				if (rb != null && !rb.isKinematic) rb.linearVelocity = Vector3.zero;
				if (_animation != null) _animation.SetMoveSpeed(0f);
			}
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
			if (_isMovementPaused) 
			{
				if (_animation != null) _animation.SetMoveSpeed(0f);
				return;
			}

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

			Vector3 adaptiveTarget;
			float progress = 0f;
			
			if (TargetOverride != null)
			{
				// Trực tiếp hướng về TargetOverride nếu đang truy đuổi
				adaptiveTarget = TargetOverride.position;
			}
			else
			{
				// Lấy mục tiêu ảo di động và tiến độ (progress) trên đường dẫn
				adaptiveTarget = _steering.CalculateAdaptivePathTarget(currentPos, currentSegment, out progress);
			}
			
			// Tính vận tốc Seek tới mục tiêu ảo + Lực đẩy Separation né nhau
			Vector3 velocity = _steering.CalculateVelocity(currentPos, adaptiveTarget, _data.MoveSpeed);
			
			// Cập nhật vị trí thông qua Velocity thay vì thay đổi transform.position cục súc
			transform.position += velocity * Time.deltaTime;

			if (_animation != null)
			{
				_animation.SetMoveSpeed(velocity.magnitude);
			}

			// Slerp mượt mà mặt nhìn theo hướng thực sự muốn đi (bỏ qua nhiễu từ Separation)
			if (_steering.DesiredDirection.sqrMagnitude > 0.001f)
			{
				Quaternion targetRotation = Quaternion.LookRotation(_steering.DesiredDirection);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
			}

			// Chỉ tính toán nhảy Segment nếu không bị ghi đè mục tiêu
			if (TargetOverride == null)
			{
				// Tính khoảng cách chiếu từ vị trí quái lên trục đường hướng tới mặt phẳng EndNode
				Vector3 segmentDir = currentSegment.EndNode - currentSegment.StartNode;
				float segmentLength = segmentDir.magnitude;
				float distanceToTargetPlane = Vector3.Dot(currentSegment.EndNode - currentPos, segmentDir.normalized);

				// Ngưỡng bẻ cua sớm: không vượt quá 20% chiều dài đoạn đường (chống skip hẳn các đoạn cua ngắn)
				float earlySwitchThreshold = Mathf.Min(0.5f, segmentLength * 0.2f);

				// Chuyển sang đoạn tiếp theo nếu tiến trình gần xong, HOẶC đã chạm ngưỡng bẻ cua
				if (progress >= 0.98f || distanceToTargetPlane < earlySwitchThreshold)
				{
					_currentSegmentIndex++;
				}
			}
		}

		private void OnReachedBase()
		{
			_isMoving = false;

			// Publish GoalReachedEvent
			EventBus.Publish(new GoalReachedEvent { damageToBase = _data.DamageToBase });

			// Invoke event so WaveManager (or GameManager) can release this object back to pool
			OnReachedBaseEvent?.Invoke();
		}

		// IPoolable
		public void OnGetFromPool()
		{
			_isMoving = false;
			_currentSegmentIndex = 0;
			TargetOverride = null;
			if (_animation != null) _animation.SetMoveSpeed(0f);
		}

		public void OnReturnToPool()
		{
			_isMoving = false;
			gameObject.SetActive(false);
		}
	}
}