using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum UITileType
{
    Move,
    Attack
}

public class GameBoard : Singleton<GameBoard>
{
    /// <summary>
    /// Tilemap 地图的数据 长,宽
    /// </summary>
    Vector2Int walkTileSize;
    Vector2Int cannotWalkTileSize;

    [Header("Tilemap 的地图")]
    [SerializeField]
    Tilemap walkTilemap = default;
    [SerializeField]
    Tilemap cannotWalkTilemap = default;

    [Header("生成移动范围/攻击范围/治疗范围的父物体")]
    [SerializeField]
    Transform uiTiles = default;

    [Header("移动范围,攻击范围,治疗范围 预制体色块")]
    [SerializeField]
    GameObject[] uiTilePrefabs;

    /// <summary>
    /// 获得到的该Tilemap中的每一个色块
    /// </summary>
    TileBase[] walkTiles;
    TileBase[] cannotWalkTiles;
    /// <summary>
    /// 每一个色块身上都带着的逻辑脚本
    /// </summary>
    LogicTile[] logicTiles;

    Queue<LogicTile> searth = new Queue<LogicTile>();
    /// <summary>
    /// 可移动范围
    /// </summary>
    List<LogicTile> moveTiles = new List<LogicTile>();

    /// <summary>
    /// 可攻击范围
    /// </summary>
    List<LogicTile> attackTiles = new List<LogicTile>();

    /// <summary>
    /// 显示移动范围/攻击范围/治疗范围的对应GameObject
    /// </summary>
    List<GameObject> allUITiles = new List<GameObject>();

    /// <summary>
    /// 字典,存储世界坐标的位置,与相应位置对应的色块身上的LogicTile
    /// </summary>
    Dictionary<Vector3, LogicTile> allLogicTilesDic = new Dictionary<Vector3, LogicTile>();

    /// <summary>
    /// 用来存储，不可行走色块的字典
    /// </summary>
    Dictionary<Vector3, LogicTile> canNotWalkDic = new Dictionary<Vector3, LogicTile>();

    public Player currentPlayer = null;

    public Player targetPlayer = null;

    [SerializeField]
    Player[] playerPrefabs;

    List<Player> AllMyPlayers = new List<Player>();

    public List<LogicTile> MoveTiles
    {
        get => moveTiles;
    }

    /// <summary>
    /// 初始化,将Tilemap地图的每个色块进行添加逻辑脚本,以及保存对应世界坐标位置
    /// </summary>
    public void InitLogicTiles()
    {
        // 返回的是 Tilemap 左下角的第一个点(基于0,0,0)的坐标,以及该 Tilemap 的Size
        var walkBounds = walkTilemap.cellBounds;
        var cannotWalkBounds = walkTilemap.cellBounds;
        // GetTilesBlock() 获取 某个范围中所有的 Tilemap 块, 左下角第一个为索引0,横向依次增加
        walkTiles = walkTilemap.GetTilesBlock(walkBounds);
        cannotWalkTiles = cannotWalkTilemap.GetTilesBlock(cannotWalkBounds);

        //size 返回 tilemap地图 长,宽
        walkTileSize = (Vector2Int)walkTilemap.size;
        cannotWalkTileSize = (Vector2Int)cannotWalkTilemap.size;

        logicTiles = new LogicTile[walkTileSize.x * walkTileSize.y];

        for (int y = 0 , i =0; y < walkTileSize.y; y++)
        {
            for (int x = 0; x < walkTileSize.x; x++,i++)
            {
                var tile = logicTiles[i] = new LogicTile(x, y, walkTiles[i]);

                if (x > 0)
                {
                    LogicTile.MakEastWestNeighbor(tile, logicTiles[i - 1]);
                }
                if (y > 0)
                {
                    LogicTile.MakeNorthSouthNeighbor(tile, logicTiles[i - walkTileSize.x]);
                }
            }
        }

        //遍历WalkTilemap地图 利用CellToWorld 将对应坐标转换成世界坐标
        int index = 0;
        foreach (var pos in walkTilemap.cellBounds.allPositionsWithin)
        {
            var worldPos = walkTilemap.CellToWorld(pos);

            allLogicTilesDic.Add(worldPos, logicTiles[index]);

            logicTiles[index].LocalPos = pos;
            index++;
        }

        foreach (var pos in cannotWalkTilemap.cellBounds.allPositionsWithin)
        {
            var worldPos = cannotWalkTilemap.CellToWorld(pos);
            var tileTemp = cannotWalkTilemap.GetTile(Vector3Int.CeilToInt(worldPos));
            if (tileTemp != null)
            {
                allLogicTilesDic[worldPos].CanWalk = false;
            }
        }
        
       
    }

