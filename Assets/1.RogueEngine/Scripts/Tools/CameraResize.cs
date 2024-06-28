using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Script that will resize the camera frame to a supported aspect ratio
    /// By default: only 16/9 and 16/10 are supported
    /// Black bars will appear on the side if the window is different
    /// </summary>

    [RequireComponent(typeof(Camera))]
    public class CameraResize : MonoBehaviour
    {
        private Camera cam;
        private Camera cam_ui;
        private int sheight;
        private int swidth;

        void Start()
        {
            cam = GetComponent<Camera>();
            sheight = Screen.height;
            swidth = Screen.width;

            if (transform.childCount > 0)
                cam_ui = transform.GetChild(0)?.GetComponent<Camera>();

            UpdateSize();
        }

        private void Update()
        {
            if (sheight != Screen.height || swidth != Screen.width)
            {
                sheight = Screen.height;
                swidth = Screen.width;
                UpdateSize();
            }
        }

        public void UpdateSize()
        {
            float screenRatio = Screen.width / (float)Screen.height;
            float targetRatio = GetAspectRatio();

            if (Mathf.Approximately(screenRatio, targetRatio))
            {
                // Screen or window is the target aspect ratio: use the whole area.
                cam.rect = new Rect(0, 0, 1, 1);
                if(cam_ui != null)
                    cam_ui.rect = new Rect(0, 0, 1, 1);
            }
            else if (screenRatio > targetRatio)
            {
                // Screen or window is wider than the target: pillarbox.
                float normalizedWidth = targetRatio / screenRatio;
                float barThickness = (1f - normalizedWidth) / 2f;
                cam.rect = new Rect(barThickness, 0, normalizedWidth, 1);
                if (cam_ui != null)
                    cam_ui.rect = new Rect(barThickness, 0, normalizedWidth, 1);
            }
            else
            {
                // Screen or window is narrower than the target: letterbox.
                float normalizedHeight = screenRatio / targetRatio;
                float barThickness = (1f - normalizedHeight) / 2f;
                cam.rect = new Rect(0, barThickness, 1, normalizedHeight);
                if (cam_ui != null)
                    cam_ui.rect = new Rect(0, barThickness, 1, normalizedHeight);
            }

            /*if (TheGame.IsMobile())
            {
                float size_min = GetCamSizeMin();
                float size_max = GetCamSizeMax();
                float value = GetAspectValue();
                float cam_size = value * size_min + (1f - value) * size_max;
                cam.orthographicSize = cam_size;
            }*/
        }


        public static float GetAspectMin()
        {
            float min = 16f / 10f;
            return min;
        }

        public static float GetAspectMax()
        {
            //bool allow_wide = TheGame.IsMobile() && TheGame.Get() != null;
            //float max = allow_wide ? 16f / 8f : 16f / 9f;
            float max = 16f / 9f;
            return max;
        }

        public static float GetCamSizeMin()
        {
            //bool allow_wide = TheGame.IsMobile() && TheGame.Get() != null;
            //float max = allow_wide ? 4.2f : 4.5f;
            float max = 4.5f;
            return max;
        }

        public static float GetCamSizeMax()
        {
            return 5f;
        }

        public static float GetAspectRatio()
        {
            float max = GetAspectMax();
            float min = GetAspectMin();
            float screenRatio = Screen.width / (float)Screen.height;
            float targetRatio = Mathf.Clamp(screenRatio, min, max);
            return targetRatio;
        }

        public static float GetAspectValue()
        {
            float max = GetAspectMax();
            float min = GetAspectMin();
            float aspect = GetAspectRatio();
            float value = (aspect - min) / (max - min);
            return value;
        }

    }
}
