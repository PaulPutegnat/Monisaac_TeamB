using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGraph : MonoBehaviour
{
    private Dictionary<Vector2Int, RoomNode> _nodes;
    public int SizeNodelist = 10;
    public int Limitedtry = 1000;
    public int RoomWidth = 100;
    public int RoomHeight = 100;

    private void Start()
    {
        GenerateGraph();
    }
    void GenerateGraph()
    {
        int currentTry = 0;
        while (currentTry != Limitedtry)
        {
            if (TryToGenerateLevels())
                break;
            Debug.Log("Relaunch");
            currentTry++;
        }

    }
    bool TryToGenerateLevels()
    {
        _nodes = new Dictionary<Vector2Int, RoomNode>();
        Utils.ORIENTATION currentOrientation = Utils.IndexToOrientation(Random.Range(0, 3));
        _nodes[Vector2Int.zero] = new RoomNode(Vector2Int.zero);
        RoomNode currentNode = _nodes[Vector2Int.zero];
        for (int i = 1; i < SizeNodelist; i++)
        {
            List<Utils.ORIENTATION> currentOrientations = Utils.AllOtherOrientation(currentOrientation);
            Vector2Int nextposition = Vector2Int.zero;
            for (int j = 0; j < currentOrientations.Count; j++)
            {
                if (currentOrientations.Count == 0)
                {
                    return false;
                }
                int currentorientationIndex = Random.Range(0, currentOrientations.Count - 1);
                currentOrientation = currentOrientations[currentorientationIndex];
                nextposition = Utils.OrientationToDir(currentOrientation) + currentNode.GraphPosition;
                if (_nodes.ContainsKey(nextposition))
                {
                    currentOrientations.Remove(currentOrientation);
                }
            }
            _nodes[nextposition] = new RoomNode(nextposition);
            if (i == SizeNodelist)
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
}
