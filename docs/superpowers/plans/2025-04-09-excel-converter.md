# Excel Converter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Unity용 Excel → GameData 변환 모듈. Sheet/Ignore 어트리뷰트로 자동 대응.

**Architecture:** ExcelConverter 제네릭 클래스. 리플렉션으로 필드명 = 시트명/컬럼명 자동 대응. ExcelDataReader로 .xlsx 파싱.

**Tech Stack:** Unity, C#, ExcelDataReader

---

## Files

```
Assets/Scripts/ExcelConverter/
├── Attributes.cs
├── ExcelConverter.cs
└── Editor/ExcelConverterEditor.cs

Assets/Plugins/
└── ExcelDataReader.dll (외부 라이브러리)
```

---

## Task 1: 어트리뷰트 정의

**Files:**
- Create: `Assets/Scripts/ExcelConverter/Attributes.cs`

- [ ] **Step 1: SheetAttribute, IgnoreAttribute 구현**

```csharp
using System;

namespace ExcelConverter
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SheetAttribute : Attribute
    {
        public string Name { get; }
        
        public SheetAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class IgnoreAttribute : Attribute
    {
    }
}
```

---

## Task 2: ExcelConverter 메인 로직

**Files:**
- Create: `Assets/Scripts/ExcelConverter/ExcelConverter.cs`

- [ ] **Step 1: ExcelConverter 클래스 구현**

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using UnityEngine;
using ExcelDataReader;

namespace ExcelConverter
{
    public static class ExcelConverter
    {
        public static T Load<T>(string fileName) where T : new()
        {
            string path = Path.Combine(Application.streamingAssetsPath, fileName);
            
            if (!File.Exists(path))
            {
                Debug.LogError($"[ExcelConverter] File not found: {path}");
                return default;
            }
            
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet();
                    return ParseGameData<T>(dataSet);
                }
            }
        }
        
        private static T ParseGameData<T>(DataSet dataSet) where T : new()
        {
            var gameData = new T();
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<IgnoreAttribute>() != null)
                    continue;
                
                if (!field.FieldType.IsGenericType || 
                    field.FieldType.GetGenericTypeDefinition() != typeof(List<>))
                    continue;
                
                string sheetName = GetSheetName(field);
                var sheet = dataSet.Tables[sheetName];
                
                if (sheet == null)
                {
                    Debug.LogWarning($"[ExcelConverter] Sheet not found: {sheetName}");
                    continue;
                }
                
                var rowType = field.FieldType.GetGenericArguments()[0];
                var list = ParseSheet(rowType, sheet);
                field.SetValue(gameData, list);
            }
            
            return gameData;
        }
        
        private static string GetSheetName(FieldInfo field)
        {
            var sheetAttr = field.GetCustomAttribute<SheetAttribute>();
            return sheetAttr != null ? sheetAttr.Name : field.Name;
        }
        
        private static List<object> ParseSheet(Type rowType, DataTable sheet)
        {
            var list = new List<object>();
            
            if (sheet.Rows.Count == 0)
                return list;
            
            var columnMap = BuildColumnMap(sheet.Rows[0], rowType);
            
            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row = sheet.Rows[i];
                var obj = ParseRow(rowType, row, columnMap);
                list.Add(obj);
            }
            
            return list;
        }
        
        private static Dictionary<string, int> BuildColumnMap(DataRow headerRow, Type rowType)
        {
            var map = new Dictionary<string, int>();
            var fields = rowType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<IgnoreAttribute>() != null)
                    continue;
                
                for (int i = 0; i < headerRow.ItemArray.Length; i++)
                {
                    var columnName = headerRow[i]?.ToString()?.Trim();
                    if (columnName == field.Name)
                    {
                        map[field.Name] = i;
                        break;
                    }
                }
            }
            
            return map;
        }
        
        private static object ParseRow(Type type, DataRow row, Dictionary<string, int> columnMap)
        {
            var obj = Activator.CreateInstance(type);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<IgnoreAttribute>() != null)
                    continue;
                
                if (!columnMap.TryGetValue(field.Name, out int columnIndex))
                {
                    Debug.LogWarning($"[ExcelConverter] Column not found: {field.Name} in {type.Name}");
                    continue;
                }
                
                var value = row[columnIndex];
                SetValue(obj, field, value);
            }
            
            return obj;
        }
        
        private static void SetValue(object obj, FieldInfo field, object value)
        {
            if (value == null || value is DBNull)
            {
                field.SetValue(obj, GetDefaultValue(field.FieldType));
                return;
            }
            
            try
            {
                var targetType = field.FieldType;
                var sourceType = value.GetType();
                
                if (targetType == sourceType)
                {
                    field.SetValue(obj, value);
                }
                else if (targetType == typeof(int))
                {
                    field.SetValue(obj, ConvertToInt(value));
                }
                else if (targetType == typeof(long))
                {
                    field.SetValue(obj, Convert.ToInt64(value));
                }
                else if (targetType == typeof(float))
                {
                    field.SetValue(obj, Convert.ToSingle(value));
                }
                else if (targetType == typeof(double))
                {
                    field.SetValue(obj, Convert.ToDouble(value));
                }
                else if (targetType == typeof(string))
                {
                    field.SetValue(obj, value.ToString());
                }
                else if (targetType == typeof(bool))
                {
                    field.SetValue(obj, ConvertToBool(value));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ExcelConverter] Failed to convert {field.Name}: {e.Message}");
                field.SetValue(obj, GetDefaultValue(field.FieldType));
            }
        }
        
        private static int ConvertToInt(object value)
        {
            if (value is double d)
                return (int)d;
            if (value is float f)
                return (int)f;
            return Convert.ToInt32(value);
        }
        
        private static bool ConvertToBool(object value)
        {
            var str = value.ToString()?.ToLower()?.Trim();
            return str == "true" || str == "1" || str == "yes";
        }
        
        private static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }
    }
}
```

---

## Task 3: 에디터 메뉴 (선택)

**Files:**
- Create: `Assets/Scripts/ExcelConverter/Editor/ExcelConverterEditor.cs`

- [ ] **Step 1: 에디터 메뉴 구현**

```csharp
using UnityEditor;
using UnityEngine;

