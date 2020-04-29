
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LogicTile
{
    /// <summary>
    /// 剩余行动力
    /// </summary>
    int leftCost;

    /// <summary>
    /// 当前格子需要的行动力
    /// </summary>
    [SerializeField]
    int moveCost = 1;

    /// <summary>
    /// 当前格子需要的攻击距离
    /// </summary>
    const int ATTACK_COST = 1;
    /// <summary>
    /// 剩余的攻击距离
    /// </summary>
    int leftAttack;

    /// <summary>
    /// 是否可移动标志
    /// </summary>
    int distance;

    public int X
    {
        get;  private set;
    }
    public int Y
    {
        get; private set;
    }
    public TileBase Tile
    {
        get; private set;
    }

    /// <summary>
    /// 路径中的下一个点
    /// </summary>
    public LogicTile NextOnPath
    {
        get; private set;
    }
    /// <summary>
    /// 对应世界坐标位置
    /// </summary>
    public Vector3Int LocalPos
    {
        get; set;
    }

    public Player PlayerOnTile
    {
        get;set;
    }

    public bool CanWalk
    {
        get; set;
    } = true;

    /// <summary>
    /// 北,东,南,西
    /// </summary>
    public LogicTile north, east, south, west;

    /// <summary>
    /// 是否走过的路径,没走过distance是最大值
    /// </summary>
    public bool HasPath => distance != int.MaxValue;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="x">基于(0,0),在地图上X的相对位置</param>
    /// <param name="y">基于(0,0),在地图上Y的相对位置</param>
    /// <param name="tile">对应的色块</param>
    public LogicTile(int x,int y,TileBase tile)
    {
        X = x;
        Y = y;
        Tile = tile;
    }

    /// <summary>
    /// 设置该色块的东,西方向临近色块
    /// </summary>
    /// <param name="east">东面</param>
    /// <param name="west">西面</param>
    public static void MakEastWestNeighbor(LogicTile east,LogicTile west)
    {
        east.west = west;
        west.east = east;
    }
    /// <summary>
    /// 设置色块的北,南的临近色块
    /// </summary>
    /// <param name="north">北面</param>
    /// <param name="south">南面</param>
    public static void MakeNorthSouthNeighbor(LogicTile north, LogicTile south)
    {
        north.south = south;
        south.north = north;
    }

    /// <summary>
    /// 将所有瓦片设置成没有走过
    /// </summary>
    public void Clear()
    {
        distance = int.MaxValue;
        leftCost = 0;
        leftAttack = 0;
        NextOnPath = null;
    }

    /// <summary>
    /// 初始化位置,玩家站立的位置
    /// </summary>
    /// <param name="movement"></param>
    public void InitPos(int movement)
    {
        leftCost = movement;
        distance = 0;
        NextOnPath = null;

    }
    /// <summary>
    /// 初始化攻击范围
    /// </summary>
    /// <param name="range"></param>
    public void InitAttackRange(int range)
    {
        leftAttack = range;
    }

    /// <summary>
    /// 一个格移动到另一个格子
    /// </summary>
    /// <param name="neighbor">目标</param>
    /// <returns>目标</returns>
    LogicTile GrowPathTo(LogicTile neighbor)
    {
        // 是否有目标,目标是否已经走过,行动力是否小于目标行动力要求
        if (!HasPath || neighbor == null || neighbor.HasPath || leftCost < neighbor.moveCost|| !neighbor.CanWalk)
        {
            return null;
        }
        if (neighbor.PlayerOnTile != null)
        {
            if (neighbor.PlayerOnTile.charAttr.Team == TeamType.Enemy)
            {
                return null;
            }
        }
        neighbor.distance = distance + 1;

        neighbor.leftCost = leftCost - neighbor.moveCost;

        neighbor.NextOnPath = this;

        return neighbor;
    }

    public LogicTile GrowNorth() => GrowPathTo(north);
    public LogicTile GrowEast() => GrowPathTo(east);
    public LogicTile GrowSouth() => GrowPathTo(south);
    public LogicTile GrowWest() => GrowPathTo(west);


    LogicTile GrowAttackPathTo(LogicTile neighbor)
    {
        if (!HasPath || neighbor == null || neighbor.HasPath || leftAttack < ATTACK_COST )
        {
            return null;
        }
        neighbor.distance = distance + 1;
        neighbor.leftAttack = leftAttack - ATTACK_COST;
        return neighbor;
    }
    public LogicTile GrowAttackNorth() => GrowAttackPathTo(north);
    public LogicTile GrowAttackEast() => GrowAttackPathTo(east);
    public LogicTile GrowAttackSouth() => GrowAttackPathTo(south);
    public LogicTile GrowAttackWest() => GrowAttackPathTo(west);



    /// <summary>
    /// 是否是边缘格子
    /// </summary>
    /// <param name="tile">判断的格子</param>
    /// <param name="tiles">移动范围</param>
    /// <returns></returns>
    static bool IsBoundTile(LogicTile tile, List<LogicTile> tiles)
    {
        if (tile.north != null && !tiles.Contains(tile.north))
        {
            return true;
        }
        if (tile.east != null && !tiles.Contains(tile.east))
        {
            return true;
        }
        if (tile.south != null && !tiles.Contains(tile.south))
        {
            return true;
        }
        if (tile.west != null && !tiles.Contains(tile.west))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 得到边缘格子
    /// </summary>
    /// <param name="tiles"></param>
    /// <returns></returns>
    public static List<LogicTile> GetBoundTiles(List<LogicTile> tiles)
    {
        List<LogicTile> result = new List<LogicTile>();
        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            if (IsBoundTile(tile,tiles))
            {
                result.Add(tile);
            }
        }
        return result;
    }
}
