using UnityEngine;

[RequireComponent(typeof(SimpleAI))]
public class SimpleAIVisual : MonoBehaviour
{
    // ─── PUBLIC SETTINGS ────────────────────────────────

    [Header("Layout")]
    public float groundOffset = 0.05f;   // lifts shapes off the floor to avoid flicker
    public int segments = 48;            // smoothness of circles and arcs

    [Header("Colors (alpha = fill opacity)")]
    public Color attackColor = new Color(0f, 0.4f, 1f, 0.15f);
    public Color detectColor = new Color(1f, 0f, 0f, 0.12f);
    public Color sightColor = new Color(1f, 1f, 1f, 0.08f);
    public Color fovColor = new Color(1f, 1f, 0f, 0.20f);
    public Color roamColor = new Color(0f, 1f, 0f, 0.10f);

    // ─── PRIVATE STATE ──────────────────────────────────

    private SimpleAI ai;

    // ─── STARTUP ────────────────────────────────────────

    void Start()
    {
        ai = GetComponent<SimpleAI>();

        Vector3 offset = new Vector3(0f, groundOffset, 0f);

        // These follow the enemy as it moves and turns
        MakeShape("AttackRange", BuildSector(ai.attackrange, 360f), attackColor, transform, offset);
        MakeShape("DetectRange", BuildSector(ai.detectrange, 360f), detectColor, transform, offset);
        MakeShape("SightRange", BuildSector(ai.sightRange, 360f), sightColor, transform, offset);
        MakeShape("FieldOfView", BuildSector(ai.sightRange, ai.fieldOfView), fovColor, transform, offset);

        // Roam ring stays at the spawn point (home), so it is not parented to the enemy
        Vector3 home = transform.position + offset;
        MakeShape("RoamRange", BuildSector(ai.roamRadius, 360f), roamColor, null, home);
    }

    // ─── HELPERS ────────────────────────────────────────

    // Builds a flat fan. 360 = full circle. Otherwise a pie slice centered on +Z.
    Mesh BuildSector(float radius, float fullAngle)
    {
        Vector3[] verts = new Vector3[segments + 2];
        int[] tris = new int[segments * 3];

        verts[0] = Vector3.zero;
        float start = -fullAngle * 0.5f;

        for (int i = 0; i <= segments; i++)
        {
            float a = start + fullAngle * i / segments;
            verts[i + 1] = Quaternion.Euler(0f, a, 0f) * Vector3.forward * radius;
        }

        for (int i = 0; i < segments; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i + 2;
        }

        Mesh mesh = new Mesh();
        mesh.name = "SightShape";
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    GameObject MakeShape(string shapeName, Mesh mesh, Color color, Transform parent, Vector3 position)
    {
        GameObject go = new GameObject(shapeName);
        go.transform.SetParent(parent, false);
        go.transform.position = parent == null ? position : parent.TransformPoint(position);
        go.transform.rotation = parent == null ? Quaternion.identity : parent.rotation;

        go.AddComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        Shader shader = Shader.Find("Sprites/Default");   // transparent, works in built-in and URP
        if (shader == null)
        {
            Debug.LogError($"{name}: Sprites/Default shader not found.", this);
            return go;
        }
        mr.sharedMaterial = new Material(shader) { color = color };
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        return go;
    }
}