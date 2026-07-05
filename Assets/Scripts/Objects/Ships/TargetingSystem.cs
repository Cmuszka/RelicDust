using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    [SerializeField] private float cursorSelectRadius = 0.75f;

    public Ship currentTarget;
    public MissileProjectile currentMissileTarget;
    public float targetingRange = 20f;

    private Ship ownerShip;
    private Vector2 aimDirection;

    public Vector2 AimDirection => aimDirection;

    private void Awake()
    {
        ownerShip = GetComponent<Ship>();
    }

    public void SetTarget(Ship target)
    {
        if (target != null && !IsValidTarget(target))
        {
            return;
        }

        currentTarget = target;
    }

    public void SetTargetIgnoringRange(Ship target)
    {
        if (target != null && !IsValidTargetIgnoringRange(target))
        {
            return;
        }

        currentTarget = target;
    }

    public bool HasTarget()
    {
        return currentTarget != null && IsValidTarget(currentTarget);
    }

    public bool HasAssignedTarget()
    {
        return currentTarget != null && IsValidTargetIgnoringRange(currentTarget);
    }

    public bool HasMissileTarget()
    {
        return currentMissileTarget != null && IsHostileMissile(currentMissileTarget);
    }

    public Vector2 CurrentTargetPosition
    {
        get
        {
            if (currentMissileTarget != null)
            {
                return currentMissileTarget.transform.position;
            }

            if (currentTarget != null && !currentTarget.IsDestroyed)
            {
                return currentTarget.transform.position;
            }

            return transform.position;
        }
    }

    public void ClearTarget()
    {
        currentTarget = null;
        currentMissileTarget = null;
    }

    public void SetAimDirection(Vector2 newAimDirection)
    {
        if (newAimDirection == Vector2.zero)
        {
            return;
        }

        aimDirection = newAimDirection.normalized;
    }

    public void CycleTarget()
    {
        currentMissileTarget = null;

        Ship[] ships = FindObjectsByType<Ship>(FindObjectsSortMode.None);
        Ship bestTarget = null;
        float bestScore = float.MaxValue;
        bool passedCurrentTarget = currentTarget == null;

        foreach (Ship candidate in ships)
        {
            if (!IsValidTarget(candidate))
            {
                continue;
            }

            if (!passedCurrentTarget)
            {
                if (candidate == currentTarget)
                {
                    passedCurrentTarget = true;
                }

                continue;
            }

            float distance = Vector2.Distance(transform.position, candidate.transform.position);
            if (distance < bestScore)
            {
                bestTarget = candidate;
                bestScore = distance;
            }
        }

        if (bestTarget == null)
        {
            foreach (Ship candidate in ships)
            {
                if (!IsValidTarget(candidate))
                {
                    continue;
                }

                float distance = Vector2.Distance(transform.position, candidate.transform.position);
                if (distance < bestScore)
                {
                    bestTarget = candidate;
                    bestScore = distance;
                }
            }
        }

        currentTarget = bestTarget;

        if (currentTarget == null)
        {
            currentMissileTarget = FindNearestHostileMissile();
        }
    }

    public void CycleMissileTarget()
    {
        currentTarget = null;
        currentMissileTarget = FindNearestHostileMissile();
    }

    public void SelectTargetUnderCursor(Camera camera)
    {
        if (camera == null)
        {
            return;
        }

        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 cursorPosition = mouseWorldPosition;
        Collider2D[] hits = Physics2D.OverlapCircleAll(cursorPosition, cursorSelectRadius);

        Ship bestShipTarget = null;
        MissileProjectile bestMissileTarget = null;
        float bestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            MissileProjectile missile = hit.GetComponentInParent<MissileProjectile>();
            if (IsHostileMissile(missile))
            {
                float missileDistance = Vector2.Distance(cursorPosition, missile.transform.position);
                if (missileDistance < bestDistance)
                {
                    bestMissileTarget = missile;
                    bestShipTarget = null;
                    bestDistance = missileDistance;
                }

                continue;
            }

            Ship candidate = hit.GetComponentInParent<Ship>();
            if (!IsValidTarget(candidate))
            {
                continue;
            }

            float distance = Vector2.Distance(cursorPosition, candidate.transform.position);
            if (distance < bestDistance)
            {
                bestShipTarget = candidate;
                bestMissileTarget = null;
                bestDistance = distance;
            }
        }

        if (bestMissileTarget != null)
        {
            currentMissileTarget = bestMissileTarget;
            currentTarget = null;
        }
        else if (bestShipTarget != null)
        {
            currentTarget = bestShipTarget;
            currentMissileTarget = null;
        }
    }

    public bool IsAimAlignedWithTarget(float allowedAngle)
    {
        return IsDirectionAlignedWithTarget(aimDirection, allowedAngle);
    }

    public bool IsDirectionAlignedWithTarget(Vector2 direction, float allowedAngle)
    {
        if (!HasTarget() && !HasMissileTarget() || direction == Vector2.zero)
        {
            return false;
        }

        Vector2 directionToTarget = GetDirectionToCurrentTarget();
        return Vector2.Angle(direction.normalized, directionToTarget) <= allowedAngle;
    }

    public MissileProjectile FindNearestHostileMissile()
    {
        return FindNearestHostileMissile(targetingRange);
    }

    public MissileProjectile FindNearestHostileMissile(float maxRange)
    {
        MissileProjectile[] missiles = FindObjectsByType<MissileProjectile>(FindObjectsSortMode.None);
        MissileProjectile bestMissile = null;
        float bestDistance = float.MaxValue;

        foreach (MissileProjectile missile in missiles)
        {
            if (!IsHostileMissile(missile))
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, missile.transform.position);
            if (distance > maxRange)
            {
                continue;
            }

            if (distance < bestDistance)
            {
                bestMissile = missile;
                bestDistance = distance;
            }
        }

        return bestMissile;
    }

    public Ship FindNearestHostileShip(float maxRange)
    {
        Ship[] ships = FindObjectsByType<Ship>(FindObjectsSortMode.None);
        Ship bestShip = null;
        float bestDistance = float.MaxValue;

        foreach (Ship ship in ships)
        {
            if (!IsValidTargetIgnoringRange(ship))
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, ship.transform.position);
            if (distance > maxRange)
            {
                continue;
            }

            if (distance < bestDistance)
            {
                bestShip = ship;
                bestDistance = distance;
            }
        }

        return bestShip;
    }

    public Vector2 GetDirectionToCurrentTarget()
    {
        if (!HasTarget() && !HasMissileTarget())
        {
            return aimDirection;
        }

        return (CurrentTargetPosition - (Vector2)transform.position).normalized;
    }

    private bool IsValidTarget(Ship candidate)
    {
        if (!IsValidTargetIgnoringRange(candidate))
        {
            return false;
        }

        if (Vector2.Distance(transform.position, candidate.transform.position) > targetingRange)
        {
            return false;
        }

        return true;
    }

    private bool IsValidTargetIgnoringRange(Ship candidate)
    {
        if (candidate == null || candidate == ownerShip)
        {
            return false;
        }

        if (candidate.IsDestroyed || candidate.health != null && candidate.health.IsDead)
        {
            return false;
        }

        if (ownerShip == null)
        {
            return true;
        }

        if (candidate.Team == ShipTeam.Neutral || candidate.Team == ownerShip.Team)
        {
            return false;
        }

        return true;
    }

    private bool IsHostileMissile(MissileProjectile missile)
    {
        if (missile == null)
        {
            return false;
        }

        if (Vector2.Distance(transform.position, missile.transform.position) > targetingRange)
        {
            return false;
        }

        if (ownerShip == null)
        {
            return true;
        }

        return missile.Team != ShipTeam.Neutral && missile.Team != ownerShip.Team;
    }
}
