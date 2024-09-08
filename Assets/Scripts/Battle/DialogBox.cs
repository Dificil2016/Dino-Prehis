using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    [Header("Dialog boxes")]
    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject benchSelector;
    [SerializeField] GameObject moveDetails;

    [SerializeField] List<Text> moveTexts;
    [SerializeField] List<BenchButton> benchButtons;

    [Header("Move Details")]
    [SerializeField] Text powerText;
    [SerializeField] Image typeIcon;
    [SerializeField] Sprite defaultTypeIcon;

    public void SetDialog(string dialog)
    { dialogText.text = dialog; }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSecondsRealtime(0.02f);
        }
        yield return new WaitForSecondsRealtime(0.6f);
    }

    public void EnableDialogText(bool enabled)
    { dialogText.enabled = enabled; }

    public void EnableActionSelector(bool enabled)
    { actionSelector.SetActive(enabled); }

    public void EnableBenchSelector(bool enabled)
    { benchSelector.SetActive(enabled); }
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i <= moveTexts.Count-1; i++) 
        {
            if (moves.Count - 1 - i >= 0)
            {
                moveTexts[i].text = moves[i].moveName;
                //Debug.Log($"asignado {moves[i].moveName} como {moveTexts[i].text} en el slot no {i}, quedan {moves.Count - 1 - i} de {moves.Count}");
            }
            else
            { moveTexts[i].text = "-----"; }
        }
    }

    public bool SetBenchDetails(List<Monster> party, Monster activeMonster)
    {
        bool ChangePosible = false;

        foreach (BenchButton button in benchButtons)
        { button.gameObject.SetActive(true); }

        int e = 0;
        for (int i = 0; i < party.Count; i++)
        {
            if (e < benchButtons.Count)
            {
                if (party[i] != activeMonster && party[i].hp > 0)
                {
                    benchButtons[e].monsterIndex = i;
                    benchButtons[e].text.text = party[i].nameTag;
                    benchButtons[e].HPBar.SetHP((float)party[i].hp / party[i].maxHP);
                    ChangePosible = true;
                    e++;
                }
            }
        }

        while(e < benchButtons.Count)
        {
            benchButtons[e].gameObject.SetActive(false);
            e++;
        }

        return ChangePosible;
    }


    public void SetMoveDetails(Move move)
    {
        typeIcon.sprite = move.type.typeIcon;
        if(move.power > 0)
        { powerText.text = $"P:{move.power} C:{move.cost}"; }
        else
        { powerText.text = $"P:--- C:{move.cost}"; }
        
    }
    public void ResetMoveDetails()
    {
        typeIcon.sprite = defaultTypeIcon;
        powerText.text = $"P:--- C:---";
    }
}