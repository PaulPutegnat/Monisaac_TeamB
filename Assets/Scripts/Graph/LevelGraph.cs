using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;
public class LevelGraph : MonoBehaviour
{
    private Dictionary<Vector2Int, RoomNode> _nodes;
    private Room[] _rooms;
    public GameDatabase db;
    [Header("Unlocked Door")]

    [Range(1, 100)] public int startSpawnUnlockedDoor = 1;
    [Range(1, 100)] public int endSpawnUnlockedDoor = 2;

    private int StartUnlockedDoor
    {
        get
        {
            return Mathf.Clamp(startSpawnUnlockedDoor, 0, mainPath.Count - 2);
        }
    }

    private int EndUnlockedDoor
    {
        get
        {
            return Mathf.Clamp(endSpawnUnlockedDoor, StartUnlockedDoor, mainPath.Count - 2);
        }
    }



    [Header("Nodes")]
    [Range(0, 100)] public int unlockedMax = 0;
    public int SizeNodelist = 10;
    private int Limitedtry = 1000;
    public bool useSeed = false;
    private List<RoomNode> lastPath = new List<RoomNode>();
    private List<RoomNode> mainPath = new List<RoomNode>();

    [Header("Gizmos")]
    public int RoomWidth = 9;
    public int RoomHeight = 11;
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    void RandomizeGraph()
    {
        _nodes.Clear();
        GenerateGraph();
        GenerateGraphMap();
    }
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    void GenerateGraphMap()
    {
        foreach (var node in _rooms)
        {
            if (node != null)
            {
                Destroy(node.gameObject);
            }
        }
        GenerateMap();
    }
    [ShowIf("useSeed")] public int seed = 0;
    public bool debug;
    readonly Vector2Int[] adjacentDir = { new Vector2Int(1,0) , new Vector2Int(0,1)
    ,new Vector2Int(-1,0),new Vector2Int(0,-1)};

    private void Start()
    {
        GenerateGraph();
        GenerateMap();
    }

