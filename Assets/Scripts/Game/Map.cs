using System.IO;
using UnityEngine;


public enum TileTypes
{
    Empy = 1,
    Grasss = 15,
    Tree,
    Toms,
    Hilis,
    Catle,
    Monster,
};

public class Map
{
    public int rows = 8;
    public int cols = 0;

    public Tile[] tiles;

    public void Init(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;

        tiles = new Tile[rows * cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; ++c)
            {
                var index = r * cols + c;
                var adjacents = tiles[index].adjacents;

                if ((r - 1) >= 0)
                {
                    adjacents[(int)Sides.Top] = tiles[index - cols];
                }
                if ((c + 1) < cols)
                {
                    adjacents[(int)Sides.Right] = tiles[index + 1];
                }
                if ((c - 1) >= 0)
                {
                    adjacents[(int)Sides.Bottom] = tiles[index - 1];
                }
                if ((r + 1) < rows)
                {
                    adjacents[(int)Sides.Left] = tiles[index + cols];
                }
            }
        }
    }
}
