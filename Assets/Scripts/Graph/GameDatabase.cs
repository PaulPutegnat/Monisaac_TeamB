using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;
using UnityEditor;

[CreateAssetMenu(fileName = "Data",menuName = "DataBase")]
public class GameDatabase : ScriptableObject
{
    public List<Room> RoomPrefabs;

    [Button]
    void UpdateRooms()
    {
        RoomPrefabs.Clear();
        RoomPrefabs = Utils.LoadAllPrefabsOfType<Room>();
        foreach (var item in RoomPrefabs)
        {
            item.StartRoom();
        }
    }

    public Room GetRandomRoomByType(RoomNode.Type type)
    {
        List<Room> roomByType = RoomPrefabs.Where(item => item.NodeType == type).ToList();
        int randomIndex = Random.Range(0, roomByType.Count -1);
        return roomByType[randomIndex];
    }

    public Room GetRandomByNodeRoom(RoomNode node,bool byDifficulty,int Difficulty) {
        var roomByNodeRoom = RoomPrefabs.Where(item =>
            (node.connections[0] != null) == (item.GetDoor(Utils.ORIENTATION.NORTH) != null)
            && (node.connections[1] != null) == (item.GetDoor(Utils.ORIENTATION.EAST) != null)
            && (node.connections[2] != null) == (item.GetDoor(Utils.ORIENTATION.SOUTH) != null)
            && (node.connections[3] != null) == (item.GetDoor(Utils.ORIENTATION.WEST) != null)
            && (item.difficulty == Difficulty|| !byDifficulty)).ToList();
        if (roomByNodeRoom.Count == 0)
        {
            if (byDifficulty&& Difficulty > 1)
            {
                return GetRandomByNodeRoom(node,true, Difficulty - 1);
            }
            else if(byDifficulty)
            {
                return GetRandomByNodeRoom(node,false,0);
            }
            else
            {
                Debug.LogWarning("Room by Default ! Not matching !");
                return GetRandomRoomByType(node.NodeType);
            }
        }
        int randomIndex = Random.Range(0, roomByNodeRoom.Count);
        return roomByNodeRoom[randomIndex];
    }


    public Room GetRandomRoomByTypeAndDifficulty(RoomNode.Type type,int Difficulty)
    {
        List<Room> roomByType = RoomPrefabs.Where(item => item.NodeType == type && item.difficulty == Difficulty).ToList();
        if (roomByType.Count == 0)
        {
            if (Difficulty > 1)
            {
               return GetRandomRoomByTypeAndDifficulty(type, Difficulty - 1);
            }
            else
            {
               return GetRandomRoomByType(type);
            }
        }
        int randomIndex = Random.Range(0, roomByType.Count);
        return roomByType[randomIndex];
    }
}
