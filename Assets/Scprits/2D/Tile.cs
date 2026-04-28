using UnityEngine;

public enum Sides
{
    None   = -1, 
    Top    =  0, 
    Left   =  1, 
    Right  =  2,
    Bottom =  3  
}

public class Tile
{
    public int id;                          
    public Tile[] adjacents = new Tile[4];  
    public int autoTileId;                  
    public int fowTileId;                
    public bool isVisited;                 

    public bool CanMove => autoTileId != (int)TileTypes.Empty; 

    // 인접 연결 상태를 비트마스크로 autoTileId에 반영
    public void UpdateAutoTileId()
    {
        autoTileId = 0;                              // 초기화
        for (int i = 0; i < adjacents.Length; i++)  // 4방향 순회
        {
            if (adjacents[i] != null)                // 연결된 이웃이 있으면
                autoTileId |= (1 << i);             // 해당 방향 비트 ON
        }
    }

    // 특정 타일과의 인접 연결 제거 후 autoTileId 갱신
    public void RemoveAdjacents(Tile tile)
    {
        for (int i = 0; i < adjacents.Length; ++i)  // 4방향 순회
        {
            if (adjacents[i] == null) continue;      // 연결 없으면 skip
            if (adjacents[i].id == tile.id)          // 제거 대상 타일이면
            {
                adjacents[i] = null;                 // 연결 끊기
                UpdateAutoTileId();                  // 비트마스크 갱신
                break;                               // 중복 없으므로 즉시 종료
            }
        }
    }

    // 모든 인접 연결 제거 (상대 타일에서도 이 타일을 제거)
    public void ClearAdjacents()
    {
        for (int i = 0; i < adjacents.Length; ++i)  // 4방향 순회
        {
            if (adjacents[i] == null) continue;              // 연결 없으면 skip
            adjacents[i].RemoveAdjacents(this);              // 상대 타일에서도 나를 제거
            adjacents[i] = null;                             // 내 연결도 끊기
        }
        UpdateAutoTileId(); // 모든 연결 제거 후 비트마스크 갱신
    }
}
