using UnityEngine;

[RequireComponent(typeof(ShipMovement))]
public class ShipSteeringComputer : MonoBehaviour
{
    [Header("Steering")]
    [SerializeField] private bool stopMovementWhenIdle = true;
    [SerializeField] private float aimDeadZone = 4f;
    [SerializeField] private float thrustAlignmentAngle = 35f;
    [SerializeField] private float defaultThrust = 1f;

    [Header("Avoidance")]
    [SerializeField] private bool useObstacleAvoidance = true;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float obstacleCheckDistance = 4f;
    [SerializeField] private float obstacleCheckRadius = 0.6f;
    [SerializeField] private float obstacleFeelAngle = 35f;
    [SerializeField] private float obstacleAvoidanceWeight = 1.5f;

    [Header("Velocity Prediction")]
    [SerializeField] private bool useVelocityPrediction = true;
    [SerializeField] private float predictionTime = 0.75f;
    [SerializeField] private float maxPredictionDistance = 8f;
    [SerializeField] private float velocityAvoidanceWeight = 2f;

    [Header("Separation")]
    [SerializeField] private bool useAllySeparation = true;
    [SerializeField] private float separationRadius = 3f;
    [SerializeField] private float separationWeight = 1f;

    [Header("Map Bounds")]
    [SerializeField] private bool useMapBounds;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;
    [SerializeField] private float boundsAvoidanceMargin = 5f;
    [SerializeField] private float boundsAvoidanceWeight = 2f;

    private Ship ship;
    private ShipMovement movement;
    private Rigidbody2D rb;

    private bool hasCommand;
    private Vector2 desiredDirection;
    private float desiredThrust;

    private void Awake()
    {
        ship = GetComponent<Ship>();
        movement = GetComponent<ShipMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (!hasCommand)
        {
            if (stopMovementWhenIdle)
            {
                movement.StopMovementInput();
            }

            return;
        }

        Vector2 steeringDirection = GetAdjustedDirection(desiredDirection);
        ApplySteering(steeringDirection, desiredThrust);
    }

    public void SetDesiredDirection(Vector2 direction)
    {
        SetDesiredDirection(direction, defaultThrust);
    }

    public void SetDesiredDirection(Vector2 direction, float thrust)
    {
        if (direction == Vector2.zero)
        {
            Stop();
            return;
        }

        hasCommand = true;
        desiredDirection = direction.normalized;
        desiredThrust = Mathf.Clamp01(thrust);
    }

    public void MoveToward(Vector2 worldPosition)
    {
        MoveToward(worldPosition, defaultThrust);
    }

    public void MoveToward(Vector2 worldPosition, float thrust)
    {
        SetDesiredDirection(worldPosition - (Vector2)transform.position, thrust);
    }

    public void FleeFrom(Vector2 worldPosition)
    {
        FleeFrom(worldPosition, defaultThrust);
    }

    public void FleeFrom(Vector2 worldPosition, float thrust)
    {
        SetDesiredDirection((Vector2)transform.position - worldPosition, thrust);
    }

    public void KeepRangeFrom(Vector2 worldPosition, float preferredRange, float tolerance)
    {
        float distance = Vector2.Distance(transform.position, worldPosition);

        if (distance > preferredRange + tolerance)
        {
            MoveToward(worldPosition);
            return;
        }

        if (distance < preferredRange - tolerance)
        {
            FleeFrom(worldPosition);
            return;
        }

        Stop();
    }

    public void FaceDirection(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            Stop();
            return;
        }

