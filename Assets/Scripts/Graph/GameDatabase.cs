using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Data",menuName = "DataBase")]
public class GameDatabase : ScriptableObject
{
    public List<GameObject> RoomPrefabs;

    public GameObject GetRandomRoomByType(RoomNode.Type type)
    {
        List<GameObject> roomByType = RoomPrefabs.Where(item => item.GetComponent<RoomNode>().NodeType == type).ToList();
        int randomIndex = Random.Range(0, roomByType.Count);
        return roomByType[randomIndex];
    }
    public GameObject GetRandomRoomByTypeAndDifficulty(RoomNode.Type type,int Difficulty)
    {
        List<GameObject> roomByType = RoomPrefabs.Where(item => item.GetComponent<RoomNode>().NodeType == type
        && item.GetComponent<RoomNode>().difficulty == Difficulty).ToList();
        int randomIndex = Random.Range(0, roomByType.Count);
        return roomByType[randomIndex];
    }
}
