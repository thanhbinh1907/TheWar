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

        public event System.Action OnHitEvent;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.cullingMode = AnimatorCullingMode.CullCompletely;
        }

        public void Initialize(EnemyDataSO data)
        {
            _enemyData = data;
            InitializeOverrideController();
            SetAttackSpeed(data.AttackSpeed); // Ngăn lỗi animation bị đứng hình nếu state dùng Multiplier
        }

        private void OnEnable()
        {
            if (_enemyData != null)
            {
                SetAttackSpeed(_enemyData.AttackSpeed);
            }
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
            
            var overrides = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
            _overrideController.GetOverrides(overrides);
            
            for (int i = 0; i < overrides.Count; i++)
            {
                if (overrides[i].Key == null) continue;
                string clipName = overrides[i].Key.name.ToLower();
                Debug.Log($"[{gameObject.name}] Found clip in base Animator: {overrides[i].Key.name}");
                
                if (clipName.Contains("idle")) {
                    overrides[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, _enemyData.IdleClip);
                    string targetName = _enemyData.IdleClip != null ? _enemyData.IdleClip.name : "NULL";
                    Debug.Log($"[{gameObject.name}] -> Overriding {overrides[i].Key.name} with IdleClip ({targetName})");
                }
                else if (clipName.Contains("run") || clipName.Contains("walk") || clipName.Contains("move")) {
                    overrides[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, _enemyData.RunClip);
                    string targetName = _enemyData.RunClip != null ? _enemyData.RunClip.name : "NULL";
                    Debug.Log($"[{gameObject.name}] -> Overriding {overrides[i].Key.name} with RunClip ({targetName})");
                }
                else if (clipName.Contains("attack") || clipName.Contains("hit") || clipName.Contains("combat")) {
                    overrides[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, _enemyData.AttackClip);
                    string targetName = _enemyData.AttackClip != null ? _enemyData.AttackClip.name : "NULL";
                    Debug.Log($"[{gameObject.name}] -> Overriding {overrides[i].Key.name} with AttackClip ({targetName})");
                }
                else if (clipName.Contains("die") || clipName.Contains("death")) {
                    overrides[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, _enemyData.DieClip);
                    string targetName = _enemyData.DieClip != null ? _enemyData.DieClip.name : "NULL";
                    Debug.Log($"[{gameObject.name}] -> Overriding {overrides[i].Key.name} with DieClip ({targetName})");
                }
            }
            
            // Backup phòng hờ trường hợp clip gốc đúng chuẩn tên "Base_X"
            _overrideController["Base_Idle"] = _enemyData.IdleClip;
            _overrideController["Base_Run"] = _enemyData.RunClip;
            _overrideController["Base_Attack"] = _enemyData.AttackClip;
            _overrideController["Base_Die"] = _enemyData.DieClip;

            _overrideController.ApplyOverrides(overrides);

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

        public void SetAttackSpeed(float attackSpeed)
        {
            if (_animator != null)
            {
                // Tránh trường hợp nhập 0 trong ScriptableObject dẫn đến việc animation bị đứng hình
                float safeSpeed = attackSpeed >= 0.1f ? attackSpeed : 0.1f;
                
                // Yêu cầu Animator Controller phải có parameter "AttackSpeed" kiểu Float, 
                // và được tick vào ô Multiplier của state Attack.
                _animator.SetFloat("AttackSpeed", safeSpeed);
            }
        }

        public void TriggerAttackAnimation()
        {
            Debug.Log($"[{gameObject.name}] EnemyAnimation: Calling _animator.SetTrigger(\"Attack\")");
            _animator.SetTrigger(_attackHash);
            
            // Check state immediately after trigger
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(0);
            Debug.Log($"[{gameObject.name}] Current State Hash: {currentState.shortNameHash} | Next State Hash (if transitioning): {nextState.shortNameHash}");
        }

        public void TriggerDieAnimation()
        {
            _animator.SetTrigger(_dieHash);
        }

        // Được gọi bằng Animation Event trong Unity (phải gõ đúng chữ Hit)
        public void Hit()
        {
            OnHitEvent?.Invoke();
        }
    }
}
