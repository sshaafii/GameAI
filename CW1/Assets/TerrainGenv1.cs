using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System.Diagnostics;
using Debug = UnityEngine.Debug;


public class TerrainGenv1 : MonoBehaviour
{

    
    Mesh mesh;
    public GameObject[] objects;
    [SerializeField] private AnimationCurve heightCurve;
    private Vector3[] vertices;
    private int[] triangles;
    //  public GameObject playerPrefab;
    // public GameObject enemyPrefab1;
    //public GameObject enemyPrefab2;

    private float startTime;
    private float endTime;

    public bool displayGridGizmos;




    [SerializeField]
    private Node[,] nodeGrid;

    private Color[] colors;
   

    private float minTerrainheight;
    private float maxTerrainheight;

    public Dictionary<Vector2, Node> NodeMap;

    public int xSize;
    public int zSize;
    public Renderer meshRenderer;
   // public int K;
    // public int iterations;

    public float scale;
    public int octaves;
    public float lacunarity;
    public int K;
    public int iterations;

    public int seed;


    private float lastNoiseHeight;

    void Awake()
    {
        // Use this method if you havn't filled out the properties in the inspector
        // SetNullProperties(); 

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        meshRenderer = GetComponent<Renderer>();

        Stopwatch sw = new Stopwatch();

        // Start the stopwatch
        sw.Start();






        //startTime = Time.realtimeSinceStartup;
        CreateNewMap();


        sw.Stop();

        // Print the elapsed time in milliseconds
        Debug.Log(" terrain gen Execution took: " + sw.ElapsedMilliseconds + " ms");


        //Debug.Log(maxTerrainheight);
        CreateNodeGrid();
        // SpawnPlayerAndEnemies();

        AssetSpawner1();
        









    }

    private void Update()
    {
        CreateNodeGrid();
    }




    private void SetNullProperties()
    {
        if (xSize <= 0) xSize = 50;
        if (zSize <= 0) zSize = 50;
        if (octaves <= 0) octaves = 5;
        if (lacunarity <= 0) lacunarity = 2;
        if (scale <= 0) scale = 50;
    }

    public void CreateNewMap()
    {
        CreateMeshShape();
       
    }

    private void CreateMeshShape()
    {
        // Creates seed
        Vector2[] octaveOffsets = GetOffsetSeed();

        if (scale <= 0) scale = 0.0001f;

        // Create vertices
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)

        {


            for (int x = 0; x <= xSize; x++)
            {
                // Set height of vertices
                float noiseHeight = GenerateNoiseHeight(z, x, octaveOffsets);
                SetMinMaxHeights(noiseHeight);
                vertices[i] = new Vector3(x, noiseHeight, z);

                i++;
            }

        }


