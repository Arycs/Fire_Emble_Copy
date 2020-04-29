using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TeamType
{
    My,
    Enemy,
    Friend
}
[System.Serializable]
public class UnitAttr
{
    public RuntimeAnimatorController rAc;

    public TeamType Team;
    public int Id;

    public Sprite Head;
    public Sprite ClassImg;

    public string Name;
    public string ClaseName; //职业ID，通过ID查表可以获得职业名
    public int Lv;
    public int CurExp;
    public int CurHp;
    public int Hp;

    #region 基础属性(可成长属性)
    public int Str;  //力量
    public int Def;  //物防
    public int Mag;  //魔力
    public int Ski;  //技巧
    public int Spd;  //速度
    public int Res;  //魔防
    public int Luck; //幸运
    public int Move; //移动力
    public int Con;  //体格
    #endregion


}
