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

    public Sprite[] fowSprites;            

    public PlayerMovement playerPrefab;    
    public PlayerMovement player;          
    private Camera mainCamera;             
    private Map map;                  
    public Map Map => map;                 

    // 타일[0,0]의 중심 월드 좌표
    // Stage 중점 → 맵 전체 절반만큼 왼쪽/위로 → 타일 반칸 오른쪽/아래로 보정
    private Vector3 FirstTilePos
    {
        get
        {
            var pos = transform.position;             // Stage 오브젝트의 월드 중심
            pos.x -= mapWedth * tileSize.x * 0.5f;  // 맵 전체 너비의 절반만큼 왼쪽
            pos.y += mapHeight * tileSize.y * 0.5f;  // 맵 전체 높이의 절반만큼 위
            pos.x += tileSize.x * 0.5f;              // 타일 중심 보정 (반칸 오른쪽)
            pos.y -= tileSize.y * 0.5f;              // 타일 중심 보정 (반칸 아래)
            return pos;                               // 타일[0,0]의 월드 중심 좌표
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main; // Camera.main은 호출마다 비용이 있으므로 한 번만 캐시
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Space 키 누르면
            ResetStage();                    // 맵 전체 재생성
    }

    private void ResetStage()
    {
        map = new Map();          // 맵 데이터 새로 생성
        map.Init(mapHeight, mapWedth); // 타일 배열 초기화 및 인접 연결
        map.CreateIsland(ecodePercent, ecodeIterations, lakePercent, treePercent,
                         hillPercent, mountainPercent, townPercent, monsterPercent); // 지형 배치
        CreateGrid();             // 타일 오브젝트 생성
        CreatePlayer();           // 플레이어 생성 및 배치
    }

    private void CreatePlayer()
    {
        if (player != null && player.gameObject.scene.IsValid()) // 기존 플레이어가 씬 인스턴스면
            Destroy(player.gameObject);     // 삭제 (프리팹 에셋은 건드리지 않음)
        player = Instantiate(playerPrefab); // 새 플레이어 생성
        player.MoveTo(map.startTile.id);    // 시작 마을로 이동 (RevealTiles도 여기서 호출됨)
    }

    // 타일 오브젝트 생성. 초기엔 전부 단색 FOW로 덮음
    private void CreateGrid()
    {
        if (tileObjs != null)               // 기존 타일이 있으면
        {
            foreach (var tile in tileObjs)
                Destroy(tile.gameObject);   // 전부 삭제
        }
        tileObjs = new GameObject[mapWedth * mapHeight]; // 타일 배열 새로 할당

        var firstPos = FirstTilePos;                            // 타일[0,0] 월드 좌표
        var position = new Vector3(firstPos.x, firstPos.y, 0); // 현재 생성 위치

        for (int i = 0; i < mapHeight; ++i)          // 행 루프 (위→아래)
        {
            for (int j = 0; j < mapWedth; ++j)       // 열 루프 (왼→오른)
            {
                var tileId = i * mapWedth + j;                   // 1차원 배열 인덱스
                var newGo = Instantiate(tilePrefabs, transform); // 타일 오브젝트 생성
                newGo.transform.position = position;             // 월드 위치 설정
                newGo.name = $"{i}, {j}";                        // 디버그용 이름 (행, 열)
                tileObjs[tileId] = newGo;                        // 배열에 저장
                newGo.GetComponent<SpriteRenderer>().enabled = false; // 초기값: 배경만 보임
                position.x += tileSize.x;                       // 다음 열로 이동
            }
            position.x = firstPos.x;  // 열 위치를 맨 왼쪽으로 리셋
            position.y -= tileSize.y; // 다음 행으로 이동 (아래 방향)
        }
    }

    // 방문 확정된 타일에 지형 스프라이트 적용
    public void DecorateTile(int tileId)
    {
        var tile = map.tiles[tileId];                            // 해당 타일 데이터
        var ren = tileObjs[tileId].GetComponent<SpriteRenderer>(); // SpriteRenderer
        if (tile.autoTileId != (int)TileTypes.Empty)             // 바다가 아니면
        {
            ren.enabled = true;                                  // 렌더러 켜기
            ren.sprite = islandSprites[tile.autoTileId];        // 지형 스프라이트 적용
        }
        else                                                     // 바다 타일이면
        {
            ren.enabled = false;                                 // 렌더러 끄기 (투명)
        }
    }

    // 플레이어 중심으로 radius 범위를 공개하고 경계에 FOW 스프라이트 적용
    public void RevealTiles(int centerTileId, int radius)
    {
        int cr = centerTileId / mapWedth; // 중심 타일의 행
        int cc = centerTileId % mapWedth; // 중심 타일의 열

        // 1단계: radius 범위 안 미방문 타일 공개
        for (int r = cr - radius; r <= cr + radius; r++)     // 행 루프
        {
            for (int c = cc - radius; c <= cc + radius; c++) // 열 루프
            {
                if (r < 0 || r >= mapHeight || c < 0 || c >= mapWedth) continue; // 맵 밖 skip
                int id = r * mapWedth + c;              // tileId 계산
                if (map.tiles[id].isVisited) continue;  // 이미 방문했으면 skip (영구 공개 유지)
                map.tiles[id].isVisited = true;         // 방문 표시
                DecorateTile(id);                       // 지형 스프라이트 적용
            }
        }

        // 2단계: 경계(radius+1) 미방문 타일에 FOW 엣지 스프라이트 갱신
        for (int r = cr - radius - 1; r <= cr + radius + 1; r++)     // 경계 행 루프
        {
            for (int c = cc - radius - 1; c <= cc + radius + 1; c++) // 경계 열 루프
            {
                if (r < 0 || r >= mapHeight || c < 0 || c >= mapWedth) continue; // 맵 밖 skip
                int id = r * mapWedth + c;              // tileId 계산
                if (!map.tiles[id].isVisited)           // 미방문 타일만
                    UpdateFowSprite(id);                // FOW 스프라이트 갱신
            }
        }
    }

    // 미방문 타일의 FOW 스프라이트를 인접 방문 타일 방향 비트마스크로 선택
    public void UpdateFowSprite(int tileId)
    {
        if (map.tiles[tileId].isVisited) return; // 방문한 타일은 처리 불필요

        int row = tileId / mapWedth; // 타일의 행
        int col = tileId % mapWedth; // 타일의 열

        // Sides 순서에 맞춘 방향 오프셋: Top=0, Left=1, Right=2, Bottom=3
        int[] dr = { -1,  0,  0, +1 }; // 각 방향의 행 오프셋
        int[] dc = {  0, -1, +1,  0 }; // 각 방향의 열 오프셋

        int mask = 0;               // 방문한 이웃 방향을 비트로 저장
        for (int i = 0; i < 4; i++) // 4방향 순회
        {
            int nr = row + dr[i];   // 이웃 행
            int nc = col + dc[i];   // 이웃 열
            if (nr < 0 || nr >= mapHeight || nc < 0 || nc >= mapWedth) continue; // 범위 밖 skip
            if (map.tiles[nr * mapWedth + nc].isVisited) // 이웃이 방문됐으면
                mask |= 1 << i;     // 해당 방향 비트 ON
        }

        var ren = tileObjs[tileId].GetComponent<SpriteRenderer>(); // SpriteRenderer
        if (mask == 0)
            ren.enabled = false;                        // 인접 방문 없음 → 배경만 보임
        else
        {
            ren.enabled = true;
            ren.sprite = fowSprites[15 - mask];         // 역순: mask=1→[14] ~ mask=15→[0]
        }
    }

    // 스크린 좌표 → tileId
    public int ScreenPosToTileId(Vector3 screenPos)
    {
        screenPos.z = Mathf.Abs(transform.position.z - mainCamera.transform.position.z); // z = 카메라와의 거리
        return WorldPosToTileId(mainCamera.ScreenToWorldPoint(screenPos));               // 스크린 → 월드 → tileId
    }

    // 월드 좌표 → tileId (-1이면 맵 범위 밖)
    public int WorldPosToTileId(Vector3 worldPos)
    {
        var first = FirstTilePos;                                         // 타일[0,0] 기준점
        int x = Mathf.RoundToInt((worldPos.x - first.x) / tileSize.x);  // 열 인덱스 계산
        int y = Mathf.RoundToInt((first.y - worldPos.y) / tileSize.y);  // 행 인덱스 계산
        if (x < 0 || x >= mapWedth || y < 0 || y >= mapHeight) return -1; // 범위 밖이면 -1
        return y * mapWedth + x;                                          // tileId 반환
    }

    // 행/열 → 월드 좌표 (타일 중심)
    public Vector3 GetTilePos(int y, int x)
        => FirstTilePos + new Vector3(x * tileSize.x, -y * tileSize.y, 0);

    // tileId → 월드 좌표
    public Vector3 GetTilePos(int tileId)
        => GetTilePos(tileId / mapWedth, tileId % mapWedth); // 행/열 분리 후 위 함수 호출
}
