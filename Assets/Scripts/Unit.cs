using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField]
    private Transform attackPoint = default;


    private Unit target;
    public Unit Target
    {
        get => target;
        set => target = value;
    }

    private Animator animator;
    private Vector3 originalPos;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        originalPos = transform.position;
    }

    public IEnumerator Attack()
    {
        animator.SetTrigger("Attack");
        GetComponent<SpriteRenderer>().sortingOrder = 1;
        Target.GetComponent<SpriteRenderer>().sortingOrder = 0;

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            yield return null;
        }
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            yield return null;
        }
        StopAllCoroutines();
    }

    /// <summary>
    /// 攻击停顿
    /// </summary>
    private void AttackFrame()
    {
        StopAllCoroutines();
        StartCoroutine(Pause());
    }

    IEnumerator Pause()
    {
        animator.speed = 0;
        yield return new WaitForSeconds(0.5f);
        animator.speed = 1;
    }

    /// <summary>
    /// 播放动画开始移动
    /// </summary>
    /// <param name="time"></param>
    private void MoveStart(float time)
    {
        StartCoroutine(MoveS(time));
    }
    private IEnumerator MoveS(float time)
    {
        float delatX = Target.originalPos.x - attackPoint.position.x;
        float moveSpeed = delatX / time; //0.7F是时间
        while (true)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 播放动画返回
    /// </summary>
    /// <param name="time"></param>
    private void MoveBack(float time)
    {
        StartCoroutine(MoveB(time));
    }
    private IEnumerator MoveB(float time)
    {
        float delatX = originalPos.x - transform.position.x;
        float moveSpeed = delatX / time; //0.7F是时间
        while (true)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            yield return null;
        }
    }

}
