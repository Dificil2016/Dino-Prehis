using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<Text> moveTexts;

    public void SetMoveData(List<Move> currentMoves, Move newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].moveName;
        }
        moveTexts[currentMoves.Count].text = newMove.moveName;
    }

}
