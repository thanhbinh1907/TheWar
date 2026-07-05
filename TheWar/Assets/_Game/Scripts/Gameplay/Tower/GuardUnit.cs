using System;
using TowerDefense.Core;
using TowerDefense.Shared;
using UnityEngine;

namespace TowerDefense.Gameplay.Tower
{
	[RequireComponent(typeof(Collider))]
	public class GuardUnit : MonoBehaviour, IDamageable, IPoolable
	{
		private float _currentHealth;
		private UnitData _data;
		private TowerSpawner _spawner;
		
		private GameObject _occupyingEnemy;

		public event Action<float> OnDamageTaken;
		public event Action OnDied;

		public bool IsOccupied => _occupyingEnemy != null;

		public void Initialize(UnitData data, TowerSpawner spawner)
		{
			_data = data;
			_spawner = spawner;
			// Tạm dùng Damage * 10 làm máu cho lính gác (vì UnitData hiện tại chưa có MaxHealth)
			_currentHealth = _data.Damage * 10f; 
		}

		public bool TryOccupy(GameObject enemy)
		{
			if (IsOccupied) return false;
			_occupyingEnemy = enemy;
			return true;
		}

		public void Release(GameObject enemy)
		{
			if (_occupyingEnemy == enemy)
			{
				_occupyingEnemy = null;
			}
		}

		public void TakeDamage(float amount)
		{
			if (_currentHealth <= 0) return;

			_currentHealth -= amount;
			OnDamageTaken?.Invoke(amount);

			if (_currentHealth <= 0)
			{
				Die();
			}
		}

		private void Die()
		{
			// Tự động Release quái đang chiếm khi chết
			if (_occupyingEnemy != null)
			{
				_occupyingEnemy = null; 
			}
			
			OnDied?.Invoke();
			if (_spawner != null)
			{
				_spawner.OnGuardDied(gameObject);
			}
		}

		public void OnGetFromPool()
		{
			_occupyingEnemy = null;
		}

		public void OnReturnToPool()
		{
			_occupyingEnemy = null;
			OnDamageTaken = null;
			OnDied = null;
		}
	}
}
