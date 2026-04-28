using UnityEngine;

public enum Sides
{
    None = -1,
    Top,
    Left,
    Right,
    Bottom,
}

public class Tile
{
    public int id;
    public Tile[] adjacents = new Tile[4];
    public int autoTileId;
    public int fowTileId;
    public bool isVisited;

    // 이동 가능 여부: Empty 타입이면 이동 불가
    public bool CanMove => autoTileId != (int)TileTypes.Empty;

    // 인접 타일 연결 상태를 비트마스크로 계산해 autoTileId 갱신
    public void UpdateAutoTileId()
    {
        autoTileId = 0;
        for (int i = 0; i < adjacents.Length; i++)
            if (adjacents[i] != null)
                autoTileId |= 1 << i;
    }

    // 인접 타일 연결 상태를 비트마스크로 계산해 fowTileId 갱신
    public void UpdateFowTileId()
    {
        fowTileId = 0;
        for (int i = 0; i < adjacents.Length; i++)
            if (adjacents[i] != null)
                fowTileId |= 1 << i;
    }

    // 특정 인접 타일과의 연결을 끊고 autoTileId 갱신
    public void RemoveAdjacent(Tile tile)
    {
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] != null && adjacents[i].id == tile.id)
            {
                adjacents[i] = null;
                UpdateAutoTileId();
                break;
            }
        }
    }

    // 모든 인접 타일과의 연결을 끊고 상대방에서도 자신을 제거 (맵 구멍 생성)
    public void ClearAdjacent()
    {
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] == null) continue;
            adjacents[i].RemoveAdjacent(this);
            adjacents[i] = null;
        }
        UpdateAutoTileId();
    }
}
