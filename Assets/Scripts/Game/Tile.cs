using UnityEngine;


public enum Sides
{
    Bottom,
    Right,
    Left,
    Top,
};

public class Tile : MonoBehaviour
{
    public int Id;

    public Tile[] adjacents = new Tile[4];

    public int autoTiledId;

    public bool isVisited = false;

    public void UpDataAutoTileId()
    {
        autoTiledId = 0;
        for (int i = 0; i < adjacents.Length; ++i)
        {
            if ( adjacents[i] != null)
            {
                autoTiledId |= 1 << adjacents.Length - 1 - i;
            }
        }
    }
}
