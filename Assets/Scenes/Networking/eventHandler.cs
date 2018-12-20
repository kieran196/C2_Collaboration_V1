using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class eventHandler : NetworkBehaviour {

    public UnityEvent triggerDownL;
    public UnityEvent triggerDownR;

    public UnityEvent triggerPressL;
    public UnityEvent triggerPressR;

    public UnityEvent triggerUpL;
    public UnityEvent triggerUpR;

    public UnityEvent spaceDown;
    public UnityEvent spaceUp;

}
