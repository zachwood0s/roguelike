using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PiecedDungeonator : MonoBehaviour
{
    [SerializeField] private int _rows;
    [SerializeField] private int _cols;
    [SerializeField] private int _numRoomsBase;
    [SerializeField] private float _numRoomsRandFactor;
    [SerializeField] private float _endPathRandChance;

    [SerializeField] private float _cellSize;

    [SerializeField] private GameObject _mainRoom;

    // Rooms with doors on these sides
    private List<GameObject> _rooms;
    private GameObject[,] _map;
    private List<(int, int)> _endRooms;
    private int _totalNumRooms;

    private Dictionary<PieceDoorController.DoorDirection, List<PieceDoorController>> _roomDoorDirections;


    // Start is called before the first frame update
    void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        _CollectRoomPieces();
        var filled = _FillCells();

        Debug.Log($"Generating {_totalNumRooms} rooms...");
        Debug.Log($"Map Generation Success: {filled}");

        var s = "";
        for (int r = 0; r < _map.GetLength(0); r++)
        {
            for (int c = 0; c < _map.GetLength(1); c++)
            {
                s += _map[r, c] != null ? "1" : "0";
            }
            s += "\n";
        }

        Debug.Log(s);

        _PlaceRooms();
    }

    private void _CollectRoomPieces()
    {
        _rooms = Resources.LoadAll<GameObject>("Prefabs/MapPieces").ToList();
        _roomDoorDirections = new Dictionary<PieceDoorController.DoorDirection, List<PieceDoorController>>
        {
            { PieceDoorController.DoorDirection.Left, new List<PieceDoorController>() },
            { PieceDoorController.DoorDirection.Right, new List<PieceDoorController>() },
            { PieceDoorController.DoorDirection.Up, new List<PieceDoorController>() },
            { PieceDoorController.DoorDirection.Down, new List<PieceDoorController>() }
        };


        foreach (var r in _rooms)
        {
            var cont = r.GetComponent<PieceDoorController>();
            Debug.Assert(cont != null);
            foreach (var d in cont.Doors)
            {
                _roomDoorDirections[d.OutDirection].Add(cont);
            }
        }
    }

    private struct PlacedRoom
    {
        public PieceDoorController Room;
        public int Row;
        public int Col;
        public PieceDoorController.Door? InDoor;
    }

    private bool _FillCells()
    {
        _map = new GameObject[_rows, _cols];
        _endRooms = new List<(int, int)>();
        _totalNumRooms = Mathf.CeilToInt(_numRoomsBase + UnityEngine.Random.Range(0, _numRoomsRandFactor));


        var currentCell = new Queue<PlacedRoom>();
        var starting = (UnityEngine.Random.Range(0, _rows), UnityEngine.Random.Range(0, _cols));
        var startingRoom = _mainRoom.GetComponent<PieceDoorController> ();

        var filledRooms = 0;
        var firstPlaced = new PlacedRoom()
        {
            Room = startingRoom,
            Row = starting.Item1,
            Col = starting.Item2,
            InDoor = null
        };
        currentCell.Enqueue(firstPlaced);

        // Don't count the starting room towards the room count
        _FillRoom(firstPlaced);

        while(currentCell.Count > 0)
        {
            var placed = currentCell.Dequeue();
            bool addedNeighbor = false;

            foreach (var door in placed.Room.Doors)
            {
                var endPath = UnityEngine.Random.Range(0f, 1f);
                if (filledRooms >= _totalNumRooms ||
                    endPath < _endPathRandChance)
                {
                    // Skipping this room
                    continue;
                }
                var room = _FindRoomToPlace(placed, door);

                if (!room.HasValue)
                {
                    // We failed to place the rooms necessary
                    continue;
                }

                _FillRoom(room.Value);
                addedNeighbor = true;
                filledRooms++;
                currentCell.Enqueue(room.Value);
            }

            if (!addedNeighbor)
            {
                //_endRooms.Add((cellX, cellY));
            }
        }


        if (filledRooms < _totalNumRooms)
        {
            // Didn't get to fill all of the rooms for some reason
            return false;
        }

        return true;
    }

    private PlacedRoom? _FindRoomToPlace(PlacedRoom relativeTo, PieceDoorController.Door OutDoor)
    {
        var opposite = OutDoor.Opposite;
        var options = _roomDoorDirections[opposite.OutDirection];

        var startIdx = UnityEngine.Random.Range(0, options.Count - 1);

        for (int i = startIdx + 1; i != startIdx; i = (i + 1) % options.Count)
        {
            var option = options[i];
            // Try to find a door that works on this side of the room
            foreach (var d in option.Doors)
            {
                // Only use doors that are facing the correct way
                if (d.OutDirection != opposite.OutDirection)
                    continue;

                var placed = new PlacedRoom()
                {
                    Col = opposite.OutPosition.x + relativeTo.Col,
                    Row = opposite.OutPosition.y + relativeTo.Row,
                    Room = option,
                    InDoor = d
                };
                if (_CanFillRoom(placed))
                {
                    return placed;
                }
            }
        }
        return null;
    }

    private bool _CoordsInRange(int row, int col) => row >= 0 && col >= 0 && row < _rows && col < _cols;

    private IEnumerable<(int, int)> _NeighborPositions(int row, int col)
    {
        yield return (row - 1, col);
        yield return (row, col - 1);
        yield return (row + 1, col);
        yield return (row, col + 1);
    }

    private PieceDoorController.RoomBounds _NormalizeCoords(PlacedRoom placed)
    {
        var roomBounds = placed.Room.GetRoomBounds();
        int startRow, startCol, endRow, endCol;
        if (!placed.InDoor.HasValue)
        {
            startRow = placed.Row + roomBounds.MinRow;
            startCol = placed.Col + roomBounds.MinCol;
            endRow = placed.Row + roomBounds.MaxRow;
            endCol = placed.Col + roomBounds.MaxCol;
        }
        else
        {
            startRow = placed.Row - placed.InDoor.Value.OutPosition.y + roomBounds.MinRow;
            startCol = placed.Col - placed.InDoor.Value.OutPosition.x + roomBounds.MinCol;
            endRow = placed.Row - placed.InDoor.Value.OutPosition.y + roomBounds.MaxRow;
            endCol = placed.Col - placed.InDoor.Value.OutPosition.x + roomBounds.MaxCol;
        }
        return new PieceDoorController.RoomBounds()
        {
            MinRow = startRow,
            MaxRow = endRow,
            MinCol = startCol,
            MaxCol = endCol
        };
    }
    
    private void _FillRoom(PlacedRoom placed)
    {
        var norm = _NormalizeCoords(placed);
        for (int r = norm.MinRow; r <= norm.MaxRow; r++)
        {
            for (int c = norm.MinCol; c <= norm.MaxCol; c++)
            {
                _map[r, c] = placed.Room.gameObject;
            }
        }
    }

    private bool _CanFillRoom(PlacedRoom placed)
    {
        var norm = _NormalizeCoords(placed);
        for (int r = norm.MinRow; r <= norm.MaxRow; r++)
        {
            for (int c = norm.MinCol; c <= norm.MaxCol; c++)
            {
                if (!_CoordsInRange(r, c) || _map[r, c] != null)
                    return false;
            }
        }
        return true;
    }

    private int _FilledNeighborCount(int row, int col)
    {
        int count = 0;
        foreach (var (r, c) in _NeighborPositions(row, col))
        {
            if (_CellFilled(r, c)) count += 1;
        }
        return count;
    }

    private bool _CellFilled(int row, int col) => _CoordsInRange(row, col) ? _map[row, col]: false;

    private void _PlaceRooms()
    {
        for(int row = 0; row < _map.GetLength(0); row++)
        {
            for(int col = 0; col < _map.GetLength(1); col++)
            {
                if(_map[row, col] != null)
                {
                    Instantiate(_map[row, col], new Vector2(col, row) * _cellSize, Quaternion.identity);
                }
            }
        }
    }
}
