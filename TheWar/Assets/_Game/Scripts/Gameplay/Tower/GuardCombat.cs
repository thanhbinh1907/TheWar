using System;
using System.Collections;
using TowerDefense.Core;
using TowerDefense.Shared;
using UnityEngine;

namespace TowerDefense.Gameplay.Tower
{
	[RequireComponent(typeof(GuardUnit), typeof(GuardDetector))]
	public class GuardCombat : MonoBehaviour
	{
		[SerializeField] private float _attackRange = 1.5f;
		public float AttackRange => _attackRange;
		
		private GuardUnit _guard;
		private GuardDetector _detector;
		private UnitData _data;
		private Coroutine _attackCoroutine;

		public event Action OnAttack;
		public event Action OnStopAttack;

		public void Initialize(UnitData data)
		{
			_data = data;
			_guard = GetComponent<GuardUnit>();
			_detector = GetComponent<GuardDetector>();
			
			InvokeRepeating(nameof(CheckCombatState), 0.5f, 0.1f);
		}

		private void OnDisable()
		{
			CancelInvoke(nameof(CheckCombatState));
			StopAttack();
		}

		private void CheckCombatState()
		{
			if (HasValidTargetInAttackRange())
			{
				if (_attackCoroutine == null)
				{
					_attackCoroutine = StartCoroutine(AttackRoutine());
				}
			}
			else
			{
				StopAttack();
			}
		}

		private bool HasValidTargetInAttackRange()
		{
			if (_detector == null) return false;
			
			// Nếu target hiện tại đã chết hoặc null, ép tìm mới
			if (_detector.CurrentTarget == null || !_detector.CurrentTarget.gameObject.activeInHierarchy)
			{
				_detector.ForceFindTarget();
			}

			if (_detector.CurrentTarget != null && _detector.CurrentTarget.gameObject.activeInHierarchy)
			{
				float dist = Vector3.Distance(transform.position, _detector.CurrentTarget.position);
				return dist <= _attackRange;
			}

			return false;
		}

		private void StopAttack()
		{
			if (_attackCoroutine != null)
			{
				StopCoroutine(_attackCoroutine);
				_attackCoroutine = null;
				OnStopAttack?.Invoke();
				Debug.Log($"[{gameObject.name}] Đã ngừng tấn công mục tiêu trong code!");
			}
		}

		private IEnumerator AttackRoutine()
		{
			while (HasValidTargetInAttackRange())
			{
				Transform target = _detector.CurrentTarget;
				
				// Xoay mặt về phía quái
				Vector3 dir = target.position - transform.position;
				dir.y = 0;
				if (dir.sqrMagnitude > 0.01f)
				{
					transform.rotation = Quaternion.LookRotation(dir);
				}

				float delay = 1f / (_data.AttackSpeed > 0 ? _data.AttackSpeed : 1f);
				
				// Kích hoạt animation đánh
				OnAttack?.Invoke();
				
				// Đợi nửa chu kỳ để damage nhảy (giả lập impact delay)
				yield return new WaitForSeconds(delay * 0.5f);

				// Kiểm tra lại lần nữa trước khi gây damage
				if (HasValidTargetInAttackRange())
				{
					var enemyHealth = _detector.CurrentTarget.GetComponent<IDamageable>();
					if (enemyHealth != null)
					{
						enemyHealth.TakeDamage(_data.Damage);
					}
				}

				// Đợi nửa chu kỳ còn lại
				yield return new WaitForSeconds(delay * 0.5f);
			}
			
			StopAttack();
		}
	}
}
