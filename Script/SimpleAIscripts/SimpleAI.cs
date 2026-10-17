using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleAI : MonoBehaviour
{
    public Transform player;

    [Header("Combat")]
    public float attackrange = 2f;
    public float detectrange = 8f;
    public float attackdamage = 10f;
    public float attackcooldown = 1f;

    [Header("Vision")]
    public Transform eyes;
    public float fieldOfView = 180f;   // full cone angle
    public float sightRange = 25f;     // long-range sight, past roam and detect ranges
    public float faceAngle = 45f;      // how directly the agent must face the player to chase
    public float turnSpeed = 180f;     // degrees per second while turning toward the player
    public LayerMask obstacleMask;

    [Header("Speed")]
    public float runSpeed = 6f;

    [Header("Roam")]
    public float roamRadius = 15f;
    public float roamInterval = 4f;
    public float minRoamDistance = 2f;

    // ─── PRIVATE STATE ──────────────────────────────────

    private NavMeshAgent agent;
    private Health playerHealth;
    private Vector3 homePosition;
    private Vector3 lastSeenPosition;
    private bool hasSeenPlayer = false;
    private bool canSeePlayer = false;
    private float walkSpeed;
    private float lastAttackTime = -999f;
    private float nextRoamTime = 0f;
    private bool hasRoamTarget = false;
    private bool investigating = false;
    private Vector3 investigateTarget;
    private float investigateStart;

    [Header("Investigate")]
    public float investigateTime = 8f;   // give up after this many seconds
    public List<Transform> players = new List<Transform>();
    public float retargetInterval = 0.3f;   // how often to re-pick the nearest visible player
    private float nextRetargetTime = 0f;
    // ─── STARTUP ────────────────────────────────────────

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        walkSpeed = agent.speed;
        homePosition = transform.position;
        lastSeenPosition = transform.position;

        if (player != null)
            playerHealth = player.GetComponent<Health>();

        if (player != null && !players.Contains(player))
            players.Add(player);

        foreach (Health h in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (h.isPlayer && !players.Contains(h.transform))
                players.Add(h.transform);
        }

        if (!agent.isOnNavMesh)
            Debug.LogError($"{name} is not on the NavMesh. Re-bake or fix the spawn position.", this);
    }

    // ─── UPDATE LOOP ────────────────────────────────────

    void Update()
    {
        if (Time.time >= nextRetargetTime)
        {
            nextRetargetTime = Time.time + retargetInterval;
            UpdateTarget();
        }
        if (player == null) return;

        canSeePlayer = CanSeePlayer();

        if (canSeePlayer)
        {
            lastSeenPosition = player.position;
            hasSeenPlayer = true;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackrange && canSeePlayer)
        {
            Attack();
        }
        else
        {
            Behave(distance);
        }
    }

    // ─── BEHAVIOUR ──────────────────────────────────────

    void Attack()
    {
        agent.isStopped = true;
        FaceFlat(player.position, 1f);

        if (Time.time - lastAttackTime >= attackcooldown && playerHealth != null)
        {
            playerHealth.Takedamage(attackdamage);
            lastAttackTime = Time.time;
        }
    }

    void Behave(float distance)
{
    // 1. Sees the player and faces them: chase (highest priority)
    if (canSeePlayer && IsFacingPlayer())
    {
        Chase(distance);
        return;
    }

    agent.speed = walkSpeed;

    // 2. Outside roam radius: roam
    if (distance > roamRadius)
    {
        investigating = false;
        Roam();
        return;
    }

    // 3. Inside roam radius: turn toward the player
    TurnTowardPlayer();

    if (canSeePlayer)
    {
        // Visible but not facing yet: hold still while turning
        investigating = false;
        agent.isStopped = true;
        return;
    }

    // 4. Lost sight: go to the last seen spot
    if (hasSeenPlayer && Investigate())
        return;

    // 5. Nothing found (or finished investigating): roam
    Roam();
}

    void Chase(float distance)
    {
    investigating = false;
    agent.speed = distance <= detectrange ? runSpeed : walkSpeed;
    agent.isStopped = false;
    agent.SetDestination(player.position);
    hasRoamTarget = false;
    }

    // ─── ROAMING ────────────────────────────────────────

    void Roam()
    {
        agent.speed = walkSpeed;

        bool arrived = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;
        bool intervalPassed = Time.time >= nextRoamTime;

        // Keep walking to the current target unless it's time for a new one
        if (hasRoamTarget && !arrived && !intervalPassed)
            return;

        // Try a few random points, since some may land off the NavMesh
        for (int i = 0; i < 10; i++)
        {
            Vector3 random = homePosition + Random.insideUnitSphere * roamRadius;

            if (NavMesh.SamplePosition(random, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
            {
                if (Vector3.Distance(hit.position, transform.position) < minRoamDistance)
                    continue;

                agent.isStopped = false;
                agent.SetDestination(hit.position);
                hasRoamTarget = true;
                nextRoamTime = Time.time + roamInterval;
                return;
            }
        }

        Debug.LogWarning($"{name}: could not find a roam point within {roamRadius}m of home.", this);
    }

    // ─── HELPERS ────────────────────────────────────────

    void TurnTowardPlayer()
    {
        FaceFlat(player.position, turnSpeed * Time.deltaTime, true);
    }

    // Rotates on the Y axis only. If smooth is false, snaps instantly.
    void FaceFlat(Vector3 target, float maxDegrees, bool smooth = false)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = smooth
            ? Quaternion.RotateTowards(transform.rotation, targetRot, maxDegrees)
            : targetRot;
    }

    bool IsFacingPlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.01f) return true;

        return Vector3.Angle(transform.forward, toPlayer) <= faceAngle;
    }

    bool CanSeePlayer()
{
    return CanSee(player);
}

