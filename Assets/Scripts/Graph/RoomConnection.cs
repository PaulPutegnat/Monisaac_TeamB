using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomConnection
{
    RoomNode[] rooms = new RoomNode[2];
    public bool Islocked;

    public RoomConnection(RoomNode current, RoomNode next, bool islocked)
    {
        rooms[0] = current;
        rooms[1] = next;
        Islocked = islocked;
    }
}
