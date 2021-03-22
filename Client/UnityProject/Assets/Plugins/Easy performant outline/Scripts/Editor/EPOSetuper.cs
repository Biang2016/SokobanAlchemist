using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.WSA;

#if HDRP_OUTLINE
using UnityEngine.Rendering.HighDefinition;
#endif

namespace EPOOutline
{
#if UNITY_2019_1_OR_NEWER
    public class EPOSetuper : EditorWindow
    {
        private static readonly string URP_OUTLINE_NAME = "URP_OUTLINE";
        private static readonly string HDRP_OUTLINE_NAME = "HDRP_OUTLINE";
        private static readonly string EPODOTweenName = "EPO_DOTWEEN";
        private static readonly string SRPShownID = "EasyPerformantOutlineWasShownAndCanceled";
        private static bool UPRWasFound = false;
        private static bool HDRPWasFound = false;

        private static ListRequest request;
        private static AddRequest addRequest;
        
        private Texture2D logoImage;

        [SerializeField]
        private SetupType setupType;

        public enum SetupType
        {
            BuiltIn,
            URP,
            HDRP
        }

        [SerializeField]
        private Vector2 scroll;

        public static bool ShouldShow
        {
            get
            {
                return PlayerPrefs.GetInt(SRPShownID, 0) == 0;
            }

            set
            {
                PlayerPrefs.SetInt(SRPShownID, value ? 0 : 1);
            }
        }

        private static bool CheckHasDefinition(string definition)
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var definitions = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var splited = definitions.Split(';');

            return Array.Find(splited, x => x == definition) != null;
        }

        private static bool CheckHasURPOutlineDefinition()
        {
            return CheckHasDefinition(URP_OUTLINE_NAME);
        }

        private static bool CheckHasHDRPOutlineDefinition()
        {
            return CheckHasDefinition(HDRP_OUTLINE_NAME);
        }

        private static bool CheckHasEPODotween()
        {
            return CheckHasDefinition(EPODOTweenName);
        }

#if URP_OUTLINE
        private static bool CheckShouldFixFeature()
        {
            var activeAssets = PipelineAssetUtility.ActiveAssets;

            foreach (var asset in activeAssets)
            {
                if (!PipelineAssetUtility.IsURPOrLWRP(asset))
                    continue;

                if (!PipelineAssetUtility.IsAssetContainsSRPOutlineFeature(asset))
                    return true;
            }

            return false;
        }
#endif
        
#if URP_OUTLINE || HDRP_OUTLINE
        private static bool CheckHasActiveRenderers()
        {
            return PipelineAssetUtility.ActiveAssets.Count > 0;
        }
#endif

        private void OnEnable()
        {
            EditorApplication.update += Check;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Check;
        }

