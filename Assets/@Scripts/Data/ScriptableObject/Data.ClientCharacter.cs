using UnityEngine;


    public abstract class ClientCharacter : ScriptableObject
    {
        [Header("[ClientCharacter] 식별 정보")]
        [SerializeField] protected int dataId;


        public int DataId => dataId;
        
    }
