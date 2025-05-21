using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;


    /// <summary>
    /// 몬스터의 시각적 요소와 데이터 참조를 관리하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ClientCustomer", menuName = "GameData/ClientCustomer", order = 1)]
    public class ClientCustomer : ClientCharacter
    {
        [Header("Sprite")]
        [SerializeField] public Sprite creatureSprite;
        [SerializeField] public Image earnMoneyImage;
        [SerializeField] public Image orderImage;
        
        [Header("Sounds")]
        [SerializeField] protected AudioClip[] creatureSounds;
        
        [Header("Sounds")]
        [SerializeField] private GameObject modelPrefab;

        [Header("Effects")]
        [SerializeField] protected GameObject spawnEffectPrefab;
        [SerializeField] protected GameObject deathEffectPrefab;
        
        [Header("Animator")]
        [SerializeField] protected RuntimeAnimatorController animatorController;

        public Sprite CreatureSprite => creatureSprite;
        public AudioClip[] CreatureSounds => creatureSounds;
        public GameObject ModelPrefab => modelPrefab;

        public GameObject SpawnEffectPrefab => spawnEffectPrefab;

        public GameObject DeathEffectPrefab => deathEffectPrefab;

        public RuntimeAnimatorController AnimatorController => animatorController;
        public Image EarnMoneyImage => earnMoneyImage;
        public Image OrderImage => orderImage;
 
    }
