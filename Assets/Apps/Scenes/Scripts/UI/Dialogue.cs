using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Dialogue
{
    public string name;

    [TextArea(3, 10)]
    public string[] sentences;
    public bool selection;
    public bool end;

    [Header("Events")]
    public UnityEvent yesEvent;
    public UnityEvent noEvent;

    public void Interact(bool yesNo)
    {
        if(yesNo == true)
        {
            yesEvent.Invoke();
        }else if(yesNo == false)
        {
            noEvent.Invoke();
        }
    }
}
