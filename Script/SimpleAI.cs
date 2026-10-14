using UnityEditor;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.AI;
public class SimpleAI : MonoBehaviour
{
    public Transform player;
    public float attackrange = 2, detectrange = 8f;
    public float attackdamage = 10f, attackcooldown = 1f;
    public float fieldOfView = 120f;
    public Transform eyes;
    public LayerMask obstacleMask;

    private float lastattacktime = 0f;
    private NavMeshAgent agent;
    private Vector3 lastseenposition;
    private Health playerhealth;
    private bool canseeplayer = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerhealth=player.GetComponent<Health>();
        lastseenposition = player.position;
    }
    // Update is called once per frame
    void Update()
    {
        if(player == null) return;

        canseeplayer = Canseeplayer();

        if(canseeplayer)
        lastseenposition = player.position;
        
        float distance = Vector3.Distance(transform.position,player.position);
        if(distance <= attackrange && canseeplayer)
        {
            agent.isStopped = true;
            transform.LookAt(player);
            if(Time.time - lastattacktime >= attackcooldown)
            {
                playerhealth.Takedamage(attackdamage);
                lastattacktime = Time.time;
            }
        }
        else if(canseeplayer && distance <= detectrange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            transform.LookAt(player);
        }
        else if(!canseeplayer && Vector3.Distance(transform.position, lastseenposition) > 1f)
        {
            agent.isStopped = false;
            NavMeshHit hit;
            agent.SetDestination(lastseenposition);
            if(NavMesh.SamplePosition(lastseenposition, out hit, 2f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackrange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectrange);
    }
    bool Canseeplayer()
    {
        float angle = Vector3.Angle(transform.forward, player.position - transform.position);
        if(angle > fieldOfView / 2f) return false;

        if(Vector3.Distance(transform.position, player.position) > detectrange) return false;

        Vector3 origin = eyes != null ? eyes.position : transform.position + Vector3.up * 1.5f;
        Vector3 target = player.position + Vector3.up * 1f;
        float dist = Vector3.Distance(origin,target);

        if(Physics.Raycast(origin, (target - origin).normalized, out RaycastHit hit, dist, obstacleMask)) return false; return true;
        
    }
}