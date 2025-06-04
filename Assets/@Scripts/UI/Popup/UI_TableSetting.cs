using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_TableSetting : UI_Popup
{
	enum GameObjects
	{
		Table1_Object,
		Table2_Object,
		Table3_Object,

		Table4_Object,
		Table5_Object,
		Table6_Object,
		Table7_Object,
	
	
	}

	enum Buttons
	{
		CloseButton,
	}

	enum Texts
	{
	
	}

	enum Images
	{
	
	}


	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		BindObjects(typeof(GameObjects));
		BindButtons(typeof(Buttons));
		BindTexts(typeof(Texts));
		BindImages(typeof(Images));

		// GetObject((int)GameObjects.CloseArea).BindEvent(OnClickCloseArea);
		GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);

		// GetObject((int)GameObjects.Table1_Object).BindDragEventToPrefab("table01");
		BindDragEventToPrefab((int)GameObjects.Table1_Object, "table01");
		BindDragEventToPrefab((int)GameObjects.Table2_Object, "table02");
		BindDragEventToPrefab((int)GameObjects.Table3_Object, "table03");
		BindDragEventToPrefab((int)GameObjects.Table4_Object, "table04");
		BindDragEventToPrefab((int)GameObjects.Table5_Object, "table05");
		BindDragEventToPrefab((int)GameObjects.Table6_Object, "table06");
		BindDragEventToPrefab((int)GameObjects.Table7_Object, "table07");

		// 필요한 만큼 추가
		Refresh();

		return true;
	}

	public void SetInfo()
	{
		Refresh();
	}

	void Refresh()
	{
		if (_init == false)
			return;

	
		// Data.HeroData data = Managers.Data.HeroDic[_heroDataId];

		// GetImage((int)Images.HeroIconImage).sprite = Managers.Resource.Load<Sprite>(data.IconImage);
		// GetText((int)Texts.NameText).text = data.DescriptionTextID;

			// GetText((int)Texts.LevelText).text = SaveData.Level.ToString();
			// GetText((int)Texts.ExpText).text = $"{SaveData.Exp} / ??";

		// TODO
		// float atk = data.Atk;
		// float hp = data.MaxHp;
		// GetText((int)Texts.BattlePowerText).text = (hp + atk * 5).ToString("F0");
		// GetText((int)Texts.DamageText).text = atk.ToString("F0");
		// GetText((int)Texts.HpText).text = hp.ToString("F0");
	}

	void OnClickCloseArea(PointerEventData evt)
	{
		Debug.Log("OnClickCloseArea");
		Managers.UI.ClosePopupUI(this);
	}

	void OnClickCloseButton(PointerEventData evt)
	{
		Debug.Log("OnClickCloseButton");
		Managers.UI.ClosePopupUI(this);
	}

	void OnClickLevelUpButton(PointerEventData evt)
	{
		Debug.Log("OnClickLevelUpButton");
		Refresh();
	}

	void OnClickSkill1Button(PointerEventData evt)
	{
		Debug.Log("OnClickSkill1Button");
	}

	void OnClickSkill2Button(PointerEventData evt)
	{
		Debug.Log("OnClickSkill2Button");
	}

				
	void BindDragEventToPrefab(int objectIdx, string prefabName)
	{
		var obj = GetObject(objectIdx);
		if (obj == null) 
		{
			Debug.LogError($"Object {objectIdx} not found");
			return;
		}

		var trigger = obj.GetComponent<EventTrigger>();
		if (trigger == null)
			trigger = obj.AddComponent<EventTrigger>();

    	trigger.triggers.Clear();


		var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
		entryDown.callback.AddListener((data) => {
			Debug.Log($"[UI] PointerDown: {prefabName}");
			// 미리 프리팹 설정만
			Managers.Placement.SetPlaceablePrefab(prefabName);
		});
		trigger.triggers.Add(entryDown);

		// 드래그 시작
		// 드래그 시작
		var entryBegin = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
		entryBegin.callback.AddListener((data) => {
			Debug.Log($"[UI_TableSetting] BeginDrag: {prefabName}");
			Managers.Placement.StartDragFromUI(prefabName);
		});
		trigger.triggers.Add(entryBegin);
		
		// 드래그 중
		var entryDrag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
		entryDrag.callback.AddListener((data) => {
			Managers.Placement.UpdateDragFromUI();
		});
		trigger.triggers.Add(entryDrag);

		// 드래그 끝
		var entryEnd = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
		entryEnd.callback.AddListener((data) => {
			Managers.Placement.EndDragFromUI();
		});
		trigger.triggers.Add(entryEnd);
		
		// 드래그 취소 (UI 밖으로 나갔을 때)
		var entryCancel = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
		entryCancel.callback.AddListener((data) => {
			// 드래그 중이 아닐 때만 취소
			if (!Managers.Placement._isDraggingFromUI) return;
			
			// 고스트와 하이라이트 제거
			Managers.Placement.CancelDragFromUI();
		});
		trigger.triggers.Add(entryCancel);
	}
}
