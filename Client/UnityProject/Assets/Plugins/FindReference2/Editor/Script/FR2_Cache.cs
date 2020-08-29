//#define FR2_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace vietlabs.fr2
{
    [InitializeOnLoad]
    public class FR2_CacheHelper : AssetPostprocessor
    {
        static FR2_CacheHelper()
        {
            //if (EditorApplication.isUpdating) return;

            EditorApplication.projectWindowItemOnGUI -= OnGUIProjectItem;
            EditorApplication.projectWindowItemOnGUI += OnGUIProjectItem;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (EditorApplication.isUpdating || EditorApplication.isPlaying) return;
            //Debug.Log("OnPostProcessAllAssets : "+ FR2_Cache.Api.isReady + ":" + importedAssets.Length + ":" + deletedAssets.Length + ":" + movedAssets.Length + ":" + movedFromAssetPaths.Length);

            if (!FR2_Cache.isReady)
            {
#if FR2_DEBUG
			Debug.Log("Not ready, will refresh anyway !");
#endif
                return;
            }

            for (var i = 0; i < importedAssets.Length; i++)
            {
                if (importedAssets[i] == FR2_Cache.CachePath) continue;

                var guid = AssetDatabase.AssetPathToGUID(importedAssets[i]);
                if (!FR2_Asset.IsValidGUID(guid)) continue;
                if (FR2_Cache.Api.AssetMap.ContainsKey(guid))
                {
                    FR2_Cache.Api.RefreshAsset(guid, false);

#if FR2_DEBUG
				Debug.Log("Changed : " + importedAssets[i]);
#endif

                    continue;
                }

                FR2_Cache.Api.AddAsset(guid);
#if FR2_DEBUG
			Debug.Log("New : " + importedAssets[i]);
#endif
            }

            for (var i = 0; i < deletedAssets.Length; i++)
            {
                var guid = AssetDatabase.AssetPathToGUID(deletedAssets[i]);
                FR2_Cache.Api.RemoveAsset(guid);

#if FR2_DEBUG
			Debug.Log("Deleted : " + deletedAssets[i]);
#endif
            }

            for (var i = 0; i < movedAssets.Length; i++)
            {
                var guid = AssetDatabase.AssetPathToGUID(movedAssets[i]);
                var asset = FR2_Cache.Api.Get(guid);
                if (asset != null) asset.LoadAssetInfo();
            }

#if FR2_DEBUG
		Debug.Log("Changes :: " + importedAssets.Length + ":" + FR2_Cache.Api.workCount);
#endif

            if (FR2_Cache.Api.workCount > 0) FR2_Cache.Api.Check4Usage();
        }

        private static void OnGUIProjectItem(string guid, Rect rect)
        {
            if (!FR2_Cache.isReady) return; // not ready
            if (!FR2_Setting.ShowReferenceCount) return;

            var api = FR2_Cache.Api;
            if (FR2_Cache.Api.AssetMap == null)
            {
                FR2_Cache.Api.Check4Changes(false, false);
            }

            FR2_Asset item;

            if (!api.AssetMap.TryGetValue(guid, out item)) return;
            if (item == null || item.UsedByMap == null) return;

            if (item.UsedByMap.Count > 0)
            {
                var content = new GUIContent(item.UsedByMap.Count.ToString());
                var w = EditorStyles.miniLabel.CalcSize(content).x;
                var r = new Rect(rect.x - w, rect.y, w, 16f);
                if (r.x < -4) r.x = -4f;
                GUI.Label(r, content, EditorStyles.miniLabel);
            }
        }
    }

    [Serializable]
    public class FR2_Setting
    {
        static FR2_Setting d;
        internal static bool showSettings;

        public bool referenceCount = true;
        public bool showSelection = false;
        public bool alternateColor = true;

        public bool pingRow = false;

        //public bool scanScripts		= false;
        public Color32 rowColor = new Color32(0, 0, 0, 12);

        public FR2_RefDrawer.Mode groupMode;
        public FR2_RefDrawer.Sort sortMode;
        public int excludeTypes; //32-bit type Mask

        /*
        Doesn't have a settings option - I will include one in next update
        
        2. Hide the reference number - Should be in the setting above so will be coming next
        3. Cache file path should be configurable - coming next in the setting
        4. Disable / Selectable color in alternative rows - coming next in the setting panel
        5. Applied filters aren't saved - Should be fixed in next update too
        6. Hide Selection part - should be com as an option so you can quickly toggle it on or off
        7. Click whole line to ping - coming next by default and can adjustable in the setting panel
        
        */

        static internal FR2_Setting s
        {
            get { return FR2_Cache.Api ? FR2_Cache.Api.setting : (d == null ? (d = new FR2_Setting()) : d); }
        }

        static void setDirty()
        {
            if (FR2_Cache.Api != null)
            {
                EditorUtility.SetDirty(FR2_Cache.Api);
            }
        }

        static public bool ShowReferenceCount
        {
            get { return s.referenceCount; }
            set
            {
                if (s.referenceCount == value) return;
                s.referenceCount = value;
                setDirty();
            }
        }

        static public bool ShowSelection
        {
            get { return s.showSelection; }
            set
            {
                if (s.showSelection == value) return;
                s.showSelection = value;
                setDirty();
            }
        }

        static public bool AlternateRowColor
        {
            get { return s.alternateColor; }
            set
            {
                if (s.alternateColor == value) return;
                s.alternateColor = value;
                setDirty();
            }
        }

        static public Color32 RowColor
        {
            get { return s.rowColor; }
            set
            {
                if (s.rowColor.Equals(value)) return;
                s.rowColor = value;
                setDirty();
            }
        }

        static public bool PingRow
        {
            get { return s.pingRow; }
            set
            {
                if (s.pingRow == value) return;
                s.pingRow = value;
                setDirty();
            }
        }

        //static public bool ScanScripts
        //{
        //	get  { return s.scanScripts; }
        //	set  {
        //		if (s.scanScripts == value) return;
        //		s.scanScripts = value; setDirty();
        //	}
        //}

        static public FR2_RefDrawer.Mode GroupMode
        {
            get { return s.groupMode; }
            set
            {
                if (s.groupMode.Equals(value)) return;
                s.groupMode = value;
                setDirty();
            }
        }

        static public FR2_RefDrawer.Sort SortMode
        {
            get { return s.sortMode; }
            set
            {
                if (s.sortMode.Equals(value)) return;
                s.sortMode = value;
                setDirty();
            }
        }

        static public bool HasTypeExcluded
        {
            get { return s.excludeTypes != 0; }
        }

        static public bool IsTypeExcluded(int type)
        {
            return ((s.excludeTypes >> type) & 1) != 0;
        }

        static public void ToggleTypeExclude(int type)
        {
            var v = ((s.excludeTypes >> type) & 1) != 0;
            if (v)
            {
                s.excludeTypes &= ~(1 << type);
            }
            else
            {
                s.excludeTypes |= (1 << type);
            }

            setDirty();
        }

        public void DrawSettings()
        {
            if (FR2_Unity.DrawToggle(ref pingRow, "Full Row click to Ping"))
            {
                setDirty();
            }

            GUILayout.BeginHorizontal();
            {
                if (FR2_Unity.DrawToggle(ref alternateColor, "Alternate Odd & Even Row Color"))
                {
                    setDirty();
                    FR2_Unity.RepaintFR2Windows();
                }

                EditorGUI.BeginDisabledGroup(!alternateColor);
                {
                    var c = EditorGUILayout.ColorField(rowColor);
                    if (!c.Equals(rowColor))
                    {
                        rowColor = c;
                        setDirty();
                        FR2_Unity.RepaintFR2Windows();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();

            if (FR2_Unity.DrawToggle(ref referenceCount, "Show Usage Count in Project panel"))
            {
                setDirty();
                FR2_Unity.RepaintProjectWindows();
            }

            if (FR2_Unity.DrawToggle(ref showSelection, "Show Selection"))
            {
                setDirty();
                FR2_Unity.RepaintFR2Windows();
            }
        }
    }

    public class FR2_Cache : ScriptableObject
    {
        internal const int FORCE_REFRESH_DURATION = 60 * 60 * 24; // force refresh once per day
        internal const string DEFAULT_CACHE_PATH = "Assets/FR2_Cache.asset";

        internal static int cacheStamp;
        internal static Action onReady;

        internal static bool _triedToLoadCache;
        internal static FR2_Cache _cache;

        internal static string _cacheGUID;

        internal static string CacheGUID
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheGUID)) return _cacheGUID;
                if (_cache != null)
                {
                    _cachePath = AssetDatabase.GetAssetPath(_cache);
                    _cacheGUID = AssetDatabase.AssetPathToGUID(_cachePath);
                    return _cacheGUID;
                }

                return null;
            }
        }

        internal static string _cachePath;

        internal static string CachePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cachePath)) return _cachePath;
                if (_cache != null)
                {
                    _cachePath = AssetDatabase.GetAssetPath(_cache);
                    return _cachePath;
                }

                return null;
            }
        }

        [SerializeField] internal FR2_Setting setting;

        [SerializeField] bool _disabled;
        [SerializeField] bool _autoRefresh;
        [SerializeField] public List<FR2_Asset> AssetList;

        [NonSerialized] internal Dictionary<string, FR2_Asset> AssetMap;
        [NonSerialized] internal List<FR2_Asset> queueLoadContent;
        [NonSerialized] internal List<FR2_Asset> queueUsedBy;

        internal bool ready;
        [NonSerialized] internal Dictionary<string, List<FR2_Asset>> ScriptMap;

