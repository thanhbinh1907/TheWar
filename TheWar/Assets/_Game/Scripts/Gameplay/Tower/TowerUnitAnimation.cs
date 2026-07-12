using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Tower
{
    [RequireComponent(typeof(Animator))]
    public class TowerUnitAnimation : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private RuntimeAnimatorController _baseController;

        private UnitData _unitData;

        private Animator _animator;
        private AnimatorOverrideController _overrideController;

        private float _currentSpeed;

        public event System.Action OnHitEvent;

        private static readonly int _speedHash = Animator.StringToHash("Speed");
        private static readonly int _attackHash = Animator.StringToHash("Attack");
        private static readonly int _victoryHash = Animator.StringToHash("Victory");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.cullingMode = AnimatorCullingMode.CullCompletely;
            _animator.applyRootMotion = false; // Tắt Root Motion để nhân vật không bị trôi đi khi đánh
        }

        public void Initialize(UnitData data)
        {
            _unitData = data;
            InitializeOverrideController();
        }

        private void InitializeOverrideController()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            if (_baseController == null || _unitData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Missing BaseController or UnitData in TowerUnitAnimation!");
                return;
            }

            // Create override controller to inject data-driven clips
            _overrideController = new AnimatorOverrideController(_baseController);
            
            // Map the raw clips to the base states by their original clip names
            _overrideController["Base_Idle"] = _unitData.IdleClip;
            _overrideController["Base_Run"] = _unitData.RunClip;
            _overrideController["Base_Attack"] = _unitData.AttackClip;
            _overrideController["Base_Victory"] = _unitData.VictoryClip;

            _animator.runtimeAnimatorController = _overrideController;
        }

        private void Update()
        {
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                // Smoothly sync speed to animator if the unit can move
                _animator.SetFloat(_speedHash, _currentSpeed, 0.1f, Time.deltaTime);
            }
        }

        public void SetMoveSpeed(float speed)
        {
            _currentSpeed = speed;
        }

        public void SetAttackSpeed(float attackSpeed)
        {
            if (_animator != null)
            {
                // Yêu cầu Animator Controller phải có parameter "AttackSpeed" kiểu Float, 
                // và được tick vào ô Multiplier của state Attack.
                _animator.SetFloat("AttackSpeed", attackSpeed);
            }
        }

        public void TriggerAttackAnimation()
        {
            _animator.SetTrigger(_attackHash);
        }

        public void StopAttackAnimation()
        {
            if (_animator != null)
            {
                _animator.ResetTrigger(_attackHash);
                // Ép Animator quay về trạng thái nghỉ, tránh trường hợp bị kẹt ở Animation đánh (nếu thiếu transition)
                _animator.Play("Base_Idle");
            }
        }

        // Hàm này sẽ được gọi bằng Animation Event trên frame vung gậy/bắn cung
        public void Hit()
        {
            OnHitEvent?.Invoke();
        }

        public void TriggerVictoryAnimation()
        {
            _animator.SetTrigger(_victoryHash);
        }
    }
}
