using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomNode
{
    public enum Type { Start, End, Default,Key,Locked }
    public Type NodeType;
    public Vector2Int GraphPosition;
    public int difficulty;
    public RoomConnection[] connections = new RoomConnection[4];
    public RoomConnection next;

    public RoomNode(Vector2Int position)
    {
        GraphPosition = position;
        Debug.Log("Postion Node :" + GraphPosition);
    }
}
