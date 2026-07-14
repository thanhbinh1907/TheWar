using TowerDefense.Core;
using TowerDefense.Shared;
using UnityEngine;
using System;

namespace TowerDefense.Gameplay.Combat
{
    public class ChargedAttack : MonoBehaviour, IAttackBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private float _minHoldTime = 0.3f;
        [SerializeField] private float _maxHoldTime = 1.5f;
        [SerializeField] private float _minDamage = 10f;
        [SerializeField] private float _maxDamage = 30f;
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private Transform _firePoint;

        private static readonly int AttackPhaseHash = Animator.StringToHash("AttackPhase");

        private float _holdStartTime;
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
            _animator.SetInteger(AttackPhaseHash, 1); // Windup phase
        }

        // Gọi từ Animation Event ở cuối clip Windup
        public void OnWindupFinishedEvent()
        {
            _holdStartTime = Time.time;
            _animator.SetInteger(AttackPhaseHash, 2); // Hold phase, loop chờ
        }

        // Gọi mỗi frame từ UnitController khi đang ở phase Hold
        public void TickHold()
        {
            if (!IsAttacking) return;
            
            bool targetLost = _currentTarget == null || !_currentTarget.gameObject.activeInHierarchy;
            bool maxHoldReached = Time.time - _holdStartTime >= _maxHoldTime;

            if (targetLost || maxHoldReached)
            {
                Release();
            }
        }

        private void Release()
        {
            _animator.SetInteger(AttackPhaseHash, 3); // Release phase
        }

        // Gọi từ Animation Event đúng frame bắn mũi tên (ví dụ: Hit)
        public void Hit()
        {
            float chargeRatio = Mathf.Clamp01((Time.time - _holdStartTime) / _maxHoldTime);
            float finalDamage = Mathf.Lerp(_minDamage, _maxDamage, chargeRatio);

            if (_currentTarget != null && _projectilePrefab != null && ProjectilePool.Instance != null)
            {
                Projectile proj = ProjectilePool.Instance.Get(
                    _projectilePrefab.gameObject, 
                    _firePoint.position, 
                    Quaternion.identity
                ).GetComponent<Projectile>();
                
                if (proj != null)
                {
                    proj.Launch(_currentTarget, finalDamage);
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

        public void CancelAttack()
        {
            IsAttacking = false;
            _currentTarget = null;
            _animator.SetInteger(AttackPhaseHash, 0);
        }

        public void SetDamageRange(float minDamage, float maxDamage)
        {
            _minDamage = minDamage;
            _maxDamage = maxDamage;
        }
    }
}
