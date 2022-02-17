using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceSystem : Singleton<ResourceSystem>
{
    private List<Tile> _dungeonTiles;
    private Dictionary<string, Tile> _dungeonTileMapping;

    public IReadOnlyList<Tile> DungeonTiles => _dungeonTiles;
    public IReadOnlyDictionary<string, Tile> DungeonTileLookup => _dungeonTileMapping;
    protected override void Awake()
    {
        base.Awake();
        _AssembleResources();
    }

    private void _AssembleResources()
    {
        _dungeonTiles = Resources.LoadAll<Tile>("Tiles/Dungeon").ToList();
        _dungeonTileMapping = _dungeonTiles.ToDictionary(t => t.name, t => t);
    }
}