// ----------------------------------- INSTANCE -------------------------------------

        [SerializeField] public int timeStamp;
        [NonSerialized] internal int workCount;

        internal static FR2_Cache Api
        {
            get
            {
                if (_cache != null) return _cache;
                if (!_triedToLoadCache) TryLoadCache();
                return _cache;
            }
        }

        internal bool autoRefresh
        {
            get { return _autoRefresh; }
            set
            {
                if (_autoRefresh == value) return;
                _autoRefresh = value;

                if (_autoRefresh)
                {
                    Debug.LogWarning("Auto refresh is ON !");
                    Check4Changes(!EditorApplication.isPlaying, true);
                }
                else
                {
                    Debug.LogWarning("Auto refresh is OFF !");
                }
            }
        }

        internal bool disabled
        {
            get { return _disabled; }
            set
            {
                if (_disabled == value) return;
                _disabled = value;

                if (_disabled)
                {
                    //Debug.LogWarning("FR2 is disabled - Stopping all works !");	
                    ready = false;
                    EditorApplication.update -= AsyncProcess;
                }
                else
                {
                    Check4Changes(!EditorApplication.isPlaying, true);
                }
            }
        }

        internal static bool isReady
        {
            get
            {
                if (!_triedToLoadCache) TryLoadCache();
                return _cache != null && _cache.ready;
            }
        }

        internal static bool hasCache
        {
            get
            {
                if (!_triedToLoadCache) TryLoadCache();
                return _cache != null;
            }
        }

        internal float progress
        {
            get
            {
                var n = workCount - queueLoadContent.Count - queueUsedBy.Count;
                return workCount == 0 ? 1 : n / (float) workCount;
            }
        }

        private static void FoundCache(bool savePrefs, bool writeFile)
        {
            var elapseTime = FR2_Unity.Epoch(DateTime.Now) - _cache.timeStamp;
            _cache.Check4Changes(!EditorApplication.isPlaying, elapseTime > FORCE_REFRESH_DURATION);
            _cachePath = AssetDatabase.GetAssetPath(_cache);
            _cacheGUID = AssetDatabase.AssetPathToGUID(_cachePath);

            if (savePrefs) EditorPrefs.SetString("fr2_cache.guid", _cacheGUID);
            if (writeFile) File.WriteAllText("Library/fr2_cache.guid", _cacheGUID);
        }

        private static bool RestoreCacheFromGUID(string guid, bool savePrefs, bool writeFile)
        {
            if (string.IsNullOrEmpty(guid)) return false;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return false;
            return RestoreCacheFromPath(path, savePrefs, writeFile);
        }

        private static bool RestoreCacheFromPath(string path, bool savePrefs, bool writeFile)
        {
            if (string.IsNullOrEmpty(path)) return false;
            _cache = FR2_Unity.LoadAssetAtPath<FR2_Cache>(path);
            if (_cache != null) FoundCache(savePrefs, writeFile);
            return _cache != null;
        }

        private static void TryLoadCache()
        {
            _triedToLoadCache = true;

            if (RestoreCacheFromPath(DEFAULT_CACHE_PATH, false, false)) return;

            // Check EditorPrefs
            var pref = EditorPrefs.GetString("fr2_cache.guid", string.Empty);
            if (RestoreCacheFromGUID(pref, false, false)) return;

            // Read GUID from File
            if (File.Exists("Library/fr2_cache.guid"))
            {
                if (RestoreCacheFromGUID(File.ReadAllText("Library/fr2_cache.guid"), true, false)) return;
            }

            // Search whole project
            var allAssets = AssetDatabase.GetAllAssetPaths();
            for (var i = 0; i < allAssets.Length; i++)
            {
                if (allAssets[i].EndsWith("/FR2_Cache.asset", StringComparison.Ordinal))
                {
                    RestoreCacheFromPath(allAssets[i], true, true);
                    break;
                }
            }
        }

        internal static void CreateCache()
        {
            _cache = CreateInstance<FR2_Cache>();
            AssetDatabase.CreateAsset(_cache, DEFAULT_CACHE_PATH);
            EditorUtility.SetDirty(_cache);
            AssetDatabase.SaveAssets();

            FoundCache(true, true);
            _cache.Check4Changes(!EditorApplication.isPlaying, true);
        }

        internal static List<string> FindUsage(string[] listGUIDs)
        {
            if (!isReady) return null;

            var refs = Api.FindAssets(listGUIDs, true);

            for (var i = 0; i < refs.Count; i++)
            {
                var tmp = FR2_Asset.FindUsage(refs[i]);

                for (var j = 0; j < tmp.Count; j++)
                {
                    var itm = tmp[j];
                    if (refs.Contains(itm)) continue;
                    refs.Add(itm);
                }
            }

            return refs.Select(item => item.guid).ToList();
        }

        private void OnEnable()
        {
#if FR2_DEBUG
		Debug.Log("OnEnabled : " + _cache);
#endif
            if (_cache == null) _cache = this;
            Check4Changes(!EditorApplication.isPlaying, false);
        }

        internal void Clear()
        {
            AssetList.Clear();
            queueLoadContent.Clear();
            queueUsedBy.Clear();
            AssetMap.Clear();

            Check4Changes(!EditorApplication.isPlaying, true);
        }

        internal void AddSymbol(string symbol, FR2_Asset asset)
        {
            List<FR2_Asset> list;
            if (ScriptMap == null) return;

            var arr = symbol.Split('.');
            var id = arr[arr.Length - 1];

            if (!ScriptMap.TryGetValue(id, out list))
            {
                list = new List<FR2_Asset>();
                ScriptMap.Add(id, list);
            }
            else if (list.Contains(asset))
            {
                return;
            }

            list.Add(asset);
        }

        internal void AddSymbols(FR2_Asset asset)
        {
            for (var j = 0; j < asset.ScriptSymbols.Count; j++)
            {
                AddSymbol(asset.ScriptSymbols[j], asset);
            }
        }

        internal FR2_Asset FindSymbol(string symbol)
        {
            List<FR2_Asset> list;
            if (!ScriptMap.TryGetValue(symbol, out list)) return null;
            return list[0];
        }

        internal List<FR2_Asset> FindAllSymbol(string symbol)
        {
            List<FR2_Asset> list;
            if (!ScriptMap.TryGetValue(symbol, out list)) return null;
            return list;
        }

        internal void ReadFromCache()
        {
            if (AssetList == null) AssetList = new List<FR2_Asset>();
            if (queueLoadContent == null) queueLoadContent = new List<FR2_Asset>();
            if (queueUsedBy == null) queueUsedBy = new List<FR2_Asset>();
            if (AssetMap == null) AssetMap = new Dictionary<string, FR2_Asset>();
            if (ScriptMap == null) ScriptMap = new Dictionary<string, List<FR2_Asset>>();

            queueLoadContent.Clear();
            queueUsedBy.Clear();
            ScriptMap.Clear();
            AssetMap.Clear();

            for (var i = 0; i < _cache.AssetList.Count; i++)
            {
                var item = _cache.AssetList[i];
                item.LoadAssetInfo();
                if (AssetMap.ContainsKey(item.guid))
                {
                    Debug.LogWarning("Something wrong, cache found twice <" + item.guid + ">");
                    continue;
                }

                AssetMap.Add(item.guid, item);
            }
        }

        internal void ReadFromProject(bool force)
        {
            var paths = AssetDatabase.GetAllAssetPaths().ToList();
            paths.RemoveAll(item => !item.StartsWith("Assets/"));
            var guids = paths.Select(item => AssetDatabase.AssetPathToGUID(item)).ToArray();

            cacheStamp++;

            // Check for new assets
            for (var i = 0; i < guids.Length; i++)
            {
                if (!FR2_Asset.IsValidGUID(guids[i])) continue;

                FR2_Asset asset;

                if (AssetMap.TryGetValue(guids[i], out asset))
                {
                    asset.cacheStamp = cacheStamp;
                    continue;
                }

                ;

                // New asset
                AddAsset(guids[i]);
            }

            // Check for deleted assets
            for (var i = AssetList.Count - 1; i >= 0; i--)
            {
                if (AssetList[i].cacheStamp != cacheStamp)
                {
                    RemoveAsset(AssetList[i]);
                }
            }

            // Refresh definition list
            for (var i = 0; i < AssetList.Count; i++)
            {
                AddSymbols(AssetList[i]);
            }

            if (force)
            {
                timeStamp = FR2_Unity.Epoch(DateTime.Now);
                workCount += AssetMap.Count;
                queueLoadContent.AddRange(AssetMap.Values.ToList());
            }
        }

        internal void Check4Changes(bool checkProject, bool checkContent)
        {
#if FR2_DEBUG
	        Debug.Log(string.Format("Check4Changes : checkProject={0} checkContent={1}", checkProject, checkContent));
#endif
            //if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            ready = false;
            ReadFromCache();

            if (checkProject)
            {
                ReadFromProject(checkContent);
            }

            Check4Usage();

#if FR2_DEBUG
		Debug.Log("After checking :: WorkCount :: " + workCount);
#endif
        }

        internal void RefreshAsset(string guid, bool force)
        {
            if (!AssetMap.ContainsKey(guid)) return;
            RefreshAsset(AssetMap[guid], force);
        }

        internal void RefreshSelection()
        {
            var list = FR2_Unity.Selection_AssetGUIDs;
            for (var i = 0; i < list.Length; i++)
            {
                RefreshAsset(list[i], true);
            }

            Check4Work();
        }

        internal void RefreshAsset(FR2_Asset asset, bool force)
        {
            workCount++;

            if (force)
            {
                asset.MarkAsDirty();

                if ((asset.type == FR2_AssetType.FOLDER) && !asset.IsMissing)
                {
                    var dirs = Directory.GetDirectories(asset.assetPath, "*", SearchOption.AllDirectories);
                    //refresh children directories as well

                    for (var i = 0; i < dirs.Length; i++)
                    {
                        var guid = AssetDatabase.AssetPathToGUID(dirs[i]);
                        var child = Api.Get(guid);
                        if (child == null) continue;

                        workCount++;
                        child.MarkAsDirty();
                        queueLoadContent.Add(child);
                    }
                }
            }

            queueLoadContent.Add(asset);
        }

        internal void AddAsset(string guid)
        {
            if (AssetMap.ContainsKey(guid))
            {
                Debug.LogWarning("guid already exist <" + guid + ">");
                return;
            }

            var first = AssetList.FirstOrDefault(item => item.guid == guid);
            if (first != null)
            {
                Debug.LogWarning("Should catch asset import ! " + first.assetPath);
                first.LoadAssetInfo();
                RefreshAsset(first, true);
                return;
            }

            var asset = new FR2_Asset(guid);
            asset.cacheStamp = cacheStamp;

            AssetList.Add(asset);
            AssetMap.Add(guid, asset);

            // Do not load content for FR2_Cache asset
            if (guid == FR2_Cache.CacheGUID) return;

            workCount++;
            queueLoadContent.Add(asset);
        }

        internal void RemoveAsset(string guid)
        {
            if (!AssetMap.ContainsKey(guid)) return;
            RemoveAsset(AssetMap[guid]);
        }

        internal void RemoveAsset(FR2_Asset asset)
        {
            AssetList.Remove(asset);

            // Deleted Asset : still in the map but not in the AssetList
            asset.state = FR2_AssetState.MISSING;

            for (var i = 0; i < asset.ScriptSymbols.Count; i++)
            {
                var s = asset.ScriptSymbols[i];
                var l = FindAllSymbol(s);
                if (l != null && l.Contains(asset)) l.Remove(asset);
            }
        }

        internal void Check4Usage()
        {
            queueUsedBy.Clear();
            queueUsedBy.AddRange(AssetMap.Values.ToArray());
            workCount += queueUsedBy.Count;

            for (var i = 0; i < queueUsedBy.Count; i++)
            {
                var q = queueUsedBy[i];

                if (q.UsedByMap == null)
                {
                    q.UsedByMap = new Dictionary<string, FR2_Asset>();
                    q.ScriptUsage = new List<string>();
                }
                else
                {
                    q.UsedByMap.Clear();
                    q.ScriptUsage.Clear();
                }
            }

            ready = workCount > 0;
            Check4Work();
        }

        internal void Check4Work()
        {
            if (disabled || workCount == 0) return;

            ready = false;
            EditorApplication.update -= AsyncProcess;
            EditorApplication.update += AsyncProcess;
        }

        public int priority = 3;
        int frameSkipped;

        //public float asyncDelay = 0.5f;
        //public const float FRAME_DURATION = 1/5000f;

        internal void AsyncProcess()
        {
            if (this == null) return;

            //EditorApplication.isPlayingOrWillChangePlaymode || 
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                //Debug.Log("Skipping Async frame .... " + EditorApplication.isUpdating + ":" + EditorApplication.isCompiling);
                return;
            }

            //Debug.Log("Async process :: ");
            if (frameSkipped++ < (10 - 2 * priority)) return;
            frameSkipped = 0;

            var t = Time.realtimeSinceStartup;

