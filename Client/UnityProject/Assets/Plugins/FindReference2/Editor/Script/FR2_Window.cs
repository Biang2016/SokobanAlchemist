using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vietlabs.fr2
{
    public class FR2_Window : EditorWindow, IHasCustomMenu
    {
        private static FR2_Window window;
        [NonSerialized] internal FR2_DuplicateTree Duplicated;

        internal int level;
        private Vector2 scrollPos;

        [NonSerialized] internal List<FR2_Asset> Selected;

        private int selectedTab;

        private string tempGUID;
        private Object tempObject;

        //[NonSerialized] List<FR2_DuplicateInfo> duplicateArray;
        //[NonSerialized] FR2_ScrollList duplicateScroller;

        [NonSerialized] private List<FR2_Asset> unusedArray;

        [NonSerialized] private FR2_TreeUI unusedScroller;
        //[NonSerialized] internal FR2_RefTree<FR2_Asset> UsedBy;
        //[NonSerialized] internal FR2_RefTree<FR2_Asset> Uses;

        [NonSerialized] internal FR2_RefDrawer UsesDrawer;
        [NonSerialized] internal FR2_RefDrawer UsedByDrawer;

        private bool IsFocusingUses
        {
            get { return selectedTab == 0; }
        }

        private bool IsFocusingUsedBy
        {
            get { return selectedTab == 1; }
        }

        private bool IsFocusingDuplicate
        {
            get { return selectedTab == 2; }
        }

        private bool IsFocusingUnused
        {
            get { return selectedTab == 3; }
        }

        private bool IsFocusingGUIDs
        {
            get { return selectedTab == 4; }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            var api = FR2_Cache.Api;
            if (api == null) return;

            menu.AddDisabledItem(new GUIContent("FR2 - v1.2.10"));
            menu.AddSeparator(string.Empty);

            menu.AddItem(new GUIContent("Enable"), !api.disabled, () => { api.disabled = !api.disabled; });
            menu.AddItem(new GUIContent("Refresh"), false, () => FR2_Cache.Api.Check4Changes(true, true));

#if FR2_DEV
		    menu.AddItem(new GUIContent("Refresh Usage"), false, () => FR2_Cache.Api.Check4Usage());
		    menu.AddItem(new GUIContent("Refresh Selected"), false, ()=> FR2_Cache.Api.RefreshSelection());
		    menu.AddItem(new GUIContent("Clear Cache"), false, () => FR2_Cache.Api.Clear());
#endif
        }

        [MenuItem("Window/Find Reference 2")]
        private static void Initialize()
        {
            if (window == null)
            {
                window = GetWindow<FR2_Window>();
                window.Init();
            }

            FR2_Unity.SetWindowTitle(window, "FR2");
            window.Show();
        }

        private void Init()
        {
            //Uses = new FR2_RefTree<FR2_Asset>(FR2_Asset.FindUsage, DrawAsset);
            //UsedBy = new FR2_RefTree<FR2_Asset>(FR2_Asset.FindUsedBy, DrawAsset);

            UsesDrawer = new FR2_RefDrawer();
            UsedByDrawer = new FR2_RefDrawer();

            Duplicated = new FR2_DuplicateTree();
            //    (item=>item.GetChildren(), (item, rect, s, b) =>
            //{
            //    item.Draw(rect);
            //    return 16f;
            //});

            //Uses.Sorter = FR2_Asset.SortByExtension;
            //UsedBy.Sorter = FR2_Asset.SortByExtension;

            //Uses.BriefDrawer = DrawBrief;
            //UsedBy.BriefDrawer = DrawBrief;

            FR2_Cache.onReady -= OnReady;
            FR2_Cache.onReady += OnReady;

            //Debug.Log(" FR2_Window ---> Init");
        }

        private void OnReady()
        {
            OnSelectionChange();

            //if (IsFocusingDuplicate)
            //{
            //    RefreshDuplicate(false);
            //}

            if (IsFocusingUnused)
            {
                unusedArray = FR2_Cache.Api.ScanUnused();
            }
        }

        private float DrawAsset(FR2_Asset asset, Rect r, bool highlight)
        {
//, bool hasMouse
            return asset.Draw(r, highlight, true); //hasMouse, 
        }

        private float DrawBrief(FR2_Asset asset, Rect r, bool highlight)
        {
            return asset.Draw(r, false, false);
        }

        private void OnSelectionChange()
        {
            if (!FR2_Cache.isReady) return;

            if (UsesDrawer == null) Init();
            //if (Uses == null) Init();

            //Selected = FR2_Cache.Api.FindAssets(FR2_Unity.Selection_AssetGUIDs, true);

            level = 0;
            //Uses.Reset(Selected);
            //UsedBy.Reset(Selected);
            UsesDrawer.Reset(FR2_Unity.Selection_AssetGUIDs, true);
            UsedByDrawer.Reset(FR2_Unity.Selection_AssetGUIDs, false);

            Repaint();
            EditorApplication.delayCall += Repaint;
        }

        bool DrawEnable()
        {
            var api = FR2_Cache.Api;
            if (api == null) return false;

            var v = api.disabled;

            if (v)
            {
                EditorGUILayout.HelpBox("Find References 2 is disabled!", MessageType.Warning);
                if (GUILayout.Button("Enable"))
                {
                    api.disabled = !api.disabled;
                    Repaint();
                }

                return !api.disabled;
            }

            if (!api.ready)
            {
                var w = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 70f;
                api.priority = EditorGUILayout.IntSlider("Priority", api.priority, 0, 5);
                EditorGUIUtility.labelWidth = w;
            }

            return !api.disabled;
        }

        private static GUIContent[] TOOLBARS = new GUIContent[]
        {
            new GUIContent("Uses"),
            new GUIContent("Used By"),
            new GUIContent("Duplicate"),
            new GUIContent("No Refs"),
            new GUIContent("GUIDs")
        };

        private void OnGUI()
        {
            if (window == null)
            {
                Initialize();
            }

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                EditorGUILayout.HelpBox("FR2 requires serialization mode set to FORCE TEXT!", MessageType.Warning);
                if (GUILayout.Button("FORCE TEXT"))
                {
                    EditorSettings.serializationMode = SerializationMode.ForceText;
                }

                return;
            }

            if (!FR2_Cache.isReady)
            {
                if (!FR2_Cache.hasCache)
                {
                    EditorGUILayout.HelpBox("FR2 cache not found!\nFirst scan may takes quite some time to finish but you would be able to work normally while the scan works in background...", MessageType.Warning);
                    if (GUILayout.Button("Scan project"))
                    {
                        FR2_Cache.CreateCache();
                    }

                    return;
                }

                if (!DrawEnable()) return;

                var api = FR2_Cache.Api;
                var text = "Refreshing ... " + (int) (api.progress * api.workCount) + " / " + api.workCount;
                var rect = GUILayoutUtility.GetRect(1f, Screen.width, 18f, 18f);
                EditorGUI.ProgressBar(rect, api.progress, text);
                Repaint();
                return;
            }

            if (!DrawEnable()) return;

            var newTab = GUILayout.Toolbar(selectedTab, TOOLBARS);

            if (newTab != selectedTab)
            {
                selectedTab = newTab;
                // Check4Changes means delay calls to OnReady :: Refresh !
                //if (FR2_Cache.Api.isReady) FR2_Cache.Api.Check4Changes();
                OnReady();
            }

            var willRepaint = Event.current.type == EventType.ScrollWheel;

            if (Selected == null)
            {
                Selected = new List<FR2_Asset>();
            }

            //if (Selected.Count == 0){
            //	GUILayout.Label("Nothing selected");
            //} 

            if (IsFocusingUses)
            {
                //Uses.Draw();
                UsesDrawer.Draw();
            }
            else if (IsFocusingUsedBy)
            {
                //UsedBy.Draw();
                UsedByDrawer.Draw();
            }
            else if (IsFocusingDuplicate)
            {
                willRepaint = Duplicated.Draw() | willRepaint;
            }
            else if (IsFocusingUnused)
            {
                DrawUnused();
            }
            else if (IsFocusingGUIDs)
            {
                DrawGUIDs();
            }

            if (willRepaint) Repaint();
        }

        private void DrawGUIDs()
        {
            var ids = FR2_Unity.Selection_AssetGUIDs;
            var objs = Selection.objects;

            GUILayout.Label("GUID to Object", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            {
                var guid = EditorGUILayout.TextField(tempGUID ?? string.Empty);
                EditorGUILayout.ObjectField(tempObject, typeof(Object), false, GUILayout.Width(120f));

                if (GUILayout.Button("Paste", EditorStyles.miniButton, GUILayout.Width(70f)))
                {
                    guid = EditorGUIUtility.systemCopyBuffer;
                }

                if (guid != tempGUID && !string.IsNullOrEmpty(guid))
                {
                    tempGUID = guid;

                    tempObject = FR2_Unity.LoadAssetAtPath<Object>
                    (
                        AssetDatabase.GUIDToAssetPath(tempGUID)
                    );
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            //GUILayout.Label("Selection", EditorStyles.boldLabel);
            if (ids.Length == objs.Length)
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                {
                    for (var i = 0; i < ids.Length; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.ObjectField(objs[i], typeof(Object), false);
                            var idi = ids[i];
                            GUILayout.TextField(idi, GUILayout.Width(240f));
                            if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Width(50f)))
                            {
                                EditorGUIUtility.systemCopyBuffer = idi;
                                //Debug.Log(EditorGUIUtility.systemCopyBuffer);  
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndScrollView();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Merge Selection"))
            {
                FR2_Export.MergeDuplicate();
            }
        }

        private void DrawUnused()
        {
            if (unusedArray == null) unusedArray = new List<FR2_Asset>();
            if (unusedScroller == null) unusedScroller = new FR2_TreeUI();

            unusedScroller.Draw(unusedArray.Count, (idx, r, s) =>
            {
                var item = unusedArray[idx];
                item.Draw(r, false, false);
            });
        }
    }
}