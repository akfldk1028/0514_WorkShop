// using System;
// using UnityEngine;
// using UnityEngine.Assertions;
// using UnityEngine.EventSystems;


//     /// <summary>
//     /// Captures inputs for a character on a client and sends them to the server.
//     /// </summary>
//     public class ClientInputSender
//     {
//         const float k_MouseInputRaycastDistance = 100f;

//         //The movement input rate is capped at 40ms (or 25 fps). This provides a nice balance between responsiveness and
//         //upstream network conservation. This matters when holding down your mouse button to move.
//         const float k_MoveSendRateSeconds = 0.04f; //25 fps.

//         const float k_TargetMoveTimeout = 0.45f;  //prevent moves for this long after targeting someone (helps prevent walking to the guy you clicked).

//         float m_LastSentMove;

//         // Cache raycast hit array so that we can use non alloc raycasts
//         readonly RaycastHit[] k_CachedHit = new RaycastHit[4];

//         // This is basically a constant but layer masks cannot be created in the constructor, that's why it's assigned int Awake.
//         LayerMask m_GroundLayerMask;

//         LayerMask m_ActionLayerMask;

//         const float k_MaxNavMeshDistance = 1f;

//         RaycastHitComparer m_RaycastHitComparer;

//         [SerializeField]
//         ServerCharacter m_ServerCharacter;

//         /// <summary>
//         /// This event fires at the time when an action request is sent to the server.
//         /// </summary>
//         public event Action<ActionRequestData> ActionInputEvent;

//         /// <summary>
//         /// This describes how a skill was requested. Skills requested via mouse click will do raycasts to determine their target; skills requested
//         /// in other matters will use the stateful target stored in NetworkCharacterState.
//         /// </summary>
//         public enum SkillTriggerStyle
//         {
//             None,        //no skill was triggered.
//             MouseClick,  //skill was triggered via mouse-click implying you should do a raycast from the mouse position to find a target.
//             Keyboard,    //skill was triggered via a Keyboard press, implying target should be taken from the active target.
//             KeyboardRelease, //represents a released key.
//             UI,          //skill was triggered from the UI, and similar to Keyboard, target should be inferred from the active target.
//             UIRelease,   //represents letting go of the mouse-button on a UI button
//         }

//         bool IsReleaseStyle(SkillTriggerStyle style)
//         {
//             return style == SkillTriggerStyle.KeyboardRelease || style == SkillTriggerStyle.UIRelease;
//         }

  
//         struct ActionRequest
//         {
//             public SkillTriggerStyle TriggerStyle;
//             public ActionID RequestedActionID;
//             public ulong TargetId;
//         }

//         /// <summary>
//         /// List of ActionRequests that have been received since the last FixedUpdate ran. This is a static array, to avoid allocs, and
//         /// because we don't really want to let this list grow indefinitely.
//         /// </summary>
//         readonly ActionRequest[] m_ActionRequests = new ActionRequest[5];

//         /// <summary>
//         /// Number of ActionRequests that have been queued since the last FixedUpdate.
//         /// </summary>
//         int m_ActionRequestCount;

//         BaseActionInput m_CurrentSkillInput;

//         bool m_MoveRequest;

//         Camera m_MainCamera;

//         public event Action<Vector3> ClientMoveEvent;

//         /// <summary>
//         /// Convenience getter that returns our CharacterData
//         /// </summary>
//         CharacterClass CharacterClass => m_ServerCharacter.CharacterClass;

//         [SerializeField]
//         PhysicsWrapper m_PhysicsWrapper;

//         public ActionState actionState1 { get; private set; }

//         public ActionState actionState2 { get; private set; }

//         public ActionState actionState3 { get; private set; }

//         public System.Action action1ModifiedCallback;

//         ServerCharacter m_TargetServerCharacter;

//         void Awake()
//         {
//             m_MainCamera = Camera.main;
//         }


//         void OnTargetChanged(ulong previousValue, ulong newValue)
//         {
//             if (m_TargetServerCharacter)
//             {
//                 m_TargetServerCharacter.NetLifeState.LifeState.OnValueChanged -= OnTargetLifeStateChanged;
//             }

//             m_TargetServerCharacter = null;

//             if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(newValue, out var selection) &&
//                 selection.TryGetComponent(out m_TargetServerCharacter))
//             {
//                 m_TargetServerCharacter.NetLifeState.LifeState.OnValueChanged += OnTargetLifeStateChanged;
//             }

