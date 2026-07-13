using UnityEngine;
using TowerDefense.Core;
using TowerDefense.Tower;

namespace TowerDefense.Gameplay.Tower
{
    [RequireComponent(typeof(GuardUnit), typeof(GuardDetector))]
    public class GuardMovement : MonoBehaviour
    {
        [Header("Separation Settings")]
        [SerializeField] private float _separationRadius = 1.2f;
        [SerializeField] private float _separationWeight = 1.5f;

        [Header("Patrol Settings")]
        [SerializeField] private float _maxPatrolRange = 3f;

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
                
                // Add simple boid-like separation to prevent overlapping
                Vector3 separation = Vector3.zero;
                int neighborCount = 0;
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, _separationRadius);
                foreach (var col in hitColliders)
                {
                    if (col.gameObject == this.gameObject) continue;
                    
                    var otherGuard = col.GetComponent<GuardUnit>();
                    if (otherGuard != null)
                    {
                        Vector3 diff = transform.position - otherGuard.transform.position;
                        diff.y = 0;
                        float mag = diff.magnitude;
                        
                        // Prevent zero division if exactly at the same spot
                        if (mag < 0.001f)
                        {
                            diff = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * 0.1f;
                            mag = 0.1f;
                        }
                        
                        separation += (diff.normalized / mag);
                        neighborCount++;
                    }
                }

                if (neighborCount > 0)
                {
                    separation /= neighborCount;
                    dir = (dir + separation * _separationWeight).normalized;
                }

                // Kiểm tra xem bước đi tiếp theo có bị lố ra khỏi vòng giới hạn không
                bool isClamped = false;
                if (_guard.Spawner != null)
                {
                    Vector3 towerPos = _guard.Spawner.transform.position;
                    Vector3 towerPos2D = new Vector3(towerPos.x, 0f, towerPos.z);
                    
                    Vector3 nextPos = transform.position + dir * (_data.MoveSpeed * Time.deltaTime);
                    Vector3 nextPos2D = new Vector3(nextPos.x, 0f, nextPos.z);
                    
                    if (Vector3.Distance(towerPos2D, nextPos2D) > _maxPatrolRange)
                    {
                        isClamped = true;
                    }
                }

                if (isClamped)
                {
                    // Nếu đụng giới hạn, đứng im và không áp dụng di chuyển (ngăn hiện tượng trượt dọc viền)
                    if (_animation != null) _animation.SetMoveSpeed(0f);
                    
                    // Vẫn cho phép xoay người nhìn theo mục tiêu
                    dir.y = 0;
                    if (dir.sqrMagnitude > 0.01f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(dir);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
                    }
                    return; 
                }

                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
                }
                
                transform.position += dir * (_data.MoveSpeed * Time.deltaTime);
                if (_animation != null) _animation.SetMoveSpeed(_data.MoveSpeed);
            }
            else
            {
                if (_animation != null) _animation.SetMoveSpeed(0f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Gizmo cho Separation (như cũ)
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Cyan
            Gizmos.DrawWireSphere(transform.position, _separationRadius);

            // Gizmo cho giới hạn di chuyển (Max Patrol Range)
            var guard = GetComponent<GuardUnit>();
            if (guard != null && guard.Spawner != null && Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(guard.Spawner.transform.position, _maxPatrolRange);
            }
        }
    }
}
