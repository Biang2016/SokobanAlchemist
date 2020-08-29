#if UNITY_5_3_OR_NEWER
#define UNITY_4_3_OR_NEWER
#define UNITY_4_4_OR_NEWER
#define UNITY_4_5_OR_NEWER
#define UNITY_4_6_OR_NEWER
#define UNITY_4_7_OR_NEWER
#define UNITY_5_0_OR_NEWER
#define UNITY_5_1_OR_NEWER
#define UNITY_5_2_OR_NEWER
#else
    #if UNITY_5
	    #define UNITY_4_3_OR_NEWER
        #define UNITY_4_4_OR_NEWER
        #define UNITY_4_5_OR_NEWER
        #define UNITY_4_6_OR_NEWER
        #define UNITY_4_7_OR_NEWER
	
        #if UNITY_5_0 
            #define UNITY_5_0_OR_NEWER
	    #elif UNITY_5_1
		    #define UNITY_5_0_OR_NEWER
		    #define UNITY_5_1_OR_NEWER
	    #elif UNITY_5_2
		    #define UNITY_5_0_OR_NEWER
		    #define UNITY_5_1_OR_NEWER
		    #define UNITY_5_2_OR_NEWER
	    #endif
    #else
        #if UNITY_4_3
            #define UNITY_4_3_OR_NEWER
        #elif UNITY_4_4
            #define UNITY_4_3_OR_NEWER
            #define UNITY_4_4_OR_NEWER
        #elif UNITY_4_5    
		    #define UNITY_4_3_OR_NEWER
            #define UNITY_4_4_OR_NEWER
            #define UNITY_4_5_OR_NEWER
        #elif UNITY_4_6
		    #define UNITY_4_3_OR_NEWER
            #define UNITY_4_4_OR_NEWER
            #define UNITY_4_5_OR_NEWER
            #define UNITY_4_6_OR_NEWER
        #elif UNITY_4_7
		    #define UNITY_4_3_OR_NEWER
            #define UNITY_4_4_OR_NEWER
            #define UNITY_4_5_OR_NEWER
            #define UNITY_4_6_OR_NEWER
            #define UNITY_4_7_OR_NEWER
        #endif
    #endif
