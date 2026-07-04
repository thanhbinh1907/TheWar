using UnityEngine;

public class EnemySteering : MonoBehaviour
{
    [Header("Separation (Dàn ngang mặt đường)")]
    [SerializeField] private float _separationRadius = 2.5f;
    [SerializeField] private float _separationWeight = 3f;

    private Vector3 _separationForce;
        
    public Vector3 CurrentVelocity { get; private set; }

    private void OnEnable()
    {
        InvokeRepeating(nameof(CalculateSeparationForce), 0.1f, 0.1f);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(CalculateSeparationForce));
    }

    public Vector3 CalculateSeekForce(Vector3 currentPos, Vector3 targetWaypoint, float maxSpeed)
    {
        Vector3 desiredVelocity = (targetWaypoint - currentPos).normalized * maxSpeed;
        CurrentVelocity = desiredVelocity + _separationForce;
            
        // Giới hạn max speed để không đi quá nhanh khi cộng vector
        if (CurrentVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            CurrentVelocity = CurrentVelocity.normalized * maxSpeed;
        }
            
        return CurrentVelocity;
    }

    private void CalculateSeparationForce()
    {
        _separationForce = Vector3.zero;
        int enemyLayer = LayerMask.GetMask("Enemy");
        // Dùng _separationRadius để quái nhận diện nhau từ xa và dàn rộng ra bề mặt đường
        Collider[] colliders = Physics.OverlapSphere(transform.position, _separationRadius, enemyLayer);
            
        int count = 0;
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                Vector3 awayFromNeighbor = transform.position - col.transform.position;
                // Force tăng dần khi khoảng cách càng gần
                _separationForce += awayFromNeighbor.normalized / (awayFromNeighbor.magnitude + 0.1f);
                count++;
            }
        }

        if (count > 0)
        {
            _separationForce /= count;
            _separationForce = _separationForce.normalized * _separationWeight; // Trọng số đẩy dàn hàng
        }
    }
}
