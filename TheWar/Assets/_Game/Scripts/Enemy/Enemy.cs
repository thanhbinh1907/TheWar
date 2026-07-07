using UnityEngine;

namespace TowerDefense.Enemy
{
	[RequireComponent(typeof(EnemyHealth))]
	[RequireComponent(typeof(EnemyMovement))]
	[RequireComponent(typeof(EnemyCombat))]
	[RequireComponent(typeof(EnemySteering))]
	[RequireComponent(typeof(EnemyAnimation))]
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
			
			// Bơm dữ liệu (Inject Data) từ gốc Enemy.cs vào các sub-components
			_health.Initialize(_data);
			GetComponent<EnemyCombat>().Initialize(_data);
			GetComponent<EnemyAnimation>().Initialize(_data);
		}
	}
}
