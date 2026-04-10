using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ExcelConverter;

namespace ExcelConverter.Editor
{
    public class ExcelConverterWindow : EditorWindow
    {
        private string excelFileName = "GameData.xlsx";
        private string outputFolder = "Assets/Resources";
        private Type gameDataType;
        private List<Type> gameDataTypes = new List<Type>();
        
        [MenuItem("Tools/Excel Converter/Convert")]
        public static void ShowWindow()
        {
            var window = GetWindow<ExcelConverterWindow>("Excel Converter");
            window.minSize = new Vector2(300, 200);
        }
        
        private void OnEnable()
        {
            FindGameDataTypes();
        }
        
        private void FindGameDataTypes()
        {
            gameDataTypes.Clear();
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(ScriptableObject)) && 
                            !type.IsAbstract &&
                            type.Name.Contains("GameData"))
                        {
                            gameDataTypes.Add(type);
                        }
                    }
                }
                catch { }
            }
            
            if (gameDataTypes.Count > 0)
                gameDataType = gameDataTypes[0];
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Excel to ScriptableObject Converter", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // Excel 파일명
            EditorGUILayout.LabelField("Excel File (StreamingAssets):");
            excelFileName = EditorGUILayout.TextField(excelFileName);
            
            if (GUILayout.Button("Browse Excel File"))
            {
                var path = EditorUtility.OpenFilePanel("Select Excel File", Application.streamingAssetsPath, "xlsx");
                if (!string.IsNullOrEmpty(path))
                {
                    excelFileName = Path.GetFileName(path);
                }
            }
            
            GUILayout.Space(10);
            
            // GameData 타입 선택
            EditorGUILayout.LabelField("GameData Type:");
            if (gameDataTypes.Count == 0)
            {
                EditorGUILayout.HelpBox("No GameData ScriptableObject types found.\nCreate a class inheriting from ScriptableObject with List<> fields.", MessageType.Warning);
            }
            else
            {
                var typeNames = gameDataTypes.ConvertAll(t => t.Name).ToArray();
                var currentIndex = gameDataTypes.IndexOf(gameDataType);
                currentIndex = EditorGUILayout.Popup(currentIndex, typeNames);
                gameDataType = gameDataTypes[currentIndex];
            }
            
            GUILayout.Space(10);
            
            // 출력 폴더
            EditorGUILayout.LabelField("Output Folder:");
            outputFolder = EditorGUILayout.TextField(outputFolder);
            
            GUILayout.Space(20);
            
            // 변환 버튼
            if (gameDataType != null && GUILayout.Button("Convert & Save", GUILayout.Height(30)))
            {
                ConvertAndSave();
            }
        }
        
        private void ConvertAndSave()
        {
            try
            {
                var convertMethod = typeof(ExcelConverter).GetMethod("Load");
                var genericMethod = convertMethod.MakeGenericMethod(gameDataType);
                var gameData = genericMethod.Invoke(null, new object[] { excelFileName }) as ScriptableObject;
                
                if (gameData == null)
                {
                    EditorUtility.DisplayDialog("Error", "Conversion failed. Check console for details.", "OK");
                    return;
                }
                
                // 폴더 확인
                if (!AssetDatabase.IsValidFolder(outputFolder))
                {
                    var folders = outputFolder.Split('/');
                    var currentPath = folders[0];
                    for (int i = 1; i < folders.Length; i++)
                    {
                        if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                        {
                            AssetDatabase.CreateFolder(currentPath, folders[i]);
                        }
                        currentPath += "/" + folders[i];
                    }
                }
                
                // 저장
                var assetPath = $"{outputFolder}/{gameDataType.Name}.asset";
                AssetDatabase.CreateAsset(gameData, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("Success", $"Saved to: {assetPath}", "OK");
                Selection.activeObject = gameData;
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", e.InnerException?.Message ?? e.Message, "OK");
                Debug.LogError($"[ExcelConverter] {e.InnerException?.Message ?? e.Message}");
            }
        }
    }
}