using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


namespace Data
{
	#region RecipeData
	[Serializable]
	public class RecipeData
	{
		public int NO;
		public string RecipeID_eng;
		public string RecipeName;
		public int Diffic;
		public string Description;
		public List<string> KeyCombination;
		public int BasePrice;
		public List<string> Tags;
		public string Category;
		public float BPM;
		public List<string> RequiredIngredientsVisual;
		public string CompletedVisualResourceID;
		public string OpenOption;
		public string IconImage;
		public string Prefab;
	}

	[Serializable]
	public class RecipeDataLoader : ILoader<int, RecipeData>
	{
		public List<RecipeData> recipes = new List<RecipeData>();

		public Dictionary<int, RecipeData> MakeDict()
		{
			Dictionary<int, RecipeData> dict = new Dictionary<int, RecipeData>();
			foreach (RecipeData recipe in recipes)
				dict.Add(recipe.NO, recipe); // 또는 RecipeID_eng를 키로 쓸 수도 있음
			return dict;
		}
	}
	#endregion



	#region CreatureData
	[Serializable]
	public class CreatureData
	{
		public int DataId;
		public string DescriptionTextID;
		public string PrefabLabel;
		public string ClientPrefab;
		public string IconImage;
		public int DefaultSkillId;
		public int EnvSkillId;
		public int SkillAId;
		public int SkillBId;
		public bool IsValidTarget;
		public bool IsNpc;
		public float MoveSpeed;
	
	}
	#endregion


	#region CustomerData
	[Serializable]
	public class CustomerData : CreatureData
	{
		public string Rarity;
		public int GachaSpawnWeight;
		public int GachaWeight;
		public int GachaExpCount;
		public string Order;

		public int UnlockCondition;
		public int OrderTendency;
		public List<string> PreferredTags;
		public string PreferredCocktail;
		public int SatisfactionThresholds;
		public string ReactionDialogueGroupID_Satisfied;
		public string ReactionDialogueGroupID_Unsatisfied;
		public int PatienceSeconds;
	}

	[Serializable]
	public class CustomerDataLoader : ILoader<int, CustomerData>
	{
		public List<CustomerData> customers = new List<CustomerData>();
		public Dictionary<int, CustomerData> MakeDict()
		{
			Dictionary<int, CustomerData> dict = new Dictionary<int, CustomerData>();
			foreach (CustomerData customer in customers)
				dict.Add(customer.DataId, customer);
			return dict;
		}
	}
	#endregion

	#region PlayerData
	[Serializable]
	public class PlayerData : CreatureData
	{
	}

	[Serializable]
	public class PlayerDataLoader : ILoader<int, PlayerData>
	{
		public List<PlayerData> players = new List<PlayerData>();
		public Dictionary<int, PlayerData> MakeDict()
		{
			Dictionary<int, PlayerData> dict = new Dictionary<int, PlayerData>();
			foreach (PlayerData player in players)
				dict.Add(player.DataId, player);
			return dict;
		}
	}
	#endregion



	#region MonsterData
	[Serializable]
	public class MonsterData : CreatureData
	{
		public int DropItemId;

		[NonSerialized]
		public DropTableData DropTable;
	}

	[Serializable]
	public class MonsterDataLoader : ILoader<int, MonsterData>
	{
		public List<MonsterData> monsters = new List<MonsterData>();
		public Dictionary<int, MonsterData> MakeDict()
		{
			Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
			foreach (MonsterData monster in monsters)
				dict.Add(monster.DataId, monster);
			return dict;
		}
	}
	#endregion

	#region HeroData
	[Serializable]
	public class HeroData : CreatureData
	{
	}

	[Serializable]
	public class HeroDataLoader : ILoader<int, HeroData>
	{
		public List<HeroData> heroes = new List<HeroData>();
		public Dictionary<int, HeroData> MakeDict()
		{
			Dictionary<int, HeroData> dict = new Dictionary<int, HeroData>();
			foreach (HeroData hero in heroes)
				dict.Add(hero.DataId, hero);
			return dict;
		}
	}
	#endregion

	#region HeroInfoData
	[Serializable]
	public class HeroInfoData
	{
		public int DataId;
		public string NameTextId;
		public string DescriptionTextId;
		public string Rarity;
		public float GachaSpawnWeight;
		public float GachaWeight;
		public int GachaExpCount;
		public string IconImage;
	}

	[Serializable]
	public class HeroInfoDataLoader : ILoader<int, HeroInfoData>
	{
		public List<HeroInfoData> heroInfo = new List<HeroInfoData>();
		public Dictionary<int, HeroInfoData> MakeDict()
		{
			Dictionary<int, HeroInfoData> dict = new Dictionary<int, HeroInfoData>();
			foreach (HeroInfoData info in heroInfo)
				dict.Add(info.DataId, info);
			return dict;
		}
	}
	#endregion

