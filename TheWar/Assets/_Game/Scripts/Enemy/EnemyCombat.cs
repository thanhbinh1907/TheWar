using System.Collections;
using TowerDefense.Core;
using TowerDefense.Gameplay.Tower;
using UnityEngine;

namespace TowerDefense.Enemy
{
	public class EnemyCombat : MonoBehaviour
	{
		[SerializeField] private float _detectRange = 5.0f;
		[SerializeField] private float _attackRange = 1.5f;
		[SerializeField] private LayerMask _guardLayer;
		
		private EnemyMovement _movement;
		private EnemyAnimation _animation;
		private EnemyDataSO _data;
		private GuardUnit _currentGuard;
		private Coroutine _attackCoroutine;
		private IEnemyAttackBehavior _attackBehavior;

		public void Initialize(EnemyDataSO data)
		{
			_data = data;
			_movement = GetComponent<EnemyMovement>();
			_animation = GetComponent<EnemyAnimation>();
			_attackBehavior = GetComponent<IEnemyAttackBehavior>();
			
			if (_animation != null)
			{
				_animation.OnHitEvent += HandleAnimationHit;
			}

			// Quét tìm lính gác xung quanh (0.5s trễ, sau đó 0.1s mỗi lần)
			InvokeRepeating(nameof(SweepForGuards), 0.5f, 0.1f);
		}

		private void OnDestroy()
		{
			if (_animation != null)
			{
				_animation.OnHitEvent -= HandleAnimationHit;
			}
		}

		private void OnDisable()
		{
			CancelInvoke(nameof(SweepForGuards));
			StopEngagement();
		}

		private void Update()
		{
			if (_currentGuard != null && _currentGuard.gameObject.activeInHierarchy && _currentGuard.IsOccupied)
			{
				float distance = Vector3.Distance(transform.position, _currentGuard.transform.position);
				
				// Nếu đã vào tầm đánh mà chưa đánh
				if (distance <= _attackRange && _attackCoroutine == null)
				{
					if (_movement != null) _movement.SetMovementPaused(true);
					_attackCoroutine = StartCoroutine(AttackRoutine());
				}
				// Nếu lính gác di chuyển (thường không có) ra khỏi tầm đánh
				else if (distance > _attackRange && _attackCoroutine != null)
				{
					if (_movement != null) _movement.SetMovementPaused(false);
					StopCoroutine(_attackCoroutine);
					_attackCoroutine = null;
				}
			}
		}

		private void SweepForGuards()
		{
			// Nếu đang có lính gác rồi, kiểm tra xem nó còn sống không
			if (_currentGuard != null)
			{
				if (!_currentGuard.gameObject.activeInHierarchy || !_currentGuard.IsOccupied)
				{
					StopEngagement();
				}
				return;
			}

			// Nếu đang rảnh rỗi thì quét tìm lính gác trong Tầm Nhìn
			Collider[] hits = Physics.OverlapSphere(transform.position, _detectRange, _guardLayer);
			
			GuardUnit nearestGuard = null;
			float minDistance = float.MaxValue;
			
			foreach (var hit in hits)
			{
				var guard = hit.GetComponent<GuardUnit>();
				// Đảm bảo lính gác đang không bị ai chiếm
				if (guard != null && !guard.IsOccupied)
				{
					float dist = Vector3.Distance(transform.position, guard.transform.position);
					if (dist < minDistance)
					{
						nearestGuard = dist < minDistance ? guard : nearestGuard;
						minDistance = dist;
					}
				}
			}
			
			// TryOccupy: nếu lính chưa ai đánh thì chiếm thành công
			if (nearestGuard != null && nearestGuard.TryOccupy(gameObject))
			{
				EngageGuard(nearestGuard);
			}
		}

		private void EngageGuard(GuardUnit guard)
		{
			_currentGuard = guard;
			
			if (_movement != null)
			{
				// Chỉ định Movement đi về phía Guard này thay vì đi theo Path
				_movement.TargetOverride = guard.transform;
			}
			else
			{
				Debug.LogError($"[{gameObject.name}] _movement is NULL! Không thể truy đuổi!");
			}
			
			// Việc vung kiếm sẽ do hàm Update quyết định khi khoảng cách <= _attackRange
		}

		private void StopEngagement()
		{
			if (_attackCoroutine != null)
			{
				StopCoroutine(_attackCoroutine);
				_attackCoroutine = null;
			}
			
			if (_currentGuard != null)
			{
				_currentGuard.Release(gameObject);
				_currentGuard = null;
			}

			if (_movement != null)
			{
				_movement.SetMovementPaused(false);
				_movement.TargetOverride = null; // Huỷ override, quay lại đường đi chính
			}
		}

		private IEnumerator AttackRoutine()
		{
			while (_currentGuard != null && _currentGuard.gameObject.activeInHierarchy)
			{
				// Chỉ kích hoạt hoạt ảnh. Việc ném đá/trừ máu sẽ do hàm HandleAnimationHit lo.
				if (_animation != null)
				{
					_animation.TriggerAttackAnimation();
				}

				// Đợi hết 1 chu kỳ đánh rồi mới vung vũ khí tiếp
				float delay = 1f / (_data.AttackSpeed > 0 ? _data.AttackSpeed : 1f);
				yield return new WaitForSeconds(delay);
			}
			
			StopEngagement();
		}

		private void HandleAnimationHit()
		{
			// Hàm này được gọi từ EnemyAnimation.cs khi tới đúng frame ném đá / chém trúng
			if (_currentGuard != null && _currentGuard.gameObject.activeInHierarchy)
			{
				if (_attackBehavior != null)
				{
					_attackBehavior.ExecuteAttack(_currentGuard, _data.Damage);
				}
				else
				{
					EventBus.Publish(new DamageEvent
					{
						target = _currentGuard,
						amount = _data.Damage
					});
				}
			}
		}

#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, _detectRange);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, _attackRange);
		}
#endif
	}
}
