using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public static MeshGenerator instance;
    public Navmesh navmesh;
    public Transform navigator;
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
    private Stack<Vector3> path;
    private float timer = 0f;

    // Start is called before the first frame update
    void Start() {
        instance = this;
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

        navmesh = new Navmesh(vertices, xSize, zSize);
        // int testVertex = (9 * xSize / 10) * (9 * zSize / 10);
        Vector3 testVertex = new Vector3(24.5f, 1f, 24.5f);
        navigator.position = testVertex + transform.position;
        path = navmesh.getPath(testVertex, vertices[0]);
    }
    
    private void Update() {
        // UpdateMesh();

        if (Input.GetKeyDown("f")) {
            path = navmesh.getRandomPath(navigator.position - transform.position, xSize, zSize, width, length);
        }
        
        if (path == null) {return;}
        timer += Time.deltaTime;
        if (timer > .25f) {
            if (path.Count > 0) {
                timer = 0f;
                Vector3 pos = path.Pop();
                navigator.position = pos + transform.position;
            }
        }
    }

    void GenerateLand() {
        vertices = new Vector3[xSize * zSize];//(xSize + 1) * (zSize + 1)];

        for (int z = 0, i = 0; z < zSize; z++) {// + 1; z++) {
            for (int x = 0; x < xSize; x++, i++) {// + 1; x++, i++) {
                float y = 1f;
                if (terraceHeight > 0) {
                    y = getHeightTerraced(x, z);
                } else {
                    y = getHeightOctaves(x, z);
                }
                vertices[i] = new Vector3(x * width, y, z * length);
            }
        }

        triangles = new int[(xSize - 1) * (zSize - 1) * 6];

        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize - 1; z++, vert++) {
            for (int x = 0; x < xSize - 1; x++, vert++, tris += 6) {
                triangles[tris] = vert;
                triangles[tris + 1] = vert + xSize;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize;
                triangles[tris + 5] = vert + xSize + 1;
            }
            // UpdateMesh();
            // yield return 0;
        }

        colors = new Color[vertices.Length];

        for (int z = 0, i = 0; z < zSize; z++) {// + 1; z++) {
            for (int x = 0; x < xSize; x++, i++) {// + 1; x++, i++) {
                // uvs[i] = new Vector2((float) x / xSize, (float) z / zSize);
                float colorHeight = vertices[i].y / height;
                colors[i] = gradient.Evaluate(colorHeight);
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

    private void OnDrawGizmos() {
        // if (vertices == null) {
        //     return;
        // }
        // Vector3 drawPosition = meshFilter.transform.position;
        // for (int i = 0; i < vertices.Length; i++) {
        //     Gizmos.color = mesh.colors[i];
        //     Gizmos.DrawSphere(drawPosition + vertices[i], .1f);
        // }
        if (path == null) {return;}
        Vector3 drawPosition = meshFilter.transform.position;
        Vector3[] pathList = path.ToArray();
        for (int i = 0; i < pathList.Length; i++) {
            Gizmos.color = mesh.colors[i];
            Gizmos.DrawSphere(drawPosition + pathList[i], .1f);
        }
        
    }
}

public class Navmesh {
    private class Cell {
        private Vector3 vertex;
        private (int, int) position;
        public float cost {get; set;}
        public Cell parent {get; set;}
        public Cell(Vector3 vertex_, (int, int) position_) {
            vertex = vertex_;
            position = position_;
        }

        public Vector3 getVertex() {
            return vertex;
        }

        public (int, int) getPosition() {
            return position;
        }

        public float getCost(Cell a) {
            return Mathf.Max(a.vertex.y - vertex.y, 0f) * 3f + 1f;
        }
    }

    private Dictionary<(int, int), Cell> cells;
    private HashSet<Cell> visited;
    private Stack<Cell> toVisit;
    private float prediction;
    private float epsilon;

    public Navmesh(Vector3[] vertices, int xSize, int zSize) {
        cells = new Dictionary<(int, int), Cell>();
        // foreach (Vector3 vertex in vertices) {
        for (int i = 0; i < vertices.Length; i += zSize * 10) {
            for (int j = 0; j < zSize; j += 10) {
                Vector3 vertex = vertices[i + j];
                (int, int) position = vector3ToTuple(vertex);
                cells.Add(position, new Cell(vertex, position));
            }
        }
    }

    public (int, int) vector2ToTuple(Vector2 vertex) {
        return ((int)Mathf.Round(vertex.x * 1000), (int)Mathf.Round(vertex.y * 1000));
    }

    public (int, int) vector3ToTuple(Vector3 vertex) {
        return ((int)Mathf.Round(vertex.x * 1000), (int)Mathf.Round(vertex.z * 1000));
    }

    private float heuristic(Cell a, Cell b) {
        Vector3 aVertex = a.getVertex();
        Vector3 bVertex = b.getVertex();
        float manhattanDistance = Mathf.Abs(aVertex.x - bVertex.x) + Mathf.Abs(aVertex.z - bVertex.z);
        float verticalDistance = Mathf.Abs(aVertex.y - bVertex.y);
        return manhattanDistance + verticalDistance;
    }

    private Cell[] getCellNeighbors((int, int) vertex) {
        if (!cells.ContainsKey(vertex)) {return null;}
        Cell[] neighbors = new Cell[8];
        int index = 0;
        for (int i = -fudge; i <= fudge; i += fudge) {
            for (int j = -fudge; j <= fudge; j += fudge) {
                if (i == 0 && j == 0) continue;
                (int, int) neighbor = (vertex.Item1 + i, vertex.Item2 + j);
                if (!cells.ContainsKey(neighbor)) {
                    index ++;
                    continue;
                }
                neighbors[index++] = cells[neighbor];
            }
        }
        return neighbors;
    }

    private int searchLimit = 1000;
    private float epsilonFudge = .05f;
    private int fudge = 500;
    // public Stack<Vector3> getPath(Vector3 start_, Vector3 end_) {
    public Stack<Vector3> getPath(Vector3 start_, Vector3 end_) {
        (int, int) startPos = vector3ToTuple(start_);
        (int, int) endPos = vector3ToTuple(end_);
        Cell start = cells.ContainsKey(startPos) ? cells[startPos] : null;
        Cell end = cells.ContainsKey(endPos) ? cells[endPos] : null;
        if (start == null || end == null) return null;

        Stack<Vector3> path = new Stack<Vector3>();
        bool finished = false;
        int limit = 0;
        prediction = heuristic(start, end) + 5;
        epsilon = float.PositiveInfinity;
        visited = new HashSet<Cell>();
        toVisit = new Stack<Cell>();
        toVisit.Push(start);
        bool started = false;
        start.cost = 0;
        start.parent = null;
        while (!finished && limit < searchLimit) {
            Cell currentCell = toVisit.Pop();
            if (currentCell == null) {
            }
            if (start == null) {
            }
            if (currentCell.Equals(start) && started) {
                // Restart pathfinding, IDA*
                visited = new HashSet<Cell>();
                toVisit = new Stack<Cell>();
                toVisit.Push(start);
                started = false;
                prediction = epsilon;
                epsilon = float.PositiveInfinity;
                continue;
            }
            if (currentCell.Equals(end)) {
                // Found it! Build the path
                finished = true;
                path = buildPath(end);
                continue;
            }
            Cell[] neighbors = getCellNeighbors(currentCell.getPosition());
            float bestCost = float.PositiveInfinity;
            Cell bestNeighbor = null;
            foreach (Cell neighbor in neighbors) {
                // Find best neighbor
                if (neighbor == null || visited.Contains(neighbor)) continue;
                float potential = currentCell.getCost(neighbor) + heuristic(neighbor, end);
                if (potential <= prediction) {
                    if (potential < bestCost) {
                        bestNeighbor = neighbor;
                        bestCost = potential;
                    }
                } else {
                    if (potential < epsilon - epsilonFudge) {
                        epsilon = potential;
                    }
                }
            }
            if (bestNeighbor != null) {
                bestNeighbor.cost = bestCost;
                bestNeighbor.parent = currentCell;
                toVisit.Push(bestNeighbor);
            } else {
                // Backtrack
                toVisit.Push(currentCell.parent);
            }
            limit ++;
        }
        return path;
    }

    public Stack<Vector3> getRandomPath(Vector3 start_, int xSize, int zSize, float width, float length) {
        (int, int) startPos = vector3ToTuple(start_);
        Cell start = cells.ContainsKey(startPos) ? cells[startPos] : null;

        (int, int) endPos = vector2ToTuple(new Vector2(
            Random.Range(0, (xSize - 2) / 10)*10 * width,
            Random.Range(0, (zSize - 2) / 10)*10 * length
        ));
        Cell end = cells.ContainsKey(endPos) ? cells[endPos] : null;
        if (end == null) {
        } else if (end.Equals(start)) {
            endPos = vector2ToTuple(new Vector2(
                ((xSize - 1) / 10)*10 * width,
                ((zSize - 1) / 10)*10 * length
            ));
        }

        return getPath(start_, end.getVertex());
    }

    private Stack<Vector3> buildPath(Cell end) {
        Cell cell = end;
        Stack<Vector3> path = new Stack<Vector3>();
        while (cell != null) {
            path.Push(cell.getVertex());
            cell = cell.parent;
        }
        return path;
    }
}
