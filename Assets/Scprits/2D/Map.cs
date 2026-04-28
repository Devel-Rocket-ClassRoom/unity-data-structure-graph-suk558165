using UnityEngine;
using System.Linq;

public enum TileTypes
{
    Empty = -1,
    // 0~14 해안선
    Grass = 15,
    Tree,
    Hills,
    Mountains,
    Towns,
    Castle,
    Monster
}

public class Map
{
    public int rows = 0;
    public int cols = 0;

    public Tile[] tiles;
    public Tile startTile;
    public Tile castleTile;

    public Tile[] CoastTiles => tiles.Where(t => t.autoTileId >= 0 && t.autoTileId < (int)TileTypes.Grass).ToArray();
    public Tile[] LandTiles => tiles.Where(t => t.autoTileId == (int)TileTypes.Grass).ToArray();

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
                if ((r + 1) < rows) tiles[index].adjacents[(int)Sides.Bottom] = tiles[index + cols];
                if ((c + 1) < cols) tiles[index].adjacents[(int)Sides.Right] = tiles[index + 1];
                if ((c - 1) >= 0)   tiles[index].adjacents[(int)Sides.Left]  = tiles[index - 1];
                if ((r - 1) >= 0)   tiles[index].adjacents[(int)Sides.Top]   = tiles[index - cols];
            }
        }

        for (int i = 0; i < tiles.Length; ++i)
            tiles[i].UpdateAutoTileId();
    }

    public void ShuffleTiles(Tile[] tiles)
    {
        for (int i = tiles.Length - 1; i > 0; --i)
        {
            int rand = Random.Range(0, i + 1);
            (tiles[rand], tiles[i]) = (tiles[i], tiles[rand]);
        }
    }

    public void DecoeateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        ShuffleTiles(tiles);
        int total = Mathf.FloorToInt(tiles.Length * percent);
        for (int i = 0; i < total; ++i)
        {
            if (tileType == TileTypes.Empty)
                tiles[i].ClearAdjacents();
            tiles[i].autoTileId = (int)tileType;
        }
    }

    public bool CreateIsland(
        float erodePercent, int erodeIterations,
        float lakePercent, float treePercent, float hillPercent,
        float mountainPercent, float townPercent, float monsterPercent)
    {
        for (int i = 0; i < erodeIterations; i++)
            DecoeateTiles(CoastTiles, erodePercent, TileTypes.Empty);

        DecoeateTiles(LandTiles, lakePercent, TileTypes.Empty);
        DecoeateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecoeateTiles(LandTiles, hillPercent, TileTypes.Hills);
        DecoeateTiles(LandTiles, mountainPercent, TileTypes.Mountains);
        DecoeateTiles(LandTiles, monsterPercent, TileTypes.Monster);
        DecoeateTiles(LandTiles, townPercent, TileTypes.Towns);

        var towns = tiles.Where(x => x.autoTileId == (int)TileTypes.Towns).ToArray();
        if (towns.Length < 2) return false;
        ShuffleTiles(towns);
        castleTile = towns[0];
        startTile = towns[1];
        castleTile.autoTileId = (int)TileTypes.Castle;

        return true;
    }
}
