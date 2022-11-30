using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LibVLCSharp
{
    class OnLoad
    {
#if UNITY_ANDROID
        const string UnityPlugin = "libVLCUnityPlugin";
#else
        const string UnityPlugin = "VLCUnityPlugin";
#endif
        [DllImport(UnityPlugin, CallingConvention = CallingConvention.Cdecl, EntryPoint = "libvlc_unity_set_color_space")]
        static extern void SetColorSpace(UnityColorSpace colorSpace);

        [DllImport(UnityPlugin, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr GetRenderEventFunc();

        enum UnityColorSpace
        {
            Gamma = 0,
            Linear = 1,
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
            Debug.Log("UnityEngine.QualitySettings.activeColorSpace: " + PlayerColorSpace);
            SetColorSpace(PlayerColorSpace);
#if UNITY_ANDROID
            GL.IssuePluginEvent(GetRenderEventFunc(), 1);
#endif
        }
        static UnityColorSpace PlayerColorSpace => QualitySettings.activeColorSpace == 0 ? UnityColorSpace.Gamma : UnityColorSpace.Linear;
    }
}