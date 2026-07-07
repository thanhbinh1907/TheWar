using UnityEngine;

namespace TowerDefense.Enemy
{
	[CreateAssetMenu(fileName = "NewEnemyData", menuName = "TowerDefense/Enemy Data")]
	public class EnemyDataSO : ScriptableObject
	{
		[Header("Base Stats")]
		[SerializeField] private float _maxHealth = 50f;
		[SerializeField] private float _moveSpeed = 3f;
		[SerializeField] private int _goldReward = 10;
		[SerializeField] private int _damageToBase = 1;
		[SerializeField] private GameObject _enemyPrefab;
		[SerializeField] private float _damage = 5f;
		[SerializeField] private float _attackSpeed = 1f;
		[SerializeField] private bool _canLeaveRoad = false;

		[Header("Animations")]
		[SerializeField] private AnimationClip _idleClip;
		[SerializeField] private AnimationClip _runClip;
		[SerializeField] private AnimationClip _attackClip;
		[SerializeField] private AnimationClip _dieClip;

		// Public getters to strictly enforce encapsulation
		public float MaxHealth => _maxHealth;
		public float MoveSpeed => _moveSpeed;
		public int GoldReward => _goldReward;
		public int DamageToBase => _damageToBase;
		public GameObject EnemyPrefab => _enemyPrefab;
		public float Damage => _damage;
		public float AttackSpeed => _attackSpeed;
		public bool CanLeaveRoad => _canLeaveRoad;

		public AnimationClip IdleClip => _idleClip;
		public AnimationClip RunClip => _runClip;
		public AnimationClip AttackClip => _attackClip;
		public AnimationClip DieClip => _dieClip;
	}
}