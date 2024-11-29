using UnityEditor;
using UnityEngine;

namespace ERang
{
    [CustomEditor(typeof(AudioClip))]
    public class AudioPreviewEditor : Editor
    {
        private AudioSource audioSource;
        private float playbackPosition = 0f;
        private bool isDraggingSlider = false;

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

            // 수평 레이아웃 시작
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Play"))
                PlayAudioClip(audioClip);

            if (GUILayout.Button("Stop"))
                StopAudioClip();

            GUILayout.EndHorizontal();

            if (audioSource.clip != null)
            {
                EditorGUI.BeginChangeCheck();
                playbackPosition = EditorGUILayout.Slider("Playback Pos", playbackPosition, 0f, audioSource.clip.length);

                if (EditorGUI.EndChangeCheck())
                {
                    isDraggingSlider = true;
                    audioSource.time = playbackPosition;
                }

                if (audioSource.isPlaying && !isDraggingSlider)
                {
                    playbackPosition = audioSource.time;
                    Repaint(); // 슬라이더를 실시간으로 업데이트
                }
            }
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