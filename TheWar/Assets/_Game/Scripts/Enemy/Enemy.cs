using UnityEngine;

namespace TowerDefense.Enemy
{
	[RequireComponent(typeof(EnemyHealth))]
	[RequireComponent(typeof(EnemyMovement))]
	public class Enemy : MonoBehaviour
	{
		[SerializeField] private EnemyDataSO _data;

		private EnemyHealth _health;
		private EnemyMovement _movement;

		public EnemyDataSO Data => _data;
		public EnemyHealth Health => _health;
		public EnemyMovement Movement => _movement;

		private void Awake()
		{
			_health = GetComponent<EnemyHealth>();
			_movement = GetComponent<EnemyMovement>();
		}
	}
}