	#region SkillData
	[Serializable]
	public class SkillData
	{
		public int DataId;
		public string Name;
		public string ClassName;
		public string Description;
		public int ProjectileId;
		public string PrefabLabel;
		public string IconLabel;
		public string AnimName;
		public float CoolTime;
		public float DamageMultiplier;
		public float Duration;
		public float AnimImpactDuration;
		public string CastingSound;
		public float SkillRange;
		public float ScaleMultiplier;
		public int TargetCount;
		public List<int> EffectIds = new List<int>();
		public int NextLevelId;
		public int AoEId;
		public EEffectSize EffectSize;
	}

	[Serializable]
	public class SkillDataLoader : ILoader<int, SkillData>
	{
		public List<SkillData> skills = new List<SkillData>();

		public Dictionary<int, SkillData> MakeDict()
		{
			Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
			foreach (SkillData skill in skills)
				dict.Add(skill.DataId, skill);
			return dict;
		}
	}
	#endregion

	#region ProjectileData
	[Serializable]
	public class ProjectileData
	{
		public int DataId;
		public string Name;
		public string ClassName;
		public string ComponentName;
		public string ProjectileSpriteName;
		public string PrefabLabel;
		public float Duration;
		public float HitSound;
		public float ProjRange;
		public float ProjSpeed;
	}

	[Serializable]
	public class ProjectileDataLoader : ILoader<int, ProjectileData>
	{
		public List<ProjectileData> projectiles = new List<ProjectileData>();

		public Dictionary<int, ProjectileData> MakeDict()
		{
			Dictionary<int, ProjectileData> dict = new Dictionary<int, ProjectileData>();
			foreach (ProjectileData projectile in projectiles)
				dict.Add(projectile.DataId, projectile);
			return dict;
		}
	}
	#endregion

	#region Env
	[Serializable]
	public class EnvData
	{
		public int DataId;
		public string DescriptionTextID;
		public string PrefabLabel;
		public float MaxHp;
		public int ResourceAmount;
		public float RegenTime;
		public List<String> SkeletonDataIDs = new List<String>();
		public int DropItemId;
	}

	[Serializable]
	public class EnvDataLoader : ILoader<int, EnvData>
	{
		public List<EnvData> envs = new List<EnvData>();
		public Dictionary<int, EnvData> MakeDict()
		{
			Dictionary<int, EnvData> dict = new Dictionary<int, EnvData>();
			foreach (EnvData env in envs)
				dict.Add(env.DataId, env);
			return dict;
		}
	}
	#endregion

	#region EffectData
	[Serializable]
	public class EffectData
	{
		public int DataId;
		public string Name;
		public string ClassName;
		public string DescriptionTextID;
		public string SkeletonDataID;
		public string IconLabel;
		public string SoundLabel;
		public float Amount;
		public float PercentAdd;
		public float PercentMult;
		public float TickTime;
		public float TickCount;
		public EEffectType EffectType;
	}

	[Serializable]
	public class EffectDataLoader : ILoader<int, EffectData>
	{
		public List<EffectData> effects = new List<EffectData>();
		public Dictionary<int, EffectData> MakeDict()
		{
			Dictionary<int, EffectData> dict = new Dictionary<int, EffectData>();
			foreach (EffectData effect in effects)
				dict.Add(effect.DataId, effect);
			return dict;
		}
	}
	#endregion

	#region AoEData
	[Serializable]
	public class AoEData
	{
		public int DataId;
		public string Name;
		public string ClassName;
		public string SkeletonDataID;
		public string SoundLabel;
		public float Duration;
		public List<int> AllyEffects = new List<int>();
		public List<int> EnemyEffects = new List<int>();
		public string AnimName;
	}

	[Serializable]
	public class AoEDataLoader : ILoader<int, AoEData>
	{
		public List<AoEData> aoes = new List<AoEData>();
		public Dictionary<int, AoEData> MakeDict()
		{
			Dictionary<int, AoEData> dict = new Dictionary<int, AoEData>();
			foreach (AoEData aoe in aoes)
				dict.Add(aoe.DataId, aoe);
			return dict;
		}
	}
	#endregion



	#region TextData
	[Serializable]
	public class TextData
	{
		public string DataId;
		public string KOR;
	}

	[Serializable]
	public class TextDataLoader : ILoader<string, TextData>
	{
		public List<TextData> texts = new List<TextData>();
		public Dictionary<string, TextData> MakeDict()
		{
			Dictionary<string, TextData> dict = new Dictionary<string, TextData>();
			foreach (TextData text in texts)
				dict.Add(text.DataId, text);
			return dict;
		}
	}
	#endregion

	#region Item
	// Equipment.Weapon.Dagger
	// Consumable.Potion.Hp
	[Serializable]
	public class BaseData
	{
		public int DataId;
	}

	[Serializable]
	public class ItemData : BaseData
	{
		public string Name;
		public EItemGroupType ItemGroupType;
		public EItemType Type;
		public EItemSubType SubType;
		public EItemGrade Grade;
		public int MaxStack;
	}

	[Serializable]
	public class EquipmentData : ItemData
	{
		public int Damage;
		public int Defence;
		public int Speed;
	}

