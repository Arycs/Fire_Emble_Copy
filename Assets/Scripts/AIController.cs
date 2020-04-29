using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public UnitAttr charAttr;

    private void Update()
    {
        if (true)
        {
            LogicTile temp = GameBoard.Instance.GetTileByPos(Vector3Int.CeilToInt(transform.position));
            GameBoard.Instance.FindMovePath(temp,charAttr.Move);

            foreach (LogicTile Lt in GameBoard.Instance.MoveTiles)
            {

            }

        }
    }
}
