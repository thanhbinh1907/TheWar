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
		[SerializeField] private float _damage = 10f;
		[SerializeField] private float _attackRange = 5f;
		[SerializeField] private float _attackSpeed = 1f; 
		[SerializeField] private GameObject _projectilePrefab;
		[SerializeField] private GameObject _deployPrefab; // Dùng cho Bruiser (Guard) hoặc Assassin (Lính trên đường)

		[Header("Visual")]
		[SerializeField] private Color _unitColor = Color.white;

		// Public properties for encapsulation
		public string UnitName => _unitName;
		public UnitClass UnitClass => _unitClass;
		public FactionVoundDat Faction => _faction;
		public DeployMode DeployMode => _deployMode;
		public float Damage => _damage;
		public float AttackRange => _attackRange;
		public float AttackSpeed => _attackSpeed;
		public GameObject ProjectilePrefab => _projectilePrefab;
		public GameObject DeployPrefab => _deployPrefab;
		public Color UnitColor => _unitColor;
	}
}