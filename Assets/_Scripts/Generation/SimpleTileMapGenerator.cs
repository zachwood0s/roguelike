using DungeonGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DungeonGenerator;


public class SimpleTileMapGenerator : TileMapGeneratorBase
{
    private Tilemap _tilemap;
    public override void GenerateTilemap(Dungeon dungeon)
    {
        _tilemap = _tilemapParent.GetComponent<Tilemap>();
        _tilemap.ClearAllTiles();

        _FloorPass(dungeon);
        _WallPass(dungeon);

        _tilemap.RefreshAllTiles();
    }

    private void _FloorPass(Dungeon dungeon)
    {
        for (int i = 0; i < dungeon.Cells.GetLength(0); i++)
        {
            for (int j = 0; j < dungeon.Cells.GetLength(1); j++)
            {
                string name;
                if ((name = _GetFloorTiles(dungeon, i, j)) != null) 
                {
                    _tilemap.SetTile(new Vector3Int(i, j, 0), ResourceSystem.Instance.DungeonTileLookup[name]);
                }
            }
        }
    }

    private void _WallPass(Dungeon dungeon)
    {
        for (int i = 0; i < dungeon.Cells.GetLength(0); i++)
        {
            for (int j = 0; j < dungeon.Cells.GetLength(1); j++)
            {
                string name;
                if ((name = _GetWallTiles(dungeon, i, j)) != null) 
                {
                    _tilemap.SetTile(new Vector3Int(i, j, 1), ResourceSystem.Instance.DungeonTileLookup[name]);
                }
            }
        }
    }
    
    private string _GetFloorTiles(Dungeon dungeon, int i, int j)
    {
        if ((dungeon.Cells[i, j] & CellBits.OPENSPACE) == 0) return null;

        var r = Random.Range(0, 100);
        if (r < 2) return TileNames.FLOOR_3;
        if (r < 4) return TileNames.FLOOR_2;
        return TileNames.FLOOR_1;
    }

    private string _GetWallTiles(Dungeon d, int i, int j)
    {
        var left = _GetCell(d, i, j - 1);
        var right = _GetCell(d, i, j + 1);
        var up = _GetCell(d, i - 1, j);
        var down = _GetCell(d, i + 1, j);

        if ((left & CellBits.PERIMETER) != 0 && (right & CellBits.PERIMETER) != 0)
            return TileNames.WALL_MID;

        if ((left & CellBits.PERIMETER) != 0)
            return TileNames.WALL_LEFT;

        if ((right & CellBits.PERIMETER) != 0)
            return TileNames.WALL_RIGHT;

        return null;
    }

    private CellBits _GetCell(Dungeon d, int i, int j)
    {
        if (i < 0 || j < 0 || i >= d.Rows || j >= d.Cols) return CellBits.NOTHING;
        return d.Cells[i, j];
    }
}
