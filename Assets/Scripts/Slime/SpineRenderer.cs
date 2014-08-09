using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshFilter))]
public class SpineRenderer : MonoBehaviour {
    [MinValue(0)]
    public float tipLength = 0.2f;
    [MinValue(0)]
    public float spineWidth = 0.1f;
    [MinValue(0.01f)]
    public float spineSegmentLength = 0.2f;
    [MinValue(0)]
    public float wiggle = 0.1f;
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
        _mesh.hideFlags = HideFlags.DontSave;
    }

    public Vector3 getTip() {
        float startDist = spineLengthPercent * (spinePath.Count - 1);
        Vector2 tipCenter = spinePath.getSmoothPoint(startDist);
        return tipCenter;
    }

    public void Update() {
        if (spinePath == null) {
            if (Application.isPlaying) {
                return;
            }
            spinePath = new Path();
            Vector2Int start = transform.position;
            spinePath.addNodeToEnd(start);
            start += Vector2Int.right;  spinePath.addNodeToEnd(start);
            start += Vector2Int.right; spinePath.addNodeToEnd(start);
            start += Vector2Int.up; spinePath.addNodeToEnd(start);
            start += Vector2Int.right; spinePath.addNodeToEnd(start);
            start += Vector2Int.up; spinePath.addNodeToEnd(start);
            start += Vector2Int.up; spinePath.addNodeToEnd(start);
        }

        _mesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        float startDist = spineLengthPercent * (spinePath.Count - 1);

        Vector3 offset = Vector3.back - transform.position;

        Vector2 tipCenter = spinePath.getSmoothPoint(startDist);
        Vector2 tipDir = spinePath.getSmoothDirection(startDist);
        Vector2 tipExt = Vector3.Cross(tipDir, Vector3.forward).normalized;

        vertices.Add((Vector3)(tipCenter + tipExt * spineWidth / 2.0f + tipDir * tipLength) + offset);
        vertices.Add((Vector3)(tipCenter - tipExt * spineWidth / 2.0f + tipDir * tipLength) + offset);
        vertices.Add((Vector3)(tipCenter - tipExt * spineWidth / 2.0f) + offset);
        vertices.Add((Vector3)(tipCenter + tipExt * spineWidth / 2.0f) + offset);

        uv.Add(new Vector2(0, 0));
        uv.Add(new Vector2(0.5f, 0));
        uv.Add(new Vector2(0.5f, 1));
        uv.Add(new Vector2(0, 1));

        tris.Add(0);
        tris.Add(2);
        tris.Add(1);

        tris.Add(0);
        tris.Add(3);
        tris.Add(2);

        float currDist = startDist;

        if (currDist > 0.0f) {
            while (true) {
                Vector2 point = spinePath.getSmoothPoint(currDist);
                Vector2 ext = Vector3.Cross(Vector3.forward, spinePath.getSmoothDirection(currDist)).normalized;
                point += wiggle * ext * Mathf.Sin(startDist - currDist);

                Vector3 p0 = point + ext * spineWidth / 2.0f;
                Vector3 p1 = point - ext * spineWidth / 2.0f;

                vertices.Add(p0 + offset);
                vertices.Add(p1 + offset);

                float segmentUV = (startDist - currDist) / spineSegmentLength;
                uv.Add(new Vector2(0.5f, segmentUV));
                uv.Add(new Vector2(1.0f, segmentUV));

                if (vertices.Count > 6) {
                    tris.Add(vertices.Count - 1);
                    tris.Add(vertices.Count - 2);
                    tris.Add(vertices.Count - 3);

                    tris.Add(vertices.Count - 4);
                    tris.Add(vertices.Count - 3);
                    tris.Add(vertices.Count - 2);
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
}
