using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Stage stage;
    private Animator animator;
    private int currentTileId = -1;  // 현재 위치 타일 id
    private int targetTileId = -1;   // 이동 중 새 클릭이 들어왔을 때 대기 목적지

    public float moveSpeed = 10f;
    public int sight = 3;
    private bool isMoving = false;
    private Coroutine coMove = null;

    private List<Tile> currentPath;          // 현재 이동 중인 경로
    private PathFinder pathFinder = new PathFinder();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;  // 시작 시 애니메이션 정지

        var findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();
    }

    private void Update()
    {
        // WASD 키보드 이동 (인접 한 칸)
        var direction = Sides.None;
        if (Input.GetKeyDown(KeyCode.W))
        {
            direction = Sides.Top;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            direction = Sides.Bottom;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            direction = Sides.Right;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            direction = Sides.Left;
        }

        if (direction != Sides.None)
        {
            var targetTile = stage.Map.tiles[currentTileId].adjacents[(int)direction];
            if (targetTile != null && targetTile.CanMove)
            {
                MoveTo(targetTile.id);
            }
        }
    }

    public void Warp(int tileId)
    {
        // 이동 중이면 즉시 중단
        if (coMove != null)
        {
            StopCoroutine(coMove);
            coMove = null;
        }
        isMoving = false;
        targetTileId = -1;

        animator.speed = 0f;
        currentTileId = tileId;
        transform.position = stage.GetTilePos(currentTileId);  // 순간이동
        stage.OnTileVisited(currentTileId);                     // 시야 갱신
    }

    public void MoveTo(int tileId)
    {
        // 코루틴이 없는데 isMoving이 true면 비정상 상태 → 강제 리셋
        if (coMove == null) isMoving = false;

        // A*로 현재 위치 → 목적지 경로 계산
        var path = pathFinder.AStar(stage.Map, stage.Map.tiles[currentTileId], stage.Map.tiles[tileId]);
        if (path.Count == 0)
        {
            Debug.LogError("경로 없음");
            return;
        }

        if (isMoving)
        {
            // 이동 중이면 현재 칸 완료 후 경로 교체 (targetTileId에 대기)
            targetTileId = tileId;
            return;
        }

        currentPath = path;
        if (coMove != null)
            StopCoroutine(coMove);
        isMoving = false;  // 새 코루틴 시작 전 명시적 리셋
        coMove = StartCoroutine(CoMovePath());
    }

    private IEnumerator CoMovePath()
    {
        isMoving = true;
        // 경로의 0번째는 현재 위치이므로 1번부터 이동
        for (int i = 1; i < currentPath.Count; i++)
        {
            int nextId = currentPath[i].id;
            yield return StartCoroutine(CoMoveStep(nextId));  // 한 칸 이동 완료 대기

            // 한 칸 도착 후 새 목적지가 대기 중이면 경로 교체
            if (targetTileId != -1)
            {
                currentPath = pathFinder.AStar(stage.Map, stage.Map.tiles[currentTileId], stage.Map.tiles[targetTileId]);
                Debug.Log($"경로 변경 → {targetTileId}");
                targetTileId = -1;
                i = 0;  // i++ 되면 1이 되어 새 경로 처음부터 재시작
            }
        }

        // 목적지 도착 후 대기 중인 새 목적지가 있으면 이어서 이동
        if (targetTileId != -1)
        {
            int pending = targetTileId;
            targetTileId = -1;
            isMoving = false;
            coMove = null;
            MoveTo(pending);
            yield break;
        }

        isMoving = false;
        coMove = null;
    }

    private IEnumerator CoMoveStep(int tileId)
    {
        animator.speed = 1f;
        var startPos = transform.position;
        var endPos = stage.GetTilePos(tileId);
        var duration = Vector3.Distance(startPos, endPos) / moveSpeed;  // 거리 / 속도 = 시간

        // Lerp로 부드럽게 한 칸 이동
        var t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        transform.position = endPos;  // 오차 보정
        animator.speed = 0f;

        currentTileId = tileId;               // 현재 위치 갱신
        stage.OnTileVisited(currentTileId);   // 시야 및 안개 갱신
        Debug.Log($"이동 완료: {currentTileId}");
    }
}
