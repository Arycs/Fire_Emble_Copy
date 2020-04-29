using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : Singleton<Battle>
{
    [SerializeField]
    private Unit active;
    [SerializeField]
    private Unit passive;
    [SerializeField]
    private GameObject battlePanel;

    [SerializeField]
    private GameObject attack;
    [SerializeField]
    private GameObject attacked;

    public void Awake()
    {
        active.Target = passive;
        passive.Target = active;
    }

    public void StartBattle(RuntimeAnimatorController active, RuntimeAnimatorController passive)
    {
        attack.GetComponent<Animator>().runtimeAnimatorController = active;
        attacked.GetComponent<Animator>().runtimeAnimatorController = passive;
        battlePanel.SetActive(true);
        StartCoroutine(StartAllTurns());
    }

    private IEnumerator StartAllTurns()
    {
        int activeTurns = 1;
        int passiveTurns = 1;
        while (activeTurns > 0 || passiveTurns >0)
        {
            if (activeTurns > 0)
            {
                yield return Turn(active);
                activeTurns--;
            }
            if (passiveTurns > 0)
            {
                yield return Turn(passive);
                passiveTurns--;
            }
        }
    }

    private IEnumerator Turn(Unit one)
    {
        int attackCount = 1;
        while (attackCount > 0)
        {
            yield return one.Attack();
            attackCount--;
        }
    }
}
