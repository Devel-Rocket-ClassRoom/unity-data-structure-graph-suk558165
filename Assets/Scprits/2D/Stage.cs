using UnityEngine;

public class Stage : MonoBehaviour
{
    public GameObject tilePrefabs;
    private GameObject[] tileObjs;

    public int mapWidth = 20;
    public int mapHeight = 20;

    [Range(0f, 0.9f)] public float erodePercent = 0.5f;
    public int erodeIterations = 2;
    [Range(0f, 0.9f)] public float lakePercent = 0.1f;
    [Range(0f, 0.9f)] public float treePercent = 0.1f;
    [Range(0f, 0.9f)] public float hillPercent = 0.1f;
    [Range(0f, 0.9f)] public float mountainPercent = 0.1f;
    [Range(0f, 0.9f)] public float townPercent = 0.1f;
    public float monsterPercent = 0.1f;

    public Vector2 tileSize = new Vector2(16, 16);
    public Sprite[] islandSprites;
    public Sprite[] FowSprites;

    private Map map;
    public Map Map => map;
    private Camera cam;

    public PlayerMovement playerPrefab;
    private PlayerMovement player;

    // 맵 좌상단 첫 번째 타일의 월드 좌표
    private Vector3 FirstPos
    {
        get
        {
            var pos = transform.position;
            pos.x += -(mapWidth * tileSize.x * 0.5f) + tileSize.x * 0.5f;
            pos.y += (mapHeight * tileSize.y * 0.5f) - tileSize.y * 0.5f;
            return pos;
        }
    }

    private GameObject prevHover;
    private void Awake() => cam = Camera.main;

    private void Update()
    {
        // Space: 맵 재생성
        if (Input.GetKeyDown(KeyCode.Space))
            ResetStage();

        if (tileObjs == null || tileObjs.Length == 0) return;

        // 마우스 호버 타일 초록색 강조
        int index = ScreenPosToTileId(Input.mousePosition);
        if (index < 0 || index >= tileObjs.Length) return;

        GameObject currentTile = tileObjs[index];
        if (prevHover == currentTile) return;

        if (prevHover != null)
            prevHover.GetComponent<SpriteRenderer>().color = Color.white;

        currentTile.GetComponent<SpriteRenderer>().color = Color.green;
        prevHover = currentTile;
    }

    // 맵과 플레이어를 초기화하고 새로 생성
    private void ResetStage()
    {
        map = new Map();
        map.Init(mapHeight, mapWidth);
        map.CreateIsland(erodePercent, erodeIterations, lakePercent, treePercent, hillPercent, mountainPercent, townPercent, monsterPercent);
        CreateGrid();
        CreatePlayer();
    }

    // 플레이어 인스턴스 생성 후 시작 타일로 순간이동
    private void CreatePlayer()
    {
        if (player != null) Destroy(player.gameObject);
        player = Instantiate(playerPrefab);
        player.Teleport(map.startTile.id);
    }

    // 플레이어 주변 range 범위 타일을 방문 처리하고 스프라이트 갱신
    public void VisitCheck(int tileId)
    {
        int range = 1;
        int centerX = tileId % mapWidth;
        int centerY = tileId / mapWidth;

        for (int y = centerY - range; y <= centerY + range; y++)
        {
            if (y < 0 || y >= mapHeight) continue;
            for (int x = centerX - range; x <= centerX + range; x++)
            {
                if (x < 0 || x >= mapWidth) continue;
                int idx = y * mapWidth + x;
                map.tiles[idx].isVisited = true;
                if (tileObjs != null) DecorateTile(idx);
            }
        }
    }

    // 타일 오브젝트를 전부 새로 생성하고 배치
    private void CreateGrid()
    {
        if (tileObjs != null)
            foreach (var tileObj in tileObjs)
                Destroy(tileObj.gameObject);

        tileObjs = new GameObject[mapWidth * mapHeight];
        var position = FirstPos;

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                int tileId = i * mapWidth + j;
                var go = Instantiate(tilePrefabs, transform);
                go.transform.position = position;
                tileObjs[tileId] = go;
                position.x += tileSize.x;
                DecorateTile(tileId);
            }
            position.x = FirstPos.x;
            position.y -= tileSize.y;
        }
    }

    // 방문 여부에 따라 지형 스프라이트 또는 안개 스프라이트 적용
    public void DecorateTile(int tileId)
    {
        var tile = map.tiles[tileId];
        var ren = tileObjs[tileId].GetComponent<SpriteRenderer>();

        if (!tile.isVisited)
        {
            ren.sprite = FowSprites[tile.fowTileId];
            return;
        }

        ren.sprite = tile.autoTileId != (int)TileTypes.Empty ? islandSprites[tile.autoTileId] : null;
    }

    // 스크린 좌표를 타일 인덱스로 변환
    public int ScreenPosToTileId(Vector3 screenPos)
    {
        screenPos.z = Mathf.Abs(transform.position.z - cam.transform.position.z);
        return WorldPosToTileId(cam.ScreenToWorldPoint(screenPos));
    }

    // 월드 좌표를 타일 인덱스로 변환
    public int WorldPosToTileId(Vector3 worldPos)
    {
        float posX = FirstPos.x - tileSize.x * 0.5f;
        float posY = FirstPos.y + tileSize.y * 0.5f;

        int x = Mathf.Clamp(Mathf.FloorToInt((worldPos.x - posX) / tileSize.x), 0, mapWidth - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((posY - worldPos.y) / tileSize.y), 0, mapHeight - 1);

        return y * mapWidth + x;
    }

    // tileId로 월드 좌표 반환
    public Vector3 GetTilePos(int tileId) => GetTilePos(tileId / mapWidth, tileId % mapWidth);
    public Vector3 GetTilePos(int y, int x) => FirstPos + new Vector3(x * tileSize.x, -y * tileSize.y);
}
