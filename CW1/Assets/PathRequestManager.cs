using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager : MonoBehaviour {

    Queue<PathRequest> pathRequestqueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;

    static  PathRequestManager instance;
    PathFinding pathfinding;
    bool isProcessingPath;

     void Awake()
    {
        instance= this;
        pathfinding = GetComponent<PathFinding>(); 
        
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback) {
    PathRequest newRequest = new PathRequest(pathStart,pathEnd,callback);
        instance.pathRequestqueue.Enqueue(newRequest);
        instance.TryProcessNext();
    
    }

    void TryProcessNext() {
        if(!isProcessingPath && pathRequestqueue.Count > 0)
        {
            currentPathRequest= pathRequestqueue.Dequeue();
            isProcessingPath= true;
            pathfinding.StartFindPath(currentPathRequest.pathStart,currentPathRequest.pathEnd);



        }
    
    
    
    
    }

    public void FinishProcessingPath(Vector3[] path, bool success) {
        currentPathRequest.callback(path, success);
        isProcessingPath= false;
        TryProcessNext();
    
    
    
    
    }

    struct PathRequest { 
        public Vector3 pathStart; public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback) 

        {
            pathStart= _start;
            pathEnd= _end;
            callback= _callback;

        
        }
    
    }

    



}