        hasCommand = true;
        desiredDirection = direction.normalized;
        desiredThrust = 0f;
    }

    public void FacePosition(Vector2 worldPosition)
    {
        FaceDirection(worldPosition - (Vector2)transform.position);
    }

    public void Stop()
    {
        hasCommand = false;
        desiredDirection = Vector2.zero;
        desiredThrust = 0f;
        movement.StopMovementInput();
    }

    private Vector2 GetAdjustedDirection(Vector2 baseDirection)
    {
        Vector2 adjustedDirection = baseDirection;

        if (useObstacleAvoidance)
        {
            adjustedDirection += GetObstacleAvoidanceDirection() * obstacleAvoidanceWeight;
        }

        if (useVelocityPrediction)
        {
            adjustedDirection += GetVelocityAvoidanceDirection() * velocityAvoidanceWeight;
        }

        if (useAllySeparation)
        {
            adjustedDirection += GetSeparationDirection() * separationWeight;
        }

        if (useMapBounds)
        {
            adjustedDirection += GetBoundsAvoidanceDirection() * boundsAvoidanceWeight;
        }

        if (adjustedDirection == Vector2.zero)
        {
            return baseDirection;
        }

        return adjustedDirection.normalized;
    }

    private void ApplySteering(Vector2 direction, float thrust)
    {
        float signedAngle = Vector2.SignedAngle(transform.up, direction);
        float rotationInput = Mathf.Abs(signedAngle) > aimDeadZone ? Mathf.Sign(signedAngle) : 0f;
        float thrustInput = Mathf.Abs(signedAngle) <= thrustAlignmentAngle ? thrust : 0f;

        movement.SetMovementInput(thrustInput, rotationInput);
    }

    private Vector2 GetObstacleAvoidanceDirection()
    {
        Vector2 avoidance = Vector2.zero;
        avoidance += GetFeelAvoidance(transform.up, 1f);
        avoidance += GetFeelAvoidance(RotateVector(transform.up, obstacleFeelAngle), 0.75f);
        avoidance += GetFeelAvoidance(RotateVector(transform.up, -obstacleFeelAngle), 0.75f);

        return avoidance == Vector2.zero ? Vector2.zero : avoidance.normalized;
    }

    private Vector2 GetFeelAvoidance(Vector2 direction, float weight)
    {
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            obstacleCheckRadius,
            direction,
            obstacleCheckDistance,
            obstacleMask);

        if (hit.collider == null)
        {
            return Vector2.zero;
        }

        float proximity = 1f - Mathf.Clamp01(hit.distance / obstacleCheckDistance);
        Vector2 awayFromObstacle = ((Vector2)transform.position - hit.point).normalized;
        Vector2 slideDirection = Vector2.Perpendicular(hit.normal).normalized;

        if (Vector2.Dot(slideDirection, desiredDirection) < 0f)
        {
            slideDirection = -slideDirection;
        }

        return (awayFromObstacle + slideDirection).normalized * proximity * weight;
    }

    public bool HasPredictedCollision()
    {
        return GetPredictedCollisionHit().collider != null;
    }

    public Vector2 GetPredictedCollisionAvoidanceDirection()
    {
        return GetVelocityAvoidanceDirection();
    }

    private Vector2 GetVelocityAvoidanceDirection()
    {
        RaycastHit2D hit = GetPredictedCollisionHit();
        if (hit.collider == null)
        {
            return Vector2.zero;
        }

        float predictionDistance = GetPredictionDistance();
        float proximity = predictionDistance > 0f
            ? 1f - Mathf.Clamp01(hit.distance / predictionDistance)
            : 0f;

        Vector2 awayFromObstacle = ((Vector2)transform.position - hit.point).normalized;
        Vector2 slideDirection = Vector2.Perpendicular(hit.normal).normalized;

        if (Vector2.Dot(slideDirection, rb.linearVelocity) < 0f)
        {
            slideDirection = -slideDirection;
        }

        return (awayFromObstacle + slideDirection).normalized * proximity;
    }

    private RaycastHit2D GetPredictedCollisionHit()
    {
        if (rb == null || rb.linearVelocity.sqrMagnitude <= 0.01f)
        {
            return new RaycastHit2D();
        }

        float predictionDistance = GetPredictionDistance();
        if (predictionDistance <= 0f)
        {
            return new RaycastHit2D();
        }

        return Physics2D.CircleCast(
            transform.position,
            obstacleCheckRadius,
            rb.linearVelocity.normalized,
            predictionDistance,
            obstacleMask);
    }

    private float GetPredictionDistance()
    {
        if (rb == null)
        {
            return 0f;
        }

        return Mathf.Min(rb.linearVelocity.magnitude * predictionTime, maxPredictionDistance);
    }

    private Vector2 GetSeparationDirection()
    {
        if (ship == null)
        {
            return Vector2.zero;
        }

        Ship[] ships = FindObjectsByType<Ship>(FindObjectsSortMode.None);
        Vector2 separation = Vector2.zero;
        int nearbyAllies = 0;

        foreach (Ship otherShip in ships)
        {
            if (otherShip == null || otherShip == ship || otherShip.Team != ship.Team)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, otherShip.transform.position);
            if (distance <= 0f || distance > separationRadius)
            {
                continue;
            }

            Vector2 away = ((Vector2)transform.position - (Vector2)otherShip.transform.position).normalized;
            separation += away * (1f - distance / separationRadius);
            nearbyAllies++;
        }

        if (nearbyAllies == 0)
        {
            return Vector2.zero;
        }

        return separation.normalized;
    }

    private Vector2 GetBoundsAvoidanceDirection()
    {
        Vector2 position = transform.position;
        Vector2 avoidance = Vector2.zero;

        if (position.x < minBounds.x + boundsAvoidanceMargin)
        {
            avoidance += Vector2.right * GetBoundsProximity(position.x, minBounds.x);
        }
        else if (position.x > maxBounds.x - boundsAvoidanceMargin)
        {
            avoidance += Vector2.left * GetBoundsProximity(maxBounds.x, position.x);
        }

        if (position.y < minBounds.y + boundsAvoidanceMargin)
        {
            avoidance += Vector2.up * GetBoundsProximity(position.y, minBounds.y);
        }
        else if (position.y > maxBounds.y - boundsAvoidanceMargin)
        {
            avoidance += Vector2.down * GetBoundsProximity(maxBounds.y, position.y);
        }

        return avoidance == Vector2.zero ? Vector2.zero : avoidance.normalized;
    }

    private float GetBoundsProximity(float position, float edge)
    {
        float distance = Mathf.Abs(position - edge);
        return 1f - Mathf.Clamp01(distance / boundsAvoidanceMargin);
    }

    private static Vector2 RotateVector(Vector2 vector, float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        ).normalized;
    }
}
