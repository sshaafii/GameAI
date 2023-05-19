using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Kmeans2 : MonoBehaviour
{
    // We don't have a dataset so we we generate a random one using these values.
    [SerializeField] private int width = 30;
    [SerializeField] private int depth = 30;
    [SerializeField] private int numberOfPoints = 30;
    [SerializeField] private int numberOfCentroids = 5;
    [SerializeField] private MeshFilter terrainMeshFilter;
    private Vector3[] terrainVertices;
    [SerializeField] private GameObject player;
    private NavMeshAgent playerNavMeshAgent;

    // GameObject references.
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private GameObject centroidPrefab;
    [SerializeField] private Transform pointsHolder;
    [SerializeField] private Transform centroidsHolder;
    [SerializeField] private GameObject doneMessage;
    [SerializeField] private int spawnMultiplier = 2;
    [SerializeField] private TerrainGenv1 tg;
    private int desiredPointsPerCluster = 6;
    private int maxAttempts = 1000;
    private int currentAttempt = 0;

    // For use in task 3.
    [SerializeField] private GameObject[] collectibles;

    // Lists containing points and centroids.
    private List<GameObject> points;
    private List<GameObject> centroids;

    // Each time we generate a new dataset, we will also generate new colours for our clusters that we can work with.
    private List<Color> colours;

    // Keys are centroid gameobjects (clusters), values are lists of gameobjects that represent the points that belong to the cluster.
    private Dictionary<GameObject, List<GameObject>> clusters;

    
    private List<Vector3> previousCentroids;

    private void Start()
    {
        playerNavMeshAgent = player.GetComponent<NavMeshAgent>();
        Mesh terrainMesh = terrainMeshFilter.sharedMesh;
        if (terrainMesh == null)
        {
            Debug.LogError("No mesh asset found in MeshFilter component. Make sure a mesh asset is assigned to the Mesh field of the MeshFilter component.");
            return;
        }

        terrainVertices = terrainMesh.vertices;

        if (terrainVertices.Length == 0)
        {
            Debug.LogError("No vertices found in the mesh asset. Make sure the mesh asset has valid vertex data.");
            return;
        }

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        points = GenerateCollectibleGameObjects(collectibles, pointsHolder, numberOfPoints); // Place collectibles.
                                                                                             // ...
        Debug.Log("numberOfPoints: " + numberOfPoints);
        Debug.Log("points.Count: " + points.Count);

        StartKMeansClustering();
        // tg.UpdateNodeWalkability();

        stopwatch.Stop();
        Debug.Log(" Kmeans Time taken to generate objects and place them on terrain: " + stopwatch.ElapsedMilliseconds + "ms");
    }

    // Method to begin clustering. It is called from Start and from the "Start Clustering" UI button.
    private void StartKMeansClustering()
    {
        if (currentAttempt >= maxAttempts)
        {
            Debug.Log("Maximum attempts reached. Could not achieve the desired cluster configuration.");
            return;
        }

        currentAttempt++;

        if (currentAttempt == 1)
        {
            // Initialize points and centroids only at the first attempt
            points = GenerateCollectibleGameObjects(collectibles, pointsHolder, numberOfPoints); // Place collectibles.
            centroids = GenerateGameObjects(centroidPrefab, numberOfCentroids, centroidsHolder); // Randomly place centroids.
        }

        previousCentroids = GetCentroidsList(); // Important for checking for convergence.
        colours = GenerateColors();
        SetColoursToCentroids();

        // Execute the rest of the K-means clustering algorithm.
        Cluster();
    }

    private List<GameObject> GenerateCollectibleGameObjects(GameObject[] prefabs, Transform parent, int totalPoints)
    {
        List<GameObject> result = new List<GameObject>();
        int pointsPerPrefab = totalPoints / prefabs.Length;
        int remainingPoints = totalPoints % prefabs.Length;

        for (int i = 0; i < prefabs.Length; i++)
        {
            int pointsToGenerate = pointsPerPrefab + (i < remainingPoints ? 1 : 0);

            for (int j = 0; j < pointsToGenerate; j++)
            {
                Vector3 worldPosition;
                int attemptCounter = 0;
                const int maxAttempts = 100;

                do
                {
                    int randomIndex = UnityEngine.Random.Range(0, terrainVertices.Length);
                    Vector3 localPosition = terrainVertices[randomIndex];
                    worldPosition = terrainMeshFilter.transform.TransformPoint(localPosition);
                    worldPosition.y += 0.25f;

                    attemptCounter++;
                    if (attemptCounter >= maxAttempts)
                    {
                        Debug.LogWarning("Could not find a valid position with a path to the player after " + maxAttempts + " attempts.");
                        break;
                    }
                } while (!IsValidPath(worldPosition));

                GameObject newGameObject = Instantiate(prefabs[i], worldPosition, Quaternion.identity, parent);
                result.Add(newGameObject);
            }
        }

        return result;
    }

    private bool IsValidPath(Vector3 position)
    {
        NavMeshPath path = new NavMeshPath();
        if (playerNavMeshAgent.CalculatePath(position, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }

    private List<GameObject> GeneratePointsFromTerrain(Vector3[] terrainVertices, GameObject pointPrefab, Transform parent)
    {
        List<GameObject> result = new List<GameObject>();

        for (int i = 0; i < numberOfPoints; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, terrainVertices.Length);
            Vector3 pointPosition = terrainVertices[randomIndex];

            GameObject newGameObject = Instantiate(pointPrefab, pointPosition, Quaternion.identity, parent);
            result.Add(newGameObject);
        }

        return result;
    }


    // Iterate the algorithm.
    public void Cluster()
    {
        InitialiseClusters(); // Initialize clusters dictionary.
        AddPointsToClusters(); // Add each of the points to the cluster they belong to.

        CheckForEmptyClusters(); // Check if there's a cluster with no points and handle it.

        SetColorToClusterPoints(); // Set colours to the points from each cluster.

        CalculateNewCentroids(); // Calculate new centroids.

        UpdatePreviousCentroids(); // Update previous centroids to the positions of the current.

        CheckForConvergence(); // Ch
    }

    // Destroys all point and centroid GameObjects and disables "Done" message.
    private void ClearData()
    {
        DestroyChildren(pointsHolder);
        DestroyChildren(centroidsHolder);
        doneMessage.SetActive(false);
    }

    // Destroys all the child GameObjects of specified parent GameObject.
    private void DestroyChildren(Transform parent)
    {
        foreach (Transform item in parent)
        {
            Destroy(item.gameObject);
        }
    }

    // Update previous centroids to the positions of the current.
    private void UpdatePreviousCentroids()
    {
        for (int i = 0; i < centroids.Count; i++)
        {
            previousCentroids[i] = centroids[i].transform.position;
        }
    }

    // Check if no centroids have changed their position.
    private void CheckForConvergence()
    {
        for (int i = 0; i < centroids.Count; i++)
        {
            if (centroids[i].transform.position != previousCentroids[i])
                return;
        }

        if (AllClustersHaveDesiredPoints())
        {
            doneMessage.SetActive(true);
        }
        else
        {
            // Restart clustering process
            StartKMeansClustering();
        }
    }

    // Check if all clusters have exactly the desired number of points.
    private bool AllClustersHaveDesiredPoints()
    {
        foreach (KeyValuePair<GameObject, List<GameObject>> cluster in clusters)
        {
            if (cluster.Value.Count != desiredPointsPerCluster)
                return false;
        }
        return true;
    }


    // Take the sum of all the positions in the cluster and divide by the number of points (to get the mean average).
    private void CalculateNewCentroids()
    {
        int clusterCounter = 0;
        foreach (KeyValuePair<GameObject, List<GameObject>> cluster in clusters)
        {
            Vector3 sumOfPositions = Vector3.zero;

            foreach (GameObject point in cluster.Value)
            {
                sumOfPositions += point.transform.position;
            }

            Vector3 averagePosition = sumOfPositions / cluster.Value.Count;
            centroids[clusterCounter].transform.position = averagePosition;
            clusterCounter++;
        }
    }

    // Set colours to the points from each cluster.
    private void SetColorToClusterPoints()
    {
        int clusterCounter = 0;
        foreach (KeyValuePair<GameObject, List<GameObject>> cluster in clusters)
        {
            foreach (GameObject point in cluster.Value)
            {
                point.GetComponent<MeshRenderer>().material.color = colours[clusterCounter];
            }
            clusterCounter++;
        }
    }

    // If there's a cluster with no points, extract the closest point and add it to the empty cluster.
    private void CheckForEmptyClusters()
    {
        foreach (KeyValuePair<GameObject, List<GameObject>> cluster in clusters)
        {
            if (cluster.Value.Count == 0)
            {
                GameObject closestPoint = ExtractClosestPointToCluster(cluster.Key.transform.position);
                cluster.Value.Add(closestPoint);
            }
        }
    }

    // Find the closest point (from any cluster) to the centroid of an empty cluster, and add that point to the empty cluster.
    private GameObject ExtractClosestPointToCluster(Vector3 clusterPosition)
    {
        GameObject closestPoint = points[0];
        GameObject clusterThePointBelongsTo = null;
        float minDistance = float.MaxValue;

        // Looping through points is not a good idea because we need to find a cluster that has more than 1 item,
        // that's why we will loop through all the clusters and the points in clusters.
        // We only take the point if the cluster has more than 1 item, otherwise we'd take the item from the cluster that has 1 item,
        // Which means that a cluster would end up with no items and we will have the same problem.
        foreach (KeyValuePair<GameObject, List<GameObject>> cluster in clusters)
        {
            foreach (GameObject point in cluster.Value)
            {
                float distance = Vector3.Distance(point.transform.position, clusterPosition);
                if (distance < minDistance && cluster.Value.Count > 1)
                {
                    closestPoint = point;
                    minDistance = distance;
                    clusterThePointBelongsTo = cluster.Key;
                }
            }
        }

        clusters[clusterThePointBelongsTo].Remove(closestPoint);
        return closestPoint;
    }

    // Construct clusters dictionary.
    private void InitialiseClusters()
    {
        // At this point we will have the centroids already generated
        Dictionary<GameObject, List<GameObject>> result = new Dictionary<GameObject, List<GameObject>>();

        for (int i = 0; i < numberOfCentroids; i++)
        {
            result.Add(centroids[i], new List<GameObject>());
        }

        clusters = result;
    }

    // Add each of the points to the cluster they belong to.
    private void AddPointsToClusters()
    {
        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector3 pointPosition = points[i].transform.position;
            float minDistance = float.MaxValue;
            GameObject closestCentroid = centroids[0]; // We can randomly pick any centroid as this will update later.

            for (int j = 0; j < numberOfCentroids; j++)
            {
                float distance = Vector3.Distance(pointPosition, centroids[j].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCentroid = centroids[j];
                }
            }

            clusters[closestCentroid].Add(points[i]);
        }
    }

    // Apply the colours from the list to the points in each centroid.
    private void SetColoursToCentroids()
    {
        for (int i = 0; i < centroids.Count; i++)
        {
            centroids[i].GetComponent<MeshRenderer>().material.color = colours[i];
        }
    }

    // Generates a list of random colours.
    private List<Color> GenerateColors()
    {
        List<Color> result = new List<Color>();

        for (int i = 0; i < numberOfCentroids; i++)
        {
            Color color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            result.Add(color);
        }

        return result;
    }

    // Returns a new list containing all the centroid GameObjects.
    private List<Vector3> GetCentroidsList()
    {
        List<Vector3> result = new List<Vector3>();

        foreach (GameObject item in centroids)
        {
            result.Add(item.transform.position);
        }

        return result;
    }


    // Instantiates and returns a list of GameObjects for the points.
    private List<GameObject> GenerateGameObjects(GameObject prefab, int size, Transform parent)
    {
        List<GameObject> result = new List<GameObject>();

        for (int i = 0; i < size; i++)
        {
            float prefabXScale = prefab.transform.localScale.x;
            float positionX = UnityEngine.Random.Range(-width / 2 + prefabXScale, width / 2 - prefabXScale);

            float prefabZScale = prefab.transform.localScale.z;
            float positionZ = UnityEngine.Random.Range(-depth / 2 + prefabZScale, depth / 2 - prefabZScale);

            Vector3 newPosition = new Vector3(positionX, prefab.transform.position.y, positionZ);
            GameObject newGameObject = Instantiate(prefab, newPosition, Quaternion.identity, parent);

            result.Add(newGameObject);
        }

        return result;
    }
}
