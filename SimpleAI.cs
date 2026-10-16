using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class SimpleAI : MonoBehaviour
{
    public Transform player;

    // ─── PUBLIC SETTINGS ────────────────────────────────

    [Header("Combat")]
    public float attackrange = 2f;
    public float detectrange = 8f;
    public float attackdamage = 10f;
    public float attackcooldown = 1f;
    public float fieldOfView = 180f;
    public Transform eyes;
    public LayerMask obstacleMask;

    [Header("Vision")]
    public float sightRange = 25f;    // long raycast, extends past roam and detect ranges
    public float faceAngle = 45f;     // how closely the agent must face the player before investigating

    [Header("Speeds")]
    public float runSpeed = 6f;       // sprint speed when charging to attack

    [Header("Roam")]
    public float roamRadius = 15f;
    public float roamInterval = 4f;

    // ─── PRIVATE STATE ──────────────────────────────────

    private float lastattacktime = 0f;
    private NavMeshAgent agent;
    private Vector3 lastseenposition;
    private Vector3 homeposition;
    private Health playerhealth;
    private bool canseeplayer = false;
    private bool hasSeenPlayer = false;
    private float walkSpeed;

    private float nextRoamTime;
    private Vector3 roamTarget;
    private bool hasRoamTarget = false;

    // ─── STARTUP ────────────────────────────────────────
void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerhealth = player.GetComponent<Health>();
        lastseenposition = player.position;
        homeposition = transform.position;
        walkSpeed = agent.speed;

        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{name} is not on the navmesh! Re-bake or fix the spawn position.");
        }
    }
   // ─── UPDATE LOOP ────────────────────────────────────
    void Update()
    {
        if (player == null) return;

        canseeplayer = Canseeplayer();

        // Record the player's position only while visible
        if (canseeplayer)
        {
            lastseenposition = player.position;
            hasSeenPlayer = true;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackrange && canseeplayer)
        {
            agent.isStopped = true;
            transform.LookAt(player);
            if (Time.time - lastattacktime >= attackcooldown)
            {
                playerhealth.Takedamage(attackdamage);
                lastattacktime = Time.time;
            }
        }
        else
        {
            PlayerInRoamRadius();
        }
    }
    // ─── MAIN BEHAVIOR ──────────────────────────────────
    void PlayerInRoamRadius()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // Seeing the player and facing them: chase, sprinting inside detect range
        if (canseeplayer && IsFacingPlayer())
        {
            agent.speed = (distance < detectrange) ? runSpeed : walkSpeed;
            hasRoamTarget = false;
            agent.isStopped = false;
            agent.SetDestination(player.position);
            return;
        }
        agent.speed = walkSpeed;

        if (distance <= roamRadius)
        {
            SmoothTurnTowardPlayer();

            if (canseeplayer)
            {
                // Visible but not facing yet: hold still while turning
                agent.isStopped = true;
                return;
            }

            // Lost sight: investigate the last known position
            if (hasSeenPlayer && Vector3.Distance(transform.position, lastseenposition) > 1f)
            {
                agent.isStopped = false;
                agent.SetDestination(lastseenposition);
                return;
            }

            // Nothing found: resume roaming
            hasRoamTarget = false;
            Roam();
        }
        else
        {
            nextRoamTime = 0f;
            agent.isStopped = false;
            Roam();
        }
    }
    // ─── HELPERS ────────────────────────────────────────
    void SmoothTurnTowardPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.05f);
        }
    }
    void Roam()
    {
    if (hasRoamTarget)
    {
        if (agent.remainingDistance > agent.stoppingDistance + 0.1f)
            return;
    }

    Vector3 randomDirection = Random.insideUnitSphere * roamRadius + homeposition;
    randomDirection.y = transform.position.y;

    if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
    {
        if (Vector3.Distance(hit.position, transform.position) < 2f)
            return;

        roamTarget = hit.position;
        hasRoamTarget = true;
        nextRoamTime = Time.time + roamInterval;
        agent.isStopped = false;
        agent.SetDestination(roamTarget);
    }
    else
    {
        Debug.LogWarning($"{name}: no navmesh found within {roamRadius}m of {homeposition}", this);
    }
    }
    void OnDrawGizmos()
    {
               Vector3 pos = transform.position;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pos, attackrange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, detectrange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? homeposition : pos, roamRadius);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(pos, sightRange);

        // Field of view edges
        Gizmos.color = Color.yellow;
        Vector3 left = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(pos, left * sightRange);
        Gizmos.DrawRay(pos, right * sightRange);
    }
bool Canseeplayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        if (toPlayer.sqrMagnitude > sightRange * sightRange) return false;
        if (Vector3.Angle(transform.forward, toPlayer) > fieldOfView * 0.5f) return false;

        Vector3 origin = eyes != null ? eyes.position : transform.position + Vector3.up * 1.5f;
        Vector3 target = player.position + Vector3.up * 1f;
        float dist = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, (target - origin).normalized, out RaycastHit hit, dist, obstacleMask)) return false;
        return true;
    }
        bool IsFacingPlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        if (toPlayer.sqrMagnitude < 0.01f) return true;
        return Vector3.Angle(transform.forward, toPlayer) <= faceAngle;
    }

}