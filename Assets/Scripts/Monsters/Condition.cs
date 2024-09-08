using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public ConditionID ID {  get; set; }
    public string conditionName {  get; set; }
    public string startMessage { get; set; }
    public string conditionIcon { get; set; }

    public Func<Monster, bool> OnBeforeMove { get; set; }
    public Action<Monster> OnAfterTurn {  get; set; }
}
