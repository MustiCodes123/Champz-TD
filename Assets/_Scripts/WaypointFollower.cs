using UnityEngine;

/// <summary>
/// Simple waypoint follower for enemies
/// Follows a list of waypoints and reaches the tree at the end
/// </summary>
public class WaypointFollower : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 2f;
    
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool isStunned = false;
    private float speedModifier = 1f;

    public void Initialize(Transform[] waypointPath)
    {
        waypoints = waypointPath;
        currentWaypointIndex = 0;
        isMoving = true;

        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }
    }

    void Update()
    {
        if (!isMoving || isStunned || waypoints == null || waypoints.Length == 0)
            return;

        MoveTowardsCurrentWaypoint();
    }

    private void MoveTowardsCurrentWaypoint()
    {
        if (currentWaypointIndex >= waypoints.Length)
        {
            ReachedEnd();
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        if (targetWaypoint == null)
        {
            currentWaypointIndex++;
            return;
        }

        // Move towards waypoint
        float actualSpeed = baseSpeed * speedModifier;
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetWaypoint.position,
            actualSpeed * Time.deltaTime
        );

        // Check if reached waypoint
        float distanceToWaypoint = Vector2.Distance(transform.position, targetWaypoint.position);
        if (distanceToWaypoint < 0.1f)
        {
            currentWaypointIndex++;
        }
    }

    private void ReachedEnd()
    {
        isMoving = false;

        // Reduce player lives
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReduceLives(1);
        }

        // Destroy enemy
        Destroy(gameObject);
    }

    public void SetStunned(bool stunned)
    {
        isStunned = stunned;
    }

    public void SetSpeedModifier(float modifier)
    {
        speedModifier = Mathf.Clamp(modifier, 0.1f, 2f); // Clamp between 10% and 200% speed
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    public Vector2 GetMovementDirection()
    {
        if (currentWaypointIndex >= waypoints.Length || waypoints == null)
            return Vector2.zero;

        Vector2 direction = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        return direction;
    }

    public bool IsMoving() => isMoving && !isStunned;
}