        private static void RemoveDefinition(string definition, Func<bool> check)
        {
            if (!check())
                return;

            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var definitions = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var splited = definitions.Split(';');

            var builder = new StringBuilder();

            var addedCount = 0;
            foreach (var item in splited)
            {
                if (item == definition)
                    continue;

                builder.Append(item);
                builder.Append(';');
                addedCount++;
            }

            if (addedCount != 0)
                builder.Remove(builder.Length - 1, 1);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, builder.ToString());
        }

        private static void RemoveSRPOutlineFromDefinitions()
        {
            RemoveDefinition(URP_OUTLINE_NAME, CheckHasURPOutlineDefinition);
        }

        private static void AddURPDefinition()
        {
            AddDefinition(URP_OUTLINE_NAME, CheckHasURPOutlineDefinition);
        }

        private static void AddHDRPDefinition()
        {
            AddDefinition(HDRP_OUTLINE_NAME, CheckHasHDRPOutlineDefinition);
        }

        private static void RemoveDOTweenDefinition()
        {
            RemoveDefinition(EPODOTweenName, CheckHasEPODotween);
        }

        private static void AddDOTweenDefinition()
        {
            AddDefinition(EPODOTweenName, CheckHasEPODotween);
        }

        private static void AddDefinition(string definition, Func<bool> check)
        {
            if (check())
                return;

            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var definitions = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definitions + ";" + definition);
        }
        
        private static void Check()
        {
            if (EditorApplication.isPlaying)
                return;

            if (request == null || request.Error != null)
            {
                request = Client.List();
                return;
            }

            if (!request.IsCompleted)
                return;

            UPRWasFound = HasURPOrLWRP(request.Result);
            HDRPWasFound = HasHDRP(request.Result);

            request = Client.List();
        }

        private static bool HasHDRP(PackageCollection result)
        {
            return HasPackage(result, "com.unity.render-pipelines.high-definition");
        }

        private static bool HasURPOrLWRP(PackageCollection result)
        {
            var name =
#if UNITY_2019_3_OR_NEWER
                "com.unity.render-pipelines.universal";
#else
                "com.unity.render-pipelines.lightweight";
#endif

            return HasPackage(result, name);
        }

        private static bool HasPackage(PackageCollection result, string packageName)
        {
            if (result == null)
                return false;

            var found = false;
            var name = packageName;
            foreach (var item in result)
            {
                if (item.name == name)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        [MenuItem("Tools/Easy performant outline/Setup")]
        private static void ForceShowWindow()
        {
            ShouldShow = true;
            ShowWindow();
        }

        [MenuItem("Tools/Easy performant outline/Online docs")]
        private static void ShowDocs()
        {
            UnityEngine.Application.OpenURL("https://docs.google.com/document/d/17GvzvXNEjpEQ8DShRrVHKQ4I6s2tTVtwX6NzCZZ28AQ");
        }

        private static void ShowWindow()
        {
            if (!ShouldShow)
                return;

            var window = EditorWindow.GetWindow<EPOSetuper>(true, "EPO Setuper", false);
            window.maxSize = new Vector2(500, 500);
            window.minSize = new Vector2(500, 500);
        }

        public void OnGUI()
        {
            if (logoImage == null)
                logoImage = Resources.Load<Texture2D>("Easy performant outline/EP Outline logo");

            var height = 180;
            GUILayout.Space(height);

            var imagePosition = new Rect(Vector2.zero, new Vector2(position.width, height));

            GUI.DrawTexture(imagePosition, logoImage, ScaleMode.ScaleAndCrop, true);

            GUILayout.Space(10);

            scroll = GUILayout.BeginScrollView(scroll);

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Please stop running the app to start setup process", MessageType.Info);

                if (GUILayout.Button("Stop"))
                    EditorApplication.isPlaying = false;

                GUILayout.EndScrollView();
                return;
            }

            if (EditorApplication.isCompiling)
            {
                EditorGUILayout.HelpBox(new GUIContent("Compiling... please wait."));
                return;
            }

            EditorGUILayout.HelpBox("Warning!\n Don't add integrations that is not available in your project. This will lead to compilation errors", MessageType.Warning);

            EditorGUILayout.LabelField("Integrations");

            EditorGUI.indentLevel = 1;

            var shouldAddDotween = EditorGUILayout.Toggle(new GUIContent("DOTween support"), CheckHasEPODotween());
            if (shouldAddDotween)
                AddDOTweenDefinition();
            else
                RemoveDOTweenDefinition();

            EditorGUILayout.Space();

            EditorGUI.indentLevel = 0;

            setupType = (SetupType)EditorGUILayout.EnumPopup("Setup type", setupType);

            switch (setupType)
            {
                case SetupType.BuiltIn:
                    DrawBuiltInSetup();
                    break;
                case SetupType.URP:
                    DrawURPSetup();
                    break;
                case SetupType.HDRP:
                    DrawHDRPSetup();
                    break;
            }

            GUILayout.EndScrollView();
        }

        private void DrawBuiltInSetup()
        {
            EditorGUILayout.HelpBox(new GUIContent("There is no need in making any changes to work with Built-In renderer. Just add Outliner to camera and Outlinable to the object you wish to highlight"));
        }

        private void DrawHDRPSetup()
        {
            if (addRequest != null && !addRequest.IsCompleted)
            {
                EditorGUILayout.HelpBox(new GUIContent("Adding package..."));
                return;
            }

            var packageName = "com.unity.render-pipelines.high-definition";

            if (!HDRPWasFound)
            {
                EditorGUILayout.HelpBox(new GUIContent("There are no package added. Chick 'Add' to add the pipeline package."));

                if (GUILayout.Button("Add"))
                    addRequest = Client.Add(packageName);

                return;
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("Pipeline asset has been found in packages"));

            if (!CheckHasHDRPOutlineDefinition())
            {
                EditorGUILayout.HelpBox(new GUIContent("There is no HDRP_OUTLINE feature added. Click 'Add' to fix it."));
                if (GUILayout.Button("Add"))
                    AddHDRPDefinition();
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("HDRP_OUTLINE definition is added"));

#if HDRP_OUTLINE
            if (!CheckHasActiveRenderers())
            {
                EditorGUILayout.HelpBox(new GUIContent("There are not renderer asset set up. Create one?"));

                if (GUILayout.Button("Create"))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Asset location", "Rendering asset", "asset", "Select the folder to save rendering asset");
                    if (string.IsNullOrEmpty(path))
                    {
                        GUILayout.EndScrollView();
                        return;
                    }

                    var pathNoExt = Path.ChangeExtension(path, string.Empty);
                    pathNoExt = pathNoExt.Substring(0, pathNoExt.Length - 1);

                    var asset = PipelineAssetUtility.CreateHDRPAsset();
                    GraphicsSettings.renderPipelineAsset = asset;
                    AssetDatabase.CreateAsset(asset, path);
                }
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("At least one renderer asset is set up"));

            var volume = FindObjectOfType<CustomPassVolume>();
            if (volume == null)
            {
                EditorGUILayout.HelpBox(new GUIContent("There is no custom pass volume in the scene. Click Add to fix it."));

                if (GUILayout.Button("Add"))
                {
                    var go = new GameObject("Custom volume");
                    go.AddComponent<CustomPassVolume>();

                    EditorUtility.SetDirty(go);
                }
            }
            else
            {
                EditorGUILayout.HelpBox(new GUIContent("The scene has custom pass volume."));

                if (volume.customPasses.Find(x => x is OutlineCustomPass) == null)
                {
                    EditorGUILayout.HelpBox(new GUIContent("The volume doesn't have custom pass. Click Add to fix it."));
                    if (GUILayout.Button("Add"))
                    {
                        volume.AddPassOfType(typeof(OutlineCustomPass));

                        EditorUtility.SetDirty(volume);
                    }
                }
                else
                    EditorGUILayout.HelpBox(new GUIContent("The custom volume is set up"));
            }
#endif
        }

        private void DrawURPSetup()
        {
            if (addRequest != null && !addRequest.IsCompleted)
            {
                EditorGUILayout.HelpBox(new GUIContent("Adding package..."));
                return;
            }

            var packageName =
#if UNITY_2019_3_OR_NEWER
                    "com.unity.render-pipelines.universal";
#else
                    "com.unity.render-pipelines.lightweight";
#endif

            if (!UPRWasFound)
            {
                EditorGUILayout.HelpBox(new GUIContent("There are no package added. Chick 'Add' to add the pipeline package."));

                if (GUILayout.Button("Add"))
                    addRequest = Client.Add(packageName);

                return;
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("Pipeline asset has been found in packages"));

            if (!CheckHasURPOutlineDefinition())
            {
                EditorGUILayout.HelpBox(new GUIContent("There is no URP_OUTLINE feature added. Click 'Add' to fix it."));
                if (GUILayout.Button("Add"))
                    AddURPDefinition();
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("URP_OUTLINE definition is added"));

#if URP_OUTLINE
            if (!CheckHasActiveRenderers())
            {
                EditorGUILayout.HelpBox(new GUIContent("There are not renderer asset set up. Create one?"));

                if (GUILayout.Button("Create"))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Asset location", "Rendering asset", "asset", "Select the folder to save rendering asset");
                    if (string.IsNullOrEmpty(path))
                    {
                        GUILayout.EndScrollView();
                        return;
                    }

                    var pathNoExt = Path.ChangeExtension(path, string.Empty);
                    pathNoExt = pathNoExt.Substring(0, pathNoExt.Length - 1);

                    var rendererAsset = PipelineAssetUtility.CreateRenderData();
                    var asset = PipelineAssetUtility.CreateAsset(rendererAsset);
                    GraphicsSettings.renderPipelineAsset = asset;
                    AssetDatabase.CreateAsset(rendererAsset, pathNoExt + " renderer.asset");
                    AssetDatabase.CreateAsset(asset, path);
                }
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("At least one renderer asset is set up"));

            if (CheckShouldFixFeature())
            {
                var assets = PipelineAssetUtility.ActiveAssets;
                foreach (var asset in assets)
                {
                    if (PipelineAssetUtility.IsAssetContainsSRPOutlineFeature(asset))
                        continue;

                    EditorGUI.indentLevel = 0;

                    var text = string.Format("There is no outline feature added to the pipeline asset called '{0}'. Please add the feature:", asset.name);
                    EditorGUILayout.HelpBox(new GUIContent(text));

                    Editor previous = null;
                    Editor.CreateCachedEditor(PipelineAssetUtility.GetRenderer(asset), null, ref previous);

                    previous.OnInspectorGUI();
                }

                for (var index = 0; index < 10; index++)
                    EditorGUILayout.Space();

                return;
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("Feature is added for all renderers in use"));
#endif

        }

        public void OnDestroy()
        {
            ShouldShow = false;
        }
    }
#endif
        }