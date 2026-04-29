using UnityEngine;

public class Stage : MonoBehaviour
{
    public GameObject tilePrefab;
    private GameObject[] tileObjs;
    private SpriteRenderer[] tileRenderers;

    public int mapWidth = 20;
    public int mapHeight = 20;

    [Range(0f, 0.9f)]
    public float erodePercent = 0.5f;
    public int erodeIteration = 2;

    [Range(0f, 0.9f)]
    public float lakePercent;
    [Range(0f, 0.9f)]
    public float treePercent;
    [Range(0f, 0.9f)]
    public float hillPercent;
    [Range(0f, 0.9f)]
    public float mountainPercent;
    [Range(0f, 0.9f)]
    public float townPercent;
    [Range(0f, 0.9f)]
    public float monsterPercent;

    public Vector2 tileSize = new(16, 16);

    public Sprite[] islandSprites;
    public Sprite[] fogSprites;

    public Map Map => map;
    private Map map;

    private Camera cam;

    private int prevTileId = -1;
    private bool isReady = false;  // 플레이 버튼 클릭이 첫 프레임에 감지되는 것을 방지

    public PlayerMovement playerPrefab;
    private PlayerMovement player;

    // 타일 배열 [0,0] 기준 월드 좌표 (왼쪽 상단)
    public Vector3 FirstTilePos
    {
        get
        {
            var pos = transform.position;
            pos.x -= (mapWidth * tileSize.x * 0.5f);
            pos.y += (mapHeight * tileSize.y * 0.5f);
            pos.x -= tileSize.x * 0.5f;
            pos.y += tileSize.y * 0.5f;
            return pos;
        }
    }

    private void Start()
    {
        cam = Camera.main;
        ResetStage();
        isReady = true;  // Start() 완료 후 마우스 입력 허용
    }

    private void Update()
    {
        // 스페이스바로 맵 재생성 (null 체크 전에 처리)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetStage();
        }

        if (cam == null || map == null || tileObjs == null) return;

        // 마우스 클릭으로 이동 (isReady: 시작 첫 프레임 클릭 무시)
        if (isReady && Input.GetMouseButtonDown(0))
        {
            int tiled = ScreenPosToTileId(Input.mousePosition);
            if (map.tiles[tiled].CanMove)
            {
                Debug.Log($"클릭한 타일 {tiled}");
                player.MoveTo(tiled);
            }
            else
            {
                Debug.Log($"이동불가한 타일입니다 ({tiled})");
            }
        }

