using System.Runtime.InteropServices;

namespace RogueEngine
{

    /// <summary>
    /// If you have issue finding the IsMobileJS function, you need to add a special file in your Assets/Plugins/WebGL folder
    /// You can get the file in a WebGL package in Assets/TcgEngine/Install folder
    /// </summary>

    public class WebGLTool
    {

#if !UNITY_EDITOR && UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern bool IsMobileJS();
#endif

        public static bool IsWebGL()
        {
#if UNITY_WEBGL
            return true;
#else
            return false;
#endif
        }

        public static bool IsMobile()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            return IsMobileJS();
#else
            return false;
#endif
        }

    }

}