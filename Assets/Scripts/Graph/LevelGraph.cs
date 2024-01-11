using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;
public class LevelGraph : MonoBehaviour
{
    private Dictionary<Vector2Int, RoomNode> _nodes;
    public int SizeNodelist = 10;
    public int branchs = 0;
    public int Limitedtry = 1000;
    public int RoomWidth = 100;
    public int RoomHeight = 100;
    public bool useSeed = false;
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    void RandomizeGraph()
    {
        _nodes.Clear();
        GenerateGraph();
    }
    [ShowIf("useSeed")] public int seed = 0;

    readonly Vector2Int[] adjacentDir = { new Vector2Int(1,0) , new Vector2Int(0,1)
    ,new Vector2Int(-1,0),new Vector2Int(0,-1)};

    private void Start()
    {
        GenerateGraph();
    }
    void GenerateGraph()
    {
        if (useSeed)
        {
            Random.InitState(seed);
        }

        int currentTry = 0;
        while (currentTry != Limitedtry)
        {
            _nodes = new Dictionary<Vector2Int, RoomNode>();
            _nodes[Vector2Int.zero] = new RoomNode(Vector2Int.zero, null);
            if (TryToGeneratePath(_nodes[Vector2Int.zero], SizeNodelist))
            {
                for (int i = 0; i < branchs; i++)
                {
                    int randomIndex = Random.Range(0, _nodes.Count);
                    TryToGeneratePath(_nodes.Values.ElementAt(randomIndex),Random.Range(1, SizeNodelist));
                }
                break;
            }
            Debug.LogWarning("Relaunch");
            currentTry++;
        }

    }
    bool TryToGeneratePath(RoomNode StartRoom, int length )
    {
        
        Utils.ORIENTATION currentOrientation = Utils.ORIENTATION.NONE;
        RoomNode currentNode = StartRoom;
        for (int i = 0; i < length; i++)
        {
            List<Utils.ORIENTATION> currentOrientations = Utils.AllOtherOrientation(currentOrientation);
            Vector2Int nextposition = Vector2Int.zero;
            while (currentOrientations.Count != 0)
            {
                int currentorientationIndex = Random.Range(0, currentOrientations.Count);
                currentOrientation = currentOrientations[currentorientationIndex];
                nextposition = Utils.OrientationToDir(currentOrientation) + currentNode.GraphPosition;
                if (!IsValidPosition(nextposition,currentNode))
                {
                    currentOrientations.Remove(currentOrientation);
                    if (currentOrientations.Count == 0)
                    {
                        return false;
                    }
                }
                else
                    break;
            }

            _nodes[nextposition] = new RoomNode(nextposition,null);
            if (i == length - 1)
                _nodes[nextposition].NodeType = RoomNode.Type.End;
            else
                _nodes[nextposition].NodeType = RoomNode.Type.Default;
            RoomConnection connection = new RoomConnection(currentNode, _nodes[nextposition], false);
            currentNode.connections[Utils.OrientationToIndex(currentOrientation)] = connection;
            _nodes[nextposition].connections[Utils.OrientationToIndex(Utils.OppositeOrientation(currentOrientation))] = connection;
            currentNode = _nodes[nextposition];
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            foreach (var item in _nodes)
            {
                if (item.Value.NodeType == RoomNode.Type.Start)
                    Gizmos.color = Color.green;
                else if (item.Value.NodeType == RoomNode.Type.End)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.blue;
                Gizmos.DrawCube(new Vector3(item.Key.x * RoomWidth, item.Key.y * RoomHeight), new Vector3(RoomWidth, RoomHeight, 1));
            }
        }
    }

    bool IsValidPosition(Vector2Int position,RoomNode PreviousRoom)
    {
        if (_nodes.ContainsKey(position))
        {
            return false;
        }
        foreach (Vector2Int dir in adjacentDir)
        {
            if (_nodes.ContainsKey(position+dir) && _nodes[position + dir] != PreviousRoom)
            {
                return false;
            }
        }
        return true;
    }
}
