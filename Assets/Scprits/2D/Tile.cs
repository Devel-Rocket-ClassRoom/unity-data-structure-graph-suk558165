using UnityEngine;

public enum Sides
{
    None = -1,
    Top,    // 0번 비트
    Left,   // 1번 비트
    Right,  // 2번 비트
    Bottom  // 3번 비트
}

public class Tile
{
    public int id;
    public Tile[] adjacents = new Tile[4];
    public int autoTileId;
    public bool isVisited;

    public bool CanMove => autoTileId != (int)TileTypes.Empty;

    public void UpdateAutoTileId()
    {
        autoTileId = 0;
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] != null)
                autoTileId |= (1 << i);
        }
    }

    public void RemoveAdjacents(Tile tile)
    {
        for (int i = 0; i < adjacents.Length; ++i)
        {
            if (adjacents[i] == null) continue;
            if (adjacents[i].id == tile.id)
            {
                adjacents[i] = null;
                UpdateAutoTileId();
                break;
            }
        }
    }

    public void ClearAdjacents()
    {
        for (int i = 0; i < adjacents.Length; ++i)
        {
            if (adjacents[i] == null) continue;
            adjacents[i].RemoveAdjacents(this);
            adjacents[i] = null;
        }
        UpdateAutoTileId();
    }
}
