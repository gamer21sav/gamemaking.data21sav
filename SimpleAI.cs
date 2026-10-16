using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class SimpleAI : MonoBehaviour
{
    public Transform player;
// 🟩 **PUBLIC SETTINGS** (Visible in Inspector)
[Header("Combat")]
public float attackrange = 2, detectrange = 8f;
public float attackdamage = 10f, attackcooldown = 1f;
public float fieldOfView = 180f;
public Transform eyes;
public LayerMask obstacleMask;

    [Header("Vision")]                       // NEW
    public float sightRange = 25f;          // long raycast — goes past roam AND detect
    public float faceAngle = 45f;           // how closely it must face you before investigating

    [Header("Speeds")]                       // NEW
    public float runSpeed = 6f;  

// 🟦 **ROAM SETTINGS**
[Header("Roam")]
public float roamRadius = 15f;
public float roamInterval = 4f;

// 🟧 **PRIVATE STATE**
private float lastattacktime = 0f;
private NavMeshAgent agent;
private Vector3 lastseenposition, homeposition;
private Health playerhealth;
private bool canseeplayer = false;

private float nextRoamTime;
private Vector3 roamTarget;
private bool hasRoamTarget = false;
private bool hasSeenPlayer = false;

private float walkSpeed;
// 🟨 **STARTUP**
void Start()
{
    agent = GetComponent<NavMeshAgent>();
    playerhealth = player.GetComponent<Health>();
    lastseenposition = player.position;
    homeposition = transform.position;
    walkSpeed = agent.speed;
    if (!agent.isOnNavMesh)
    {
        Debug.LogError($"{name} is Not on the navmesh! Re-bake or fix spawn position.");
    }
}
// 🟣 **UPDATE LOOP**
    void Update()
    {
    if (player == null) return;

    canseeplayer = Canseeplayer();

    // ✅ FIX: Only update lastseenposition when player is visible
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
     void PlayerInRoamRadius()
    {
    float distance = Vector3.Distance(transform.position, player.position);
    if(canseeplayer && IsFacingPlayer())
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
            // Chase player directly
            agent.isStopped = true;
            return;
        }
        
        
            // Player invisible — walk to last seen position
            if (hasSeenPlayer && Vector3.Distance(transform.position, lastseenposition) > 1f)
            {
                agent.isStopped = false;
                agent.SetDestination(lastseenposition);
                return;
            }

            // Arrived — resume roaming
            hasRoamTarget = false;
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackrange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectrange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, roamRadius);

        Gizmos.color = Color.black;                     // long sight range
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.yellow;                    // facing direction
        Gizmos.DrawRay(transform.position, transform.forward * sightRange);
    }
    bool Canseeplayer()
    {
    Vector3 toPlayer = player.position - transform.position;
    if (toPlayer.sqrMagnitude > sightRange * sightRange) return false;
    if(Vector3.Angle(transform.forward, toPlayer) > fieldOfView * 0.5f) return false;

        Vector3 origin = eyes != null ? eyes.position : transform.position + Vector3.up * 1.5f;
        Vector3 target = player.position + Vector3.up * 1f;
        float dist = Vector3.Distance(origin,target);
        if(Physics.Raycast(origin, (target - origin).normalized, out RaycastHit hit, dist, obstacleMask)) return false; return true;
    }
        bool IsFacingPlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        if (toPlayer.sqrMagnitude < 0.01f) return true;
        return Vector3.Angle(transform.forward, toPlayer) <= faceAngle;
    }
}