using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[Serializable]
public class Node
{

    public bool walkable;
    public Vector3 worldPos;
    public int gCost;
    public int hCost;
    List<Vector2> positions;
    public int gridX;
    public int gridY;
    public Node parent;
    public int movementPenalty;



    public Node(bool _walkable,Vector3 _worldPos,  int gridX, int gridY,int movementPenalty) { 

        walkable = _walkable;
        worldPos = _worldPos;

        this.gridX = gridX;
        this.gridY = gridY;
        this.movementPenalty= movementPenalty;


    }

    public Vector2 vector2Position(Vector3 pos) {

       return new Vector2(pos.x, pos.z);

    }

    public override string ToString()
    {
        return $" IsWalkable: {walkable}, Position: {worldPos}";


    }

    public int fCost {

        get { return gCost *hCost; }
    
    }

   

}