        // 마우스 호버 타일 초록색 하이라이트
        if (tileObjs != null)
        {
            int currentTileId = ScreenPosToTileId(Input.mousePosition);

            if (prevTileId != currentTileId)
            {
                tileRenderers[currentTileId].color = Color.green;
                if (prevTileId >= 0 && prevTileId < tileRenderers.Length)
                {
                    tileRenderers[prevTileId].color = Color.white;  // 이전 타일 원래 색으로 복원
                }

                prevTileId = currentTileId;
            }
        }
    }

    private void ResetStage()
    {
        int attempts = 0;
        do
        {
            map = new Map();
            map.Init(mapHeight, mapWidth);
            attempts++;
            if (attempts >= 100)
            {
                Debug.LogError("생성 실패: 100회 시도 후에도 유효한 맵을 만들지 못했습니다.");
                break;
            }
        } while (!map.CreateIsland(erodePercent, erodeIteration, lakePercent, treePercent, hillPercent, mountainPercent, townPercent, monsterPercent));
        // CreateIsland가 false면 시작점→캐슬 경로 없음 → 재시도

        Debug.Log($"맵 생성 완료 ({attempts}회 시도)");
        CreateGrid();
        CreatePlayer();
    }

    private void CreatePlayer()
    {
        if (player != null)
        {
            Destroy(player.gameObject);  // 기존 플레이어 제거 (재생성 시)
        }

        player = Instantiate(playerPrefab);
        player.Warp(map.startTile.id);  // 시작 타일로 순간이동
    }

    private void CreateGrid()
    {
        // 기존 타일 오브젝트 제거 (재생성 시)
        if (tileObjs != null)
        {
            foreach (var tile in tileObjs)
            {
                Destroy(tile.gameObject);
            }
        }

        tileObjs = new GameObject[mapWidth * mapHeight];
        tileRenderers = new SpriteRenderer[mapWidth * mapHeight];

        var pos = FirstTilePos;

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                var tileId = i * mapWidth + j;

                var newGo = Instantiate(tilePrefab, transform);
                newGo.transform.position = pos;
                pos.x += tileSize.x;  // 다음 열로 이동

                tileObjs[tileId] = newGo;
                tileRenderers[tileId] = newGo.GetComponent<SpriteRenderer>();
                DecorateTile(tileId);  // 초기 스프라이트 적용
            }

            pos.x = FirstTilePos.x;  // 행 끝에서 x 초기화
            pos.y -= tileSize.y;     // 다음 행으로 이동
        }
    }

    public void DecorateTile(int tileId)
    {
        var tile = map.tiles[tileId];
        var renderer = tileRenderers[tileId];

        if (tile == null) return;

        if (tile.isVisited)
        {
            // 방문한 타일: 실제 지형 스프라이트 표시
            if (tile.autoTileId != (int)TileTypes.Empty)
                renderer.sprite = islandSprites[tile.autoTileId];
            else
                renderer.sprite = null;
        }
        else
        {
            // 미방문 타일: 안개 스프라이트 표시 (fogTileId가 유효할 때만)
            if (tile.autoTileId != (int)TileTypes.Empty && tile.fogTileId != (int)TileTypes.Empty && tile.fogTileId != -1)
                renderer.sprite = fogSprites[tile.fogTileId];
            else
                renderer.sprite = null;
        }
    }

    public void DecorateAllTiles()
    {
        // 모든 타일 스프라이트 일괄 갱신 (이동 후 시야 업데이트 시 호출)
        for (int i = 0; i < tileObjs.Length; i++)
        {
            DecorateTile(i);
        }
    }

    public void OnTileVisited(int tileId)
    {
        int centerX = tileId % mapWidth;
        int centerY = tileId / mapWidth;

        // 시야 범위 내 타일 방문 처리
        for (int i = -player.sight; i <= player.sight; i++)
        {
            for (int j = -player.sight; j <= player.sight; j++)
            {
                int x = centerX + i;
                int y = centerY + j;

                if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) continue;

                int viewTileId = y * mapWidth + x;
                Map.tiles[viewTileId].Visit();
            }
        }

        // 시야 외곽 +1 범위: 안개 경계 타일 fogTileId 갱신용
        var radius = player.sight + 1;
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                int x = centerX + i;
                int y = centerY + j;

                if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) continue;

                int viewTileId = y * mapWidth + x;
                Map.tiles[viewTileId].Visit();
            }
        }

        DecorateAllTiles();  // 방문 후 시각적 갱신
    }

    // 스크린 좌표 → 타일 id 변환
    public int ScreenPosToTileId(Vector3 screenPos)
    {
        screenPos.z = Mathf.Abs(transform.position.z - cam.transform.position.z);
        return WorldPosToTileId(cam.ScreenToWorldPoint(screenPos));
    }

    // 월드 좌표 → 타일 id 변환
    public int WorldPosToTileId(Vector3 worldPos)
    {
        int xIndex = (int)((worldPos.x - FirstTilePos.x) / tileSize.x + 0.5f);
        int yIndex = (int)((FirstTilePos.y - worldPos.y) / tileSize.y + 0.5f);
        xIndex = Mathf.Clamp(xIndex, 0, mapWidth - 1);
        yIndex = Mathf.Clamp(yIndex, 0, mapHeight - 1);
        return yIndex * mapWidth + xIndex;
    }

    public Vector3 GetTilePos(int tileId)
    {
        var y = tileId / mapWidth;
        var x = tileId % mapWidth;
        return GetTilePos(y, x);
    }

    // 행/열 인덱스 → 월드 좌표 변환
    public Vector3 GetTilePos(int y, int x)
    {
        var pos = FirstTilePos;
        pos.x += x * tileSize.x;
        pos.y -= y * tileSize.y;
        return pos;
    }
}
