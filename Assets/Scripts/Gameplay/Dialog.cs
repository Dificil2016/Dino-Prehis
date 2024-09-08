using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialog 
{
    [TextArea]
    [SerializeField] List<string> lines;

    public List<string> Lines { get { return lines; } }

    public Dialog(List<string> lines )
    {
        this.lines = lines;
    }
}
