using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBase : MonoBehaviour
{
    public Item item;
    public abstract void StartState(CharacterAction action);
    public abstract void UpdateState(CharacterAction action);
}
