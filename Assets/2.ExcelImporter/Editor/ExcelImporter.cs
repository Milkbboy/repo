using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

public class ExcelImporter : AssetPostprocessor
{
	class ExcelAssetInfo
	{
		public Type AssetType { get; set; }
		public ExcelAssetAttribute Attribute { get; set; }
		public string ExcelName
		{
			get
			{
				return string.IsNullOrEmpty(Attribute.ExcelName) ? AssetType.Name : Attribute.ExcelName;
			}
		}
	}

	static List<ExcelAssetInfo> cachedInfos = null; // Clear on compile.

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		bool imported = false;
		foreach (string path in importedAssets)
		{
			if (Path.GetExtension(path) == ".xls" || Path.GetExtension(path) == ".xlsx")
			{
				if (cachedInfos == null) cachedInfos = FindExcelAssetInfos();

				var excelName = Path.GetFileNameWithoutExtension(path);
				if (excelName.StartsWith("~$")) continue;

				ExcelAssetInfo info = cachedInfos.Find(i =>
				{
					// Debug.Log($"i.ExcelName: {i.ExcelName}, excelName: {excelName}");
					return i.ExcelName == $"{excelName}Table";
				});

				// Debug.Log($"Importing {excelName}..., Path: {path}, AssetInfos: {cachedInfos.Count}, Info AssetType: {info.AssetType.Name}");

				if (info == null) continue;

				ImportExcel(path, info);
				imported = true;
			}
		}

		if (imported)
		{
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}

