using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

public class BattleUnit : MonoBehaviour
{
    public bool isPlayerUnit;
    public Monster monster;
    public BattleHud battleHud;

    [Header("animation")]
    [SerializeField] Image image;
    Vector3 originalPos;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
    }

    public void SetUp(Monster mMonster)
    {
        monster = mMonster;
        battleHud.gameObject.SetActive(true);

        //Set sprite
        SetMonsterImage();

        PlayEnterAnimation();
    }

    void SetMonsterImage()
    {
        if (monster.development >= monster.monsterBase.maxDevelopment)
        {
            if (isPlayerUnit)
            { image.sprite = monster.monsterBase.allySprite; }
            else
            { image.sprite = monster.monsterBase.enemySprite; }
        }
        else
        {
            if (isPlayerUnit)
            { image.sprite = monster.monsterBase.allySpriteBaby; }
            else
            { image.sprite = monster.monsterBase.enemySpriteBaby; }
        }
    }

    public void Clear()
    {
        GetComponent<Image>().sprite = null;
        battleHud.gameObject.SetActive(false);
    }

    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
        { image.transform.localPosition = new Vector3( originalPos.x - 220f, originalPos.y, originalPos.z); }
        else
        { image.transform.localPosition = new Vector3(originalPos.x + 220f, originalPos.y, originalPos.z); }
        image.color = new Color(1,1,1,0);

        image.transform.DOLocalMoveX(originalPos.x, 1.2f);
        image.DOColor(Color.white, 1.0f);
    }

    public void PlayExitAnimation()
    {
        image.transform.localPosition = new Vector3(originalPos.x, originalPos.y, originalPos.z);
        image.color = Color.white;
        if (isPlayerUnit)
        { image.transform.DOLocalMoveX(originalPos.x - 220f, 0.9f); }
        else
        { image.transform.DOLocalMoveX(originalPos.x + 220f, 0.9f); }

        image.DOColor(new Color(1,1,1,0), 0.75f);
    }

    public void PlayHitAnimation(Move attMove)
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(attMove.type.typeColor, 0.1f));
        sequence.Append(image.DOColor(Color.white, 0.1f));
    }
    public void PlayStatusAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 20f, 0.1f));
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 20f, 0.1f));
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.1f));
        sequence.Append(image.DOColor(Color.white, 0.1f));
    }

    public IEnumerator PlayGrowAnimation()
    {
        image.transform.localPosition = new Vector3(originalPos.x, originalPos.y, originalPos.z);
        image.color = Color.white;

        image.DOColor(new Color(1, 1, 1, 0), 0.75f);
        yield return new WaitForSeconds(1);
        SetMonsterImage();
        image.DOColor(Color.white, 0.75f);
        yield return new WaitForSeconds(0.02f);
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 120, 0.5f));
        sequence.Join(image.DOColor(new Color(0, 0, 0, 0), 0.5f));
    }

}
