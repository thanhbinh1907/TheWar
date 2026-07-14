using TowerDefense.Core;
using TowerDefense.Shared;
using UnityEngine;

namespace TowerDefense.Gameplay.Combat
{
    public class SimpleRangedAttack : MonoBehaviour, IAttackBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private Transform _firePoint;

        private static readonly int AttackPhaseHash = Animator.StringToHash("AttackPhase");
        private Transform _currentTarget;
        
        public bool IsAttacking { get; private set; }

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
            if (_firePoint == null)
            {
                _firePoint = transform;
            }
        }

        public void StartAttack(Transform target)
        {
            if (IsAttacking) return;
            
            IsAttacking = true;
            _currentTarget = target;
            
            // Phase 3 tương đương với vung vũ khí chém/bắn luôn (giống lính cận chiến)
            _animator.SetInteger(AttackPhaseHash, 3); 
        }

        // Gọi từ Animation Event đúng lúc quả cầu phép bay ra khỏi gậy (ví dụ: Hit)
        public void Hit()
        {
            if (_currentTarget != null && _projectilePrefab != null && ProjectilePool.Instance != null)
            {
                Projectile proj = ProjectilePool.Instance.Get(
                    _projectilePrefab.gameObject, 
                    _firePoint.position, 
                    Quaternion.identity
                ).GetComponent<Projectile>();
                
                if (proj != null)
                {
                    proj.Launch(_currentTarget, _damage);
                }
            }
        }

        // Gọi từ Animation Event ở cuối clip bắn
        public void OnAttackFinishedEvent()
        {
            IsAttacking = false;
            _currentTarget = null;
            _animator.SetInteger(AttackPhaseHash, 0);
        }

        public void CancelAttack()
        {
            IsAttacking = false;
            _currentTarget = null;
            _animator.SetInteger(AttackPhaseHash, 0);
        }

        public void SetDamage(float damage)
        {
            _damage = damage;
        }
    }
}