//         }


   
//         void SendInput(ActionRequestData action)
//         {
//             ActionInputEvent?.Invoke(action);
//             m_ServerCharacter.ServerPlayActionRpc(action);
//         }

       
       
//         bool GetActionRequestForTarget(Transform hit, ActionID actionID, SkillTriggerStyle triggerStyle, out ActionRequestData resultData)
//         {
//             resultData = new ActionRequestData();

//             var targetNetObj = hit != null ? hit.GetComponentInParent<NetworkObject>() : null;

//             //if we can't get our target from the submitted hit transform, get it from our stateful target in our ServerCharacter.
//             if (!targetNetObj && !GameDataSource.Instance.GetActionPrototypeByID(actionID).IsGeneralTargetAction)
//             {
//                 ulong targetId = m_ServerCharacter.TargetId.Value;
//                 NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out targetNetObj);
//             }

//             //sanity check that this is indeed a valid target.
//             if (targetNetObj == null || !ActionUtils.IsValidTarget(targetNetObj.NetworkObjectId))
//             {
//                 return false;
//             }

//             if (targetNetObj.TryGetComponent<ServerCharacter>(out var serverCharacter))
//             {
//                 //Skill1 may be contextually overridden if it was generated from a mouse-click.
//                 if (actionID == CharacterClass.Skill1.ActionID && triggerStyle == SkillTriggerStyle.MouseClick)
//                 {
//                     if (!serverCharacter.IsNpc && serverCharacter.LifeState == LifeState.Fainted)
//                     {
//                         //right-clicked on a downed ally--change the skill play to Revive.
//                         actionID = GameDataSource.Instance.ReviveActionPrototype.ActionID;
//                     }
//                 }
//             }

//             Vector3 targetHitPoint;
//             if (PhysicsWrapper.TryGetPhysicsWrapper(targetNetObj.NetworkObjectId, out var movementContainer))
//             {
//                 targetHitPoint = movementContainer.Transform.position;
//             }
//             else
//             {
//                 targetHitPoint = targetNetObj.transform.position;
//             }

//             // record our target in case this action uses that info (non-targeted attacks will ignore this)
//             resultData.ActionID = actionID;
//             resultData.TargetIds = new ulong[] { targetNetObj.NetworkObjectId };
//             PopulateSkillRequest(targetHitPoint, actionID, ref resultData);
//             return true;
//         }

//         /// <summary>
//         /// Populates the ActionRequestData with additional information. The TargetIds of the action should already be set before calling this.
//         /// </summary>
//         /// <param name="hitPoint">The point in world space where the click ray hit the target.</param>
//         /// <param name="actionID">The action to perform (will be stamped on the resultData)</param>
//         /// <param name="resultData">The ActionRequestData to be filled out with additional information.</param>
//         void PopulateSkillRequest(Vector3 hitPoint, ActionID actionID, ref ActionRequestData resultData)
//         {
//             resultData.ActionID = actionID;
//             var actionConfig = GameDataSource.Instance.GetActionPrototypeByID(actionID).Config;

//             //most skill types should implicitly close distance. The ones that don't are explicitly set to false in the following switch.
//             resultData.ShouldClose = true;

//             // figure out the Direction in case we want to send it
//             Vector3 offset = hitPoint - m_PhysicsWrapper.Transform.position;
//             offset.y = 0;
//             Vector3 direction = offset.normalized;

//             switch (actionConfig.Logic)
//             {
//                 //for projectile logic, infer the direction from the click position.
//                 // case ActionLogic.LaunchProjectile:
//                 //     resultData.Direction = direction;
//                 //     resultData.ShouldClose = false; //why? Because you could be lining up a shot, hoping to hit other people between you and your target. Moving you would be quite invasive.
//                 //     return;
//                 // case ActionLogic.Melee:
//                 //     resultData.Direction = direction;
//                 //     return;
//                 // case ActionLogic.Target:
//                 //     resultData.ShouldClose = false;
//                 //     return;
//                 // case ActionLogic.Emote:
//                 //     resultData.CancelMovement = true;
//                 //     return;
//                 // case ActionLogic.RangedFXTargeted:
//                 //     resultData.Position = hitPoint;
//                 //     return;
//                 // case ActionLogic.DashAttack:
//                 //     resultData.Position = hitPoint;
//                 //     return;
//                 // case ActionLogic.PickUp:
//                 //     resultData.CancelMovement = true;
//                 //     resultData.ShouldQueue = false;
//                 //     return;
//             }
//         }

