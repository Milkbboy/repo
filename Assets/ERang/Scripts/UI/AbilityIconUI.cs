using UnityEngine;
using TMPro;

namespace ERang
{
    public class AbilityIconUI : MonoBehaviour
    {
        public MeshRenderer iconMeshRenderer;
        public MeshRenderer bgMeshRenderer;
        public TextMeshPro turnCountText;

        private Texture2D abilityIconTexture;
        private Texture2D buffBGTexture;
        private Texture2D debuffBGTexture;

        void Start()
        {
            buffBGTexture = Utils.LoadTexture("Textures/Blank_Green");
            debuffBGTexture = Utils.LoadTexture("Textures/Blank_Red");
        }

        public void SetIcon(Texture2D iconTexture, AiDataType type)
        {
            abilityIconTexture = iconTexture;

            if (abilityIconTexture != null)
                iconMeshRenderer.materials[0].SetTexture("_MainTex", abilityIconTexture);

            if (type == AiDataType.Buff)
            {
                bgMeshRenderer.materials[0].SetTexture("_MainTex", buffBGTexture);
            }
            else if (type == AiDataType.Debuff)
            {
                bgMeshRenderer.materials[0].SetTexture("_MainTex", debuffBGTexture);
            }

            // Debug.Log($"TextMeshPro sortingOrder: {turnCountText.sortingOrder}");
        }

        public void SetDuration(int turn)
        {
            turnCountText.text = turn.ToString();
        }
    }
}