#endif

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vietlabs.fr2
{
    public class FR2_Unity
    {
        //private static Texture2D _whiteTexture;
        //public static Texture2D whiteTexture {
        //	get {
        //		return EditorGUIUtility.whiteTexture;

        //		#if UNITY_5_0_OR_NEWER
        //		return EditorGUIUtility.whiteTexture;
        //		#else
        //		if (_whiteTexture != null) return _whiteTexture;
        //		_whiteTexture = new Texture2D(1,1, TextureFormat.RGBA32, false);
        //        _whiteTexture.SetPixel(0, 0, Color.white);
        //		_whiteTexture.hideFlags = HideFlags.DontSave;
        //		return _whiteTexture;
        //		#endif
        //	}
        //}

        public static T LoadAssetAtPath<T>(string path) where T : Object
        {
#if UNITY_5_1_OR_NEWER
            return AssetDatabase.LoadAssetAtPath<T>(path);
#else
			return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
#endif
        }

        public static void SetWindowTitle(EditorWindow window, string title)
        {
#if UNITY_5_1_OR_NEWER
            window.titleContent = new GUIContent(title);
#else
	        window.title = title;
#endif
        }

        public static void GetCompilingPhase(string path, out bool isPlugin, out bool isEditor)
        {
#if (UNITY_5_2_0 || UNITY_5_2_1) && !UNITY_5_2_OR_NEWER
			bool oldSystem = true;
#else
            bool oldSystem = false;
#endif

            // ---- Old system: Editor for the plugin should be Plugins/Editor
            if (oldSystem)
            {
                var isPluginEditor = path.StartsWith("Assets/Plugins/Editor/", StringComparison.Ordinal)
                                     || path.StartsWith("Assets/Standard Assets/Editor/", StringComparison.Ordinal)
                                     || path.StartsWith("Assets/Pro Standard Assets/Editor/", StringComparison.Ordinal);

                if (isPluginEditor)
                {
                    isPlugin = true;
                    isEditor = true;
                    return;
                }
            }

            isPlugin = path.StartsWith("Assets/Plugins/", StringComparison.Ordinal)
                       || path.StartsWith("Assets/Standard Assets/", StringComparison.Ordinal)
                       || path.StartsWith("Assets/Pro Standard Assets/", StringComparison.Ordinal);

            isEditor = (oldSystem && isPlugin) ? false : path.Contains("/Editor/");
        }

        public static T LoadAssetWithGUID<T>(string guid) where T : Object
        {
            if (string.IsNullOrEmpty(guid)) return null;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;

#if UNITY_5_1_OR_NEWER
            return AssetDatabase.LoadAssetAtPath<T>(path);
#else
			return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
#endif
        }

        public static void UnloadUnusedAssets()
        {
#if UNITY_5_0_OR_NEWER
            EditorUtility.UnloadUnusedAssetsImmediate();
#else
			EditorUtility.UnloadUnusedAssets();
#endif
            Resources.UnloadUnusedAssets();
        }

        public static string[] Selection_AssetGUIDs
        {
            get
            {
#if UNITY_5_0_OR_NEWER
                return Selection.assetGUIDs;
#else
			var mInfo = typeof(Selection).GetProperty("assetGUIDs", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (mInfo != null){
				return (string[]) mInfo.GetValue(null, null);
			}
			Debug.LogWarning("Unity changed ! Selection.assetGUIDs not found !");
		    return new string[0];
#endif
            }
        }

        internal static int Epoch(DateTime time)
        {
            return (int) (time.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        internal static bool DrawToggle(ref bool v, string label)
        {
            var v1 = GUILayout.Toggle(v, label);
            if (v1 != v)
            {
                v = v1;
                return true;
            }

            return false;
        }

        internal static bool DrawToggleToolbar(ref bool v, string label, float width)
        {
            var v1 = GUILayout.Toggle(v, label, EditorStyles.toolbarButton, GUILayout.Width(width));
            if (v1 != v)
            {
                v = v1;
                return true;
            }

            return false;
        }

        internal static bool DrawToggle(bool v, string label, Action<bool> setter)
        {
            var v1 = GUILayout.Toggle(v, label);
            if (v1 != v)
            {
                if (setter != null) setter(v1);
                return true;
            }

            return false;
        }

        internal static EditorWindow FindEditor(string className)
        {
            var list = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var item in list)
                if (item.GetType().FullName == className)
                    return item;
            return null;
        }

        internal static void RepaintAllEditor(string className)
        {
            var list = Resources.FindObjectsOfTypeAll<EditorWindow>();

            foreach (var item in list)
            {
#if FR2_DEV
			Debug.Log(item.GetType().FullName);
#endif

                if (item.GetType().FullName != className) continue;
                item.Repaint();
            }
        }

        internal static void RepaintProjectWindows()
        {
            RepaintAllEditor("UnityEditor.ProjectBrowser");
        }

        internal static void RepaintFR2Windows()
        {
            RepaintAllEditor("vietlabs.fr2.FR2_Window");
        }

        internal static void ExportSelection()
        {
            Type packageExportT = null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                packageExportT = assembly.GetType("UnityEditor.PackageExport");
                if (packageExportT != null) break;
            }

            if (packageExportT == null)
            {
                Debug.LogWarning("Export Package Error : UnityEditor.PackageExport not found !");
                return;
            }

            var panel = EditorWindow.GetWindow(packageExportT, true, "Exporting package");
#if UNITY_5_2_OR_NEWER
            var prop = "m_IncludeDependencies";
#else
			var prop = "m_bIncludeDependencies";
#endif

            var fieldInfo = packageExportT.GetField(prop, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
            {
                Debug.LogWarning("Export Package error : " + prop + " not found !");
                return;
            }

            var methodInfo = packageExportT.GetMethod("BuildAssetList", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null)
            {
                Debug.LogWarning("Export Package error : BuildAssetList method not found !");
                return;
            }

            fieldInfo.SetValue(panel, false);
            methodInfo.Invoke(panel, null);
            panel.Repaint();
        }
    }
}