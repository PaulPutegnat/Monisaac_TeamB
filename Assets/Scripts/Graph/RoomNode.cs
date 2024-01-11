using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomNode
{
   public enum Type {Start,End,Default }
   public Type NodeType;
    public Vector2Int GraphPosition;
    public int difficulty;
    public RoomConnection[] connections = new RoomConnection[4];
    //public GameObject @object;

    public RoomNode(Vector2Int position,GameObject room)
    {
        GraphPosition = position;
       // @object =  room;
        Debug.Log("Postion Node :"+ GraphPosition);
    }
}
