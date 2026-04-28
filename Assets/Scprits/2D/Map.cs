using UnityEngine;
using System.Linq;

public enum TileTypes
{
    Empty = -1,    // 바다 (렌더러 꺼짐)
    // 0~14 해안선 (autoTileId 비트마스크 값)
    Grass = 15,    // 육지 (4방향 모두 연결된 순수 내륙)
    Tree,          // 나무
    Hills,         // 언덕
    Mountains,     // 산
    Towns,         // 마을
    Castle,        // 성 (맵에 1개, towns[0]이 변환됨)
    Monster        // 몬스터
}

public class Map
{
    public int rows = 0; // 맵 세로 타일 수
    public int cols = 0; // 맵 가로 타일 수

    public Tile[] tiles;      // 전체 타일 1차원 배열 (인덱스 = tileId)
    public Tile startTile;    // 플레이어 시작 타일 (마을 중 하나)
    public Tile castleTile;   // 성 타일 (마을 중 하나가 성으로 변환)

    // autoTileId가 0~14인 타일 (해안선: 4방향 중 일부가 끊긴 육지)
    public Tile[] CoastTiles => tiles.Where(t => t.autoTileId >= 0 && t.autoTileId < (int)TileTypes.Grass).ToArray();
    // autoTileId가 15인 타일 (순수 내륙: 4방향 모두 연결)
    public Tile[] LandTiles => tiles.Where(t => t.autoTileId == (int)TileTypes.Grass).ToArray();

    // 타일 배열 초기화 및 인접 연결 설정
    public void Init(int rows, int cols)
    {
        this.rows = rows; // 행 수 저장
        this.cols = cols; // 열 수 저장
        tiles = new Tile[rows * cols]; // 전체 타일 수만큼 배열 할당

        for (int i = 0; i < tiles.Length; i++) // 타일 생성 루프
        {
            tiles[i] = new Tile(); // 타일 인스턴스 생성
            tiles[i].id = i;       // tileId 부여 (= 배열 인덱스)
        }

        for (int r = 0; r < rows; r++)   // 행 루프
        {
            for (int c = 0; c < cols; c++) // 열 루프
            {
                int index = r * cols + c;  // 현재 타일 인덱스
                if ((r + 1) < rows) tiles[index].adjacents[(int)Sides.Bottom] = tiles[index + cols]; // 아래 타일 연결
                if ((c + 1) < cols) tiles[index].adjacents[(int)Sides.Right]  = tiles[index + 1];   // 오른쪽 타일 연결
                if ((c - 1) >= 0)   tiles[index].adjacents[(int)Sides.Left]   = tiles[index - 1];   // 왼쪽 타일 연결
                if ((r - 1) >= 0)   tiles[index].adjacents[(int)Sides.Top]    = tiles[index - cols]; // 위 타일 연결
            }
        }

        for (int i = 0; i < tiles.Length; ++i)
            tiles[i].UpdateAutoTileId(); // 인접 연결 기반으로 autoTileId(비트마스크) 계산
    }

    // Fisher-Yates 셔플: 배열을 무작위로 섞음
    public void ShuffleTiles(Tile[] tiles)
    {
        for (int i = tiles.Length - 1; i > 0; --i) // 뒤에서부터 순회
        {
            int rand = Random.Range(0, i + 1);           // 0~i 사이 무작위 인덱스
            (tiles[rand], tiles[i]) = (tiles[i], tiles[rand]); // 위치 교환
        }
    }

    // 주어진 타일 배열에서 percent 비율만큼 tileType으로 변경
    public void DecoeateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        ShuffleTiles(tiles);                                 // 먼저 셔플해서 무작위 선택
        int total = Mathf.FloorToInt(tiles.Length * percent); // 변경할 타일 수 계산
        for (int i = 0; i < total; ++i)                      // total 수만큼 변경
        {
            if (tileType == TileTypes.Empty)
                tiles[i].ClearAdjacents(); // 바다로 만들 때는 인접 연결도 제거
            tiles[i].autoTileId = (int)tileType; // 타일 타입 변경
        }
    }

    // 섬 생성: 침식 → 지형 배치 → 성/시작 타일 지정
    public bool CreateIsland(
        float erodePercent, int erodeIterations,
        float lakePercent, float treePercent, float hillPercent,
        float mountainPercent, float townPercent, float monsterPercent)
    {
        for (int i = 0; i < erodeIterations; i++)
            DecoeateTiles(CoastTiles, erodePercent, TileTypes.Empty); // 해안선 반복 침식

        DecoeateTiles(LandTiles, lakePercent,    TileTypes.Empty);     // 호수 생성
        DecoeateTiles(LandTiles, treePercent,    TileTypes.Tree);      // 나무 배치
        DecoeateTiles(LandTiles, hillPercent,    TileTypes.Hills);     // 언덕 배치
        DecoeateTiles(LandTiles, mountainPercent,TileTypes.Mountains); // 산 배치
        DecoeateTiles(LandTiles, monsterPercent, TileTypes.Monster);   // 몬스터 배치
        DecoeateTiles(LandTiles, townPercent,    TileTypes.Towns);     // 마을 배치

        var towns = tiles.Where(x => x.autoTileId == (int)TileTypes.Towns).ToArray(); // 마을 타일만 추출
        if (towns.Length < 2) return false; // 마을이 2개 미만이면 생성 실패
        ShuffleTiles(towns);                // 마을 목록 셔플
        castleTile = towns[0];              // 첫 번째 마을 → 성
        startTile  = towns[1];              // 두 번째 마을 → 시작 지점
        castleTile.autoTileId = (int)TileTypes.Castle; // 성 타입으로 변경
        return true;                        // 생성 성공
    }
}
