using System.Linq;
using UnityEngine;

public enum TileTypes
{
    Empty = -1,
    Grass = 15,
    Tree,
    Hills,
    Mountains,
    Towns,
    Castle,
    Monster,
}

public class Map
{
    public int rows;
    public int cols;
    public Tile[] tiles;
    public Tile startTile;
    public Tile castleTile;

    // autoTileId가 0 이상 Grass 미만인 타일 (해안선)
    public Tile[] CoastTiles => tiles.Where(t => t.autoTileId >= 0 && t.autoTileId < (int)TileTypes.Grass).ToArray();
    // autoTileId가 Grass인 타일 (평지)
    public Tile[] LandTiles => tiles.Where(t => t.autoTileId == (int)TileTypes.Grass).ToArray();

    // 타일 배열 생성 및 상하좌우 인접 연결
    public void Init(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;
        tiles = new Tile[rows * cols];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new Tile();
            tiles[i].id = i;
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int index = r * cols + c;
                if (r + 1 < rows) tiles[index].adjacents[(int)Sides.Bottom] = tiles[index + cols];
                if (c - 1 >= 0)   tiles[index].adjacents[(int)Sides.Left]   = tiles[index - 1];
                if (c + 1 < cols) tiles[index].adjacents[(int)Sides.Right]  = tiles[index + 1];
                if (r - 1 >= 0)   tiles[index].adjacents[(int)Sides.Top]    = tiles[index - cols];
            }
        }

        foreach (var tile in tiles)
        {
            tile.UpdateAutoTileId();
            tile.UpdateFowTileId();
        }
    }

    // Fisher-Yates 알고리즘으로 타일 배열을 무작위 섞기
    public void ShuffleTiles(Tile[] tiles)
    {
        for (int i = tiles.Length - 1; i > 0; --i)
        {
            int rand = Random.Range(0, i + 1);
            (tiles[rand], tiles[i]) = (tiles[i], tiles[rand]);
        }
    }

    // tiles 중 percent 비율만큼 tileType으로 설정 (Castle은 중앙 1개만)
    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        ShuffleTiles(tiles);
        int total = Mathf.FloorToInt(tiles.Length * percent);

        if (tileType == TileTypes.Castle)
        {
            tiles[total / 2].autoTileId = (int)tileType;
            return;
        }

        for (int i = 0; i < total; ++i)
        {
            if (tileType == TileTypes.Empty)
                tiles[i].ClearAdjacent();
            tiles[i].autoTileId = (int)tileType;
        }
    }

    // 침식 → 지형 배치 순서로 섬 생성, startTile(마을)과 castleTile 결정
    public void CreateIsland(
        float erodePercent, int erodeIterations,
        float lakePercent, float treePercent,
        float hillPercent, float mountainPercent,
        float townPercent, float monsterPercent)
    {
        for (int i = 0; i < erodeIterations; ++i)
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);

        DecorateTiles(LandTiles, 1, TileTypes.Castle);
        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);
        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hills);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountains);
        DecorateTiles(LandTiles, townPercent, TileTypes.Towns);
        DecorateTiles(LandTiles, monsterPercent, TileTypes.Monster);

        castleTile = tiles.First(t => t.autoTileId == (int)TileTypes.Castle);
        var towns = tiles.Where(t => t.autoTileId == (int)TileTypes.Towns).ToArray();
        startTile = towns[Random.Range(0, towns.Length)];
    }
}
