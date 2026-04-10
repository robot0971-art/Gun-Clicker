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
        public static T Load<T>(string fileName) where T : ScriptableObject
        {
            string path = Path.Combine(Application.streamingAssetsPath, fileName);
            
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Excel file not found: {path}");
            }
            
            DataSet dataSet;
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    dataSet = reader.AsDataSet();
                }
            }
            
            return ParseGameData<T>(dataSet);
        }
        
        private static T ParseGameData<T>(DataSet dataSet) where T : ScriptableObject
        {
            var gameData = ScriptableObject.CreateInstance<T>();
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
                    throw new Exception($"Sheet not found: {sheetName}");
                }
                
                var rowType = field.FieldType.GetGenericArguments()[0];
                var list = ParseSheet(rowType, sheet);
                
                var listType = typeof(List<>).MakeGenericType(rowType);
                var typedList = Activator.CreateInstance(listType);
                var addMethod = listType.GetMethod("Add");
                
                foreach (var item in list)
                {
                    addMethod.Invoke(typedList, new[] { item });
                }
                
                field.SetValue(gameData, typedList);
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
            {
                throw new Exception($"Sheet is empty: {rowType.Name}");
            }
            
            var columnMap = BuildColumnMap(sheet.Rows[0], rowType);
            
            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row = sheet.Rows[i];
                try
                {
                    var obj = ParseRow(rowType, row, columnMap);
                    list.Add(obj);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to parse row {i} in sheet {rowType.Name}: {e.Message}");
                }
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
            
            var targetType = field.FieldType;
            
            try
            {
                // 기본 타입
                if (targetType == typeof(int))
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
                // 클래스 타입 (JSON 파싱)
                else if (!targetType.IsPrimitive && targetType != typeof(string) && !targetType.IsValueType)
                {
                    var jsonStr = value.ToString();
                    if (!string.IsNullOrEmpty(jsonStr) && jsonStr.Trim().StartsWith("{"))
                    {
                        var parsedObj = JsonUtility.FromJson(jsonStr, targetType);
                        field.SetValue(obj, parsedObj);
                    }
                    else
                    {
                        field.SetValue(obj, null);
                    }
                }
                else
                {
                    field.SetValue(obj, value);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to convert {field.Name} ({targetType.Name}): {e.Message}");
            }
        }
        
        private static int ConvertToInt(object value)
        {
            if (value is double d) return (int)d;
            if (value is float f) return (int)f;
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