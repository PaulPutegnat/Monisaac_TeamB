using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomNode
{
   public enum Type {Start,End,Default }
   public Type NodeType;
    public Vector2Int GraphPosition;
    public RoomConnection[] connections = new RoomConnection[4];

    public RoomNode(Vector2Int position)
    {
        GraphPosition = position;
        Debug.Log("Postion Node :"+ GraphPosition);
    }
}
