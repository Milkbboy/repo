using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ERang
{
    [CustomEditor(typeof(AudioClip))]
    public class AudioPreviewEditor : Editor
    {
        private AudioSource audioSource;

        private void OnEnable()
        {
            if (audioSource == null)
            {
                GameObject audoSourceGameObject = new GameObject("AudioPreview");
                audioSource = audoSourceGameObject.AddComponent<AudioSource>();
                audioSource.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void OnDisable()
        {
            if (audioSource != null)
                DestroyImmediate(audioSource.gameObject);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AudioClip audioClip = (AudioClip)target;

            if (audioClip == null)
            {
                EditorGUILayout.HelpBox("AudioClip이 선택되지 않았습니다.", MessageType.Warning);
                return;
            }

            // GUI.enabled를 true로 설정하여 버튼을 활성화
            GUI.enabled = true;

            if (GUILayout.Button("Play"))
                PlayAudioClip(audioClip);

            if (GUILayout.Button("Stop"))
                StopAudioClip();
        }

        private void PlayAudioClip(AudioClip clip)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = clip;
            audioSource.Play();
        }

        private void StopAudioClip()
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}