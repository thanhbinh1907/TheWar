using System;
using System.Collections.Generic;
using TowerDefense.Enemy;
using UnityEngine;

namespace TowerDefense.Wave
{
	[Serializable]
	public struct EnemySpawnEntry
	{
		public EnemyDataSO enemyData;
		public int count;
		public float spawnInterval;
		public float delayBefore;
	}

	[CreateAssetMenu(fileName = "NewWaveData", menuName = "TowerDefense/Wave Data")]
	public class WaveDataSO : ScriptableObject
	{
		public string waveName;
		public List<EnemySpawnEntry> enemies;
	}
}
