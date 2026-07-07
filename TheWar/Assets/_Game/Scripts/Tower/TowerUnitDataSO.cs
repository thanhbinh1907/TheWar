using UnityEngine;

namespace TowerDefense.Tower
{
    [CreateAssetMenu(menuName = "Game/Tower Unit Data", fileName = "NewTowerUnitData")]
    public class TowerUnitDataSO : ScriptableObject
    {
        [Header("Stats")]
        [SerializeField] private string _unitName;
        [SerializeField] private float _damage;
        [SerializeField] private float _attackRange;
        [SerializeField] private float _fireRate;

        [Header("Animations")]
        [SerializeField] private AnimationClip _idleClip;
        [SerializeField] private AnimationClip _runClip;
        [SerializeField] private AnimationClip _attackClip;
        [SerializeField] private AnimationClip _victoryClip;

        public string UnitName => _unitName;
        public float Damage => _damage;
        public float AttackRange => _attackRange;
        public float FireRate => _fireRate;

        public AnimationClip IdleClip => _idleClip;
        public AnimationClip RunClip => _runClip;
        public AnimationClip AttackClip => _attackClip;
        public AnimationClip VictoryClip => _victoryClip;
    }
}
