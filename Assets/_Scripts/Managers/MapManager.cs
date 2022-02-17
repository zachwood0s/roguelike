using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonGenerator;

public class MapManager : Singleton<MapManager>
{
    private DungeonGeneratorBase _generator;
    private TileMapGeneratorBase _tileGenerator;
    [SerializeField] private GenerationOptions _options;

    void Start()
    {
    }

    private void _Generate()
    {
        _generator = GetComponent<DungeonGeneratorBase>();
        var d = _generator.CreateDungeon(_options);
        _tileGenerator = GetComponent<TileMapGeneratorBase>();
        _tileGenerator.GenerateTilemap(d);
    }
}

