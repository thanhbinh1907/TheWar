using UnityEngine;

namespace TowerDefense.Core
{
	public enum UnitClass { Bruiser, Ranger, Mage, Assassin, Artillery }
	public enum FactionVoundDat { Kingdom, Adventure, Pirate, Samurai, Dungeons }
	public enum DeployMode { SocketRanged, SocketSpawner, FreeDeploy }

	[CreateAssetMenu(fileName = "NewUnitData", menuName = "TowerDefense/Unit Data")]
	public class UnitData : ScriptableObject
	{
		[SerializeField] private string _unitName;
		[SerializeField] private UnitClass _unitClass;
		[SerializeField] private FactionVoundDat _faction;
		[SerializeField] private DeployMode _deployMode;

		[Header("Combat Stats")]
		[SerializeField] private float _maxHealth = 100f;
		[SerializeField] private float _damage = 10f;
		[SerializeField] private float _attackRange = 5f;
		[SerializeField] private float _attackSpeed = 1f; 
		[SerializeField] private float _moveSpeed = 2.5f;
		[SerializeField] private GameObject _projectilePrefab;
		[SerializeField] private GameObject _deployPrefab; // Dùng cho mô hình đứng trên tháp (Lều trại, Chỉ huy)
		[SerializeField] private GameObject _spawnedGuardPrefab; // Dùng cho mô hình lính rớt xuống đường (Bruiser)

		[Header("Visual")]
		[SerializeField] private Color _unitColor = Color.white;

		[Header("Animations")]
		[SerializeField] private AnimationClip _idleClip;
		[SerializeField] private AnimationClip _runClip;
		[SerializeField] private AnimationClip _attackClip;
		[SerializeField] private AnimationClip _victoryClip;

		// Public properties for encapsulation
		public string UnitName => _unitName;
		public UnitClass UnitClass => _unitClass;
		public FactionVoundDat Faction => _faction;
		public DeployMode DeployMode => _deployMode;
		public float MaxHealth => _maxHealth;
		public float Damage => _damage;
		public float AttackRange => _attackRange;
		public float AttackSpeed => _attackSpeed;
		public float MoveSpeed => _moveSpeed;
		public GameObject ProjectilePrefab => _projectilePrefab;
		public GameObject DeployPrefab => _deployPrefab;
		public GameObject SpawnedGuardPrefab => _spawnedGuardPrefab;
		public Color UnitColor => _unitColor;

		public AnimationClip IdleClip => _idleClip;
		public AnimationClip RunClip => _runClip;
		public AnimationClip AttackClip => _attackClip;
		public AnimationClip VictoryClip => _victoryClip;
	}
}