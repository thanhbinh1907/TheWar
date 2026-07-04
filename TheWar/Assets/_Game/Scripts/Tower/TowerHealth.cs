using System;
using UnityEngine;
using TowerDefense.Shared;

namespace TowerDefense.Tower
{
	public class TowerHealth : MonoBehaviour, IDamageable
	{
		[Header("Stats")]
		[SerializeField] private float _maxHealth = 100f;

		private float _currentHealth;
		private bool _isDead;

		public event Action OnTowerDied;

		public bool IsDead => _isDead;

		private void OnEnable()
		{
			// Reset máu khi được kích hoạt (cắm core)
			Initialize();
		}

		private void OnDisable()
		{
			// Dọn dẹp event tránh leak memory
			OnTowerDied = null;
		}

		public void Initialize()
		{
			_currentHealth = _maxHealth;
			_isDead = false;
		}

		public void TakeDamage(float amount)
		{
			if (_isDead) return;

			_currentHealth -= amount;
			_currentHealth = Mathf.Max(_currentHealth, 0f);

			#if UNITY_EDITOR
			Debug.Log($"[{gameObject.name}] Tower took {amount} damage. HP: {_currentHealth}/{_maxHealth}");
			#endif

			if (_currentHealth <= 0f)
			{
				Die();
			}
		}

		private void Die()
		{
			_isDead = true;
			
			#if UNITY_EDITOR
			Debug.Log($"[{gameObject.name}] Tower destroyed!");
			#endif

			OnTowerDied?.Invoke();
		}
	}
}
