using UnityEngine;

namespace TowerDefense.Core
{
	public enum UnitClass { Bruiser, Ranger, Mage, Assassin, Artillery }
	public enum FactionVoundDat { Kingdom, Adventure, Pirate, Samurai, Dungeons }

	[CreateAssetMenu(fileName = "NewUnitData", menuName = "TowerDefense/Unit Data")]
	public class UnitData : ScriptableObject
	{
		[SerializeField] private string _unitName;
		[SerializeField] private UnitClass _unitClass;
		[SerializeField] private FactionVoundDat _faction;

		[Header("Combat Stats")]
		[SerializeField] private float _damage = 10f;
		[SerializeField] private float _attackRange = 5f;
		[SerializeField] private float _attackSpeed = 1f; 

		[Header("Visual")]
		[SerializeField] private Color _unitColor = Color.white;

		// Public properties for encapsulation
		public string UnitName => _unitName;
		public UnitClass UnitClass => _unitClass;
		public FactionVoundDat Faction => _faction;
		public float Damage => _damage;
		public float AttackRange => _attackRange;
		public float AttackSpeed => _attackSpeed;
		public Color UnitColor => _unitColor;
	}
}