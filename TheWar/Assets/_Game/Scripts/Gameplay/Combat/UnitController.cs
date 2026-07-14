using UnityEngine;

namespace TowerDefense.Gameplay.Combat
{
    public class UnitController : MonoBehaviour
    {
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _rotationSpeed = 15f;
        [SerializeField] private bool _rotateTowardsTarget = true;

        [SerializeField] private float _attackSpeed = 1f;

        private IAttackBehaviour _attackBehaviour;
        private ITargetDetector _targetDetector;
        private float _lastAttackStartTime;
        private float _lastAttackFinishedTime;
        private bool _wasAttacking;

        public float AttackRange => _attackRange;

        private void Awake()
        {
            _attackBehaviour = GetComponent<IAttackBehaviour>();
            _targetDetector = GetComponent<ITargetDetector>();

            if (_attackBehaviour == null)
            {
                Debug.LogWarning($"[{gameObject.name}] UnitController requires an IAttackBehaviour component!");
            }
            if (_targetDetector == null)
            {
                Debug.LogWarning($"[{gameObject.name}] UnitController requires an ITargetDetector component!");
            }
        }

        private void Update()
        {
            if (_targetDetector == null || _attackBehaviour == null) return;

            bool hasValidTarget = _targetDetector.HasValidTarget && _targetDetector.CurrentTarget != null;
            bool isInRange = false;

            if (hasValidTarget)
            {
                float sqrDistance = (_targetDetector.CurrentTarget.position - transform.position).sqrMagnitude;
                isInRange = sqrDistance <= (_attackRange * _attackRange);
            }

            // Handle Attack State
            if (isInRange)
            {
                if (!_attackBehaviour.IsAttacking)
                {
                    // Đảm bảo có khoảng nghỉ giữa 2 đòn đánh dựa trên AttackSpeed (tính từ lúc bắt đầu đòn trước)
                    // VÀ bắt buộc phải có ít nhất 0.1s nghỉ sau khi kết thúc đòn trước để Animator kịp chuyển state
                    float cooldown = 1f / Mathf.Max(_attackSpeed, 0.1f);
                    if (Time.time - _lastAttackStartTime >= cooldown && Time.time - _lastAttackFinishedTime >= 0.1f)
                    {
                        _lastAttackStartTime = Time.time;
                        _attackBehaviour.StartAttack(_targetDetector.CurrentTarget);
                    }
                }

                // Custom logic for charged attack (e.g., ticking hold duration)
                if (_attackBehaviour is ChargedAttack charged)
                {
                    charged.TickHold();
                }
            }

            // Ghi nhận thời điểm kết thúc đòn đánh để tính Cooldown
            if (_wasAttacking && !_attackBehaviour.IsAttacking)
            {
                _lastAttackFinishedTime = Time.time;
            }
            _wasAttacking = _attackBehaviour.IsAttacking;

            // Chỉ CancelAttack khi target chết hoặc mất hẳn (tránh bị giật cancel liên tục khi quái vừa nhích ra khỏi tầm)
            if (!hasValidTarget && _attackBehaviour.IsAttacking)
            {
                _attackBehaviour.CancelAttack();
            }

            // Handle Rotation
            if (_rotateTowardsTarget && hasValidTarget && _attackBehaviour.IsAttacking) 
            {
                RotateTowards(_targetDetector.CurrentTarget);
            }
        }

        private void RotateTowards(Transform target)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0; // Prevent looking up/down

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        // Optional: Method to dynamically update attack range (e.g. from UnitData)
        public void SetAttackRange(float range)
        {
            _attackRange = range;
        }

        public void SetAttackSpeed(float speed)
        {
            _attackSpeed = speed;
        }
    }
}
