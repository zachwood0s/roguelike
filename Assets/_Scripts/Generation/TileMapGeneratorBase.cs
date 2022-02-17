using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileNames
{
    public const string WALL_MID = "wall_mid";
    public const string WALL_LEFT = "wall_left";
    public const string WALL_RIGHT = "wall_right";

    public const string WALL_SIDE_MID_LEFT = "wall_side_mid_left";
    public const string WALL_SIDE_MID_RIGHT = "wall_side_mid_right";

    public const string FLOOR_1 = "floor_1";
    public const string FLOOR_2 = "floor_2";
    public const string FLOOR_3 = "floor_3";
    public const string FLOOR_4 = "floor_4";
    public const string FLOOR_5 = "floor_5";


    private static string[] _floors = new []{ FLOOR_1, FLOOR_2, FLOOR_3, FLOOR_4, FLOOR_5 };
    public static string GetRandomFloor() => _floors[Random.Range(0, _floors.Length)];
}

public abstract class TileMapGeneratorBase : MonoBehaviour
{
    [SerializeField] protected GameObject _tilemapParent;
    public abstract void GenerateTilemap(DungeonGenerator.Dungeon dungeon);
}
