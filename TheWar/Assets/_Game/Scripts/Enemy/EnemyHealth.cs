using System;
using UnityEngine;
using TowerDefense.Shared;
using TowerDefense.Core;
using TowerDefense.Core.Events;

namespace TowerDefense.Enemy
{
	public class EnemyHealth : MonoBehaviour, IDamageable
	{
		[SerializeField] private EnemyDataSO _enemyData;

		private float _currentHealth;
		private bool _isDead;

		// Events for decoupling architecture
		public event Action<float> OnDamageTaken;
		public event Action OnDied;

		public bool IsDead => _isDead;

		private void OnEnable()
		{
			ResetHealth();
		}

		private void OnDisable()
		{
			// Clear all event subscriptions to prevent event accumulation memory leaks in ObjectPool
			OnDamageTaken = null;
			OnDied = null;
		}

		public void Initialize(EnemyDataSO data)
		{
			_enemyData = data;
			ResetHealth();
		}

		public void TakeDamage(float amount)
		{
			if (_isDead) return;

			_currentHealth -= amount;
			_currentHealth = Mathf.Max(_currentHealth, 0f);

#if UNITY_EDITOR
			Debug.Log($"[{gameObject.name}] Took {amount} damage. Current HP: {_currentHealth}");
#endif

			OnDamageTaken?.Invoke(amount);

			if (_currentHealth <= 0f)
			{
				Die();
			}
		}

		private void ResetHealth()
		{
			if (_enemyData == null) return;

			_currentHealth = _enemyData.MaxHealth;
			_isDead = false;
		}

		private void Die()
		{
			if (_isDead) return;

			_isDead = true;

			// Trả thưởng vàng TRƯỚC khi gọi OnDied (để WaveManager thu hồi)
			if (_enemyData != null)
			{
				EventBus.Publish(new EnemyDiedEvent { goldReward = _enemyData.GoldReward });
			}

			OnDied?.Invoke();

#if UNITY_EDITOR
			Debug.Log($"[{gameObject.name}] Has Died.");
#endif
		}
	}
}