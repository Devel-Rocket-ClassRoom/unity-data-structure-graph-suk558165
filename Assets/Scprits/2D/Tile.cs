using UnityEngine;

public enum Sides
{
    None = -1,
    Top,
    Left,
    Right,
    Bottom
}


public class Tile
{
    public int id;
    public int autoTileId;  // 에셋의 인덱스로 사용
    public int fogTileId = -1;  // 꽉 채워진 안개로 초기화

    public Tile[] adjacents = new Tile[4];  // 인접 노드 배열 (없으면 null)

    public bool isVisited = false;

    // Empty와 Mountains는 이동 불가
    public bool CanMove => autoTileId != (int)TileTypes.Empty && autoTileId != (int)TileTypes.Mountains;

    // 타일 종류별 이동 비용 (A* 가중치)
    public int MoveCost => (TileTypes)autoTileId switch
    {
      TileTypes.Grass => 1,
      TileTypes.Tree  => 2,
      TileTypes.Hills => 4,
      TileTypes.Towns => 1,
      TileTypes.Castle => 1,
      TileTypes.Mountains => int.MaxValue,  // 통과 불가
      TileTypes.Empty => int.MaxValue,      // 통과 불가
      _ => 1                                // 해안선 타일(0~14): 통과 가능
    };

    // A* 경로 역추적용 이전 타일 포인터
    public Tile previous = null;

    public void UpdateAutoTileId()
    {
        autoTileId = 0;

        // 인접 타일이 존재하는 방향의 비트를 켬 (스프라이트 인덱스 결정)
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] != null)
            {
                autoTileId |= 1 << i;
            }
        }
    }

    public void UpdateFogTileId()
    {
        fogTileId = 0;

        // 인접 타일이 없거나 미방문이면 해당 방향 안개 비트를 켬
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] == null || !adjacents[i].isVisited)
            {
                fogTileId |= 1 << i;
            }
        }
    }

    public void Visit()
    {
        if (isVisited) return;  // 이미 방문했으면 스킵

        isVisited = true;
        // 인접 타일들의 안개 상태 갱신
        foreach (var adjacent in adjacents)
        {
            if (adjacent != null)
            {
                adjacent.UpdateFogTileId();
            }
        }
    }

    public void RemoveAdjacents(Tile tile)
    {
        // 특정 타일과의 인접 관계를 제거
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] == null) continue;

            if (adjacents[i].id == tile.id)
            {
                adjacents[i] = null;
                UpdateAutoTileId();  // 스프라이트 인덱스 재계산
                break;
            }
        }
    }

    public void ClearAdjacents()
    {
        // 모든 인접 관계를 양방향으로 제거 (물/빈 타일로 만들 때 사용)
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] == null) continue;

            adjacents[i].RemoveAdjacents(this);  // 상대방도 나를 제거
            adjacents[i] = null;
        }

        UpdateAutoTileId();
    }
}
