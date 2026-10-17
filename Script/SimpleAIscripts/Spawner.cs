using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class DummySpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject dummyPrefab;
    public Transform player;
    public List<Transform> players = new List<Transform>();
    
    [Header("Spawn Settings")]
    public int maxAlive = 5;
    public float spawnInterval = 3f;
    public float spawnRadius = 15f;
    public bool spawnOnStart = true;

    [Header("Optional Spawn Points (leave empty to use this object's position)")]
    public Transform[] spawnPoints;

    readonly List<GameObject> alive = new List<GameObject>();
    float timer;

    void Start()
    {
        timer = spawnOnStart ? 0f : spawnInterval;
    }

    void Update()
    {
        alive.RemoveAll(d => d == null);

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = spawnInterval;
            if (alive.Count < maxAlive)
                TrySpawn();
        }
    }

    void TrySpawn()
    {
        if (dummyPrefab == null) return;

        if (!GetSpawnPosition(out Vector3 pos)) return;

        Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject dummy = Instantiate(dummyPrefab, pos, rot);
        SimpleAI ai = dummy.GetComponent<SimpleAI>();
        if (ai != null)
        {
            ai.player = players.Count > 0 ? players[0] : player;
            ai.players = new List<Transform>(players);
        }
        alive.Add(dummy);
    }

    bool GetSpawnPosition(out Vector3 result)
    {
        Vector3 origin = transform.position;
        if (spawnPoints != null && spawnPoints.Length > 0)
            origin = spawnPoints[Random.Range(0, spawnPoints.Length)].position;

        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        Vector3 candidate = origin + new Vector3(circle.x, 2f, circle.y);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
}