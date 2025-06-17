using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

public class ExcelAssetScriptMenu
{
	const string ScriptTemplateName = "ExcelAssetScriptTemplete.cs.txt";
	const string FieldTemplete = "\t//public List<EntityType> #FIELDNAME#; // Replace 'EntityType' to an actual type that is serializable.";

	[MenuItem("Assets/Create/ERang/ExcelAssetScript", false, 1)]
	static void CreateScript()
	{
		// string savePath = EditorUtility.SaveFolderPanel("Save ExcelAssetScript", Application.dataPath, "");
		// if (savePath == "") return;

		// 고정된 저장 경로 설정
		string relativePath = "ERang/Excels/ExcelAssetScripts";
		string savePath = Path.Combine(Application.dataPath, relativePath);
		if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

		var selectedAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);

		string excelPath = AssetDatabase.GetAssetPath(selectedAssets[0]);
		string excelName = Path.GetFileNameWithoutExtension(excelPath);
		List<string> sheetNames = GetSheetNames(excelPath);

		string scriptString = BuildScriptString(excelName, sheetNames);

		string path = Path.ChangeExtension(Path.Combine(savePath, $"{excelName}Table"), "cs");
		File.WriteAllText(path, scriptString);

		AssetDatabase.Refresh();
	}

	// [MenuItem("Assets/Create/ERang/ExcelAssetScript", true)]
	static bool CreateScriptValidation()
	{
		var selectedAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
		if (selectedAssets.Length != 1) return false;
		var path = AssetDatabase.GetAssetPath(selectedAssets[0]);
		return Path.GetExtension(path) == ".xls" || Path.GetExtension(path) == ".xlsx";
	}

	static List<string> GetSheetNames(string excelPath)
	{
		var sheetNames = new List<string>();
		using (FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			IWorkbook book = null;
			if (Path.GetExtension(excelPath) == ".xls") book = new HSSFWorkbook(stream);
			else book = new XSSFWorkbook(stream);

			for (int i = 0; i < book.NumberOfSheets; i++)
			{
				var sheet = book.GetSheetAt(i);
				sheetNames.Add(sheet.SheetName);
			}
		}
		return sheetNames;
	}

	static string GetScriptTempleteString()
	{
		string currentDirectory = Directory.GetCurrentDirectory();
		string[] filePath = Directory.GetFiles(currentDirectory, ScriptTemplateName, SearchOption.AllDirectories);
		if (filePath.Length == 0) throw new Exception("Script template not found.");

		string templateString = File.ReadAllText(filePath[0]);
		return templateString;
	}

	static string BuildScriptString(string excelName, List<string> sheetNames)
	{
		string scriptString = GetScriptTempleteString();

		scriptString = scriptString.Replace("#ASSETSCRIPTNAME#", excelName);

		foreach (string sheetName in sheetNames)
		{
			string fieldString = String.Copy(FieldTemplete);
			fieldString = fieldString.Replace("#FIELDNAME#", sheetName);
			fieldString += "\n#ENTITYFIELDS#";
			scriptString = scriptString.Replace("#ENTITYFIELDS#", fieldString);
		}
		scriptString = scriptString.Replace("#ENTITYFIELDS#\n", "");

		return scriptString;
	}

	// 🔄 엑셀 데이터 강제 새로고침 메뉴
	// [MenuItem("ERang/Tools/Force Refresh All Excel Assets", false, 100)]
	static void ForceRefreshAllExcelAssets()
	{
		bool confirmed = EditorUtility.DisplayDialog(
			"Force Refresh Excel Assets",
			"This will DELETE all existing ScriptableObject assets and recreate them from Excel data.\n\n" +
			"⚠️ WARNING: Any manual changes made through the Data Relationship Window will be LOST!\n\n" +
			"Are you sure you want to continue?",
			"Yes, Refresh from Excel",
			"Cancel"
		);

		if (!confirmed) return;

		try
		{
			// TableExports 폴더의 모든 .asset 파일 삭제
			string[] assetFiles = Directory.GetFiles(Application.dataPath + "/ERang/Resources/TableExports", "*.asset");

			foreach (string assetFile in assetFiles)
			{
				string relativePath = "Assets" + assetFile.Substring(Application.dataPath.Length).Replace('\\', '/');
				AssetDatabase.DeleteAsset(relativePath);
				Debug.Log($"[ForceRefresh] Deleted: {relativePath}");
			}

			AssetDatabase.Refresh();

			// 엑셀 파일들 재임포트 유도
			EditorUtility.DisplayDialog(
				"Assets Deleted",
				"All ScriptableObject assets have been deleted.\n\n" +
				"Now please manually re-import your Excel files by:\n" +
				"1. Right-click on Excel files in Project window\n" +
				"2. Select 'Reimport'\n\n" +
				"This will recreate fresh assets from Excel data.",
				"OK"
			);
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[ForceRefresh] Error: {e.Message}");
			EditorUtility.DisplayDialog("Error", $"Failed to delete assets: {e.Message}", "OK");
		}
	}
}