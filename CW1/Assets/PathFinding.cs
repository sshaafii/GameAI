using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using System;

public class PathFinding : MonoBehaviour
{
    //public Transform seeker;
    //public Transform target;

    PathRequestManager pathRequestManager;
    TerrainGenv1 gen;

    private void Awake()
    {
        pathRequestManager= GetComponent<PathRequestManager>();
        gen = GetComponent<TerrainGenv1>();
    }

    private void Update()
    {
        //FindPath(seeker.position, target.position);
    }
    public void StartFindPath(Vector3 startPos,Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
        
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos) 
    
    {

        Vector3[] wayPoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = gen.GetClosestNode(startPos);
        Node targetNode = gen.GetClosestNode(targetPos);

        Node prevNode = null;

        if (startNode.walkable && targetNode.walkable) {
        
        
        

        List<Node> openSet = new List<Node>();
        HashSet<Node> closeSet = new HashSet<Node>();

         

        openSet.Add(startNode);




        while(openSet.Count > 0)
        {

            Node currentNode = openSet[0];
            for(int i =1;i<openSet.Count;i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost <currentNode.hCost)
                {
                    currentNode = openSet[i];

                }


            }
            openSet.Remove(currentNode);
            closeSet.Add(currentNode);

            if(gen.GetNeighbours(targetNode).Contains(currentNode)) {
                pathSuccess= true;
                prevNode = currentNode;
                 
                break;
                
            
            }

            foreach (Node neighbour in gen.GetNeighbours(currentNode)) {
                if (!neighbour.walkable || closeSet.Contains(neighbour)) 
                {
                    continue;
            
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + currentNode.movementPenalty;

                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }



                }
            }
             


        }



            

        }

        yield return null;




        if (pathSuccess) { 
        
        wayPoints = RetracePath(startNode, prevNode);


        }

        pathRequestManager.FinishProcessingPath(wayPoints,pathSuccess);

   
    
    }



     Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = simplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
        
        
    }

    Vector3[] simplifyPath(List<Node> path) {

        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for(int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld) {
                waypoints.Add(path[i].worldPos);
            
            
            }

            directionOld = directionNew;



        }

        return waypoints.ToArray();


    
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstZ = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstZ)
        {
            return 14 * dstZ + 10 * (dstX - dstZ);
        }

        return 14 * dstX + 10 * (dstZ - dstX);
    }




}