    /// <summary>
    /// 将地图上移动位置/攻击位置/治疗位置隐藏
    /// </summary>
    public void ClearAllUITiles()
    {
        for (int i = 0; i < allUITiles.Count; i++)
        {
            Destroy(allUITiles[i]);
        }
        allUITiles.Clear();
    }

    /// <summary>
    /// 显示移动范围UI
    /// </summary>
    public void ShowUITile(LogicTile tile, int movePower, int attackRange,bool showMove = true,bool showAttack = true)
    {
        ClearAllUITiles();

        FindMovePath(tile, movePower);

        FindAttackPath(attackRange);

        if (showMove)
        {
            ShowOneTile(MoveTiles, UITileType.Move);
        }
        if (showAttack)
        {
            ShowOneTile(attackTiles, UITileType.Attack);
        }
    }

    /// <summary>
    /// 显示范围
    /// </summary>
    void ShowOneTile(List<LogicTile> tiles, UITileType type)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];

            Vector3Int localPos = tile.LocalPos;

            var worldP = walkTilemap.CellToWorld(localPos);

            var prefab = Instantiate(uiTilePrefabs[(int)type]);
            prefab.transform.SetParent(uiTiles);
            prefab.transform.position = worldP;
            allUITiles.Add(prefab);
        }
    }

    /// <summary>
    /// 寻找可移动路径
    /// </summary>
    public void FindMovePath(LogicTile startTile, int movePower)
    {
        foreach (var tile in logicTiles)
        {
            //将每块色块的距离设置为无限大
            tile.Clear();
        }

        MoveTiles.Clear();

        searth.Enqueue(startTile);
        startTile.InitPos(movePower);

        while (searth.Count > 0)
        {
            var tile = searth.Dequeue();
            if (tile != null)
            {
                MoveTiles.Add(tile);
                searth.Enqueue(tile.GrowNorth());
                searth.Enqueue(tile.GrowEast());
                searth.Enqueue(tile.GrowSouth());
                searth.Enqueue(tile.GrowWest());
            }
        }
    }

    /// <summary>
    /// 寻找攻击范围
    /// </summary>
    void FindAttackPath(int attackRange)
    {
        attackTiles.Clear();
        //寻找 MoveTile的边缘格子
        var boundTiles = LogicTile.GetBoundTiles(MoveTiles);
        for (int i = 0; i < boundTiles.Count; i++)
        {
            var tile = boundTiles[i];
            searth.Enqueue(tile);
            tile.InitAttackRange(attackRange);
        }
        while (searth.Count > 0)
        {
            var tile = searth.Dequeue();
            if (tile != null)
            {
                if (!MoveTiles.Contains(tile))
                {
                    attackTiles.Add(tile);
                }
                searth.Enqueue(tile.GrowAttackNorth());
                searth.Enqueue(tile.GrowAttackEast());
                searth.Enqueue(tile.GrowAttackSouth());
                searth.Enqueue(tile.GrowAttackWest());
            }
        }
    }

    /// <summary>
    /// 寻找路径
    /// </summary>
    public List<LogicTile> MoveToDestination(LogicTile start,LogicTile end)
    {
        List<LogicTile> results = new List<LogicTile>();
        LogicTile current = end;
        while (current.NextOnPath != null)
        {
            results.Add(current);
            current = current.NextOnPath;
        }
        results.Add(start);
        results.Reverse();
        return results;
    }

    /// <summary>
    /// 根据世界坐标获得Tile
    /// </summary>
    public LogicTile GetTileByPos(Vector3Int worldPos)
    {
        if (allLogicTilesDic.TryGetValue(worldPos,out LogicTile tile))
        {
            return tile;
        }
        return null;
    }

    public void ClickOneTile(LogicTile tile)
    {
        if (tile == null)
        {
            return;
        }
        if (currentPlayer == null)
        {
            var player = tile.PlayerOnTile;
            if (player != null && player.CanBeSelect())
            {
                currentPlayer = player;
            }
        }
        else
        {
            if (IsMoveRange(tile) && currentPlayer.State != PlayerState.Moving && currentPlayer.State != PlayerState.MoveEnd && (tile.PlayerOnTile == currentPlayer || tile.PlayerOnTile == null ))
            {
                currentPlayer.MoveTo(tile);
            }
            else if (IsAttackRange(tile) && tile.PlayerOnTile != null)
            {
                if (tile.PlayerOnTile.charAttr.Team == TeamType.Enemy)
                {
                    Debug.Log(tile.PlayerOnTile.name);
                    targetPlayer = tile.PlayerOnTile;
                    Battle.Instance.StartBattle(currentPlayer.charAttr.rAc,targetPlayer.charAttr.rAc);
                }
            }
        }
    }

    /// <summary>
    /// 点击点是否在移动范围
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    bool IsMoveRange(LogicTile tile)
    {
        return MoveTiles.Contains(tile);
    }

    bool IsAttackRange(LogicTile tile)
    {
        return attackTiles.Contains(tile);
    }
    /// <summary>
    /// 初始化 角色位置
    /// </summary>
    /// <param name="index"></param>
    public void InitPlayers(Vector2Int[] index)
    {
        for (int i = 0; i < index.Length; i++)
        {
            var tile = logicTiles[index[i].y];
            var worldpos = GetTileWorldPos(tile);
            var player = Instantiate(playerPrefabs[index[i].x]);
            AllMyPlayers.Add(player);
            player.transform.position = worldpos + new Vector3(0.5f, 0,0);

            tile.PlayerOnTile = player;
            player.Tile = tile;
        }
    }

    /// <summary>
    /// 检查四周敌人
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>Vector2，第一个代表敌人数量，第二个代表友军数量</returns>
    public Vector2 CheckAround(LogicTile tile)
    {
        Vector2 temp = new Vector2(0, 0); 
        if (tile.north.PlayerOnTile != null)
        {
            if (tile.north.PlayerOnTile.charAttr.Team == TeamType.Enemy)
            {
                temp.x++;
            }
            if (tile.north.PlayerOnTile.charAttr.Team == TeamType.My)
            {
                temp.y++;
            }
        }
        if (tile.west.PlayerOnTile != null )
        {
            if (tile.west.PlayerOnTile.charAttr.Team == TeamType.Enemy)
            {
                temp.x++;
            }
            if (tile.west.PlayerOnTile.charAttr.Team == TeamType.My)
            {
                temp.y++;
            }
        }
        if (tile.east.PlayerOnTile != null)
        {
            if (tile.east.PlayerOnTile.charAttr.Team == TeamType.Enemy)
            {
                temp.x++;
            }
            if (tile.east.PlayerOnTile.charAttr.Team == TeamType.My)
            {
                temp.y++;
            }
        }
        if (tile.south.PlayerOnTile != null)
        {
            if (tile.south.PlayerOnTile.charAttr.Team == TeamType.Enemy)
            {
                temp.x++;
            }
            if (tile.south.PlayerOnTile.charAttr.Team == TeamType.My)
            {
                temp.y++;
            }
        }
        return temp;
    }


    public Vector3 GetTileWorldPos(LogicTile tile)
    {
        var pos = tile.LocalPos;
        return walkTilemap.CellToWorld(pos);
    }


    public void Cancel()
    {
        UIManager.Instance.HideMene();
        if (currentPlayer !=null)
        {
            switch (currentPlayer.State)
            {
                case PlayerState.ReadyMove:
                    currentPlayer.Recover();
                    currentPlayer = null;
                    break;
                case PlayerState.MoveEnd:
                    currentPlayer.GoBack();
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 待机
    /// </summary>
    public void Standby()
    {
        ClearAllUITiles();
        UIManager.Instance.HideMene();

        if (currentPlayer != null && currentPlayer.State == PlayerState.MoveEnd)
        {
            currentPlayer.Standby();
            currentPlayer = null;
        }
    }

    /// <summary>
    /// 下一回合
    /// </summary>
    public void NextTurn()
    {
        currentPlayer = null;
        foreach (var player in AllMyPlayers)
        {
            player.NextTurn();
        }
    }

    public void ShowMenu(Vector2 temp)
    {
        ClearAllUITiles();
        UIManager.Instance.ShowMenu(temp);
    }

    public void ShowAttack()
    {
        UIManager.Instance.HideMene();

        //显示攻击范围
        GameBoard.Instance.ShowUITile(GetTileByPos(new Vector3Int((int)(currentPlayer.transform.position.x - 0.5f), (int)currentPlayer.transform.position.y, (int)currentPlayer.transform.position.z)), 0, currentPlayer.AttackRange, false);
    }
}
