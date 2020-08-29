using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace vietlabs.fr2
{
    public class FR2_Ref
    {
        public int index;
        public int type;
        public int depth;
        public int matchingScore;
        public FR2_Asset asset;
        public FR2_Asset addBy;

        public FR2_Ref(int index, int depth, FR2_Asset asset, FR2_Asset by)
        {
            this.index = index;
            this.depth = depth;

            this.asset = asset;
            type = FR2_RefDrawer.AssetType.GetIndex(asset.extension);
            addBy = by;
        }

        internal List<FR2_Ref> Append(Dictionary<string, FR2_Ref> dict, params string[] guidList)
        {
            var result = new List<FR2_Ref>();

            if (FR2_Cache.Api.disabled)
            {
                //Debug.LogWarning("Cache is disabled!");
                return result;
            }

            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("Cache not yet ready! Please wait!");
                return result;
            }

            //filter to remove items that already in dictionary
            for (var i = 0; i < guidList.Length; i++)
            {
                var guid = guidList[i];
                if (dict.ContainsKey(guid)) continue;

                var child = FR2_Cache.Api.Get(guid);
                if (child == null) continue;

                var r = new FR2_Ref(dict.Count, depth + 1, child, asset);
                if (!asset.IsFolder) dict.Add(guid, r);
                result.Add(r);
            }

            return result;
        }

        internal void AppendUsedBy(Dictionary<string, FR2_Ref> result, bool deep)
        {
            var list = Append(result, FR2_Asset.FindUsedByGUIDs(asset).ToArray());
            if (!deep) return;

            // Add next-level
            for (var i = 0; i < list.Count; i++)
            {
                list[i].AppendUsedBy(result, true);
            }
        }

        internal void AppendUsage(Dictionary<string, FR2_Ref> result, bool deep)
        {
            var list = Append(result, FR2_Asset.FindUsageGUIDs(asset, true).ToArray());
            if (!deep) return;

            // Add next-level
            for (var i = 0; i < list.Count; i++)
            {
                list[i].AppendUsage(result, true);
            }
        }

        // --------------------- STATIC UTILS -----------------------

        internal static Dictionary<string, FR2_Ref> FindRefs(string[] guids, bool usageOrUsedBy, bool addFolder)
        {
            var dict = new Dictionary<string, FR2_Ref>();
            var list = new List<FR2_Ref>();

            for (var i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                if (dict.ContainsKey(guid)) continue;

                var asset = FR2_Cache.Api.Get(guid);
                if (asset == null) continue;

                var r = new FR2_Ref(i, 0, asset, null);
                if (!asset.IsFolder || addFolder) dict.Add(guid, r);
                list.Add(r);
            }

            for (var i = 0; i < list.Count; i++)
            {
                if (usageOrUsedBy)
                {
                    list[i].AppendUsage(dict, true);
                }
                else
                {
                    list[i].AppendUsedBy(dict, true);
                }
            }

            //var result = dict.Values.ToList();
            //result.Sort((item1, item2)=>{
            //	return item1.index.CompareTo(item2.index);
            //});

            return dict;
        }

        static public Dictionary<string, FR2_Ref> FindUsage(string[] guids)
        {
            return FindRefs(guids, true, true);
        }

        static public Dictionary<string, FR2_Ref> FindUsedBy(string[] guids)
        {
            return FindRefs(guids, false, true);
        }
    }

    public class FR2_RefDrawer
    {
        public enum Mode
        {
            Dependency,
            Type,
            Extension,
            Folder,
            None
        }

        public enum Sort
        {
            Type,
            Path
        }

        // ORIGINAL
        FR2_Asset[] source;
        Dictionary<string, FR2_Ref> refs;

        // FILTERING
        //static Sort sort;
        //static Mode mode;
        //static HashSet<int> excludes = new HashSet<int>();
        static string searchTerm = string.Empty;

        FR2_TreeUI2.GroupDrawer groupDrawer;
        List<FR2_Ref> list;

        // STATUS
        bool dirty;
        bool caseSensitive;
        bool selectFilter;
        bool showSearch;

        int excludeCount;

        public FR2_RefDrawer()
        {
            groupDrawer = new FR2_TreeUI2.GroupDrawer(DrawGroup, DrawAsset);
        }

        void DrawGroup(Rect r, string label, int childCount)
        {
            if (FR2_Setting.GroupMode == Mode.Folder)
            {
                var tex = AssetDatabase.GetCachedIcon("Assets");
                GUI.DrawTexture(new Rect(r.x - 2f, r.y - 2f, 16f, 16f), tex);
                r.xMin += 16f;
            }

            GUI.Label(r, label + " (" + childCount + ")", EditorStyles.boldLabel);
        }

        void DrawAsset(Rect r, string guid)
        {
            FR2_Ref rf;
            if (!refs.TryGetValue(guid, out rf)) return;

            if (rf.depth == 1) //mode != Mode.Dependency && 
            {
                var c = GUI.color;
                GUI.color = Color.blue;
                GUI.DrawTexture(new Rect(r.x - 4f, r.y + 2f, 2f, 2f), EditorGUIUtility.whiteTexture);
                GUI.color = c;
            }

            rf.asset.Draw(r, false, FR2_Setting.GroupMode != Mode.Folder);
        }

        string GetGroup(FR2_Ref rf)
        {
            if (FR2_Setting.GroupMode == Mode.None) return string.Empty;

            if (rf.depth == 0) return "Selection";

            switch (FR2_Setting.GroupMode)
            {
                case Mode.Extension: return rf.asset.extension;
                case Mode.Type:
                {
                    return AssetType.FILTERS[rf.type].name;
                }

                case Mode.Folder: return rf.asset.assetFolder;

                case Mode.Dependency:
                {
                    return rf.depth == 1 ? "Direct Usage" : "Indirect Usage";
                }
            }

            return string.Empty;
        }

        void SortGroup(List<string> groups)
        {
            groups.Sort((item1, item2) =>
            {
                if (item1 == "Others" || item2 == "Selection") return 1;
                if (item2 == "Others" || item1 == "Selection") return -1;
                return item1.CompareTo(item2);
            });
        }

        public FR2_RefDrawer Reset(string[] assetGUIDs, bool isUsage)
        {
            //Debug.Log("Reset :: " + assetGUIDs.Length + "\n" + string.Join("\n", assetGUIDs));

            if (isUsage)
            {
                refs = FR2_Ref.FindUsage(assetGUIDs);
            }
            else
            {
                refs = FR2_Ref.FindUsedBy(assetGUIDs);
            }

            //RefreshFolders();

            // Remove folders && items in assetGUIDs
            //var map = new Dictionary<string, int>();
            //for (var i = 0;i < assetGUIDs.Length; i++)
            //{
            //	map.Add(assetGUIDs[i], i);	
            //}

            //for (var i = refs.Count-1; i>=0; i--)
            //{
            //	var a = refs[i].asset;
            //	if (!a.IsFolder) continue; // && !map.ContainsKey(refs[i].asset.guid)
            //	refs.RemoveAt(i); //Remove folders and items in Selection
            //}

            dirty = true;
            if (list != null) list.Clear();
            return this;
        }

        void RefreshSort()
        {
            list.Sort((r1, r2) =>
            {
                int v = string.IsNullOrEmpty(searchTerm) ? 0 : r2.matchingScore.CompareTo(r1.matchingScore);
                if (v != 0) return v;

                return SortAsset(
                    r1.asset.assetPath, r2.asset.assetPath,
                    r1.asset.extension, r2.asset.extension,
                    FR2_Setting.SortMode == Sort.Path
                );
            });

            //folderDrawer.GroupByAssetType(list);
            groupDrawer.Reset<FR2_Ref>(list, rf => rf.asset.guid, GetGroup, SortGroup);
        }

        void ApplyFilter()
        {
            dirty = false;

            if (refs == null) return;

            if (list == null)
            {
                list = new List<FR2_Ref>();
            }
            else
            {
                list.Clear();
            }

            var minScore = searchTerm.Length;

            var term1 = searchTerm;
            if (!caseSensitive) term1 = term1.ToLower();
            var term2 = term1.Replace(" ", string.Empty);

            excludeCount = 0;

            foreach (var item in refs)
            {
                var r = item.Value;

                if (r.depth == 0 && !FR2_Setting.ShowSelection) continue;
                if (FR2_Setting.IsTypeExcluded(r.type))
                {
                    excludeCount++;
                    continue; //skip this one
                }

                if (!showSearch || string.IsNullOrEmpty(searchTerm))
                {
                    r.matchingScore = 0;
                    list.Add(r);
                    continue;
                }

                //calculate matching score
                var name1 = r.asset.assetName;
                if (!caseSensitive) name1 = name1.ToLower();
                var name2 = name1.Replace(" ", string.Empty);

                var score1 = StringMatch(term1, name1);
                var score2 = StringMatch(term2, name2);

                r.matchingScore = Mathf.Max(score1, score2);
                if (r.matchingScore > minScore) list.Add(r);
            }

            RefreshSort();
        }

        int SortAsset(string term11, string term12, string term21, string term22, bool swap)
        {
            var v1 = term11.CompareTo(term12);
            var v2 = term21.CompareTo(term22);
            return swap ? (v1 == 0 ? v2 : v1) : (v2 == 0 ? v1 : v2);
        }

        static GUIStyle toolbarSearchField;
        static GUIStyle toolbarSearchFieldCancelButton;
        static GUIStyle toolbarSearchFieldCancelButtonEmpty;

        public void Draw()
        {
            if (refs == null) return;

            if (dirty || list == null) ApplyFilter();
            groupDrawer.Draw();

            if (showSearch)
            {
                if (toolbarSearchField == null)
                {
                    toolbarSearchField = "ToolbarSeachTextFieldPopup";
                    toolbarSearchFieldCancelButton = "ToolbarSeachCancelButton";
                    toolbarSearchFieldCancelButtonEmpty = "ToolbarSeachCancelButtonEmpty";
                }

                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    var v = GUILayout.Toggle(caseSensitive, "Aa", EditorStyles.toolbarButton, GUILayout.Width(24f));
                    if (v != caseSensitive)
                    {
                        caseSensitive = v;
                        dirty = true;
                    }

                    GUILayout.Space(2f);
                    var value = GUILayout.TextField(searchTerm, toolbarSearchField);
                    if (searchTerm != value)
                    {
                        searchTerm = value;
                        dirty = true;
                    }

                    var style = string.IsNullOrEmpty(searchTerm) ? toolbarSearchFieldCancelButtonEmpty : toolbarSearchFieldCancelButton;
                    if (GUILayout.Button("Cancel", style))
                    {
                        searchTerm = string.Empty;
                        dirty = true;
                    }

                    GUILayout.Space(2f);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (FR2_Unity.DrawToggleToolbar(ref FR2_Setting.showSettings, "*", 20f))
                {
                    dirty = true;
                    if (FR2_Setting.showSettings) selectFilter = false;
                }

                bool v;
                if (excludeCount > 0)
                {
                    var oc = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;
                    v = GUILayout.Toggle(selectFilter, "Filter", EditorStyles.toolbarButton, GUILayout.Width(50f));
                    GUI.backgroundColor = oc;
                }
                else
                {
                    v = GUILayout.Toggle(selectFilter, "Filter", EditorStyles.toolbarButton, GUILayout.Width(50f));
                }

                if (v != selectFilter)
                {
                    selectFilter = v;
                    if (selectFilter) FR2_Setting.showSettings = false;
                }

                v = GUILayout.Toggle(showSearch, "Search", EditorStyles.toolbarButton, GUILayout.Width(50f));
                if (v != showSearch)
                {
                    showSearch = v;
                    dirty = true;
                }

                var ss = FR2_Setting.ShowSelection;
                v = GUILayout.Toggle(ss, "Selection", EditorStyles.toolbarButton, GUILayout.Width(60f));
                if (v != ss)
                {
                    FR2_Setting.ShowSelection = v;
                    dirty = true;
                }

                GUILayout.FlexibleSpace();

                var o = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 42f;

                var ov = FR2_Setting.GroupMode;
                var vv = (Mode) EditorGUILayout.EnumPopup("Group", ov, GUILayout.Width(122f));
                if (vv != ov)
                {
                    FR2_Setting.GroupMode = vv;
                    dirty = true;
                }

                GUILayout.Space(4f);
                EditorGUIUtility.labelWidth = 30f;

                var s = FR2_Setting.SortMode;
                var vvv = (Sort) EditorGUILayout.EnumPopup("Sort", s, GUILayout.Width(100f));
                if (vvv != s)
                {
                    FR2_Setting.SortMode = vvv;
                    RefreshSort();
                }

                EditorGUIUtility.labelWidth = o;
            }

            GUILayout.EndHorizontal();

            if (FR2_Setting.showSettings)
            {
                FR2_Setting.s.DrawSettings();
            }
            else if (selectFilter)
            {
                if (AssetType.DrawSearchFilter())
                {
                    dirty = true;
                }
            }
        }

        int StringMatch(string pattern, string input)
        {
            if (input == pattern) return int.MaxValue;
            if (input.Contains(pattern)) return int.MaxValue - 1;

            int pidx = 0;
            int score = 0;
            int tokenScore = 0;

            for (var i = 0; i < input.Length; i++)
            {
                var ch = input[i];
                if (ch == pattern[pidx])
                {
                    tokenScore += tokenScore + 1; //increasing score for continuos token
                    pidx++;
                    if (pidx >= pattern.Length) break;
                }
                else
                {
                    tokenScore = 0;
                }

                score += tokenScore;
            }

            return score;
        }

        // --------------------------------  AssetType ----------------------------

        internal class AssetType
        {
            public string name;
            public HashSet<string> extension;

            public AssetType(string name, params string[] exts)
            {
                this.name = name;
                this.extension = new HashSet<string>();
                for (var i = 0; i < exts.Length; i++)
                {
                    this.extension.Add(exts[i]);
                }
            }

            // ------------------------------- STATIC -----------------------------

            static internal readonly AssetType[] FILTERS = new AssetType[]
            {
                new AssetType("Scene", ".unity"),
                new AssetType("Prefab", ".prefab"),
                new AssetType("Model", ".3df", ".3dm", ".3dmf", ".3dv", ".3dx", ".c5d", ".lwo", ".lws", ".ma", ".mb", ".mesh", ".vrl", ".wrl", ".wrz", ".fbx", ".dae", ".3ds", ".dxf", ".obj", ".skp", ".max", ".blend"),
                new AssetType("Material", ".mat", ".cubemap", ".physicsmaterial"),
                new AssetType("Texture", ".ai", ".apng", ".png", ".bmp", ".cdr", ".dib", ".eps", ".exif", ".ico", ".icon", ".j", ".j2c", ".j2k", ".jas", ".jiff", ".jng", ".jp2", ".jpc", ".jpe", ".jpeg", ".jpf", ".jpg", "jpw", "jpx", "jtf", ".mac", ".omf", ".qif", ".qti", "qtif", ".tex", ".tfw",
                    ".tga", ".tif", ".tiff", ".wmf", ".psd", ".exr", ".rendertexture"),
                new AssetType("Video", ".asf", ".asx", ".avi", ".dat", ".divx", ".dvx", ".mlv", ".m2l", ".m2t", ".m2ts", ".m2v", ".m4e", ".m4v", "mjp", ".mov", ".movie", ".mp21", ".mp4", ".mpe", ".mpeg", ".mpg", ".mpv2", ".ogm", ".qt", ".rm", ".rmvb", ".wmv", ".xvid", ".flv"),
                new AssetType("Audio", ".mp3", ".wav", ".ogg", ".aif", ".aiff", ".mod", ".it", ".s3m", ".xm"),
                new AssetType("Script", ".cs", ".js", ".boo"),
                new AssetType("Text", ".txt", ".json", ".xml", ".bytes", ".sql"),
                new AssetType("Shader", ".shader", ".cginc"),
                new AssetType("Animation", ".anim", ".controller", ".overridecontroller", ".mask"),
                new AssetType("Unity Asset", ".asset", ".guiskin", ".flare", ".fontsettings", ".prefs"),
                new AssetType("Others") //
            };

            static internal int GetIndex(string ext)
            {
                for (var i = 0; i < FILTERS.Length - 1; i++)
                {
                    if (FILTERS[i].extension.Contains(ext)) return i;
                }

                return FILTERS.Length - 1; //Others
            }

            public static bool DrawSearchFilter()
            {
                var n = FILTERS.Length;
                var nCols = 4;
                var nRows = Mathf.CeilToInt(n / (float) nCols);
                var result = false;

                GUILayout.BeginHorizontal();
                for (var i = 0; i < nCols; i++)
                {
                    GUILayout.BeginVertical();
                    for (var j = 0; j < nRows; j++)
                    {
                        var idx = i * nCols + j;
                        if (idx >= n) break;

                        var s = !FR2_Setting.IsTypeExcluded(idx);
                        var s1 = GUILayout.Toggle(s, FILTERS[idx].name);
                        if (s1 != s)
                        {
                            result = true;
                            FR2_Setting.ToggleTypeExclude(idx);
                        }
                    }

                    GUILayout.EndVertical();
                    if ((i + 1) * nCols >= n) break;
                }

                GUILayout.EndHorizontal();

                return result;
            }
        }
    }
}