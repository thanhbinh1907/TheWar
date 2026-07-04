using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Wave
{
	[CreateAssetMenu(fileName = "NewLevelData", menuName = "TowerDefense/Level Data")]
	public class LevelDataSO : ScriptableObject
	{
		public string levelName;
		public List<WaveDataSO> waves;
		public int baseHP = 20;
		public int startingGold = 100;
	}
}
