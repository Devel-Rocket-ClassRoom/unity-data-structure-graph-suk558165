using UnityEngine;
using System.Linq;

public enum TileTypes
{
    Empty = -1,
    // 0 ~ 14 : 해안선 타일
    Grass = 15, // 평지
    Tree,
    Hills,
    Mountains,
    Towns,
    Castle,
    Monster
}

public class Map    // 타일 전체를 관리
{
    public int rows = 0;    // 행
    public int cols = 0;    // 열

    public Tile[] tiles;
    public Tile[] CoastTiles => tiles.Where(t => t.autoTileId >= 0 && t.autoTileId < (int)TileTypes.Grass).ToArray();   // 해안선
    public Tile[] LandTiles => tiles.Where(t => t.autoTileId == (int)TileTypes.Grass).ToArray(); // 육지 (Grass만)

    public Tile startTile;   // 플레이어 시작 위치
    public Tile castleTile;  // 목표 위치

    public void Init(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;

        // 타일 배열 생성 및 id 부여
        tiles = new Tile[rows * cols];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new Tile();
            tiles[i].id = i;
        }

        // 각 타일의 인접 타일 연결 (상하좌우)
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var index = row * cols + col;
                var adjacents = tiles[index].adjacents;

                if ((row - 1) >= 0)
                    adjacents[(int)Sides.Top] = tiles[index - cols];    // 위

                if ((col + 1) < cols)
                    adjacents[(int)Sides.Right] = tiles[index + 1];     // 오른쪽

                if ((col - 1) >= 0)
                    adjacents[(int)Sides.Left] = tiles[index - 1];      // 왼쪽

                if ((row + 1) < rows)
                    adjacents[(int)Sides.Bottom] = tiles[index + cols]; // 아래
            }
        }

        // 인접 정보 기반으로 스프라이트 인덱스 계산
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].UpdateAutoTileId();
        }
    }

    public void ShuffleTiles(Tile[] tiles)
    {
        // Fisher-Yates 셔플
        for (int i = tiles.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (tiles[rand], tiles[i]) = (tiles[i], tiles[rand]);
        }
    }

    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        ShuffleTiles(tiles);  // 무작위 순서로 섞은 뒤

        int total = Mathf.FloorToInt(tiles.Length * percent);  // 비율만큼 개수 계산
        for (int i = 0; i < total; i++)
        {
            if (tileType == TileTypes.Empty)
            {
                tiles[i].ClearAdjacents();  // 물/빈 타일은 인접 관계도 끊음
            }

            tiles[i].autoTileId = (int)tileType;
        }
    }

    public bool CreateIsland(
        float erodePercent, int erodeIterations, float lakePercent,
        float treePercent, float hillPercent, float mountainPercent, float townPercent, float monsterPercent)
    {
        // 해안선 침식 (섬 모양 다듬기)
        for (int i = 0; i < erodeIterations; i++)
        {
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }

        // 육지 타일에 지형 배치
        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);
        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hills);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountains);
        DecorateTiles(LandTiles, townPercent, TileTypes.Towns);
        DecorateTiles(LandTiles, monsterPercent, TileTypes.Monster);

        // Towns 중 2개를 골라 시작점/캐슬로 지정
        var towns = tiles.Where(x => x.autoTileId == (int)TileTypes.Towns).ToArray();
        ShuffleTiles(towns);

        startTile = towns[0];
        castleTile = towns[1];
        castleTile.autoTileId = (int)TileTypes.Castle;

        // 시작점→캐슬 경로가 존재하는 맵만 유효 (없으면 false → 재생성)
        var pathFinder = new PathFinder();
        var path = pathFinder.AStar(this, startTile, castleTile);
        return path.Count > 0;
    }
}
