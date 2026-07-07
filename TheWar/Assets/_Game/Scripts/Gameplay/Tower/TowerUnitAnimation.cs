using UnityEngine;

namespace TowerDefense.Tower
{
    [RequireComponent(typeof(Animator))]
    public class TowerUnitAnimation : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private TowerUnitDataSO _unitData;
        [SerializeField] private RuntimeAnimatorController _baseController;

        private Animator _animator;
        private AnimatorOverrideController _overrideController;

        private float _currentSpeed;

        private static readonly int _speedHash = Animator.StringToHash("Speed");
        private static readonly int _attackHash = Animator.StringToHash("Attack");
        private static readonly int _victoryHash = Animator.StringToHash("Victory");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.cullingMode = AnimatorCullingMode.CullCompletely;
            
            InitializeOverrideController();
        }

        private void InitializeOverrideController()
        {
            if (_baseController == null || _unitData == null)
            {
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
            // Smoothly sync speed to animator if the unit can move
            _animator.SetFloat(_speedHash, _currentSpeed, 0.1f, Time.deltaTime);
        }

        public void SetMoveSpeed(float speed)
        {
            _currentSpeed = speed;
        }

        public void TriggerAttackAnimation()
        {
            _animator.SetTrigger(_attackHash);
        }

        public void TriggerVictoryAnimation()
        {
            _animator.SetTrigger(_victoryHash);
        }
    }
}
