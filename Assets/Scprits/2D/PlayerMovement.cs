using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Animator animator;
    private Stage stage;
    private Map map;
    private int currentTileId;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;
        var findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();
        map = stage.Map;
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
            var targetTile = stage.Map.tiles[currentTileId].adjacents[(int)direection];
            if (targetTile != null && targetTile.CanMove)
            {
                MoveTo(targetTile.id);
            }
        }
    }

    public void MoveTo(int tileId)
    {
        currentTileId = tileId;
        transform.position = stage.GetTilePos(currentTileId);
    }
}
