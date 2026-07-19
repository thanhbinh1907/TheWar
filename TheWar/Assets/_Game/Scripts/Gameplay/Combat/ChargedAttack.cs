using TowerDefense.Core;
using TowerDefense.Shared;
using UnityEngine;
using System;

namespace TowerDefense.Gameplay.Combat
{
    public class ChargedAttack : MonoBehaviour, IAttackBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _firePoint;

        private float _damage;
        private Projectile _projectilePrefab;

        private static readonly int AttackPhaseHash = Animator.StringToHash("AttackPhase");

        private Transform _currentTarget;
        private BowStringController _bowStringController;
        
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
            
            _bowStringController = GetComponent<BowStringController>();
        }

        public void StartAttack(Transform target)
        {
            if (IsAttacking) return;
            
            IsAttacking = true;
            _currentTarget = target;
            _animator.SetInteger(AttackPhaseHash, 1); // Windup phase
            
            // Đảm bảo mũi tên giả luôn hiện ra khi bắt đầu một chu kỳ tấn công mới
            // (Đề phòng trường hợp lần bắn trước bị hủy ngang lúc chưa kịp Reload)
            if (_bowStringController != null)
            {
                _bowStringController.ShowArrow();
            }
        }

        // Gọi từ Animation Event ở cuối clip Windup
        public void OnWindupFinishedEvent()
        {
            _animator.SetInteger(AttackPhaseHash, 2); // Hold phase, loop chờ hoặc chuyển tiếp dựa vào Animator
        }

        // Gọi mỗi frame từ UnitController khi đang ở phase Hold
        public void TickHold()
        {
            if (!IsAttacking) return;
            
            bool targetLost = _currentTarget == null || !_currentTarget.gameObject.activeInHierarchy;

            if (targetLost)
            {
                CancelAttack();
            }
        }

        //        // Gọi từ Animation Event ở cuối clip Hold (Tùy chọn)
        public void OnHoldFinishedEvent()
        {
            _animator.SetInteger(AttackPhaseHash, 3); // Release phase
        }

        // Gọi từ Animation Event ở cuối clip Release (nếu có Reload)
        public void OnReleaseFinishedEvent()
        {
            _animator.SetInteger(AttackPhaseHash, 4); // Reload phase
        }

        // Gọi từ Animation Event ở cuối cùng (sau Release hoặc Reload)
        public void OnAttackFinishedEvent()
        {
            IsAttacking = false;
            _currentTarget = null;
            _animator.SetInteger(AttackPhaseHash, 0);
        }

        // Gọi từ Animation Event đúng frame bắn mũi tên (ví dụ: Hit)
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



        public void CancelAttack()
        {
            IsAttacking = false;
            _currentTarget = null;
            _animator.SetInteger(AttackPhaseHash, 0);

            if (_bowStringController != null)
            {
                _bowStringController.ReleaseString();
                // Không HideArrow ở đây nữa, giữ mũi tên trên tay để dùng bắn con tiếp theo
            }
        }

        public void Initialize(float damage, GameObject projectilePrefab)
        {
            _damage = damage;
            if (projectilePrefab != null)
            {
                _projectilePrefab = projectilePrefab.GetComponent<Projectile>();
            }
        }
    }
}
