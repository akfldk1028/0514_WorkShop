using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBase : MonoBehaviour
{
    public Item item;
    public abstract void StartState(Action_Test action);
    public abstract void UpdateState(Action_Test action);
}
