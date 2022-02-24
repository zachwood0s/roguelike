using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PieceDoorController : MonoBehaviour
{
    public enum DoorDirection { Left, Right, Up, Down };

    [Serializable]
    public struct Door
    {
        public DoorDirection OutDirection;
        public Vector2Int OutPosition;

        public DoorDirection InDirection => OutDirection switch
        {
            DoorDirection.Down => DoorDirection.Up,
            DoorDirection.Up => DoorDirection.Down,
            DoorDirection.Right => DoorDirection.Left,
            DoorDirection.Left => DoorDirection.Right,
            _ => throw new ArgumentException("Enum type doesn't exist")
        };

        public Vector2Int InPosition => OutDirection switch
        {
            DoorDirection.Down => OutPosition + new Vector2Int(0, -1),
            DoorDirection.Up => OutPosition + new Vector2Int(0, 1),
            DoorDirection.Right => OutPosition + new Vector2Int(1, 0),
            DoorDirection.Left => OutPosition + new Vector2Int(-1, 0),
            _ => throw new ArgumentException("Enum type doesn't exist")
        };

        public Door Opposite => new Door()
        {
            OutDirection = InDirection,
            OutPosition = InPosition
        };
    }

    public struct RoomBounds
    {
        public int MinRow;
        public int MaxRow;
        public int MinCol;
        public int MaxCol;
    }

    [SerializeField] private List<Door> _doors;

    public List<Door> Doors => _doors;

    public RoomBounds GetRoomBounds()
    {
        int minRow, maxRow, minCol, maxCol;
        minRow = minCol = int.MaxValue;
        maxRow = maxCol = int.MinValue;

        foreach (var d in _doors)
        {
            if (d.OutPosition.x < minCol)
                minCol = d.OutPosition.x;
            if (d.OutPosition.x > maxCol)
                maxCol = d.OutPosition.x;

            if (d.OutPosition.y < minRow)
                minRow = d.OutPosition.y;
            if (d.OutPosition.y > maxRow)
                maxRow = d.OutPosition.y;
        }

        return new RoomBounds()
        {
            MinCol = minCol,
            MaxCol = maxCol,
            MinRow = minRow,
            MaxRow = maxRow
        };
    }
}