//         /// <summary>
//         /// Request an action be performed. This will occur on the next FixedUpdate.
//         /// </summary>
//         /// <param name="actionID"> The action you'd like to perform. </param>
//         /// <param name="triggerStyle"> What input style triggered this action. </param>
//         /// <param name="targetId"> NetworkObjectId of target. </param>
//         public void RequestAction(ActionID actionID, SkillTriggerStyle triggerStyle, ulong targetId = 0)
//         {
//             Assert.IsNotNull(GameDataSource.Instance.GetActionPrototypeByID(actionID),
//                 $"Action with actionID {actionID} must be contained in the Action prototypes of GameDataSource!");

//             if (m_ActionRequestCount < m_ActionRequests.Length)
//             {
//                 m_ActionRequests[m_ActionRequestCount].RequestedActionID = actionID;
//                 m_ActionRequests[m_ActionRequestCount].TriggerStyle = triggerStyle;
//                 m_ActionRequests[m_ActionRequestCount].TargetId = targetId;
//                 m_ActionRequestCount++;
//             }
//         }

//         void Update()
//         {
//             // if (Input.GetKeyDown(KeyCode.Alpha1) && CharacterClass.Skill1)
//             // {
//             //     RequestAction(actionState1.actionID, SkillTriggerStyle.Keyboard);
//             // }
//             // else if (Input.GetKeyUp(KeyCode.Alpha1) && CharacterClass.Skill1)
//             // {
//             //     RequestAction(actionState1.actionID, SkillTriggerStyle.KeyboardRelease);
//             // }
//             // if (Input.GetKeyDown(KeyCode.Alpha2) && CharacterClass.Skill2)
//             // {
//             //     RequestAction(actionState2.actionID, SkillTriggerStyle.Keyboard);
//             // }
//             // else if (Input.GetKeyUp(KeyCode.Alpha2) && CharacterClass.Skill2)
//             // {
//             //     RequestAction(actionState2.actionID, SkillTriggerStyle.KeyboardRelease);
//             // }
//             // if (Input.GetKeyDown(KeyCode.Alpha3) && CharacterClass.Skill3)
//             // {
//             //     RequestAction(actionState3.actionID, SkillTriggerStyle.Keyboard);
//             // }
//             // else if (Input.GetKeyUp(KeyCode.Alpha3) && CharacterClass.Skill3)
//             // {
//             //     RequestAction(actionState3.actionID, SkillTriggerStyle.KeyboardRelease);
//             // }

//             if (Input.GetKeyDown(KeyCode.Alpha5))
//             {
//                 RequestAction(GameDataSource.Instance.Emote1ActionPrototype.ActionID, SkillTriggerStyle.Keyboard);
//             }
//             if (Input.GetKeyDown(KeyCode.Alpha6))
//             {
//                 RequestAction(GameDataSource.Instance.Emote2ActionPrototype.ActionID, SkillTriggerStyle.Keyboard);
//             }
//             if (Input.GetKeyDown(KeyCode.Alpha7))
//             {
//                 RequestAction(GameDataSource.Instance.Emote3ActionPrototype.ActionID, SkillTriggerStyle.Keyboard);
//             }
//             if (Input.GetKeyDown(KeyCode.Alpha8))
//             {
//                 RequestAction(GameDataSource.Instance.Emote4ActionPrototype.ActionID, SkillTriggerStyle.Keyboard);
//             }

//             if (!EventSystem.current.IsPointerOverGameObject() && m_CurrentSkillInput == null)
//             {
//                 //IsPointerOverGameObject() is a simple way to determine if the mouse is over a UI element. If it is, we don't perform mouse input logic,
//                 //to model the button "blocking" mouse clicks from falling through and interacting with the world.

//                 if (Input.GetMouseButtonDown(1))
//                 {
//                     RequestAction(CharacterClass.Skill1.ActionID, SkillTriggerStyle.MouseClick);
//                 }

//                 if (Input.GetMouseButtonDown(0))
//                 {
//                     RequestAction(GameDataSource.Instance.GeneralTargetActionPrototype.ActionID, SkillTriggerStyle.MouseClick);
//                 }
//                 else if (Input.GetMouseButton(0))
//                 {
//                     m_MoveRequest = true;
//                 }
//             }
//         }

 

//         public class ActionState
//         {
//             public ActionID actionID { get; internal set; }

//             public bool selectable { get; internal set; }

//             internal void SetActionState(ActionID newActionID, bool isSelectable = true)
//             {
//                 actionID = newActionID;
//                 selectable = isSelectable;
//             }
//         }
//     }
