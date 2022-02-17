using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace DungeonGenerator
{  
    [Flags]
    public enum CellBits
    {
        NOTHING = 0x0000_0000,
        BLOCKED = 0x0000_0001,
        ROOM = 0x0000_0002,
        CORRIDOR = 0x0000_0004,

        PERIMETER = 0x0000_0010,
        ENTRANCE = 0x0000_0020,
        ROOM_ID = 0x0000_FFC0,

        ARCH = 0x0001_0000,
        DOOR = 0x0002_0000,
        LOCKED = 0x0004_0000,
        TRAPPED = 0x0008_0000,

        OPENSPACE = ROOM | CORRIDOR,
        DOORSPACE = ARCH | DOOR | LOCKED | TRAPPED,
        ESCAPE = ENTRANCE | DOORSPACE,

        BLOCK_ROOM = BLOCKED | ROOM,
        BLOCK_CORR = BLOCKED | PERIMETER | CORRIDOR,
        BLOCK_DOOR = BLOCKED | DOORSPACE,
    }

    [Serializable]
    public struct GenerationOptions
    {
        public enum RoomLayout
        {
            Packed,
            Scattered
        };

        public enum CorridorLayout
        {
            Bent,
            Labyrinth,
            Straight
        };

        [SerializeField] private CorridorLayout _corridorType;
        [SerializeField] private RoomLayout _roomType;
        [SerializeField] private int _rows;
        [SerializeField] private int _cols;
        [SerializeField] private int _roomMax;
        [SerializeField] private int _roomMin;
        [SerializeField] private int _seed;

        public CorridorLayout CorridorType => _corridorType;
        public RoomLayout RoomType => _roomType;
        public int Rows => _rows;
        public int Cols => _cols;
        public int RoomMax => _roomMax;
        public int RoomMin => _roomMin;
        public int Seed => _seed;

    }

    public enum Direction
    {
        North, South, East, West, None
    }

    public class Room
    {
        public int RoomId { get; internal set; }
        public int Row { get; internal set; }
        public int Col { get; internal set; }
        public int North { get; internal set; }
        public int South { get; internal set; }
        public int West { get; internal set; }
        public int East { get; internal set; }
        public int Height { get; internal set; }
        public int Width { get; internal set; }
        public int Area => Width * Height;
        public Dictionary<Direction, List<Door>> Doors { get; internal set; }
    }

    public class Door
    {
        public enum Type
        {
            Arch,
            Open,
            Locked
        }
        public int OutId { get; internal set; }
        public Type DoorType { get; internal set; }
        public int Row { get; internal set; }
        public int Col { get; internal set; }

    }

    public class Dungeon
    {

        public CellBits[,] Cells { get; internal set; }
        public List<Room> Rooms { get; internal set; }
        public int Rows { get; internal set; }
        public int Cols { get; internal set; }
        public int MaxRow => Rows - 2;
        public int MaxCol => Cols - 2;
        public int RoomMin { get; internal set; }
        public int RoomMax { get; internal set; }
        public int NRooms { get; internal set; }
        public GenerationOptions.CorridorLayout CorridorLayout { get; internal set; }
        internal int HalfRows { get; set; }
        internal int HalfCols { get; set; }
        internal int RoomBase => (RoomMin + 1) / 2;
        internal int RoomRadix => ((RoomMax - RoomMin) / 2) + 1;

        public override string ToString()
        {
            var s = new StringBuilder();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    string c = Cells[i, j] switch
                    {
                        CellBits.NOTHING => " ",
                        CellBits.BLOCKED => "B",
                        CellBits.CORRIDOR => "C",
                        CellBits.PERIMETER => "P",
                        CellBits.ENTRANCE => "E",
                        CellBits.ARCH => "A",
                        CellBits.DOOR => "D",
                        CellBits.LOCKED => "L",
                        CellBits.TRAPPED => "T",
                        var b when (b & CellBits.ROOM) != 0 => $"{(int)(b & CellBits.ROOM_ID) >> 6}"
                    };
                    s.Append(c);
                }
                s.Append("\n");
            }
            return s.ToString();
        }
    }

    public abstract class DungeonGeneratorBase : MonoBehaviour
    {
        public abstract Dungeon CreateDungeon(GenerationOptions options);
    }
}