    void GenerateMap()
    {
        _rooms = new Room[_nodes.Count];
        int i = 0;
        foreach (var node in _nodes)
        {
            if (node.Value.NodeType != RoomNode.Type.None)
            {
                Room room = db.GetRandomByNodeRoom(node.Value, true, node.Value.difficulty);
                if (!room)
                {
                    Debug.LogError("Room not found Generation broken !");
                }
                _rooms[i] = Instantiate(room);
                foreach (var item in _rooms[i].GetDoors())
                {
                    RoomConnection connection = null;
                    switch (item.Orientation)
                    {
                        case Utils.ORIENTATION.NORTH:
                            connection = node.Value.connections[0];
                            break;
                        case Utils.ORIENTATION.EAST:
                            connection = node.Value.connections[1];
                            break;
                        case Utils.ORIENTATION.SOUTH:
                            connection = node.Value.connections[2];
                            break;
                        case Utils.ORIENTATION.WEST:
                            connection = node.Value.connections[3];
                            break;
                        default:
                            break;
                    }
                    if (connection != null && connection.Islocked)
                    {
                        item.SetState(Door.STATE.CLOSED);
                    }
                    else if (connection != null && node.Value.NodeType == RoomNode.Type.Secret)
                    {
                        item.SetState(Door.STATE.SECRET);
                    }
                    else
                    {
                        item.SetState(Door.STATE.OPEN);
                    }
                }
                _rooms[i].isStartRoom = node.Value.NodeType == RoomNode.Type.Start;
                _rooms[i].position = node.Key;
                _rooms[i].transform.position = Utils.ConvertGraphPosToWorldPos(_rooms[i].transform.lossyScale, _rooms[i].position) *
                    new Vector2(RoomWidth, RoomHeight);
                i++;
            }
        }
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
            _nodes[Vector2Int.zero] = new RoomNode(Vector2Int.zero);
            mainPath.Clear();
            bool failedToGeneratelocked = false;
            if (TryToGeneratePath(_nodes[Vector2Int.zero], SizeNodelist))
            {
                mainPath.AddRange(lastPath);
                for (int i = 0; i < unlockedMax; i++)
                {
                    bool indexFound = false;
                    int randomIndex = 0;
                    int secondaryPathIndex = 0;
                    while (!indexFound)
                    {
                        randomIndex = Random.Range(StartUnlockedDoor, EndUnlockedDoor);
                        secondaryPathIndex = Random.Range(0, randomIndex + 1);
                        indexFound = mainPath[randomIndex].NodeType != RoomNode.Type.Locked;
                    }
                    bool k = TryToGeneratePath(mainPath[randomIndex], Random.Range(1, SizeNodelist));
                    if (k)
                    {
                        mainPath[randomIndex].NodeType = RoomNode.Type.Locked;
                        lastPath[lastPath.Count - 1].NodeType = RoomNode.Type.Key;
                        mainPath[randomIndex].next.Islocked = true;

                    }
                    else
                    {
                        failedToGeneratelocked = true;
                        break;
                    }
                }
                if (failedToGeneratelocked)
                {
                    currentTry++;
                    Debug.LogWarning("Relaunch");
                    continue;
                }

                for (int x = FoundMinNodePositon().x; x < FoundMaxNodePositon().x; x++)
                {
                    for (int y = FoundMinNodePositon().y; y < FoundMaxNodePositon().y; y++)
                    {
                        Vector2Int currentPos = new Vector2Int(x, y);
                        if (!_nodes.ContainsKey(currentPos))
                        {
                            _nodes[currentPos] = new RoomNode(currentPos);
                            _nodes[currentPos].NodeType = RoomNode.Type.None;
                        }
                    }
                }

                PlaceSecretRoom();
                break;
            }
            Debug.LogWarning("Relaunch");
            currentTry++;
        }

    }
    bool TryToGeneratePath(RoomNode StartRoom, int length)
    {
        Utils.ORIENTATION currentOrientation = Utils.ORIENTATION.NONE;
        RoomNode currentNode = StartRoom;
        lastPath.Clear();
        for (int i = 0; i < length; i++)
        {
            if (i >= 4)
            {
                currentNode.difficulty = 1;
            }
            if (i >= 6)
            {
                currentNode.difficulty = 2;
            }

            List<Utils.ORIENTATION> currentOrientations = Utils.AllOtherOrientation(currentOrientation);
            Vector2Int nextposition = Vector2Int.zero;
            while (currentOrientations.Count != 0)
            {
                int currentorientationIndex = Random.Range(0, currentOrientations.Count);
                currentOrientation = currentOrientations[currentorientationIndex];
                nextposition = Utils.OrientationToDir(currentOrientation) + currentNode.GraphPosition;
                if (!IsValidPosition(nextposition, currentNode))
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

            _nodes[nextposition] = new RoomNode(nextposition);
            if (i == length - 1)
                _nodes[nextposition].NodeType = RoomNode.Type.End;
            else
                _nodes[nextposition].NodeType = RoomNode.Type.Default;
            RoomConnection connection = new RoomConnection(currentNode, _nodes[nextposition], false);
            currentNode.connections[Utils.OrientationToIndex(currentOrientation)] = connection;
            if (currentNode.next == null && _nodes[nextposition].NodeType != RoomNode.Type.End)
            {
                currentNode.next = connection;
            }
            _nodes[nextposition].connections[Utils.OrientationToIndex(Utils.OppositeOrientation(currentOrientation))] = connection;
            currentNode = _nodes[nextposition];
            lastPath.Add(_nodes[nextposition]);
        }
        return true;
    }

    void PlaceSecretRoom()
    {
        for (int attempt = 0; attempt < 900; attempt++)
        {
            Vector2Int randomPosition = FindRandomEmptyPosition(attempt);
            if (randomPosition != Vector2Int.zero)
            {
                _nodes[randomPosition] = new RoomNode(randomPosition);
                _nodes[randomPosition].NodeType = RoomNode.Type.Secret;

                int index = 0;
                foreach (Vector2Int dir in adjacentDir)
                {
                    Vector2Int adjacentPosition = randomPosition + dir;
                    if (_nodes.ContainsKey(adjacentPosition) && _nodes[adjacentPosition].NodeType != RoomNode.Type.None)
                    {
                        _nodes[randomPosition].connections[index] = new RoomConnection(_nodes[randomPosition], null, false);
                    }
                    index++;
                }
                return;
            }
        }

        Debug.LogWarning("Unable to find a suitable position for the secret room.");
    }

    private Vector2Int FindRandomEmptyPosition(int attempt)
    {
        Vector2Int[] emptyPositions = _nodes
        .Where(pair => pair.Value.NodeType == RoomNode.Type.None)
        .Select(pair => pair.Key)
        .ToArray();

        if (emptyPositions.Length == 0)
            return Vector2Int.zero; // No empty positions available.

        if (attempt < 300)
        {
            // Use strict criteria for the first 300 attempts.
            return FindValidEmptyPosition(emptyPositions);
        }
        else if (attempt < 600)
        {
            // Relax criteria a bit for attempts 301 to 600.
            return FindValidEmptyPosition(emptyPositions, true);
        }
        else
        {
            // Relax criteria even more for attempts beyond 600.
            return FindValidEmptyPosition(emptyPositions, true, true);
        }
    }

    Vector2Int FindValidEmptyPosition(Vector2Int[] positions, bool relaxCriteria1 = false, bool relaxCriteria2 = false)
    {
        for (int i = 0; i < 10; i++) // Attempt up to 10 times to find a suitable position.
        {
            Vector2Int randomPosition = positions[Random.Range(0, positions.Length)];

            // Check if the position is adjacent to at least three rooms and not adjacent to an end room.
            int adjacentRoomCount = 0;
            foreach (Vector2Int dir in adjacentDir)
            {
                Vector2Int adjacentPosition = randomPosition + dir;
                if (_nodes.ContainsKey(adjacentPosition) && _nodes[adjacentPosition].NodeType != RoomNode.Type.None)
                {
                    adjacentRoomCount++;
                }
            }

            if ((relaxCriteria1 && adjacentRoomCount >= 2) || (relaxCriteria2 && adjacentRoomCount >= 1))
            {
                return randomPosition;
            }
        }
        Debug.LogWarning("Unable to find a suitable position.");
        return Vector2Int.zero; // Unable to find a suitable position.
    }

    Vector2Int FoundMaxNodePositon()
    {
        int xMax = 0, yMax = 0;
        foreach (var item in _nodes)
        {
            if (item.Key.x > xMax)
            {
                xMax = item.Key.x;
            }
            if (item.Key.y > yMax)
            {
                yMax = item.Key.y;
            }
        }

        Debug.Log(new Vector2Int(xMax, yMax));
        return new Vector2Int(xMax, yMax);
    }

    Vector2Int FoundMinNodePositon()
    {
        int xMin = 0, yMin = 0;
        foreach (var item in _nodes)
        {
            if (item.Key.x < xMin)
            {
                xMin = item.Key.x;
            }
            if (item.Key.y < yMin)
            {
                yMin = item.Key.y;
            }
        }
        Debug.Log(new Vector2Int(xMin, yMin));
        return new Vector2Int(xMin, yMin);
    }



    private void OnDrawGizmos()
    {
        if (Application.isPlaying && debug)
        {
            foreach (var item in _nodes)
            {
                if (item.Value.NodeType == RoomNode.Type.Start)
                    Gizmos.color = Color.green;
                else if (item.Value.NodeType == RoomNode.Type.Key)
                    Gizmos.color = Color.magenta;
                else if (item.Value.NodeType == RoomNode.Type.Locked)
                    Gizmos.color = Color.yellow;
                else if (item.Value.NodeType == RoomNode.Type.Secret)
                    Gizmos.color = Color.white;
                else if (item.Value.NodeType == RoomNode.Type.End)
                    Gizmos.color = Color.red;

                else if (item.Value.NodeType == RoomNode.Type.None)
                    Gizmos.color = Color.black;
                //continue;

                else
                    Gizmos.color = Color.blue;
                Gizmos.DrawCube(new Vector3(item.Key.x * RoomWidth, item.Key.y * RoomHeight), new Vector3(RoomWidth, RoomHeight, 1));
            }
        }
    }

    bool IsValidPosition(Vector2Int position, RoomNode PreviousRoom)
    {
        if (_nodes.ContainsKey(position))
        {
            return false;
        }
        foreach (Vector2Int dir in adjacentDir)
        {
            if (_nodes.ContainsKey(position + dir) && _nodes[position + dir] != PreviousRoom)
            {
                return false;
            }
        }
        return true;
    }
}
