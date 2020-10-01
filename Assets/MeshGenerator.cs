using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    Color[] colors;

    public int xSize;
    public int zSize;

    public float xOffset;
    public float zOffset;

    public float width;
    public float length;

    public float height;
    public float terraceHeight;
    public float scale1;
    public float scale2;
    public float scale3;
    public float octave1;
    public float octave2;
    public float octave3;
    public float waterHeightRatio;

    public MeshFilter meshFilter;
    public Material material;
    public Gradient gradient;

    // Start is called before the first frame update
    void Start() {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh = mesh;
        Vector3 position = new Vector3(-xSize * width / 2f, 0f, -zSize * length / 2f);
        meshFilter.transform.Translate(position, Space.World);

        // material.SetFloat("TerraceHeight", terraceHeight);
        material.SetFloat("Height", height);

        // StartCoroutine(IEnumerable CreateShapeCoroutine());
        GenerateLand();
        UpdateMesh();
    }
    
    // private void Update() {
    //     UpdateMesh();
    // }

    void GenerateLand() {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int z = 0, i = 0; z < zSize + 1; z++) {
            for (int x = 0; x < xSize + 1; x++, i++) {
                float y = 1f;
                if (terraceHeight > 0) {
                    y = getHeightTerraced(x, z);
                } else {
                    y = getHeightOctaves(x, z);
                }
                vertices[i] = new Vector3(x * width, y, z * length);
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++, vert++) {
            for (int x = 0; x < xSize; x++, vert++, tris += 6) {
                triangles[tris] = vert;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
            }
            // UpdateMesh();
            // yield return 0;
        }

        colors = new Color[vertices.Length];

        for (int z = 0, i = 0; z < zSize + 1; z++) {
            for (int x = 0; x < xSize + 1; x++, i++) {
                // uvs[i] = new Vector2((float) x / xSize, (float) z / zSize);
                float colorHeight = vertices[i].y / height;
                colors[i] = gradient.Evaluate(colorHeight);
                // Debug.Log("GRADIENT: " + colorHeight + " VERTEX: " + i + " COLOR: " + colors[i].ToString());
            }
        }
    }

    float getHeightOctaves(float x, float z) {
        float y = Mathf.Max(height * (
            octave1 * Mathf.PerlinNoise((x + xOffset) * scale1, (z + zOffset) * scale1) +
            octave2 * Mathf.PerlinNoise((x + xOffset) * scale2, (z + zOffset) * scale2) +
            octave3 * Mathf.PerlinNoise((x + xOffset) * scale3, (z + zOffset) * scale3)
            ), waterHeightRatio * height);
        return y;
    }

    float getHeightTerraced(float x, float z) {
        float y = Mathf.Floor(Mathf.Max(height * (
            octave1 * Mathf.PerlinNoise((x + xOffset) * scale1, (z + zOffset) * scale1) +
            octave2 * Mathf.PerlinNoise((x + xOffset) * scale2, (z + zOffset) * scale2) +
            octave3 * Mathf.PerlinNoise((x + xOffset) * scale3, (z + zOffset) * scale3)
            ), waterHeightRatio * height) / terraceHeight) * terraceHeight;
        return y;
    }

    void UpdateMesh() {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
    }

    // private void OnDrawGizmos() {
    //     if (vertices == null) {
    //         return;
    //     }
    //     Vector3 drawPosition = meshFilter.transform.position;
    //     for (int i = 0; i < vertices.Length; i++) {
    //         Gizmos.color = mesh.colors[i];
    //         Gizmos.DrawSphere(drawPosition + vertices[i], .1f);
    //     }
    // }
}
