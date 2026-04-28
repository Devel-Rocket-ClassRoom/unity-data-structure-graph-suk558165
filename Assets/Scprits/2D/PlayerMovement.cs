using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveDuration = 0.5f;

    private Stage stage;
    private int currentTileId;
    private bool isMoving;

    private void Awake()
    {
        stage = GameObject.FindWithTag("Map").GetComponent<Stage>();
    }

    private void Update()
    {
        if (isMoving) return;

        // 방향키 입력 감지
        var direction = Sides.None;

        if      (Input.GetKeyDown(KeyCode.UpArrow))    direction = Sides.Top;
        else if (Input.GetKeyDown(KeyCode.DownArrow))  direction = Sides.Bottom;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Sides.Right;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))  direction = Sides.Left;

        if (direction == Sides.None) return;

        // 해당 방향의 인접 타일이 존재하고 이동 가능하면 이동
        var targetTile = stage.Map.tiles[currentTileId].adjacents[(int)direction];
        if (targetTile != null && targetTile.CanMove)
            MoveTo(targetTile.id);
    }

    // 초기화 시 사용: 즉시 해당 타일로 이동
    public void Teleport(int tileId)
    {
        currentTileId = tileId;
        transform.position = stage.GetTilePos(tileId);
        stage.VisitCheck(tileId);
    }

    // 방향키 입력 시 사용: 부드럽게 해당 타일로 이동
    private void MoveTo(int tileId)
    {
        currentTileId = tileId;
        stage.VisitCheck(tileId);
        StartCoroutine(SmoothMove(stage.GetTilePos(tileId)));
    }

    // moveDuration 시간 동안 Lerp로 target까지 이동
    private IEnumerator SmoothMove(Vector3 target)
    {
        isMoving = true;
        var start = transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, elapsed / moveDuration);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }
}
