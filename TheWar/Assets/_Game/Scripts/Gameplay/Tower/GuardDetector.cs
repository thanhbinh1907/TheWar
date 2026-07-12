using System.Collections;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Gameplay.Tower
{
    public class GuardDetector : MonoBehaviour
    {
        [SerializeField] private float _detectRange = 5f;
        [SerializeField] private LayerMask _enemyLayerMask;
        private Coroutine _detectionCoroutine;
        
        public Transform CurrentTarget { get; private set; }

        private void Awake()
        {
            if (_enemyLayerMask.value == 0)
            {
                _enemyLayerMask = LayerMask.GetMask("Enemy");
                if (_enemyLayerMask.value == 0)
                {
                    _enemyLayerMask = 1 << 6; 
                }
            }
        }

        public void Initialize(UnitData data)
        {
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

        private void OnDisable()
        {
            StopDetecting();
        }

        private IEnumerator DetectionRoutine()
        {
            var wait = new WaitForSeconds(0.2f);
            while (true)
            {
                ForceFindTarget();
                yield return wait;
            }
        }

        public void ForceFindTarget()
        {
            if (CurrentTarget != null)
            {
                if (!CurrentTarget.gameObject.activeInHierarchy || 
                    Vector3.Distance(transform.position, CurrentTarget.position) > _detectRange)
                {
                    CurrentTarget = null;
                }
                else
                {
                    return;
                }
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, _detectRange, _enemyLayerMask);

            float closestDistanceSqr = Mathf.Infinity;
            Transform closestTarget = null;

            foreach (var hit in hits)
            {
                // Bỏ qua quái đã chết hoặc bị disable
                if (!hit.gameObject.activeInHierarchy) continue;

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
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _detectRange);
        }
#endif
    }
}
