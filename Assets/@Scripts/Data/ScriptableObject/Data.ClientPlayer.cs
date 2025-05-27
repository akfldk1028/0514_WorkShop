using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;


    /// <summary>
    /// 몬스터의 시각적 요소와 데이터 참조를 관리하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ClientPlayer", menuName = "GameData/ClientPlayer", order = 2)]
    public class ClientPlayer : ClientCharacter
    {
        [Header("Sprite")]
        [SerializeField] public Sprite playerSprite;
        [SerializeField] public Image earnMoneyImage;
        
        [Header("Sounds")]
        [SerializeField] protected AudioClip[] creatureSounds;
        
        [Header("Sounds")]
        [SerializeField] private GameObject modelPrefab;

        [Header("Effects")]
        [SerializeField] protected GameObject spawnEffectPrefab;
        
        [Header("Animator")]
        [SerializeField] protected RuntimeAnimatorController animatorController;

        public Sprite PlayerSprite => playerSprite;
        // public AudioClip[] PlayerSounds => playerSounds;
        public GameObject ModelPrefab => modelPrefab;

        public GameObject SpawnEffectPrefab => spawnEffectPrefab;

        public RuntimeAnimatorController AnimatorController => animatorController;
        public Image EarnMoneyImage => earnMoneyImage;
 
    }
