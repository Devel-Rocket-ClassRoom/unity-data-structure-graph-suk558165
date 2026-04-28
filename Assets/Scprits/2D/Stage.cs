using UnityEngine;

public class Stage : MonoBehaviour
{
    public GameObject tilePrefabs;
    private GameObject[] tileObjs;

    public int mapWedth = 20;
    public int mapHeight = 20;

    public int ecodeIterations = 2;

    [Range(0f, 0.9f)] public float ecodePercent = 0.5f;
    [Range(0f, 0.9f)] public float lakePercent = 0.1f;
    [Range(0f, 0.9f)] public float treePercent = 0.2f;
    [Range(0f, 0.9f)] public float hillPercent = 0.1f;
    [Range(0f, 0.9f)] public float mountainPercent = 0.05f;
    [Range(0f, 0.9f)] public float townPercent = 0.03f;
    [Range(0f, 0.9f)] public float monsterPercent = 0.02f;

    public Vector2 tileSize = new(16, 16);
    public Sprite[] islandSprites;

    public PlayerMovement playerPrefab;
    public PlayerMovement player;

    private Camera mainCamera;
    private Map map;
    public Map Map => map;

    private Vector3 FirstTilePos
    {
        get
        {
            var pos = transform.position;
            pos.x -= mapWedth * tileSize.x * 0.5f;
            pos.y += mapHeight * tileSize.y * 0.5f;
            pos.x += tileSize.x * 0.5f;
            pos.y -= tileSize.y * 0.5f;
            return pos;
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ResetStage();
    }

    private void ResetStage()
    {
        map = new Map();
        map.Init(mapHeight, mapWedth);
        map.CreateIsland(ecodePercent, ecodeIterations, lakePercent, treePercent, hillPercent, mountainPercent, townPercent, monsterPercent);
        CreateGrid();
        CreatePlayer();
    }

    private void CreatePlayer()
    {
        if (player != null && player.gameObject.scene.IsValid())
            Destroy(player.gameObject);
        player = Instantiate(playerPrefab);
        player.MoveTo(map.startTile.id);
    }

    private void CreateGrid()
    {
        if (tileObjs != null)
        {
            foreach (var tile in tileObjs)
                Destroy(tile.gameObject);
        }
        tileObjs = new GameObject[mapWedth * mapHeight];

        var firstPos = FirstTilePos;
        var position = new Vector3(firstPos.x, firstPos.y, 0);

        for (int i = 0; i < mapHeight; ++i)
        {
            for (int j = 0; j < mapWedth; ++j)
            {
                var tileId = i * mapWedth + j;
                var newGo = Instantiate(tilePrefabs, transform);
                newGo.transform.position = position;
                newGo.name = $"{i}, {j}";
                tileObjs[tileId] = newGo;
                position.x += tileSize.x;
                DecorateTile(tileId);
            }
            position.x = firstPos.x;
            position.y -= tileSize.y;
        }
    }

    public void DecorateTile(int tileId)
    {
        var tile = map.tiles[tileId];
        var ren = tileObjs[tileId].GetComponent<SpriteRenderer>();
        if (tile.autoTileId != (int)TileTypes.Empty)
        {
            ren.enabled = true;
            ren.sprite = islandSprites[tile.autoTileId];
        }
        else
        {
            ren.enabled = false;
        }
    }

    public int ScreenPosToTileId(Vector3 screenPos)
    {
        screenPos.z = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
        return WorldPosToTileId(mainCamera.ScreenToWorldPoint(screenPos));
    }

    public int WorldPosToTileId(Vector3 worldPos)
    {
        var first = FirstTilePos;
        int x = Mathf.RoundToInt((worldPos.x - first.x) / tileSize.x);
        int y = Mathf.RoundToInt((first.y - worldPos.y) / tileSize.y);
        if (x < 0 || x >= mapWedth || y < 0 || y >= mapHeight) return -1;
        return y * mapWedth + x;
    }

    public Vector3 GetTilePos(int y, int x)
        => FirstTilePos + new Vector3(x * tileSize.x, -y * tileSize.y, 0);

    public Vector3 GetTilePos(int tileId)
        => GetTilePos(tileId / mapWedth, tileId % mapWedth);
}