	[Serializable]
	public class ConsumableData : ItemData
	{
		public double Value;
		public int CoolTime;
	}

	[Serializable]
	public class ItemDataLoader<T> : ILoader<int, T> where T : BaseData
	{
		public List<T> items = new List<T>();

		public Dictionary<int, T> MakeDict()
		{
			Dictionary<int, T> dict = new Dictionary<int, T>();
			foreach (T item in items)
				dict.Add(item.DataId, item);

			return dict;
		}
	}
	#endregion

	#region DropTable

	public class RewardData
	{
		public int Probability; // 100분율
		public int ItemTemplateId;
		// public int Count;
	}

	[Serializable]
	public class DropTableData_Internal
	{
		public int DataId;
		public int RewardExp;
		public int Prob1;
		public int Item1;
		public int Prob2;
		public int Item2;
		public int Prob3;
		public int Item3;
		public int Prob4;
		public int Item4;
		public int Prob5;
		public int Item5;
	}

	[Serializable]
	public class DropTableData
	{
		public int DataId;
		public int RewardExp;
		public List<RewardData> Rewards = new List<RewardData>();
	}

	[Serializable]
	public class DropTableDataLoader : ILoader<int, DropTableData>
	{
		public List<DropTableData_Internal> dropTables = new List<DropTableData_Internal>();

		public Dictionary<int, DropTableData> MakeDict()
		{
			Dictionary<int, DropTableData> dict = new Dictionary<int, DropTableData>();

			foreach (DropTableData_Internal tempData in dropTables)
			{
				DropTableData data = new DropTableData()
				{
					DataId = tempData.DataId,
					RewardExp = tempData.RewardExp,
				};

				if (tempData.Item1 > 0)
				{
					data.Rewards.Add(new RewardData()
					{
						Probability = tempData.Prob1,
						ItemTemplateId = tempData.Item1,
					});
				}

				if (tempData.Item2 > 0)
				{
					data.Rewards.Add(new RewardData()
					{
						Probability = tempData.Prob2,
						ItemTemplateId = tempData.Item2,
					});
				}

				if (tempData.Item3 > 0)
				{
					data.Rewards.Add(new RewardData()
					{
						Probability = tempData.Prob3,
						ItemTemplateId = tempData.Item3,
					});
				}

				if (tempData.Item4 > 0)
				{
					data.Rewards.Add(new RewardData()
					{
						Probability = tempData.Prob4,
						ItemTemplateId = tempData.Item4,
					});
				}

				if (tempData.Item5 > 0)
				{
					data.Rewards.Add(new RewardData()
					{
						Probability = tempData.Prob5,
						ItemTemplateId = tempData.Item5,
					});
				}

				dict.Add(tempData.DataId, data);
			}

			return dict;
		}
	}


	#endregion

	#region ItemShopData
	[Serializable]
	public class ItemShopData
	{
		public int NO; // 아이템 번호
		public string ItemID_eng; // 영문 아이템 ID
		public string ItemName; // 아이템 이름
		public string ItemType; // 아이템 타입 (업그레이드, 가구, 장식 등)
		public string Description; // 아이템 설명
		public int Price; // 가격
		public int Max_Amount; // 최대 구매 가능 수량
		public List<int> ShopUnlockStageID; // 상점 해금 스테이지 ID 리스트
		public int Increase_CurrentMaxGlasses; // 잔 용량 증가량
	}

	[Serializable]
	public class ItemShopDataLoader : ILoader<int, ItemShopData>
	{
		public List<ItemShopData> items = new List<ItemShopData>();

		public Dictionary<int, ItemShopData> MakeDict()
		{
			Dictionary<int, ItemShopData> dict = new Dictionary<int, ItemShopData>();
			foreach (ItemShopData item in items)
				dict.Add(item.NO, item);
			return dict;
		}
	}
	#endregion

	#region StageData
	[Serializable]
	public class StageData
	{
		public int NO; // 스테이지 번호
		public int StageNumber; // 스테이지 넘버
		public string StageName; // 스테이지 이름
		public string ClearCondition_Desc; // 클리어 조건 설명
		public int TargetSales; // 목표 매출
		public int StageTime; // 스테이지 시간 (분)
		public int AppearingCustomer; // 등장하는 고객 수
		public int MinTotalServedCustomers; // 최소 서빙 고객 수
		public int MaxUnsatisfiedCustomersAllowed; // 허용되는 최대 불만족 고객 수
		public List<int> AutoUnlockRecipeIDs; // 자동 해금되는 레시피 ID 리스트
	}

	[Serializable]
	public class StageDataLoader : ILoader<int, StageData>
	{
		public List<StageData> stages = new List<StageData>();

		public Dictionary<int, StageData> MakeDict()
		{
			Dictionary<int, StageData> dict = new Dictionary<int, StageData>();
			foreach (StageData stage in stages)
				dict.Add(stage.NO, stage);
			return dict;
		}
	}
	#endregion
}