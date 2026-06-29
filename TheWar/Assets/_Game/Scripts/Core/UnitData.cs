using UnityEngine;

namespace TowerDefense.Core
{
	public enum UnitClass { Bruiser, Ranger, Mage, Assassin, Artillery }
	public enum FactionVoundDat { Kingdom, Adventure, Pirate, Samurai, Dungeons }

	[CreateAssetMenu(fileName = "NewUnitData", menuName = "TowerDefense/Unit Data")]
	public class UnitData : ScriptableObject
	{
		public string unitName;
		public UnitClass unitClass;
		public FactionVoundDat faction;

		[Header("Combat Stats")]
		public float damage = 10f;
		public float attackRange = 5f;
		public float attackSpeed = 1f; 

		[Header("Visual")]
		public Color unitColor = Color.white; 
	}
}