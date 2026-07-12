using System;
using TowerDefense.Core;
using TowerDefense.Shared;
using TowerDefense.Tower;
using UnityEngine;

namespace TowerDefense.Gameplay.Tower
{
	[RequireComponent(typeof(Collider))]
	public class GuardUnit : MonoBehaviour, IDamageable, IPoolable
	{
		private float _currentHealth;
		private UnitData _data;
		private TowerSpawner _spawner;
		private TowerUnitAnimation _animation;
		
		private GameObject _occupyingEnemy;

		public event Action<float> OnDamageTaken;
		public event Action OnDied;

		public bool IsOccupied => _occupyingEnemy != null;
		public GameObject OccupyingEnemy => _occupyingEnemy;
		
		public Vector3 SpawnPoint { get; private set; }

		public void Initialize(UnitData data, TowerSpawner spawner, Vector3 spawnPoint)
		{
			_data = data;
			_spawner = spawner;
			SpawnPoint = spawnPoint;
			// Sử dụng MaxHealth từ UnitData
			_currentHealth = _data.MaxHealth; 

			// Khởi tạo Animation để thoát khỏi T-pose
			_animation = GetComponent<TowerUnitAnimation>();
			if (_animation != null)
			{
				_animation.Initialize(data);
				_animation.SetAttackSpeed(data.AttackSpeed); // Set tốc độ đánh, nếu thiếu anim sẽ bị đứng hình vì multiplier = 0
			}

			// Khởi tạo Detector và Movement
			var detector = GetComponent<GuardDetector>();
			if (detector != null) detector.Initialize(data);

			var movement = GetComponent<GuardMovement>();
			if (movement != null) movement.Initialize(data);

			// Khởi tạo Combat và nối Event Animation
			var combat = GetComponent<GuardCombat>();
			if (combat != null)
			{
				combat.Initialize(data);
				if (_animation != null)
				{
					combat.OnAttack += _animation.TriggerAttackAnimation;
					combat.OnStopAttack += _animation.StopAttackAnimation;
				}
			}
		}

		private void OnDestroy()
		{
			var combat = GetComponent<GuardCombat>();
			if (combat != null && _animation != null)
			{
				combat.OnAttack -= _animation.TriggerAttackAnimation;
				combat.OnStopAttack -= _animation.StopAttackAnimation;
			}
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
			Debug.Log($"[GuardUnit] {gameObject.name} bị quái chém {amount} sát thương! Máu còn lại: {_currentHealth}");
			
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
