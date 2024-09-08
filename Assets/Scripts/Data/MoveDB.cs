using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDB 
{
    public static Dictionary<string, Move> moves;

    public static void Init()
    {
        moves = new Dictionary<string, Move>();

        var MonsterArray = Resources.LoadAll<Move>("");
        foreach (var move in MonsterArray)
        {
            if (moves.ContainsKey(move.moveName))
            {
                Debug.LogError("Hay 2 movimientos con el mismo nombre");
                continue;
            }
            moves[move.moveName] = move;
        }
    }
}