#if FR2_DEBUG
//Debug.Log(Mathf.Round(t) + " : " + progress*workCount + "/" + workCount + ":" + isReady);
#endif

            if (!AsyncWork(queueLoadContent, AsyncLoadContent, t)) return;
            if (!AsyncWork(queueUsedBy, AsyncUsedBy, t)) return;

            workCount = 0;
            ready = true;
            EditorApplication.update -= AsyncProcess;
            EditorUtility.SetDirty(this);
            //AssetDatabase.SaveAssets(); //DO NOT SAVE ASSET !

            //var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            //for (var i = 0; i < windows.Length; i++)
            //{
            //    windows[i].Repaint();
            //}

            //Refresh selection
            if (onReady != null) onReady();
        }

        internal bool AsyncWork<T>(List<T> arr, Action<int, T> action, float t)
        {
            var FRAME_DURATION = 1 / 1000f * (priority * 5 + 1); //prevent zero

            var c = arr.Count;
            var counter = 0;

            while (c-- > 0)
            {
                var last = arr[c];
                arr.RemoveAt(c);
                action(c, last);
                //workCount--;

                var dt = Time.realtimeSinceStartup - t - FRAME_DURATION;
                if (dt >= 0)
                {
                    //Debug.Log("violateTime : " + dt + ":" + counter + "-->" + last);
                    return false;
                }

                counter++;
            }

            return true;
        }

        internal void AsyncLoadContent(int idx, FR2_Asset asset)
        {
            asset.LoadContent(false);
            if (asset.ScriptSymbols.Count > 0)
            {
                AddSymbols(asset);
                //if (asset.assetName == "Script1.cs") Debug.Log(asset.assetName + " ---> " + asset.ScriptTokens.Count + ":" + asset.ScriptSymbols.Count);
            }
        }

        internal void AsyncUsedBy(int idx, FR2_Asset asset)
        {
            if (AssetMap == null) Check4Changes(!EditorApplication.isPlaying, false);
            if (asset.IsFolder) return;

            for (var i = 0; i < asset.ScriptTokens.Count; i++)
            {
                var token = asset.ScriptTokens[i];
                var all = FindAllSymbol(token);

                if (all == null) continue;

                //Debug.Log(asset.assetName +  " :: Find <" + token + "> ---> " + all.Count);

                asset.ScriptUsage.Add(token); //definition found !

                for (var j = 0; j < all.Count; j++)
                {
                    // add to usage list
                    if (all[j].IsMissing || all[j].UsedByMap == null) continue;

                    if (!all[j].UsedByMap.ContainsKey(asset.guid))
                    {
                        all[j].UsedByMap.Add(asset.guid, asset);
                    }
                }
            }

            for (var i = 0; i < asset.UseGUIDs.Count; i++)
            {
                FR2_Asset tAsset;
                if (AssetMap.TryGetValue(asset.UseGUIDs[i], out tAsset))
                {
                    if (tAsset == null || tAsset.UsedByMap == null) continue;

                    if (!tAsset.UsedByMap.ContainsKey(asset.guid))
                    {
                        tAsset.UsedByMap.Add(asset.guid, asset);
                    }
                }
            }
        }

        //---------------------------- Dependencies -----------------------------

        internal FR2_Asset Get(string guid)
        {
            return AssetMap.ContainsKey(guid) ? AssetMap[guid] : null;
        }

        internal List<FR2_Asset> FindAssetsOfType(FR2_AssetType type)
        {
            var result = new List<FR2_Asset>();
            foreach (var item in AssetMap)
            {
                if (item.Value.type != type) continue;
                result.Add(item.Value);
            }

            return result;
        }

        internal List<FR2_Asset> FindAssets(string[] guids, bool scanFolder)
        {
            if (AssetMap == null) Check4Changes(!EditorApplication.isPlaying, false);

            var result = new List<FR2_Asset>();

            if (!isReady)
            {
#if FR2_DEBUG
			Debug.LogWarning("Cache not ready !");
#endif
                return result;
            }

            var folderList = new List<FR2_Asset>();

            if (guids.Length == 0) return result;

            for (var i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                FR2_Asset asset;
                if (!AssetMap.TryGetValue(guid, out asset)) continue;
                if (asset.IsMissing)
                {
                    //Debug.LogWarning("Asset not found < " + guid + "> : Missing reference or Refresh required !");
                    continue;
                }

                if (asset.IsFolder)
                {
                    if (!folderList.Contains(asset)) folderList.Add(asset);
                }
                else
                {
                    result.Add(asset);
                }
            }

            if (!scanFolder || folderList.Count == 0) return result;

            var count = folderList.Count;
            for (var i = 0; i < count; i++)
            {
                var item = folderList[i];

                for (var j = 0; j < item.UseGUIDs.Count; j++)
                {
                    FR2_Asset a;
                    if (!AssetMap.TryGetValue(item.UseGUIDs[j], out a)) continue;

                    if (a.IsMissing) continue;
                    if (a.IsFolder)
                    {
                        if (!folderList.Contains(a))
                        {
                            folderList.Add(a);
                            count++;
                        }
                    }
                    else
                    {
                        result.Add(a);
                    }
                }
            }

            return result;
        }

        //---------------------------- Dependencies -----------------------------

        internal List<List<string>> ScanSimilar()
        {
            if (AssetMap == null) Check4Changes(!EditorApplication.isPlaying, true);
            var dict = new Dictionary<string, List<FR2_Asset>>();
            foreach (var item in AssetMap)
            {
                if (item.Value.IsMissing || item.Value.IsFolder) continue;
                var hash = item.Value.fileInfoHash;

                List<FR2_Asset> list;
                if (!dict.TryGetValue(hash, out list))
                {
                    list = new List<FR2_Asset>();
                    dict.Add(hash, list);
                }

                list.Add(item.Value);
            }

            var result = dict.Values.Where(item => item.Count > 1).ToList();

            result.Sort((item1, item2) => { return item2[0].fileSize.CompareTo(item1[0].fileSize); });

            return result.Select(l => l.Select(i => i.assetPath).ToList()).ToList();
        }

        //internal List<FR2_DuplicateInfo> ScanDuplication(){
        //	if (AssetMap == null) Check4Changes(false);

        //	var dict = new Dictionary<string, FR2_DuplicateInfo>();
        //	foreach (var item in AssetMap){
        //		if (item.Value.IsMissing || item.Value.IsFolder) continue;
        //		var hash = item.Value.GetFileInfoHash();
        //		FR2_DuplicateInfo info;

        //		if (!dict.TryGetValue(hash, out info)){
        //			info = new FR2_DuplicateInfo(hash, item.Value.fileSize);
        //			dict.Add(hash, info);
        //		}

        //		info.assets.Add(item.Value);
        //	}

        //	var result = new List<FR2_DuplicateInfo>();
        //	foreach (var item in dict){
        //		if (item.Value.assets.Count > 1){
        //			result.Add(item.Value);
        //		}
        //	}

        //	result.Sort((item1, item2)=>{
        //		return item2.fileSize.CompareTo(item1.fileSize);
        //	});

        //	return result;
        //}

        internal List<FR2_Asset> ScanUnused()
        {
            if (AssetMap == null) Check4Changes(!EditorApplication.isPlaying, false);

            var result = new List<FR2_Asset>();
            foreach (var item in AssetMap)
            {
                var v = item.Value;
                if (v.IsMissing || v.inEditor || v.IsScript || v.inResources || v.inPlugins || v.inStreamingAsset ||
                    v.IsFolder) continue;
                if (v.UsedByMap.Count == 0)
                {
                    result.Add(v);
                }
            }

            result.Sort((item1, item2) =>
            {
                if (item1.extension == item2.extension)
                {
                    return item1.assetPath.CompareTo(item2.assetPath);
                }

                return item1.extension.CompareTo(item2.extension);
            });
            return result;
        }
    }
}