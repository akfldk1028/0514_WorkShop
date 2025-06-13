using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Linq;
using Data;
using System;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using Newtonsoft.Json;

public class DataTransformer : EditorWindow
{
#if UNITY_EDITOR
	[MenuItem("Tools/RemoveSaveData")]
	public static void RemoveSaveData()
	{
		string path = Application.persistentDataPath + "/SaveData.json";
		if (File.Exists(path))
		{
			File.Delete(path);
			Debug.Log("SaveFile Deleted");
		}
		else
		{
			Debug.Log("No SaveFile Detected");
		}
	}

	[MenuItem("Tools/ParseExcel %#K")]
	public static void ParseExcelDataToJson()
	{
		// ParseExcelDataToJson<HeroDataLoader, HeroData>("Hero");
		// ParseExcelDataToJson<MonsterDataLoader, MonsterData>("Monster");
		ParseExcelDataToJson<CustomerDataLoader, CustomerData>("Customer");
		ParseExcelDataToJson<PlayerDataLoader, PlayerData>("Player");
		ParseExcelDataToJson<RecipeDataLoader, RecipeData>("Recipe");
		ParseExcelDataToJson<ItemShopDataLoader, ItemShopData>("Item");
		ParseExcelDataToJson<StageDataLoader, StageData>("Stage");
		// ParseExcelDataToJson<HeroInfoDataLoader, HeroInfoData>("HeroInfo");
		// ParseExcelDataToJson<SkillDataLoader, SkillData>("Skill");
		// ParseExcelDataToJson<ProjectileDataLoader, ProjectileData>("Projectile");

		// ParseExcelDataToJson<TextDataLoader, TextData>("Text");

		Debug.Log("DataTransformer Completed");
	}

	#region Helpers
	private static void ParseExcelDataToJson<Loader, LoaderData>(string filename) where Loader : new() where LoaderData : new()
	{
		Loader loader = new Loader();
		FieldInfo field = loader.GetType().GetFields()[0];
		field.SetValue(loader, ParseExcelDataToList<LoaderData>(filename));

		string jsonStr = JsonConvert.SerializeObject(loader, Formatting.Indented);
		File.WriteAllText($"{Application.dataPath}/@Resources/Data/JsonData/{filename}Data.json", jsonStr);
		AssetDatabase.Refresh();
	}

	private static List<LoaderData> ParseExcelDataToList<LoaderData>(string filename) where LoaderData : new()
	{
		List<LoaderData> loaderDatas = new List<LoaderData>();

		string filePath = $"{Application.dataPath}/@Resources/Data/ExcelData/{filename}Data.csv";
		var lines = File.ReadAllLines(filePath);
		if (lines.Length < 2) return loaderDatas;

		// 헤더 줄 파싱
		string[] headers = SplitCsvLine(lines[0]);
        // LoaderData 타입에서 모든 필드를 수집 (상속 포함)
        var fields = GetFieldsInBase(typeof(LoaderData));
		// 필드 이름을 소문자로 정규화하여 매핑 딕셔너리 생성
		var fieldMap = fields.ToDictionary(f => f.Name.ToLower(), f => f);

		// 본문 데이터 라인 파싱
		for (int i = 1; i < lines.Length; i++)
		{
			if (string.IsNullOrWhiteSpace(lines[i])) continue;

			string[] row = SplitCsvLine(lines[i]);
			LoaderData loaderData = new LoaderData();

			for (int c = 0; c < headers.Length && c < row.Length; c++)
			{
				string header = headers[c].Trim().Replace(" ", "").ToLower();

				// 헤더가 필드 이름과 매칭되는지 확인
				if (!fieldMap.TryGetValue(header, out FieldInfo field))
					continue;

				Type type = field.FieldType;
				string cell = row[c].Trim();

				try
				{   // [NonSerialized] 필드는 무시
					if (field.GetCustomAttributes(typeof(NonSerializedAttribute), false).Length > 0)
						continue;

					// 제네릭(List)인지 일반 타입인지 구분하여 변환
					object value = type.IsGenericType ? ConvertList(cell, type) : ConvertValue(cell, type);
					field.SetValue(loaderData, value);
				}
				catch (Exception ex)
				{
					Debug.LogWarning($"[Parse Error] Line {i + 1}, Column '{header}' = '{cell}': {ex.Message}");
				}
			}

			loaderDatas.Add(loaderData);
		}

		return loaderDatas;
	}