	static List<ExcelAssetInfo> FindExcelAssetInfos()
	{
		var list = new List<ExcelAssetInfo>();
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach (var type in assembly.GetTypes())
			{
				var attributes = type.GetCustomAttributes(typeof(ExcelAssetAttribute), false);
				if (attributes.Length == 0) continue;
				var attribute = (ExcelAssetAttribute)attributes[0];
				var info = new ExcelAssetInfo()
				{
					AssetType = type,
					Attribute = attribute
				};
				list.Add(info);
			}
		}
		return list;
	}

	static UnityEngine.Object LoadOrCreateAsset(string assetPath, Type assetType)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(assetPath));

		var asset = AssetDatabase.LoadAssetAtPath(assetPath, assetType);

		if (asset == null)
		{
			asset = ScriptableObject.CreateInstance(assetType.Name);
			AssetDatabase.CreateAsset((ScriptableObject)asset, assetPath);
			asset.hideFlags = HideFlags.None;
		}

		return asset;
	}

	static IWorkbook LoadBook(string excelPath)
	{
		using (FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			if (Path.GetExtension(excelPath) == ".xls") return new HSSFWorkbook(stream);
			else return new XSSFWorkbook(stream);
		}
	}

	static List<string> GetFieldNamesFromSheetHeader(ISheet sheet)
	{
		IRow headerRow = sheet.GetRow(0);

		var fieldNames = new List<string>();
		for (int i = 0; i < headerRow.LastCellNum; i++)
		{
			var cell = headerRow.GetCell(i);
			if (cell == null || cell.CellType == CellType.Blank) break;

			// cell.StringCellValue 값을 가져옵니다.
			string cellValue = cell.StringCellValue;

			// # 문자를 제거합니다.
			string cleanedCellValue = cellValue.Replace("#", "");

			// Debug.Log($"cell.StringCellValue: {cell.StringCellValue} => {cleanedCellValue}");

			fieldNames.Add(cleanedCellValue);
		}
		return fieldNames;
	}

	static object CellToFieldObject(ICell cell, FieldInfo fieldInfo, bool isFormulaEvalute = false)
	{
		var type = isFormulaEvalute ? cell.CachedFormulaResultType : cell.CellType;

		switch (type)
		{
			case CellType.String:
				if (fieldInfo.FieldType.IsEnum) return Enum.Parse(fieldInfo.FieldType, cell.StringCellValue);
				else return cell.StringCellValue;
			case CellType.Boolean:
				return cell.BooleanCellValue;
			case CellType.Numeric:
				return Convert.ChangeType(cell.NumericCellValue, fieldInfo.FieldType);
			case CellType.Formula:
				if (isFormulaEvalute) return null;
				return CellToFieldObject(cell, fieldInfo, true);
			default:
				if (fieldInfo.FieldType.IsValueType)
				{
					return Activator.CreateInstance(fieldInfo.FieldType);
				}
				return null;
		}
	}

	static object CreateEntityFromRow(IRow row, List<string> columnNames, Type entityType, string sheetName)
	{
		var entity = Activator.CreateInstance(entityType);

		// Debug.Log($"row.RowNum: ${row.RowNum}, columnNames.Count: {columnNames.Count}");

		for (int i = 0; i < columnNames.Count; i++)
		{
			// Debug.Log($"columnName[{i}]: {columnNames[i]}");

			// 컬럼 이름, 필드 이름을 대문자로 변환하여 비교
			string columnName = columnNames[i].ToUpper(); // 컬럼 이름을 대문자로 변환
			FieldInfo entityField = entityType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.FirstOrDefault(f => f.Name.ToUpper() == columnName); // 필드 이름을 대문자로 변환하여 비교

			// columnNames[i] 와 일치하는 필드를 찾고, 해당 필드에 값을 설정
			// FieldInfo entityField = entityType.GetField(
			// 	columnNames[i],
			// 	BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
			// );
			if (entityField == null) continue;
			if (!entityField.IsPublic && entityField.GetCustomAttributes(typeof(SerializeField), false).Length == 0) continue;

			ICell cell = row.GetCell(i);
			if (cell == null) continue;

			try
			{
				object fieldValue = CellToFieldObject(cell, entityField);
				entityField.SetValue(entity, fieldValue);
			}
			catch
			{
				throw new Exception(string.Format("Invalid excel cell type at row {0}, column {1}, {2} sheet.", row.RowNum, cell.ColumnIndex, sheetName));
			}
		}
		return entity;
	}

	static object GetEntityListFromSheet(ISheet sheet, Type entityType)
	{
		List<string> excelColumnNames = GetFieldNamesFromSheetHeader(sheet);

		Type listType = typeof(List<>).MakeGenericType(entityType);
		MethodInfo listAddMethod = listType.GetMethod("Add", new Type[] { entityType });
		object list = Activator.CreateInstance(listType);

		// row of index 0 is header
		for (int i = 1; i <= sheet.LastRowNum; i++)
		{
			IRow row = sheet.GetRow(i);
			if (row == null) break;

			ICell entryCell = row.GetCell(0);
			if (entryCell == null || entryCell.CellType == CellType.Blank) break;

			var entity = CreateEntityFromRow(row, excelColumnNames, entityType, sheet.SheetName);
			listAddMethod.Invoke(list, new object[] { entity });
		}
		return list;
	}

	static void ImportExcel(string excelPath, ExcelAssetInfo info)
	{
		string assetPath = "";
		string assetName = info.AssetType.Name + ".asset";

		if (string.IsNullOrEmpty(info.Attribute.AssetPath))
		{
			string basePath = Path.GetDirectoryName(excelPath);
			assetPath = Path.Combine(basePath, assetName);
		}
		else
		{
			var path = Path.Combine("Assets", info.Attribute.AssetPath);
			assetPath = Path.Combine(path, assetName);
		}

		// Debug.Log($"excelPath: {excelPath}, assetPath: {assetPath}, info.AssetType: {info.AssetType}");

		UnityEngine.Object asset = LoadOrCreateAsset(assetPath, info.AssetType);

		IWorkbook book = LoadBook(excelPath);

		var assetFields = info.AssetType.GetFields();
		int sheetCount = 0;

		foreach (var assetField in assetFields)
		{
			ISheet sheet = book.GetSheet(assetField.Name);
			if (sheet == null) continue;

			Type fieldType = assetField.FieldType;
			if (!fieldType.IsGenericType || (fieldType.GetGenericTypeDefinition() != typeof(List<>))) continue;

			Type[] types = fieldType.GetGenericArguments();
			Type entityType = types[0];

			object entities = GetEntityListFromSheet(sheet, entityType);
			assetField.SetValue(asset, entities);
			sheetCount++;
		}

		if (info.Attribute.LogOnImport)
		{
			Debug.Log(string.Format("Imported {0} sheets form {1}.", sheetCount, excelPath));
		}

		EditorUtility.SetDirty(asset);
	}
}
