using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshFilter))]
public class SpineRenderer : MonoBehaviour {
    public Transform endP;
    [MinValue(0)]
    public float tipLength = 0.2f;
    [MinValue(0)]
    public float spineWidth = 0.1f;
    [MinValue(0.01f)]
    public float spineSegmentLength = 0.2f;
    [Range(0, 1)]
    public float spineLengthPercent = 1.0f;
    [Range(0.01f, 1.0f)]
    public float resolution = 0.1f;
    public Path spinePath = null;

    private MeshFilter _meshFilter;
    private Mesh _mesh;

    void Awake() {
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = new Mesh();
        _mesh.name = "Spine Mesh";
        _meshFilter.mesh = _mesh;
    }

    public void Update() {
        spinePath = Astar.findPath(transform.position, endP.position);

        _mesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        float startDist = spineLengthPercent * (spinePath.Count - 1);

        Vector3 offset = Vector3.back - transform.position;

        Vector2 tipCenter = getPointOnPath(startDist);
        Vector2 tipExt = getExtents(startDist);
        Vector2 tipDir = Vector3.Cross(tipExt, Vector3.forward).normalized;

        vertices.Add((Vector3)(tipCenter + tipExt * spineWidth / 2.0f) + offset);
        vertices.Add((Vector3)(tipCenter - tipExt * spineWidth / 2.0f) + offset);
        vertices.Add((Vector3)(tipCenter - tipExt * spineWidth / 2.0f - tipDir * tipLength) + offset);
        vertices.Add((Vector3)(tipCenter + tipExt * spineWidth / 2.0f - tipDir * tipLength) + offset);

        uv.Add(new Vector2(0, 0));
        uv.Add(new Vector2(0.25f, 0));
        uv.Add(new Vector2(0.25f, 1));
        uv.Add(new Vector2(0, 1));

        tris.Add(0);
        tris.Add(1);
        tris.Add(2);

        tris.Add(0);
        tris.Add(2);
        tris.Add(3);

        float currDist = startDist;

        if (currDist > 0.0f) {
            while (true) {
                Vector2 point = getPointOnPath(currDist);
                Vector2 ext = getExtents(currDist);
                point += 0.1f * ext * Mathf.Sin(startDist - currDist);

                Vector3 p0 = point + ext * spineWidth / 2.0f;
                Vector3 p1 = point - ext * spineWidth / 2.0f;

                vertices.Add(p0 + offset);
                vertices.Add(p1 + offset);

                float segmentUV = (startDist - currDist) / spineSegmentLength;
                uv.Add(new Vector2(0.5f, segmentUV));
                uv.Add(new Vector2(0.75f, segmentUV));

                if (vertices.Count > 6) {
                    tris.Add(vertices.Count - 1);
                    tris.Add(vertices.Count - 3);
                    tris.Add(vertices.Count - 2);

                    tris.Add(vertices.Count - 4);
                    tris.Add(vertices.Count - 2);
                    tris.Add(vertices.Count - 3);
                }

                if (currDist == 0.0f) {
                    break;
                }
                currDist = Mathf.Max(0, currDist - resolution);
            }
        }

        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = tris.ToArray();
        _mesh.uv = uv.ToArray();
        _mesh.RecalculateNormals();
    }

    private Vector2 getExtents(float d) {
        Vector2 point = getPointOnPath(d);
        Vector2 dir = d == 0.0f ? getPointOnPath(d + 0.1f) - point : point - getPointOnPath(d - 0.1f);
        return Vector3.Cross(dir, Vector3.forward).normalized;
    }

    private Vector2 getPointOnPath(float distance) {
        int currentNode = Mathf.RoundToInt(distance);
        int previousNode = currentNode - 1;
        int nextNode = currentNode + 1;

        Vector2 p1 = spinePath[currentNode];
        Vector2 p2 = Vector2.zero, p0 = Vector2.zero;
        if (previousNode != -1) {
            p0 = p1 + (Tilemap.getWorldLocation(spinePath[previousNode]) - p1) / 2.0f;
        }
        if (nextNode != spinePath.Count) {
            p2 = p1 + (Tilemap.getWorldLocation(spinePath[nextNode]) - p1) / 2.0f;
        }
        if (previousNode == -1) {
            p0 = p1 + (p1 - p2);
        }
        if (nextNode == spinePath.Count) {
            p2 = p1 + (p1 - p0);
        }

        float t = distance - currentNode + 0.5f;

        return (1 - t) * ((1 - t) * p0 + t * p1) + t * ((1 - t) * p1 + t * p2);
    }

    public void OnDrawGizmosSelected() {
        Vector2 prev = Vector2.zero;
        bool set = false;
        Gizmos.color = Color.blue;
        for (float d = 0; d < spinePath.Count - 1.0f; d += 0.1f) {
            Vector2 p = getPointOnPath(d);
            if (set) {
                Gizmos.DrawLine(p, prev);
            }
            set = true;
            prev = p;
        }
    }
}
