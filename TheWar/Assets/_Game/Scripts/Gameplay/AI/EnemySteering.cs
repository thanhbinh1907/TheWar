using UnityEngine;

public class EnemySteering : MonoBehaviour
{
    [Header("Separation (Dàn ngang mặt đường)")]
    [SerializeField] private float _separationRadius = 1.0f;
    [SerializeField] private float _separationWeight = 3f;

    [Header("Adaptive Path Following")]
    [SerializeField] private float _lookAheadDistance = 1.0f;
    
    private Vector3 _targetSeparationForce;
    private Vector3 _currentSeparationForce;
    private float _normalizedLaneFactor;
        
    public Vector3 CurrentVelocity { get; private set; }
    public Vector3 DesiredDirection { get; private set; }

    private void OnEnable()
    {
        // Giảm range xuống một chút nữa để quái không đi quá sát mép
        _normalizedLaneFactor = Random.Range(-0.5f, 0.5f);
        InvokeRepeating(nameof(CalculateSeparationForce), 0.1f, 0.1f);
        
        _currentSeparationForce = Vector3.zero;
        _targetSeparationForce = Vector3.zero;
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(CalculateSeparationForce));
    }

    /// <summary>
    /// Calculates an adaptive target position along the current segment, factoring in look-ahead distance and lane offset.
    /// </summary>
    public Vector3 CalculateAdaptivePathTarget(Vector3 currentPos, BakedSegment segment, out float progress)
    {
        Vector3 segmentDir = segment.EndNode - segment.StartNode;
        float segmentLength = segmentDir.magnitude;
        Vector3 segmentDirNorm = segmentDir / segmentLength;

        // Vector from start node to current position
        Vector3 toPos = currentPos - segment.StartNode;
        
        // Project current position onto the segment to find distance along the segment
        float dotProduct = Vector3.Dot(toPos, segmentDirNorm);
        
        // Calculate progress (0.0 to 1.0)
        progress = Mathf.Clamp01(dotProduct / segmentLength);
        
        // Look ahead by a fixed distance
        float lookAheadDot = Mathf.Clamp(dotProduct + _lookAheadDistance, 0f, segmentLength);
        Vector3 projectedPos = segment.StartNode + segmentDirNorm * lookAheadDot;

        // Apply lane offset
        Vector3 rightDir = Vector3.Cross(Vector3.up, segmentDirNorm).normalized;
        float offsetDistance = _normalizedLaneFactor >= 0 
            ? _normalizedLaneFactor * segment.MaxRightWidth 
            : _normalizedLaneFactor * segment.MaxLeftWidth; // Negative because factor is negative

        return projectedPos + rightDir * offsetDistance;
    }

    /// <summary>
    /// Calculates the final velocity to reach the target, combined with separation forces to avoid stacking.
    /// </summary>
    public Vector3 CalculateVelocity(Vector3 currentPos, Vector3 targetPos, float maxSpeed)
    {
        // Làm mượt lực Separation để quái không bị lắc lư mỗi 0.1s
        _currentSeparationForce = Vector3.Lerp(_currentSeparationForce, _targetSeparationForce, Time.deltaTime * 5f);

        Vector3 desiredVelocity = (targetPos - currentPos).normalized * maxSpeed;
        DesiredDirection = desiredVelocity.normalized;
        Vector3 targetVelocity = desiredVelocity + _currentSeparationForce;
            
        // Limit to max speed
        if (targetVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            targetVelocity = targetVelocity.normalized * maxSpeed;
        }
        
        // Smoothly interpolate velocity to prevent stuttering/jittering when target jumps at corners
        CurrentVelocity = Vector3.Lerp(CurrentVelocity, targetVelocity, Time.deltaTime * 12f);
            
        return CurrentVelocity;
    }

    private void CalculateSeparationForce()
    {
        _targetSeparationForce = Vector3.zero;
        int enemyLayer = LayerMask.GetMask("Enemy");
        
        // Use OverlapSphere to detect nearby enemies for separation
        Collider[] colliders = Physics.OverlapSphere(transform.position, _separationRadius, enemyLayer);
            
        int count = 0;
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                Vector3 awayFromNeighbor = transform.position - col.transform.position;
                // Force increases as they get closer
                _targetSeparationForce += awayFromNeighbor.normalized / (awayFromNeighbor.magnitude + 0.1f);
                count++;
            }
        }

        if (count > 0)
        {
            _targetSeparationForce /= count;
            _targetSeparationForce = _targetSeparationForce.normalized * _separationWeight; 
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Màu xanh dương nhạt để dễ nhìn
        Gizmos.DrawWireSphere(transform.position, _separationRadius);
    }
#endif
}
