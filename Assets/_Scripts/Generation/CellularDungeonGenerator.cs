using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonGenerator
{
    public class CellularDungeonGenerator : DungeonGeneratorBase
    {
        private Dungeon _dungeon;
        public override Dungeon CreateDungeon(GenerationOptions options)
        {
            _dungeon = new Dungeon()
            {
                HalfRows = options.Rows / 2,
                HalfCols = options.Cols / 2,
                RoomMin = options.RoomMin,
                RoomMax = options.RoomMax,
                NRooms = 0,
                Rooms = new List<Room>(),
                CorridorLayout = options.CorridorType,
            };
            _dungeon.Rows = _dungeon.HalfRows * 2; 
            _dungeon.Cols = _dungeon.HalfCols * 2;
            _dungeon.Cells = new CellBits[_dungeon.Rows, _dungeon.Cols];

            _InitCells(options.Seed, options);
            _EmplaceRooms(options);
            _OpenRooms();
            _Corridors();
            _CleanDungeon();
            return _dungeon;
        }

        private void _InitCells(int seed, GenerationOptions options)
        {
            UnityEngine.Random.InitState(seed);

            for (var y = 0; y < _dungeon.Cells.GetLength(0); y++)
            {
                for (var x = 0; x < _dungeon.Cells.GetLength(1); x++)
                {
                    _dungeon.Cells[x, y] = 0;
                }
            }
        }

        #region Place Rooms

        private void _EmplaceRooms(GenerationOptions options)
        {
            switch (options.RoomType)
            {
                case GenerationOptions.RoomLayout.Packed:
                    _PackRooms(options);
                    break;
                case GenerationOptions.RoomLayout.Scattered:
                    _ScatterRooms(options);
                    break;
            }
        }

        private void _ScatterRooms(GenerationOptions options)
        {
            var area = _dungeon.Cells.GetLength(0) * _dungeon.Cells.GetLength(1);
            var roomArea = _dungeon.RoomMax * _dungeon.RoomMax;
            var nRooms = area / roomArea;

            for (var i = 0; i < nRooms; i++)
            {
                _EmplaceRoom(options);
            }
        }

        private void _PackRooms(GenerationOptions options)
        {
            for (var i = 0; i < _dungeon.HalfRows; i++)
            {
                var r = (i * 2) + 1;
                for (var j = 0; j < _dungeon.HalfCols; j++)
                {
                    var c = (j * 2) + 1;

                    // If its a room or its on the edge (50% chance)
                    if ((_dungeon.Cells[r, c] & CellBits.ROOM) != 0 ||
                        ((i == 0 || j == 0) && UnityEngine.Random.Range(0, 2) != 0))
                        continue;

                    _EmplaceRoom(options, i, j);
                }
            }
        }

        private struct RoomProto
        {
            public int width, height, i, j;
        }

        private void _EmplaceRoom(GenerationOptions options, int? startI = null, int? startJ = null)
        {
            // Room Size

            var roomProto = _SetRoom(startI, startJ);

            // Room Boundaries

            var r1 = roomProto.i * 2 + 1;
            var c1 = roomProto.j * 2 + 1;
            var r2 = (roomProto.i + roomProto.height) * 2 - 1;
            var c2 = (roomProto.j + roomProto.width) * 2 - 1;

            if (r1 < 1 || r2 >= _dungeon.MaxRow) return;
            if (c1 < 1 || c2 >= _dungeon.MaxCol) return;

            // Check for collisions with existing rooms

            if (_CheckRoomCollision(r1, c1, r2, c2))
            {
                // Collided with other rooms
                return;
            }

            var roomId = _dungeon.NRooms;
            _dungeon.NRooms += 1;

            // Emplace Room

            for (int r = r1; r <= r2; r++)
            {
                for (int c = c1; c <= c2; c++)
                {
                    if ((_dungeon.Cells[r, c] & CellBits.ENTRANCE) != 0)
                    {
                        _dungeon.Cells[r, c] &= ~CellBits.ESCAPE;
                    }
                    else if ((_dungeon.Cells[r, c] & CellBits.PERIMETER) != 0)
                    {
                        _dungeon.Cells[r, c] &= ~CellBits.PERIMETER;
                    }
                    _dungeon.Cells[r, c] |= CellBits.ROOM | (CellBits) (roomId << 6);
                }
            }

            int height = ((r2 - r1) + 1) * 10;
            int width = ((c2 - c1) + 1) * 10;

            var room = new Room()
            {
                RoomId = roomId,
                Row = r1, Col = c1,
                North = r1, South = r2, West = c1, East = c2,
                Height = height, Width = width,
                Doors = new Dictionary<Direction, List<Door>>()
            };

            _dungeon.Rooms.Add(room);

            // Block Corridors from Room Boundary

            for (int r = r1 - 1; r <= r2 + 1; r++)
            {
                if ((_dungeon.Cells[r, c1 - 1] & (CellBits.ROOM | CellBits.ENTRANCE)) == 0)
                {
                    // If its not a room or an entrance then mark it as the perimeter
                    _dungeon.Cells[r, c1 - 1] |= CellBits.PERIMETER;
                }
                if ((_dungeon.Cells[r, c2 + 1] & (CellBits.ROOM | CellBits.ENTRANCE)) == 0)
                {
                    // If its not a room or an entrance then mark it as the perimeter
                    _dungeon.Cells[r, c2 + 1] |= CellBits.PERIMETER;
                }
            }
            for (int c = c1 - 1; c <= c2 + 1; c++)
            {
                if ((_dungeon.Cells[r1 - 1, c] & (CellBits.ROOM | CellBits.ENTRANCE)) == 0)
                {
                    // If its not a room or an entrance then mark it as the perimeter
                    _dungeon.Cells[r1 - 1, c] |= CellBits.PERIMETER;
                }
                if ((_dungeon.Cells[r2 + 1, c] & (CellBits.ROOM | CellBits.ENTRANCE)) == 0)
                {
                    // If its not a room or an entrance then mark it as the perimeter
                    _dungeon.Cells[r2 + 1, c] |= CellBits.PERIMETER;
                }
            }
        }

        private RoomProto _SetRoom(int? startI, int? startJ)
        {
            int height, width, i, j;
            if (startI is null)
            {
                height = UnityEngine.Random.Range(0, _dungeon.RoomRadix) + _dungeon.RoomBase;
                i = UnityEngine.Random.Range(0, _dungeon.HalfRows - height);
            }
            else
            {
                var a = _dungeon.HalfRows - _dungeon.RoomBase - startI.Value;
                a = a < 0 ? 0 : a;
                var r = (a < _dungeon.RoomRadix) ? a : _dungeon.RoomRadix;
                height = UnityEngine.Random.Range(0, r) + _dungeon.RoomBase;
                i = startI.Value;
            }

            if (startJ is null)
            {
                width = UnityEngine.Random.Range(0, _dungeon.RoomRadix) + _dungeon.RoomBase;
                j = UnityEngine.Random.Range(0, _dungeon.HalfCols - width);
            }
            else
            {
                var a = _dungeon.HalfCols - _dungeon.RoomBase - startJ.Value;
                a = a < 0 ? 0 : a;
                var r = (a < _dungeon.RoomRadix) ? a : _dungeon.RoomRadix;
                width = UnityEngine.Random.Range(0, r) + _dungeon.RoomBase;
                j = startJ.Value;
            }

            return new RoomProto()
            {
                i = i,
                j = j,
                width = width,
                height = height
            };

        }

        private bool _CheckRoomCollision(int r1, int c1, int r2, int c2)
        {
            for (int r = r1; r <= r2; r++)
            {
                for (int c = c1; c <= c2; c++)
                {
                    if ((_dungeon.Cells[r, c] & CellBits.BLOCKED) != 0)
                    {
                        return true;
                    }
                    if ((_dungeon.Cells[r, c] & CellBits.ROOM) != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Open Rooms

        private void _OpenRooms()
        {
            foreach (var room in _dungeon.Rooms)
            {
                _OpenRoom(room);
            }

        }

        private void _OpenRoom(Room room)
        {
            // Calculate number of sills
            var sills = _DoorSills(room).OrderBy(_ => UnityEngine.Random.Range(0, 100)).ToList();
            if (sills.Count == 0)
                return;

            // Calculate number of openings
            var roomH = (room.South - room.North) / 2 + 1;
            var roomW = (room.East - room.West) / 2 + 1;
            var flumph = (int)Math.Sqrt(roomH * roomW);
            var nOpens = flumph + UnityEngine.Random.Range(0, flumph);

            Debug.Log($"Gen {nOpens} openings");
            for (var i = 0; i < nOpens; i++)
            {
                if (sills.Count == 0) break;

                var rand = UnityEngine.Random.Range(0, sills.Count);
                var sill = sills[rand];
                sills.RemoveAt(rand);

                if((_dungeon.Cells[sill.doorR, sill.doorC] & CellBits.DOORSPACE) != 0)
                {
                    // Redo the loop
                    i--;
                    continue;
                }

                var openR = sill.sillR;
                var openC = sill.sillC;
                var openDir = sill.dir;

                // TODO: Connection

                // Open door
                for (var x = 0; x < 3; x++)
                {
                    var (di, dj) = _DirInt(openDir);
                    var r = openR + di * x;
                    var c = openC + dj * x;
                    _dungeon.Cells[r, c] &= ~CellBits.PERIMETER;
                    _dungeon.Cells[r, c] |= CellBits.ENTRANCE;

                    // TODO: Decide door type
                    _dungeon.Cells[sill.doorR, sill.doorC] |= CellBits.DOOR;

                    // TODO: Add to room object
                    var door = new Door()
                    {
                        Row = sill.doorR, Col = sill.doorC,
                        DoorType = Door.Type.Open,
                        OutId = sill.outId
                    };

                    if (room.Doors.TryGetValue(openDir, out var doors))
                    {
                        doors.Add(door);
                    }
                    else
                    {
                        room.Doors[openDir] = new List<Door>() { door };
                    }
                }
            }
        }

        private struct Sill
        {
            public int sillR;
            public int sillC;
            public Direction dir;
            public int doorR;
            public int doorC;
            public int outId;
        }

        private IEnumerable<Sill> _DoorSills(Room room)
        {
            if (room.North >= 3)
            {
                for (var c = room.West; c <= room.East; c += 2)
                {
                    var sill = _CheckSill(room, room.North, c, Direction.North);
                    if (sill != null) yield return sill.Value;
                }
            }
            if (room.South <= _dungeon.Rows - 3)
            {
                for (var c = room.West; c <= room.East; c += 2)
                {
                    var sill = _CheckSill(room, room.South, c, Direction.South);
                    if (sill != null) yield return sill.Value;
                }

            }
            if (room.West >= 3)
            {
                for (var r = room.North; r <= room.South; r += 2)
                {
                    var sill = _CheckSill(room, r, room.West, Direction.West);
                    if (sill != null) yield return sill.Value;
                }
            }
            if (room.East <= _dungeon.Cols - 3)
            {
                for (var r = room.North; r <= room.South; r += 2)
                {
                    var sill = _CheckSill(room, r, room.East, Direction.East);
                    if (sill != null) yield return sill.Value;
                }
            }

        }


        private (int, int) _DirInt(Direction dir) => dir switch
        {
            Direction.North => (-1, 0),
            Direction.South => (1, 0),
            Direction.East => (0, 1),
            Direction.West => (0, -1),
            _ => (0, 0)
        };

        private Sill? _CheckSill(Room room, int sillR, int sillC, Direction dir)
        {
            var (di, dj) = _DirInt(dir);

            var doorR = sillR + di;
            var doorC = sillC + dj;
            var doorCell = _dungeon.Cells[doorR, doorC];

            // Return if we're not on the perimeter or the door is blocked
            if ((doorCell & CellBits.PERIMETER) == 0 || (doorCell & CellBits.BLOCK_DOOR) != 0)
                return null;

            var outR = doorR + di;
            var outC = doorC + dj;
            var outCell = _dungeon.Cells[outR, outC];

            // Return if out door is blocked
            if ((doorCell & CellBits.BLOCKED) != 0)
                return null;

            var outId = -1;
            if((outCell & CellBits.ROOM_ID) != 0)
            {
                outId = (int)(outCell & CellBits.ROOM_ID) >> 6;
                // Make sure this doesn't go to the same room
                if (outId == room.RoomId)
                    return null;
            }

            return new Sill
            {
                sillR = sillR, sillC = sillC,
                doorR = doorR, doorC = doorC,
                dir = dir,
                outId = outId
            };
        }

        #endregion

        #region Corridors

        private void _Corridors()
        {
            for (var i = 1; i < _dungeon.HalfRows; i++)
            {
                var r = i * 2 + 1;
                for (var j = 1; j < _dungeon.HalfCols; j++)
                {
                    var c = j * 2 + 1;

                    if ((_dungeon.Cells[r, c] & CellBits.CORRIDOR) != 0)
                        continue;

                    _Tunnel(i, j);
                }
            }

        }

        private int _CorridorLayoutMapping(GenerationOptions.CorridorLayout layout) => layout switch
        {
            GenerationOptions.CorridorLayout.Labyrinth => 0,
            GenerationOptions.CorridorLayout.Bent => 50,
            GenerationOptions.CorridorLayout.Straight => 100,
        };

        private List<Direction> _RandomDirections() =>
            ((Direction[])Enum.GetValues(typeof(Direction)))
                .Take(4)
                .OrderBy(x => UnityEngine.Random.Range(0, 100))
                .ToList();

        private (int, int) _SortTwo(int one, int two) => one < two ? (one, two) : (two, one);


        private void _Tunnel(int i, int j, Direction lastDir = Direction.None)
        {
            var dirs = _TunnelDirs(lastDir);
            Debug.Log(string.Join(", ", dirs.Select(x => x.ToString())));
            foreach (var dir in dirs)
            {
                if (_OpenTunnel(i, j, dir))
                {
                    var (di, dj) = _DirInt(dir);
                    var nextI = i + di;
                    var nextJ = j + dj;

                    _Tunnel(nextI, nextJ, dir);
                }
            }
        }

        private List<Direction> _TunnelDirs(Direction lastDir)
        {
            var p = _CorridorLayoutMapping(_dungeon.CorridorLayout);
            var dirs = _RandomDirections();

            if (lastDir != Direction.None && p != 0)
            {
                if (UnityEngine.Random.Range(0, 100) < p)
                {
                    dirs.Insert(0, lastDir);
                }
            }
            return dirs;
        }

        private bool _OpenTunnel(int i, int j, Direction dir)
        {
            var thisR = i * 2 + 1;
            var thisC = j * 2 + 1;
            var (di, dj) = _DirInt(dir);
            var nextR = ((i + di) * 2) + 1;
            var nextC = ((j + dj) * 2) + 1;
            var midR = (thisR + nextR) / 2;
            var midC = (thisC + nextC) / 2;

            if (_SoundTunnel(midR, midC, nextR, nextC))
            {
                // Delve tunnel
                var (r1, r2) = _SortTwo(thisR, nextR);
                var (c1, c2) = _SortTwo(thisC, nextC);

                for (var r = r1; r <= r2; r++)
                {
                    for (var c = c1; c <= c2; c++)
                    {
                        _dungeon.Cells[r, c] &= ~CellBits.ENTRANCE;
                        _dungeon.Cells[r, c] |= CellBits.CORRIDOR;
                    }
                }

                return true;
            }
            return false;
        }

        // Don't open blocked cells, room perimeters or other corridors
        private bool _SoundTunnel(int midR, int midC, int nextR, int nextC)
        {

            if (nextR < 0 || nextR > _dungeon.Rows || nextC < 0 || nextC > _dungeon.Cols)
                return false;

            var (r1, r2) = _SortTwo(midR, nextR);
            var (c1, c2) = _SortTwo(midC, nextC);

            for(var r = r1; r <= r2; r++)
            {
                for (var c = c1; c <= c2; c++)
                {
                    if ((_dungeon.Cells[r, c] & CellBits.BLOCK_CORR) != 0)
                    {
                        // Corridor is blocked
                        return false;
                    }
                }
            }
            return true;
        }


        #endregion

        #region Clean Dungeon

        private void _CleanDungeon()
        {
            _RemoveDeadends();
            _FixDoors();
            _EmptyBlocks();
            _FixPerimeter();
        }

        private void _RemoveDeadends()
        {
            // Collapse Tunnels
            for (var i = 0; i < _dungeon.HalfRows; i++)
            {
                var r = (i * 2) + 1;
                for (var j = 0; j < _dungeon.HalfCols; j++)
                {
                    var c = (j * 2) + 1;

                    if ((_dungeon.Cells[r, c] & CellBits.OPENSPACE) != 0)
                    {
                        _Collapse(r, c);
                    }
                }
            }

        }

        private struct CorrEndStruct
        {
            public (int, int)[] Walled;
            public (int, int)[] Close;
            public (int, int) Recurse;
        }

        private Dictionary<Direction, CorrEndStruct> CloseEnd = new Dictionary<Direction, CorrEndStruct>
        {
            [Direction.North] = new CorrEndStruct
            {
                Walled = new[] {(0, -1), (1, -1), (1, 0), (1, 1), (0, 1)},
                Close = new[] {(0, 0)},
                Recurse = (-1, 0)
            },
            [Direction.South] = new CorrEndStruct
            {
                Walled = new[] {(0, -1), (-1, -1), (-1, 0), (-1, 1), (0, 1)},
                Close = new[] {(0, 0)},
                Recurse = (1, 0)
            },
            [Direction.West] = new CorrEndStruct
            {
                Walled = new[] {(-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0)},
                Close = new[] {(0, 0)},
                Recurse = (0, -1)
            },
            [Direction.East] = new CorrEndStruct
            {
                Walled = new[] {(-1, 0), (-1, -1), (0, -1), (1, -1), (1, 0)},
                Close = new[] {(0, 0)},
                Recurse = (0, 1)
            },
        };

        private void _Collapse(int r, int c)
        {
            // Needs to be open space
            if ((_dungeon.Cells[r, c] & CellBits.OPENSPACE) == 0) return;
            if (r >= _dungeon.Rows - 1 || c >= _dungeon.Cols - 1) return;

            foreach (var dir in CloseEnd.Keys)
            {
                var xc = CloseEnd[dir];
                if(_CheckTunnel(r, c, xc))
                {
                    foreach (var (p1, p2) in xc.Close)
                    {
                        _dungeon.Cells[r + p1, c + p2] = CellBits.NOTHING;
                    }

                    _Collapse(r + xc.Recurse.Item1, c + xc.Recurse.Item2);
                }

            }
        }

        private bool _CheckTunnel(int r, int c, CorrEndStruct check)
        {
            foreach (var (p1, p2) in check.Walled)
            {
                if ((_dungeon.Cells[r + p1, c + p2] & CellBits.OPENSPACE) != 0)
                {
                    return false;
                }

            }
            return true;
        }

        private Direction _Opposite(Direction dir) => dir switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.West => Direction.East,
            Direction.East => Direction.West
        };

        private void _FixDoors()
        {
            bool[,] alreadyFixed = new bool[_dungeon.Rows, _dungeon.Cols];

            foreach (var room in _dungeon.Rooms)
            {
                var toRemove = new List<Direction>();
                var newSet = new Dictionary<Direction, List<Door>>();
                foreach (var dir in room.Doors.Keys)
                {
                    var shiny = new List<Door>();
                    foreach (var door in room.Doors[dir])
                    {
                        CellBits doorCell = _dungeon.Cells[door.Row, door.Col];

                        // Skip if openspace
                        if ((doorCell & CellBits.OPENSPACE) == 0)
                            continue;

                        if (alreadyFixed[door.Row, door.Col])
                        {
                            shiny.Add(door);
                        }
                        else
                        {
                            if (door.OutId >= 0)
                            {
                                // Link it with the opposite room
                                var outDir = _Opposite(dir);
                                if(_dungeon.Rooms[door.OutId].Doors.TryGetValue(outDir, out var outDoors))
                                {
                                    outDoors.Add(door);
                                }
                                else
                                {
                                    _dungeon.Rooms[door.OutId].Doors[outDir] = new List<Door>() { door };
                                }
                            }
                            shiny.Add(door);
                            alreadyFixed[door.Row, door.Col] = true;
                        }
                    }
                    if (shiny.Count > 0)
                    {
                        newSet[dir] = shiny;
                        // TODO: add doors to dungeon
                    }
                    else
                    {
                        toRemove.Add(dir);
                    }
                }
                foreach (var d in newSet.Keys)
                {
                    room.Doors[d] = newSet[d];
                }
                foreach (var d in toRemove)
                    room.Doors.Remove(d);
            }
        }

        private void _EmptyBlocks()
        {
            for (var r = 0; r < _dungeon.Rows; r++)
            {
                for (var c = 0; c < _dungeon.Cols; c++)
                {
                    if ((_dungeon.Cells[r, c] & CellBits.BLOCKED) != 0)
                        _dungeon.Cells[r, c] = CellBits.NOTHING;
                }
            }
        }

        private void _FixPerimeter()
        {
            for (var r = 0; r < _dungeon.Rows; r++)
            {
                for (var c = 0; c < _dungeon.Cols; c++)
                {
                    var cell = _dungeon.Cells[r, c];
                    if(cell == CellBits.NOTHING && _AnyFloorsAround(r, c))
                    {
                        _dungeon.Cells[r, c] |= CellBits.PERIMETER;
                    }
                }
            }
        }

        private bool _AnyFloorsAround(int r, int c)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int checkR = r + i;
                    int checkC = c + j;

                    if (checkR < 0 || checkR >= _dungeon.Rows || checkC < 0 || checkC >= _dungeon.Cols)
                    {
                        continue;
                    }

                    if ((_dungeon.Cells[checkR, checkC] & CellBits.OPENSPACE) != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }

}