	// 쉼표와 따옴표가 섞인 CSV 라인을 정확히 분리
	private static string[] SplitCsvLine(string line)
	{
		List<string> result = new List<string>();
		bool inQuotes = false;
		string current = "";

		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];

			if (c == '"')
			{
				inQuotes = !inQuotes; // 따옴표 안/밖 상태 전환
				continue;
			}

			if (c == ',' && !inQuotes)
			{
				result.Add(current);
				current = "";
			}
			else
			{
				current += c;
			}
		}
		result.Add(current); // 마지막 필드 추가

		return result.ToArray();
	}

	// 문자열을 지정된 타입으로 변환
	private static object ConvertValue(string value, Type type)
	{
		if (string.IsNullOrWhiteSpace(value))
			return GetDefaultValue(type);

		try
		{
			if (type == typeof(string))
				return value;

			TypeConverter converter = TypeDescriptor.GetConverter(type);
			return converter.ConvertFromString(value.Replace(",", "")); // 쉼표 포함 숫자 처리
		}
		catch
		{
			return GetDefaultValue(type); // 오류 시 기본값 반환
		}
	}

	// 타입의 기본값 반환 (int → 0, float → 0f 등)
	private static object GetDefaultValue(Type type)
	{
		if (type.IsValueType)
			return Activator.CreateInstance(type);
		return null;
	}

	// 문자열을 리스트 타입으로 변환
	private static object ConvertList(string value, Type type)
	{
		if (string.IsNullOrEmpty(value))
			return null;

		Type valueType = type.GetGenericArguments()[0];
		Type genericListType = typeof(List<>).MakeGenericType(valueType);
		var genericList = Activator.CreateInstance(genericListType) as IList;

		List<string> splitList;
  

  
		// CASE 1: ["a", "b", "c"] or ['a','b','c']
		if (value.StartsWith("[") && value.EndsWith("]"))
		{
			value = value.Trim('[', ']');
			value = value.Replace("\"", "").Replace("'", "");
			splitList = value.Split(',').Select(x => x.Trim()).ToList();
		}
		// CASE 2: a&b&c 스타일
		else if (value.Contains("&"))
		{
			splitList = value.Split('&').Select(x => x.Trim()).ToList();
		}
		// CASE 3: 단일 값
		else
		{
			splitList = new List<string> { value.Trim() };
		}

		foreach (var item in splitList)
		{
			object converted = ConvertValue(item, valueType);
			genericList.Add(converted);
		}

		return genericList;
	}

	// 상속을 포함한 클래스 필드 전부를 가져오는 유틸리티
	public static List<FieldInfo> GetFieldsInBase(Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
	{
		List<FieldInfo> fields = new List<FieldInfo>();
		HashSet<string> fieldNames = new HashSet<string>(); // 중복방지
		Stack<Type> stack = new Stack<Type>();

		// 상속 계층 스택 구성 (Base → Derived)
		while (type != typeof(object))
		{
			stack.Push(type);
			type = type.BaseType;
		}

		// 모든 클래스의 필드 수집
		while (stack.Count > 0)
		{
			Type currentType = stack.Pop();

			foreach (var field in currentType.GetFields(bindingFlags))
			{
				if (fieldNames.Add(field.Name))
				{
					fields.Add(field);
				}
			}
		}

		return fields;
	}
	#endregion

#endif
}