bool CanSee(Transform target)
{
    if (target == null) return false;

    Vector3 toTarget = target.position - transform.position;

    if (toTarget.sqrMagnitude > sightRange * sightRange) return false;
    if (Vector3.Angle(transform.forward, toTarget) > fieldOfView * 0.5f) return false;

    Vector3 origin = eyes != null ? eyes.position : transform.position + Vector3.up * 1.5f;
    Vector3 aim = target.position + Vector3.up * 1f;
    Vector3 dir = aim - origin;
    float dist = dir.magnitude;

    // Ray hits an obstacle first: blocked, unless that obstacle belongs to the target
    if (Physics.Raycast(origin, dir / dist, out RaycastHit hit, dist, obstacleMask, QueryTriggerInteraction.Ignore))
    {
        return hit.transform.root == target.root;
    }

    return true;
}

void UpdateTarget()
{
    Transform nearestVisible = null;
    float nearestVisibleDist = float.MaxValue;
    Transform nearestAny = null;
    float nearestAnyDist = float.MaxValue;

    foreach (Transform p in players)
    {
        if (p == null) continue;

        float d = Vector3.Distance(transform.position, p.position);

        if (d < nearestAnyDist)
        {
            nearestAnyDist = d;
            nearestAny = p;
        }

        if (d < nearestVisibleDist && CanSee(p))
        {
            nearestVisibleDist = d;
            nearestVisible = p;
        }
    }

    // Prefer the nearest visible player; otherwise the nearest one overall
    Transform chosen = nearestVisible != null ? nearestVisible : nearestAny;

    if (chosen == null || chosen == player) return;

    player = chosen;
    playerHealth = player.GetComponent<Health>();
}
bool Investigate()
{
    if (!investigating)
    {
        // Snap the last seen point onto the NavMesh so it's reachable
        if (!NavMesh.SamplePosition(lastSeenPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            hasSeenPlayer = false;
            return false;
        }

        investigating = true;
        investigateTarget = hit.position;
        investigateStart = Time.time;
        hasRoamTarget = false;

        agent.isStopped = false;
        agent.SetDestination(investigateTarget);
    }

    bool arrived = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f;
    bool timedOut = Time.time - investigateStart > investigateTime;

    if (arrived || timedOut)
    {
        // Done checking this spot: forget it and go back to roaming
        investigating = false;
        hasSeenPlayer = false;
        return false;
    }

    return true;
}
    // ─── GIZMOS ─────────────────────────────────────────

    void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        bool playing = Application.isPlaying;

        // Range rings, drawn flat on the ground so they're easier to read
        Gizmos.color = Color.blue;
        DrawRing(pos, attackrange);

        Gizmos.color = Color.red;
        DrawRing(pos, detectrange);

        Gizmos.color = Color.green;
        DrawRing(playing ? homePosition : pos, roamRadius);

        Gizmos.color = Color.white;
        DrawRing(pos, sightRange);

        // Field of view: outer arc plus both edges
        Gizmos.color = Color.yellow;
        float half = fieldOfView * 0.5f;
        int steps = 32;
        Vector3 prev = pos + Quaternion.Euler(0, -half, 0) * transform.forward * sightRange;
        Gizmos.DrawLine(pos, prev);
        for (int i = 1; i <= steps; i++)
        {
            float angle = -half + (fieldOfView * i / steps);
            Vector3 next = pos + Quaternion.Euler(0, angle, 0) * transform.forward * sightRange;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
        Gizmos.DrawLine(pos, prev);

        // ─── LIVE STATE (Play mode only) ───

        if (!playing) return;

        Vector3 eye = eyes != null ? eyes.position : pos + Vector3.up * 1.5f;

        // Line of sight: green when the enemy can see the player, gray when not
        if (player != null)
        {
            Gizmos.color = canSeePlayer ? Color.green : Color.gray;
            Gizmos.DrawLine(eye, player.position + Vector3.up * 1f);
        }

        // Last seen position
        if (hasSeenPlayer)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastSeenPosition + Vector3.up * 0.5f, 0.5f);
        }

        // Current investigate target
        if (investigating)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(pos, investigateTarget);
            Gizmos.DrawSphere(investigateTarget + Vector3.up * 0.5f, 0.3f);
        }
    }
    void DrawRing(Vector3 center, float radius)
    {
        int segments = 48;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float a = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}