using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum GameState
{
    PlayerAction,
    EnemyAction
}
public enum PanelState
{
    None,
    ShowInformation,
    ShowToolTips,
}

public class Game : MonoBehaviour
{
    /// <summary>
    /// 地图管理器
    /// </summary>
    [Header("地图管理器")]
    [SerializeField]
    GameBoard board = default;

    //主动攻击方
    [SerializeField]
    private Unit active = default;
    //被动攻击方
    [SerializeField]
    private Unit passive = default;

    private PanelState panelState = PanelState.None;

    private void Awake()
    {
        board.InitLogicTiles();
        board.InitPlayers(new Vector2Int[] { new Vector2Int(0, 64), new Vector2Int(2, 68) });
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            //Battle.Instance.StartBattle();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() && UIManager.Instance.Information.alpha == 1)
            {
                PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
                eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                List<RaycastResult> result = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventDataCurrentPosition, result);

                if (result.Count > 0)
                {
                    foreach (var item in result)
                    {
                        //TODO   显示Tooltips 位置
                        UIManager.Instance.ShowToolTips(Input.mousePosition, item.gameObject.name);
                        panelState = PanelState.ShowToolTips;
                    }
                }
            }

            var point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector3Int worldPos = new Vector3Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), 0);
            var tile = board.GetTileByPos(worldPos);

            board.ClickOneTile(tile);

            //显示可移动范围
            //board.ShowUITile(worldPos,movePower, attackRange);
        }
        
        if(Input.GetKeyDown(KeyCode.K))
        {
            board.NextTurn();
        }

        if (Input.GetMouseButtonDown(1))
        {
            //鼠标放在角色身上右键显示角色信息
            if (GameBoard.Instance.currentPlayer == null && panelState == PanelState.None)
            {
                var point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                Vector3Int worldPos = new Vector3Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), 0);
                var tile = board.GetTileByPos(worldPos);

                var player = tile.PlayerOnTile;
                if (player != null)
                {
                    UIManager.Instance.ShowInformation(player);
                    panelState = PanelState.ShowInformation;
                }
            }

            if (GameBoard.Instance.currentPlayer == null && panelState == PanelState.ShowToolTips)
            {
                UIManager.Instance.HideToolTips();
                panelState = PanelState.ShowInformation;
            }


            //返回
            board.Cancel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelState == PanelState.ShowInformation)
            {
                UIManager.Instance.HideInformation();
                panelState = PanelState.None;
            }
            if (panelState == PanelState.ShowToolTips)
            {
                UIManager.Instance.HideToolTips();
                panelState = PanelState.ShowInformation;
            }
        }
    }
}
