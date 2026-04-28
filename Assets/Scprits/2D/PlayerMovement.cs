using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Animator animator;      
    private Stage stage;            
    private int currentTileId;     

    private void Awake()
    {
        animator = GetComponent<Animator>(); 
        animator.speed = 0f;                 
        var findGo = GameObject.FindWithTag("Map"); 
        stage = findGo.GetComponent<Stage>();       
    }

    private void Update()
    {
        var direection = Sides.None;                     
        if (Input.GetKeyDown(KeyCode.UpArrow))            
            direection = Sides.Top;
        else if (Input.GetKeyDown(KeyCode.DownArrow))     
            direection = Sides.Bottom;
        else if (Input.GetKeyDown(KeyCode.RightArrow))    
            direection = Sides.Right;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))     
            direection = Sides.Left;

        if (direection != Sides.None) 
        {
            var targetTile = stage.Map.tiles[currentTileId].adjacents[(int)direection]; // 해당 방향 인접 타일
            if (targetTile != null && targetTile.CanMove) // 인접 타일이 존재하고 이동 가능하면
            {
                MoveTo(targetTile.id); // 이동
            }
        }
    }

    public void MoveTo(int tileId)
    {
        currentTileId = tileId;                          // 현재 타일 ID 갱신
        transform.position = stage.GetTilePos(currentTileId); // 월드 좌표로 이동
        stage.RevealTiles(tileId, 1);                    // 반경 1(3x3) 범위 FOW 공개
    }
}
