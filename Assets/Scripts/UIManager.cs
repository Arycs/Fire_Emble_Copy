using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private CanvasGroup information;
    [SerializeField]
    private GameObject toolTips;
    [SerializeField]
    private GameObject menu;

    public CanvasGroup Information
    {
        get => information;
    }
    public GameObject ToolTips
    {
        get => toolTips;
    }
    public GameObject Menu
    {
        get => menu;
    }


    [Header("菜单按钮")]
    [SerializeField]
    private GameObject resuce;
    [SerializeField]
    private GameObject attack;
    [SerializeField]
    private GameObject bag;
    [SerializeField]
    private GameObject exchange;
    [SerializeField]
    private GameObject standBy;


    [Header("角色信息右侧栏")]
    [SerializeField]
    private CanvasGroup personalData;
    [SerializeField]
    private CanvasGroup weaponLevel;
    [SerializeField]
    private CanvasGroup items;

    private int informationIndex = 1;

    [Header("角色信息左侧栏")]
    [SerializeField]
    private Image charHead;
    [SerializeField]
    private Text charName;
    [SerializeField]
    private Text charProfession;
    [SerializeField]
    private Image charProfessionImg;
    [SerializeField]
    private Text charLv;
    [SerializeField]
    private Text charExp;
    [SerializeField]
    private Text charCurHp;
    [SerializeField]
    private Text charMaxHp;

   

    #region 显示详细信息面板
    public void ShowInformation(Player player)
    {
        charHead.sprite = player.charAttr.Head;
        charName.text = player.charAttr.Name;
        charProfession.text = player.charAttr.ClaseName;
        charLv.text = player.charAttr.Lv.ToString();
        charCurHp.text = player.charAttr.CurHp.ToString();
        charExp.text = player.charAttr.CurExp.ToString();
        charMaxHp.text = player.charAttr.Hp.ToString();
        charProfessionImg.sprite = player.charAttr.ClassImg;

        Information.alpha = 1;
        Information.blocksRaycasts = true;
    }

    public void HideInformation()
    {
        Information.alpha = 0;
        Information.blocksRaycasts = false;
    }
    #endregion

    public void ShowMenu(Vector2 temp)
    {
        Menu.SetActive(true);
        if (temp.y > 0)
        {
            resuce.SetActive(true);
            exchange.SetActive(true);
        }
        if (temp.x > 0)
        {
            attack.SetActive(true);
        }
    }

    public void HideMene()
    {
        Menu.SetActive(false);
        resuce.SetActive(false);
        attack.SetActive(false);
        exchange.SetActive(false);
    }

    #region 显示提示信息
    public void ShowToolTips(Vector3 pos , string text)
    {
        ToolTips.SetActive(true);
        ToolTips.transform.position = pos;
        ToolTips.GetComponentInChildren<Text>().text = text;
    }

    public void HideToolTips()
    {
        ToolTips.SetActive(false);
    }
    #endregion

    #region 角色信息右侧栏
    public void NextPage()
    {
        informationIndex++;
        if (informationIndex > 3)
        {
            informationIndex = 1;
        }
        switch (informationIndex)
        {
            case 1:
                ShowInformationPage(personalData);
                break;
            case 2:
                ShowInformationPage(items);
                break;
            case 3:
                ShowInformationPage(weaponLevel);
                break;
            default:
                break;
        }
    }

    public void PrePage()
    {
        informationIndex--;
        if (informationIndex < 1)
        {
            informationIndex = 3;
        }
        switch (informationIndex)
        {
            case 1:
                ShowInformationPage(personalData);
                break;
            case 2:
                ShowInformationPage(items);
                break;
            case 3:
                ShowInformationPage(weaponLevel);
                break;
            default:
                break;
        }
    }

    public void ShowInformationPage(CanvasGroup canvasGroup)
    {
        CloseInformationPage();
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }

    public void CloseInformationPage()
    {
        personalData.alpha = 0;
        personalData.blocksRaycasts = false;

        weaponLevel.alpha = 0;
        weaponLevel.blocksRaycasts = false;

        items.alpha = 0;
        items.blocksRaycasts = false;
    }
    #endregion
}
