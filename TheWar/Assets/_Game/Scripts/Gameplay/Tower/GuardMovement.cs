using UnityEngine;
using TowerDefense.Core;
using TowerDefense.Tower;

namespace TowerDefense.Gameplay.Tower
{
    [RequireComponent(typeof(GuardUnit), typeof(GuardDetector))]
    public class GuardMovement : MonoBehaviour
    {
        private GuardUnit _guard;
        private GuardDetector _detector;
        private TowerUnitAnimation _animation;
        private GuardCombat _combat;
        private UnitData _data;

        public void Initialize(UnitData data)
        {
            _data = data;
            _guard = GetComponent<GuardUnit>();
            _detector = GetComponent<GuardDetector>();
            _animation = GetComponent<TowerUnitAnimation>();
            _combat = GetComponent<GuardCombat>();
        }

        private void Update()
        {
            if (_guard == null || _data == null) return;

            Vector3 targetPos = _guard.SpawnPoint;
            bool isMovingToEnemy = false;

            // Ép tìm target mới ngay lập tức nếu target hiện tại vừa chết
            if (_detector != null && (_detector.CurrentTarget == null || !_detector.CurrentTarget.gameObject.activeInHierarchy))
            {
                _detector.ForceFindTarget();
            }

            if (_detector != null && _detector.CurrentTarget != null && _detector.CurrentTarget.gameObject.activeInHierarchy)
            {
                targetPos = _detector.CurrentTarget.position;
                isMovingToEnemy = true;
            }

            float distToTarget = Vector3.Distance(transform.position, targetPos);
            
            // Tính toán khoảng cách dừng sao cho luôn lọt vào trong tầm đánh của GuardCombat
            float stopDist = 0.1f;
            if (isMovingToEnemy)
            {
                stopDist = _combat != null ? Mathf.Max(0.1f, _combat.AttackRange - 0.1f) : 1.3f;
            }

            if (distToTarget > stopDist)
            {
                Vector3 dir = (targetPos - transform.position).normalized;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(dir);
                }
                
                transform.position += dir * (_data.MoveSpeed * Time.deltaTime);
                if (_animation != null) _animation.SetMoveSpeed(_data.MoveSpeed);
            }
            else
            {
                if (_animation != null) _animation.SetMoveSpeed(0f);
            }
        }
    }
}
