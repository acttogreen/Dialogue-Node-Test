using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : Template_UIManager
{
    public bool useTap;
    public override void DetectInput()
    {
        if (useTap) base.interactionKey = KeyCode.Mouse0;
        base.DetectInput();
    }
}
