using System.Collections;
using TowerDefense.Core;
using TowerDefense.Gameplay.Tower;
using UnityEngine;

namespace TowerDefense.Enemy
{
	public class EnemyCombat : MonoBehaviour
	{
		[SerializeField] private float _engageRange = 1.5f;
		[SerializeField] private LayerMask _guardLayer;
		
		private EnemyMovement _movement;
		private EnemyDataSO _data;
		private GuardUnit _currentGuard;
		private Coroutine _attackCoroutine;

		public void Initialize(EnemyDataSO data)
		{
			_data = data;
			_movement = GetComponent<EnemyMovement>();
			
			// Quét tìm lính gác xung quanh (0.5s trễ, sau đó 0.1s mỗi lần)
			InvokeRepeating(nameof(SweepForGuards), 0.5f, 0.1f);
		}

		private void OnDisable()
		{
			CancelInvoke(nameof(SweepForGuards));
			StopEngagement();
		}

		private void SweepForGuards()
		{
			// Nếu đang đánh một lính gác rồi, kiểm tra xem nó còn sống không
			if (_currentGuard != null)
			{
				if (!_currentGuard.gameObject.activeInHierarchy || !_currentGuard.IsOccupied)
				{
					StopEngagement();
				}
				return;
			}

			// Nếu đang rảnh rỗi thì quét tìm lính gác
			Collider[] hits = Physics.OverlapSphere(transform.position, _engageRange, _guardLayer);
			foreach (var hit in hits)
			{
				var guard = hit.GetComponent<GuardUnit>();
				// TryOccupy: nếu lính chưa ai đánh thì chiếm thành công, ngược lại bỏ qua
				if (guard != null && guard.TryOccupy(gameObject))
				{
					EngageGuard(guard);
					break;
				}
			}
		}

		private void EngageGuard(GuardUnit guard)
		{
			_currentGuard = guard;
			if (_movement != null)
			{
				_movement.SetMovementPaused(true);
			}

			if (_attackCoroutine == null)
			{
				_attackCoroutine = StartCoroutine(AttackRoutine());
			}
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
			}
		}

		private IEnumerator AttackRoutine()
		{
			while (_currentGuard != null && _currentGuard.gameObject.activeInHierarchy)
			{
				// AttackSpeed: số lần đánh trong 1 giây. Vd 2 -> 0.5s delay
				float delay = 1f / (_data.AttackSpeed > 0 ? _data.AttackSpeed : 1f);
				yield return new WaitForSeconds(delay);

				// Double check vì trong lúc Wait lính gác có thể đã chết
				if (_currentGuard != null && _currentGuard.gameObject.activeInHierarchy)
				{
					EventBus.Publish(new DamageEvent
					{
						target = _currentGuard,
						amount = _data.Damage
					});
				}
			}
			
			StopEngagement();
		}

#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, _engageRange);
		}
#endif
	}
}
