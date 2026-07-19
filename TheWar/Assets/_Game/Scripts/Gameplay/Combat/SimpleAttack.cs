using TowerDefense.Shared;
using UnityEngine;
using System;

namespace TowerDefense.Gameplay.Combat
{
    public class SimpleAttack : MonoBehaviour, IAttackBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private float _damage;

        private static readonly int AttackPhaseHash = Animator.StringToHash("AttackPhase");
        
        private Transform _currentTarget;
        
        public bool IsAttacking { get; private set; }

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        public void StartAttack(Transform target)
        {
            if (IsAttacking) return;
            
            IsAttacking = true;
            _currentTarget = target;
            
            // Kích hoạt phase 1 để Animator chạy Base_Attack
            _animator.SetInteger(AttackPhaseHash, 1); 
        }

        public void CancelAttack()
        {
            IsAttacking = false;
            _currentTarget = null;
            _animator.SetInteger(AttackPhaseHash, 0);
        }

        // Gọi từ Animation Event (ví dụ: Hit)
        public void Hit()
        {
            if (_currentTarget != null && _currentTarget.gameObject.activeInHierarchy)
            {
                var damageable = _currentTarget.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(_damage);
                }
            }
        }

        // Gọi từ Animation Event ở cuối clip Release
        public void OnAttackFinishedEvent()
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
