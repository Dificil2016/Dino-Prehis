using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dino-Prehis/ new Item / new TM")]
public class TmItem : ItemBase
{
    [SerializeField] Move move;

    public Move Move { get { return move; } }

    public override bool Use(Monster monster)
    {
        return monster.HasMove(move);
    }

}
