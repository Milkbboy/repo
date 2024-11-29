using System.Collections.Generic;
using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    public class TextData
    {
        /// <summary>
        /// 텍스트 ID 값
        /// </summary>
        public string descID;
        /// <summary>
        /// 기획 참조 값
        /// </summary>
        public string nameDesc;
        /// <summary>
        /// 한글 텍스트 내용
        /// </summary>
        public string descKo;
        /// <summary>
        /// 영어 텍스트 내용
        /// </summary>
        public string descEn;

        public static List<TextData> textDatas = new();
        public static Dictionary<string, TextData> textDataDict = new();

        public static void Load(string path = "")
        {
            TextDataTable textDataTable = Resources.Load<TextDataTable>(path);

            if (textDataTable == null)
            {
                Debug.LogError("textDataTable is not found");
                return;
            }

            foreach (var textEntity in textDataTable.items)
            {
                if (textDataDict.ContainsKey(textEntity.DescID))
                    continue;

                TextData textData = new()
                {
                    descID = textEntity.DescID,
                    nameDesc = textEntity.NameDesc,
                    descKo = textEntity.Desc_Ko,
                    descEn = textEntity.Desc_En
                };

                textDatas.Add(textData);
                textDataDict.Add(textData.descID, textData);
            }
        }

        public static TextData GetTextData(string descID)
        {
            if (textDataDict.ContainsKey(descID))
                return textDataDict[descID];

            Debug.LogError($"TextData is not found: {descID}");

            return null;
        }

        public static string GetKr(string textId)
        {
            TextData textData = GetTextData(textId);

            return textData != null ? textData.descKo : $"{Utils.RedText(textId)} TextData 없음";
        }
    }
}