namespace ExcelConverter
{
    public static class ExcelConverterEditor
    {
        [MenuItem("Tools/Excel Converter/Load Test")]
        public static void LoadTestData()
        {
            var gameData = ExcelConverter.Load<GameData>("GameData.xlsx");
            if (gameData != null)
            {
                Debug.Log($"[ExcelConverter] Loaded successfully");
            }
        }
    }
}
```

---

## Task 4: ExcelDataReader 라이브러리 설정

**Note:** ExcelDataReader는 NuGet에서 다운로드 후 Unity Plugins 폴더에 복사 필요.

- [ ] **Step 1: 라이브러리 다운로드**

1. NuGet 사이트에서 `ExcelDataReader` 다운로드
2. `ExcelDataReader.DataSet` 다운로드
3. `Assets/Plugins/` 폴더에 .dll 파일 복사

---

## Example GameData

```csharp
using System.Collections.Generic;
using ExcelConverter;

public class GameData
{
    [Sheet("Weapons")]
    public List<WeaponData> weapons;
    
    public List<ItemData> items;
    
    [Ignore]
    public long TotalGold;
}

public class WeaponData
{
    public int Id;
    public string Name;
    public int Damage;
    public float Speed;
    
    [Ignore]
    public string temp;
}

public class ItemData
{
    public int Id;
    public string Name;
    public int Price;
}
```

---

## Excel Format

| Sheet: "Weapons" | | | |
|---|---|---|---|
| Id | Name | Damage | Speed |
| 1 | Sword | 10 | 1.5 |
| 2 | Bow | 5 | 2.0 |

| Sheet: "items" | | |
|---|---|---|
| Id | Name | Price |
| 1 | Potion | 100 |
| 2 | Key | 50 |

---

## Testing

1. ExcelDataReader.dll 복사
2. StreamingAssets/GameData.xlsx 생성
3. Tools > Excel Converter > Load Test 실행
4. Console에서 로그 확인