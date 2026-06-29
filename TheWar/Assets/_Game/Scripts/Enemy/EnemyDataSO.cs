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
		[SerializeField] private GameObject _enemyPrefab;

		// Public getters to strictly enforce encapsulation
		public float MaxHealth => _maxHealth;
		public float MoveSpeed => _moveSpeed;
		public int GoldReward => _goldReward;
		public GameObject EnemyPrefab => _enemyPrefab;
	}
}