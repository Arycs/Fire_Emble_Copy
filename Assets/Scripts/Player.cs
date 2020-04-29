using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,
    ReadyMove,
    Moving,
    MoveEnd,
    Gray
}

public class Player : MonoBehaviour
{
    [SerializeField]
    Animator animator;

    [SerializeField]
    [Header("玩家攻击范围")]
    private int attackRange;

    public LogicTile Tile;

    public PlayerState State
    {
        get; private set;
    } = PlayerState.Idle;

    public int AttackRange
    {
        get => attackRange;
    }

    [SerializeField]
    float moveSpeed;
    LogicTile endTile;

    public UnitAttr charAttr;

    void SetAnimation(int x,int y,bool isActive = true)
    {
        animator.SetBool("IsActive", isActive);
        animator.SetInteger("X", x);
        animator.SetInteger("Y", y);
    }

    public bool CanBeSelect()
    {
        if (State == PlayerState.Idle)
        {
            State = PlayerState.ReadyMove;
            //显示移动范围
            GameBoard.Instance.ShowUITile(Tile, charAttr.Move, AttackRange);
            SetAnimation(0, -1);
            return true;
        }
        return false;
    }

    public void MoveTo(LogicTile endTile)
    {
        var path = GameBoard.Instance.MoveToDestination(Tile, endTile);
        StartCoroutine(Move(path));
    }

    IEnumerator Move(List<LogicTile> path)
    {
        if (path.Count >=2)
        {
            State = PlayerState.Moving;
            Vector2Int previousVector = Vector2Int.zero;
            for (int i = 1; i < path.Count; i++)
            {
                var tile = path[i - 1];
                var nextTile = path[i];
                Vector3 nextPos = GameBoard.Instance.GetTileWorldPos(nextTile) + new Vector3(0.5f, 0, 0);
                Vector2Int currentVector = new Vector2Int(nextTile.X - tile.X, nextTile.Y - tile.Y);
                if (currentVector != previousVector)
                {
                    SetAnimation(currentVector.x, currentVector.y);
                }

                while (Vector3.Distance(transform.position,nextPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, nextPos,moveSpeed * Time.deltaTime);
                    yield return null;
                }
            }
        }
        State = PlayerState.MoveEnd;
        endTile = path[path.Count - 1];

        Vector2 temp = GameBoard.Instance.CheckAround(endTile);

        GameBoard.Instance.ShowMenu(temp);

        //显示攻击范围
        //GameBoard.Instance.ShowUITile(path[path.Count - 1], 0, AttackRange, false);
    }

    public void GoBack()
    {
        State = PlayerState.ReadyMove;
        SetAnimation(0, -1);
        GameBoard.Instance.ShowUITile(Tile, charAttr.Move, AttackRange);
        transform.position = GameBoard.Instance.GetTileWorldPos(Tile) + new Vector3(0.5f, 0, 0);
    }
    
    public void Standby()
    {
        State = PlayerState.Gray;
        SetAnimation(0, 0, false);
        Tile.PlayerOnTile = null;
        Tile = endTile;
        Tile.PlayerOnTile = this;
    }

    public void NextTurn()
    {
        State = PlayerState.Idle;
        SetAnimation(0, 0);
    }

    public void Recover()
    {
        State = PlayerState.Idle;
        SetAnimation(0, 0);
        GameBoard.Instance.ClearAllUITiles();
    }
}