        CreateTriangles();
    }

    private void CreateTriangles()
    {
        // Need 6 vertices to create a square (2 triangles)
        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;
        // Go to next row
        for (int z = 0; z < xSize; z++)
        {
            // fill row
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        UpdateMesh();
    }

    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        GetComponent<MeshCollider>().sharedMesh = mesh;

        //MapEmbellishments();
       // CreateNodeGrid();

        GetComponent<NavMeshSurface>().BuildNavMesh();
        //DisplayNodeInfo();
       // MapEmbellishments();
        //SpawnPlayerAndEnemies();





    }


  /*  private void SpawnPlayerAndEnemies()
    {
        // Spawn player at a random walkable position
       Vector3 playerSpawnPosition = GetRandomWalkablePosition();
       Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);

        // Spawn enemy 1 at a random walkable position
        Vector3 enemy1SpawnPosition = GetRandomWalkablePosition();
        Instantiate(enemyPrefab1, enemy1SpawnPosition, Quaternion.identity);

        // Spawn enemy 2 at a random walkable position
        Vector3 enemy2SpawnPosition = GetRandomWalkablePosition();
        Instantiate(enemyPrefab2, enemy2SpawnPosition, Quaternion.identity);
    }
  */

    public  Vector3 GetRandomWalkablePosition(float yOffset = 2f)
    {
        Node randomNode;
        int randomX, randomZ;

        do
        {
            randomX = Random.Range(0, xSize);
            randomZ = Random.Range(0, zSize);
            randomNode = nodeGrid[randomX, randomZ];
        }
        while (!randomNode.walkable);

        return new Vector3(randomNode.worldPos.x, randomNode.worldPos.y + yOffset, randomNode.worldPos.z);
    }


    public Vector3 GetRandomWalkablePosition1()
    {
        Node randomNode;
        int randomX, randomZ;

        do
        {
            randomX = Random.Range(0, xSize);
            randomZ = Random.Range(0, zSize);
            randomNode = nodeGrid[randomX, randomZ];
        }
        while (!randomNode.walkable);

        return new Vector3(randomNode.worldPos.x, randomNode.worldPos.y, randomNode.worldPos.z);


    }
    private void AssetSpawner1()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            // find actual position of vertices in the game
            Vector3 worldPt = transform.TransformPoint(mesh.vertices[i]);
            var noiseHeight = worldPt.y;
            // Stop generation if height difference between 2 vertices is too steep
           
                // min height for object generation
                if (noiseHeight < 0.4)
                {


                    // Chance to generate
                    if (Random.Range(1, 12) == 1)
                    {
                        if (objects == null || objects.Length == 0)
                        {
                            return;

                        }






                        // GameObject objectToSpawn = objects[Random.Range(0, objects.Length)];
                        var spawnAboveTerrainBy = noiseHeight * 3;
                        // Instantiate(objectToSpawn, new Vector3(mesh.vertices[i].x, spawnAboveTerrainBy, mesh.vertices[i].z), Quaternion.identity);


                        if (IsPositionAccessible(new Vector3(mesh.vertices[i].x, spawnAboveTerrainBy, mesh.vertices[i].z), 3f))
                        {
                            GameObject objectToSpawn1 = objects[Random.Range(0, objects.Length)];
                            Instantiate(objectToSpawn1, new Vector3(mesh.vertices[i].x, spawnAboveTerrainBy, mesh.vertices[i].z), Quaternion.identity);
                        }


                    }
                }
            
            lastNoiseHeight = noiseHeight;
        }
    }



    private void AssetSpawner()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            // find actual position of vertices in the game
            Vector3 worldPt = transform.TransformPoint(mesh.vertices[i]);
            var noiseHeight = worldPt.y;
            // Stop generation if height difference between 2 vertices is too steep
            if (System.Math.Abs(lastNoiseHeight - worldPt.y) > 0.2)
            {
                // min height for object generation
                if (noiseHeight < 0.6)
                {


                    // Chance to generate
                    if (Random.Range(1, 6) == 1)
                    {
                        if (objects == null || objects.Length == 0)
                        {
                            return;

                        }






                       // GameObject objectToSpawn = objects[Random.Range(0, objects.Length)];
                         var spawnAboveTerrainBy = noiseHeight * 3;
                        // Instantiate(objectToSpawn, new Vector3(mesh.vertices[i].x, spawnAboveTerrainBy, mesh.vertices[i].z), Quaternion.identity);


                        if (IsPositionAccessible(new Vector3(mesh.vertices[i].x, spawnAboveTerrainBy, mesh.vertices[i].z), 1f))
                        {
                            GameObject objectToSpawn1 = objects[Random.Range(0, objects.Length)];
                            Instantiate(objectToSpawn1, new Vector3(mesh.vertices[i].x, spawnAboveTerrainBy, mesh.vertices[i].z), Quaternion.identity);
                        }


                    }
                }
            }
            lastNoiseHeight = noiseHeight;
        }
    }


    private bool IsPositionAccessible(Vector3 position, float maxDistance)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas);
    }



    private void CreateNodeGrid()
    {
        nodeGrid = new Node[xSize + 1, zSize + 1];

        for (int x = 0; x <= xSize; x++)
        {
            for (int z = 0; z <= zSize; z++)
            {
                // Get the world position of the vertex
                Vector3 worldPt = transform.TransformPoint(vertices[z * (xSize + 1) + x]);
                int movementPenalty = 0;

                // Determine if the node is walkable by checking its height
                bool walkable = true; // Customize this threshold value as needed

                if (worldPt.y > 4f) {
                    movementPenalty = 1;
                
                }

                // Create a new node and assign it to the grid
                nodeGrid[x, z] = new Node(walkable, worldPt, x, z,movementPenalty);
                
            }
        }
    }

    private void DisplayNodeInfo()
    {
        for (int x = 0; x <= xSize; x++)
        {
            for (int z = 0; z <= zSize; z++)
            {
                Node node = nodeGrid[x, z];
                Debug.Log($"Node at position {node.worldPos} is {(node.walkable ? "walkable" : "not walkable")}");
            }
        }
    }



    private Vector2[] GetOffsetSeed()
    {
        // seed = Random.Range(0, 1000);
        

        // changes area of map
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int o = 0; o < octaves; o++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[o] = new Vector2(offsetX, offsetY);
        }
        return octaveOffsets;
    }

    private float GenerateNoiseHeight(int z, int x, Vector2[] octaveOffsets)
    {
        float amplitude = 20;
        float frequency = 1;
        float persistence = 0.5f;
        float noiseHeight = 0;

        // loop over octaves
        for (int y = 0; y < octaves; y++)
        {
            float mapZ = z / scale * frequency + octaveOffsets[y].y;
            float mapX = x / scale * frequency + octaveOffsets[y].x;



            //The *2-1 is to create a flat floor level
            Vector3 point = new Vector3(mapX, mapZ, 0);
            float perlinValue2 = (PerlinNoise.PerlinNoise3D(point, 1f));
           
            float perlinValue = (Mathf.PerlinNoise(mapZ, mapX)) * 2 - 1;
            noiseHeight += heightCurve.Evaluate(perlinValue2) * amplitude;
            frequency *= lacunarity;
            amplitude *= persistence;
        }
        return noiseHeight;
    }

    private void SetMinMaxHeights(float noiseHeight)
    {
        // Set min and max height of map for color gradient
        if (noiseHeight > maxTerrainheight)
            maxTerrainheight = noiseHeight;
        if (noiseHeight < minTerrainheight)
            minTerrainheight = noiseHeight;
    }


    public Node GetClosestNode(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x);
        int z = Mathf.RoundToInt(worldPosition.z);

        x = Mathf.Clamp(x, 0, xSize);
        z = Mathf.Clamp(z, 0, zSize);

        return nodeGrid[x, z];
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        int gridX = nodeGrid.GetLength(0);
        int gridY = nodeGrid.GetLength(1);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Skip the current node (0, 0)
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridX && checkY >= 0 && checkY < gridY)
                {
                    neighbours.Add(nodeGrid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

   public List<Node> path;
    void OnDrawGizmos()
    {
        // Only draw the Gizmos if the nodeGrid has been initialized
        if (nodeGrid != null )
        {
            // Loop through the nodeGrid
            for (int x = 0; x <= xSize; x++)
            {
                for (int z = 0; z <= zSize; z++)
                {
                    Node node = nodeGrid[x, z];

                    // Choose a color based on the walkability of the node
                    Gizmos.color = node.walkable ? Color.green : Color.red;

                    if (node.worldPos.y > 4f) {
                        Gizmos.color = Color.cyan;
                    }
                    if (path != null) {
                        if (path.Contains(node)) {
                            Gizmos.color = Color.black;

                        }
                    
                    }

                    // Draw a sphere for the node at its world position
                    Gizmos.DrawSphere(node.worldPos, 0.5f);
                }
            }
        }
    }

}
