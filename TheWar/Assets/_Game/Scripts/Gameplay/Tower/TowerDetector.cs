using System.Collections;
using UnityEngine;

namespace TowerDefense.Gameplay.Tower
{
    public class TowerDetector : MonoBehaviour
    {
        private float _attackRange;
        private Coroutine _detectionCoroutine;
        private int _enemyLayerMask;

        public Transform CurrentTarget { get; private set; }

        private void Awake()
        {
            _enemyLayerMask = LayerMask.GetMask("Enemy");
            if (_enemyLayerMask == 0)
            {
                // Fallback if "Enemy" layer isn't defined yet
                Debug.LogWarning("TowerDetector: 'Enemy' layer not found, using layer 6 as fallback.");
                _enemyLayerMask = 1 << 6; 
            }
        }

        public void Initialize(float range)
        {
            _attackRange = range;
            if (_detectionCoroutine != null)
            {
                StopCoroutine(_detectionCoroutine);
            }
            _detectionCoroutine = StartCoroutine(DetectionRoutine());
        }

        public void StopDetecting()
        {
            if (_detectionCoroutine != null)
            {
                StopCoroutine(_detectionCoroutine);
                _detectionCoroutine = null;
            }
            CurrentTarget = null;
        }

        private IEnumerator DetectionRoutine()
        {
            var wait = new WaitForSeconds(0.1f);
            while (true)
            {
                FindClosestEnemy();
                yield return wait;
            }
        }

        private void FindClosestEnemy()
        {
            // Only search if we don't have a valid target or the current target is dead/out of range
            if (CurrentTarget != null)
            {
                if (!CurrentTarget.gameObject.activeInHierarchy || 
                    Vector3.Distance(transform.position, CurrentTarget.position) > _attackRange)
                {
                    CurrentTarget = null;
                }
                else
                {
                    // Current target is still valid
                    return;
                }
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, _attackRange, _enemyLayerMask);
            float closestDistanceSqr = Mathf.Infinity;
            Transform closestTarget = null;

            foreach (var hit in hits)
            {
                Vector3 directionToTarget = hit.transform.position - transform.position;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    closestTarget = hit.transform;
                }
            }

            CurrentTarget = closestTarget;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
#endif
    }
}
