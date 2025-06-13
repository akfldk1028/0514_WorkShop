using UnityEngine;
using System;

[Serializable]
public class RecipeSoundSet
{
    public int recipeId;
    public AudioClip soundA;
    public AudioClip soundS;
    public AudioClip soundD;
    public AudioClip soundW;
    public AudioClip soundZ;
}

[CreateAssetMenu(fileName = "RecipeSoundData", menuName = "ScriptableObjects/RecipeSoundData")]
public class RecipeSoundData : ScriptableObject
{
    public RecipeSoundSet[] recipeSounds;

    public RecipeSoundSet GetSoundSet(int recipeId)
    {
        foreach (var soundSet in recipeSounds)
        {
            if (soundSet.recipeId == recipeId)
                return soundSet;
        }
        return null;
    }
} 