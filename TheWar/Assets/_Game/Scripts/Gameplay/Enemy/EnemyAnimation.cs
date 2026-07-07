using UnityEngine;

namespace TowerDefense.Enemy
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimation : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private RuntimeAnimatorController _baseController;

        private EnemyDataSO _enemyData;

        private Animator _animator;
        private AnimatorOverrideController _overrideController;

        private float _currentSpeed;

        private static readonly int _speedHash = Animator.StringToHash("Speed");
        private static readonly int _attackHash = Animator.StringToHash("Attack");
        private static readonly int _dieHash = Animator.StringToHash("Die");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.cullingMode = AnimatorCullingMode.CullCompletely;
        }

        public void Initialize(EnemyDataSO data)
        {
            _enemyData = data;
            InitializeOverrideController();
        }

        private void InitializeOverrideController()
        {
            if (_animator == null) 
            {
                _animator = GetComponent<Animator>();
            }

            if (_baseController == null || _enemyData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Missing BaseController or EnemyData in EnemyAnimation!");
                return;
            }

            // Create override controller to inject data-driven clips
            _overrideController = new AnimatorOverrideController(_baseController);
            
            // Map the raw clips to the base states by their original clip names
            _overrideController["Base_Idle"] = _enemyData.IdleClip;
            _overrideController["Base_Run"] = _enemyData.RunClip;
            _overrideController["Base_Attack"] = _enemyData.AttackClip;
            _overrideController["Base_Die"] = _enemyData.DieClip;

            _animator.runtimeAnimatorController = _overrideController;
        }

        private void Update()
        {
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                // Smoothly sync speed to animator to prevent snappy transitions.
                // Uses Animator's built-in damping for optimal performance.
                _animator.SetFloat(_speedHash, _currentSpeed, 0.1f, Time.deltaTime);
            }
        }

        /// <summary>
        /// Called by the movement system/steering to pass the current target speed.
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            _currentSpeed = speed;
        }

        public void TriggerAttackAnimation()
        {
            _animator.SetTrigger(_attackHash);
        }

        public void TriggerDieAnimation()
        {
            _animator.SetTrigger(_dieHash);
        }
    }
}
