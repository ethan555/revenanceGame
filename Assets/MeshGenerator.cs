using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public static MeshGenerator instance;
    public Navmesh navmesh;
    public Transform[] navigators;
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
    private Stack<Vector3>[] paths;
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

        navmesh = new Navmesh(vertices, xSize, zSize, width, length);
        // int testVertex = (9 * xSize / 10) * (9 * zSize / 10);
        Vector3 testVertex = navmesh.findClosestVertex(new Vector3(24.4f, 1f, 24.4f));
        // Debug.Log("Closest Vertex: " + testVertex.ToString());
        paths = new Stack<Vector3>[navigators.Length];
        int index = 0;
        foreach (Transform navigator in navigators)
        {
            navigator.position = testVertex + transform.position;
            paths[index] = navmesh.getSmartPath(testVertex, vertices[0]);
            index ++;
        }
        // path = navmesh.getPath(testVertex, vertices[0]);
    }
    
    private void Update() {
        // UpdateMesh();

        int index = 0;
        if (Input.GetKeyDown("f")) {
            // path = navmesh.getRandomPath(navigator.position - transform.position, xSize, zSize, width, length);
            foreach (Transform navigator in navigators)
            {
                paths[index] = navmesh.getRandomPath(navigator.position - transform.position, xSize, zSize, width, length);
                index ++;
            }
        }
        
        index = 0;
        timer += Time.deltaTime;
        if (timer > .25f) {
            timer = 0f;
            foreach (Transform navigator in navigators) {
                if (paths[index] == null) {return;}
                if (paths[index].Count > 0) {
                    Vector3 pos = paths[index].Pop();
                    navigator.position = pos + transform.position;
                }
                index ++;
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
        int index = 0;
        foreach (Transform navigator in navigators)
        {
            if (paths == null || paths.Length == 0 || paths[index] == null) {return;}
            Vector3 drawPosition = meshFilter.transform.position;
            Vector3[] pathList = paths[index].ToArray();
            Gizmos.color = Color.red;
            for (int i = 0; i < pathList.Length; i++) {
                // Gizmos.DrawSphere(drawPosition + pathList[i], .1f);
                if (i > 0) {
                    Gizmos.DrawLine(drawPosition + pathList[i-1], drawPosition + pathList[i]);
                }
                if (i > pathList.Length - 20) {
                    Gizmos.color = Color.green;
                }
            }
            index++;
        }
        
    }
}

public class Navmesh {
    private class Cell {
        private Vector3 vertex;
        private (int, int) position;
        public float cost {get; set;}
        public Cell parent {get; set;}
        public List<Cell> neighborsHighRes {get; set;}
        public List<Cell> neighborsLowRes {get; set;}
        public Cell(Vector3 vertex_, (int, int) position_) {
            vertex = vertex_;
            position = position_;
            neighborsHighRes = null;
            neighborsLowRes = null;
        }

        public Vector3 getVertex() {
            return vertex;
        }

        public (int, int) getPosition() {
            return position;
        }
    }
    private float getCost(Cell a, Cell b) {
        return Mathf.Max(b.getVertex().y - a.getVertex().y, 0f) * heightCost + 1f;
    }

    private Dictionary<(int, int), Cell> cells;
    private HashSet<Cell> visited;
    private Stack<Cell> toVisit;
    private float prediction;
    private float epsilon;
    public int resolutionFactor = 8;
    public int highResolutionFactor = 2;
    private int xSize, zSize;
    private float width, length;
    // Search Parameters
    private int searchLimit = 1000;
    private int intFactor = 1000;
    private float epsilonFudge = .05f;
    private int xFudge = 500; // for resolution factor 10
    private int xFudgeHighRes = 250; // for resolution factor 5
    private int zFudge = 500; // for resolution factor 10
    private int zFudgeHighRes = 250; // for resolution factor 5
    private float heightCost = 3f;
    private int highResPathLength = 10;

    public Navmesh(Vector3[] vertices, int xSize_, int zSize_, float width_, float length_) {
        xSize = xSize_;
        zSize = zSize_;
        width = width_;
        length = length_;

        xFudge = (int) Mathf.Round(width * resolutionFactor * intFactor);
        xFudgeHighRes = (int) Mathf.Round(width * highResolutionFactor * intFactor);

        zFudge = (int) Mathf.Round(length * resolutionFactor * intFactor);
        zFudgeHighRes = (int) Mathf.Round(length * highResolutionFactor * intFactor);

        cells = new Dictionary<(int, int), Cell>();
        // int index = 0;
        for (int i = 0; i < vertices.Length; i += zSize * highResolutionFactor) {
            for (int j = 0; j < zSize; j += highResolutionFactor) {//, index++) {
                Vector3 vertex = vertices[i + j];
                (int, int) position = vector3ToTuple(vertex);
                cells.Add(position, new Cell(vertex, position));
                // if (index < 10) {
                    // Debug.Log("Position: " + position.ToString() + " Vertex: " + vertex.ToString());
                // }
            }
        }
    }

    public (int, int) vector2ToTuple(Vector2 vertex) {
        return ((int)Mathf.Round(vertex.x * intFactor), (int)Mathf.Round(vertex.y * intFactor));
    }

    public (int, int) vector3ToTuple(Vector3 vertex) {
        return ((int)Mathf.Round(vertex.x * intFactor), (int)Mathf.Round(vertex.z * intFactor));
    }

    public Vector3 findClosestVertex(Vector3 vertex) {
        float resolutionFixX = ((float)xFudge) / ((float)intFactor);
        float resolutionFixZ = ((float)zFudge) / ((float)intFactor);
        return new Vector3(
                Mathf.Round(Mathf.Clamp(vertex.x, 0, xSize * width) / resolutionFixX) * resolutionFixX,
                vertex.y,
                Mathf.Round(Mathf.Clamp(vertex.z, 0, zSize * length) / resolutionFixZ) * resolutionFixZ
            );
    }

    private float heuristic(Cell a, Cell b) {
        Vector3 aVertex = a.getVertex();
        Vector3 bVertex = b.getVertex();
        float heuristicDistance = Vector3.Distance(aVertex, bVertex);
        float verticalDistance = 0f;
        return heuristicDistance + verticalDistance;
    }

    private List<Cell> getCellNeighbors((int, int) vertex) {
        if (!cells.ContainsKey(vertex)) {return null;}
        Cell baseCell = cells[vertex];
        if (baseCell.neighborsLowRes != null) {
            return baseCell.neighborsLowRes;
        }
        List<Cell> neighbors = new List<Cell>();
        // int index = 0;
        for (int i = -xFudge; i <= xFudge; i += xFudge) {
            for (int j = -zFudge; j <= zFudge; j += zFudge) {
                if (i == 0 && j == 0) continue;
                (int, int) neighbor = (vertex.Item1 + i, vertex.Item2 + j);
                if (!cells.ContainsKey(neighbor)) {
                    // index ++;
                    continue;
                }
                // neighbors[index++] = cells[neighbor];
                neighbors.Add(cells[neighbor]);
            }
        }
        baseCell.neighborsLowRes = neighbors;
        return neighbors;
    }

    private List<Cell> getCellNeighborsHighRes((int, int) vertex) {
        if (!cells.ContainsKey(vertex)) {return null;}
        Cell baseCell = cells[vertex];
        if (baseCell.neighborsHighRes != null) {
            return baseCell.neighborsHighRes;
        }
        List<Cell> neighbors = new List<Cell>();
        // int index = 0;
        for (int i = -xFudgeHighRes; i <= xFudgeHighRes; i += xFudgeHighRes) {
            for (int j = -zFudgeHighRes; j <= zFudgeHighRes; j += zFudgeHighRes) {
                if (i == 0 && j == 0) continue;
                (int, int) neighbor = (vertex.Item1 + i, vertex.Item2 + j);
                if (!cells.ContainsKey(neighbor)) {
                    // index ++;
                    continue;
                }
                // neighbors[index++] = cells[neighbor];
                neighbors.Add(cells[neighbor]);
            }
        }
        baseCell.neighborsHighRes = neighbors;
        return neighbors;
    }

    public Stack<Vector3> getSmartPath(Vector3 start_, Vector3 end_) {
        Vector3[] path = getPath(findClosestVertex(start_), findClosestVertex(end_)).ToArray();
        // Debug.Log("PATH: " + path.Length.ToString());
        if (path.Length < highResPathLength) {
            return getPath(start_, end_, true);
        }
        Vector3 highResStart = path[path.Length - highResPathLength];
        Vector3[] highResPath = getPath(highResStart, end_, true).ToArray();
        // Debug.Log("HighResPath: " + highResPath.Length.ToString());

        Stack<Vector3> smartPath = new Stack<Vector3>();
        for (int i = highResPath.Length - 1; i >= 0; i--) {
            smartPath.Push(highResPath[i]);
        }
        for (int i = path.Length - (highResPathLength + 1); i >= 0; i--) {
            smartPath.Push(path[i]);
        }
        // Debug.Log("SmartPath: " + smartPath.Count.ToString());
        return smartPath;
    }

    // public Stack<Vector3> getPath(Vector3 start_, Vector3 end_) {
    public Stack<Vector3> getPath(Vector3 start_, Vector3 end_, bool highRes = false) {
        (int, int) startPos = vector3ToTuple(start_);
        (int, int) endPos = vector3ToTuple(end_);
        Cell start = cells.ContainsKey(startPos) ? cells[startPos] : null;
        Cell end = cells.ContainsKey(endPos) ? cells[endPos] : null;
        // Debug.Log("StartPOS: " + startPos.ToString() + " ENDPOS: " + endPos.ToString());
        // Debug.Log("Start: " + (start == null).ToString() + " END: " + (end == null).ToString());
        if (start == null || end == null) {
            return null;
        }

        Stack<Vector3> path = new Stack<Vector3>();
        bool finished = false;
        int limit = 0;
        visited = new HashSet<Cell>();
        toVisit = new Stack<Cell>();
        toVisit.Push(start);
        prediction = heuristic(start, end);
        epsilon = float.PositiveInfinity;
        start.cost = 0;
        start.parent = null;
        while (!finished && limit < searchLimit) {
            limit ++;
            Cell currentCell = toVisit.Pop();
            visited.Add(currentCell);
            if (currentCell.Equals(end)) {
                // Found it! Build the path
                finished = true;
                path = buildPath(end);
                continue;
            }
            List<Cell> neighbors = highRes ? getCellNeighborsHighRes(currentCell.getPosition()) : getCellNeighbors(currentCell.getPosition());
            float bestCost = float.PositiveInfinity;
            Cell bestNeighbor = null;
            foreach (Cell neighbor in neighbors) {
                // Find best neighbor
                if (neighbor == null || visited.Contains(neighbor)) continue;
                float potential = getCost(currentCell, neighbor) + heuristic(neighbor, end);
                if (potential <= prediction) {
                    if (potential < bestCost) {
                        bestNeighbor = neighbor;
                        bestCost = potential;
                    }
                } else {
                    if (potential < epsilon) {
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
                if (!currentCell.Equals(start)) {
                    toVisit.Push(currentCell.parent);
                } else {
                    // Restart pathfinding, IDA*
                    visited = new HashSet<Cell>();
                    toVisit = new Stack<Cell>();
                    toVisit.Push(start);
                    if (epsilon != float.PositiveInfinity) {
                        prediction = epsilon;
                    } else {
                        break;
                    }
                    epsilon = float.PositiveInfinity;
                    continue;
                }
            }
        }
        // Debug.Log(limit);
        return path;
    }

    public Stack<Vector3> getRandomPath(Vector3 start_, int xSize, int zSize, float width, float length) {
        (int, int) startPos = vector3ToTuple(findClosestVertex(start_));
        Cell start = cells.ContainsKey(startPos) ? cells[startPos] : null;

        (int, int) endPos = vector2ToTuple(new Vector2(
            Random.Range(0, (xSize - 2) / resolutionFactor)*resolutionFactor * width,
            Random.Range(0, (zSize - 2) / resolutionFactor)*resolutionFactor * length
        ));
        // Debug.Log(endPos);
        Cell end = cells.ContainsKey(endPos) ? cells[endPos] : null;
        if (end == null) {
        } else if (end.Equals(start)) {
            endPos = vector2ToTuple(new Vector2(
                ((xSize - 1) / resolutionFactor)*resolutionFactor * width,
                ((zSize - 1) / resolutionFactor)*resolutionFactor * length
            ));
        }

        return getSmartPath(start_, end.getVertex());
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
