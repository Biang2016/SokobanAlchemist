//#define FR2_DEBUG_BRACE_LEVEL
//#define FR2_DEBUG_SYMBOL
//#define FR2_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vietlabs.fr2
{
    public enum FR2_AssetType
    {
        UNKNOWN,
        FOLDER,
        SCRIPT,
        SCENE,
        DLL,
        REFERENCABLE,
        BINARY_ASSET,
        MODEL,
        TERRAIN,
        NON_READABLE
    }

    public enum FR2_AssetState
    {
        NEW,
        CACHE,
        MISSING
    }

    [Serializable]
    public class FR2_Asset
    {
        // ------------------------------ CONSTANTS ---------------------------

        private static readonly HashSet<string> SCRIPT_EXTENSIONS = new HashSet<string>()
        {
            ".cs",
            ".js",
            ".boo"
        };

        private static readonly HashSet<string> REFERENCABLE_EXTENSIONS = new HashSet<string>()
        {
            ".anim",
            ".controller",
            ".mat",
            ".unity",
            ".guiskin",
            ".prefab",
            ".overridecontroller",
            ".mask",
            ".rendertexture",
            ".cubemap",
            ".flare",
            ".mat",
            ".prefab",
            ".physicsmaterial",
            ".fontsettings",
            ".asset",
            ".prefs"
        };

        private static readonly HashSet<string> IGNORE_GUIDS = new HashSet<string>()
        {
            "00000000000000001000000000000000", // Assets
            "00000000000000002000000000000000", // ProjectSettings/InputManager.asset
            "00000000000000003000000000000000", // ProjectSettings/TagManager.asset
            "00000000000000004000000000000000", // ProjectSettings/ProjectSettings.asset
            "00000000000000005000000000000000", // Library/BuildPlayer.prefs
            "00000000000000006000000000000000", // ProjectSettings/AudioManager.asset
            "00000000000000007000000000000000", // ProjectSettings/TimeManager.asset
            "00000000000000008000000000000000", // ProjectSettings/DynamicsManager.asset
            "00000000000000009000000000000000", // ProjectSettings/QualitySettings.asset

            // Reserved
            "00000000000000000000000000000000",
            "0000000000000000a000000000000000",
            "0000000000000000b000000000000000",
            "0000000000000000c000000000000000",
            "0000000000000000d000000000000000",
            "0000000000000000e000000000000000",
            "0000000000000000f000000000000000"
        };

        private static readonly HashSet<string> SCRIPT_KEYWORDS = new HashSet<string>()
        {
            "abstract",
            "as",
            "base",
            "break",
            "case",
            "catch",
            "checked",
            "class",
            "const",
            "continue",
            "default",
            "delegate",
            "do",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "finally",
            "fixed",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "interface",
            "internal",
            "is",
            "lock",
            "namespace",
            "new",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sealed",
            "sizeof",
            "stackalloc",
            "static",
            "struct",
            "switch",
            "this",
            "throw",
            "try",
            "typeof",
            "unchecked",
            "unsafe",
            "using",
            "virtual",
            "volatile",
            "while",
            "define",
            "elif",
            "else",
            "endif",
            "endregion",
            "error",
            "if",
            "line",
            "pragma",
            "region",
            "undef",
            "warning",
            "bool",
            "byte",
            "char",
            "decimal",
            "double",
            "float",
            "int",
            "long",
            "object",
            "sbyte",
            "short",
            "string",
            "uint",
            "ulong",
            "ushort",
            "void",
            "var",
            "true",
            "false",
            "Rect",
            "RectOffset",
            "CustomEditor",
            "null",
            "UNITY_4_3",
            "UNITY_5",
            "UNITY_4",
            "Type",
            "BindingFlags",
            "get",
            "set",
            "NonSerialized",
            "Serialized",
            "Package",
            "GUIContent",
            "AppDomain"
        };

        private static readonly HashSet<string> SCRIPT_SYMBOL = new HashSet<string>()
        {
            "class",
            "interface",
            "enum",
            "struct",
            "delegate"
        };

        internal string assetFolder;
        internal string assetName;
        internal string assetPath;

        internal int cacheStamp;
        internal string extension;
        internal string fileInfoHash;

        internal long fileSize;

        public string guid;

        internal bool inEditor;
        internal bool inPlugins;
        internal bool inResources;
        internal bool inStreamingAsset;

        internal bool loaded;
        public List<string> ScriptSymbols; // class, enum, delegate, interface definitions
        public List<string> ScriptTokens; // possibly used symbols

        [NonSerialized] internal List<string> ScriptUsage = new List<string>();

        public int stamp;

        //internal FR2_AssetType __type;

        //{
        //    get { return __type; }
        //    set { __type = value;
        //        if (assetPath.EndsWith("paintjob.asset"))
        //        {
        //            Debug.Log(assetPath + ":" + value);
        //        }
        //    }
        //}

        internal FR2_AssetState state;
        public FR2_AssetType type;
        public List<string> UseGUIDs;

        //[NonSerialized] internal List<FR2_Asset> UsedBy		= new List<FR2_Asset>();
        internal Dictionary<string, FR2_Asset> UsedByMap = new Dictionary<string, FR2_Asset>();

        public FR2_Asset(string guid)
        {
            this.guid = guid;

            type = FR2_AssetType.UNKNOWN;
            UseGUIDs = new List<string>();
            ScriptSymbols = new List<string>();
            ScriptTokens = new List<string>();
        }

        // ------------------------------- GETTERS -----------------------------

        internal string parentFolderPath
        {
            get { return assetPath.Substring(0, assetPath.LastIndexOf('/')); }
        }

        internal bool IsFolder
        {
            get { return type == FR2_AssetType.FOLDER; }
        }

        internal bool IsScript
        {
            get { return type == FR2_AssetType.SCRIPT; }
        }

        internal bool IsReferencable
        {
            get { return type == FR2_AssetType.REFERENCABLE || type == FR2_AssetType.SCENE; }
        }

        internal bool IsBinaryAsset
        {
            get
            {
                return type == FR2_AssetType.BINARY_ASSET ||
                       type == FR2_AssetType.MODEL ||
                       type == FR2_AssetType.TERRAIN;
            }
        }

        internal bool IsMissing
        {
            get { return state == FR2_AssetState.MISSING; }
        }

        public override string ToString()
        {
            return string.Format("FR2_Asset[{0}]", assetName);
        }

        //--------------------------------- STATIC ----------------------------

        internal static bool IsValidGUID(string guid)
        {
            if (IGNORE_GUIDS.Contains(guid))
            {
                return false;
            }

            var p = AssetDatabase.GUIDToAssetPath(guid);
            if (p != null && !p.StartsWith("Assets/", StringComparison.Ordinal))
                return false; // Asset can be missing but can not be at invalid path 
            return p != FR2_Cache.CachePath;
        }

        internal void MarkAsDirty()
        {
            //Debug.Log("Mark Dirty : " + assetName + ":" + type);
            fileInfoHash = null;
            cacheStamp = 0;
            stamp = 0;
            loaded = false;
        }

        // --------------------------------- APIs ------------------------------

        internal void GuessAssetType()
        {
            if (SCRIPT_EXTENSIONS.Contains(extension))
            {
                type = FR2_AssetType.SCRIPT;
            }
            else if (REFERENCABLE_EXTENSIONS.Contains(extension))
            {
                var isUnity = extension == ".unity";
                type = isUnity ? FR2_AssetType.SCENE : FR2_AssetType.REFERENCABLE;

                if (extension == ".asset" || isUnity)
                {
                    var buffer = new byte[5];

                    try
                    {
                        var stream = File.OpenRead(assetPath);
                        stream.Read(buffer, 0, 5);
                        stream.Close();
                    }
#if FR2_DEBUG
                    catch (Exception e)
                    {
                        Debug.LogWarning("Guess Asset Type error :: " + e + "\n" + assetPath);
#else
                    catch
                    {
#endif
                        state = FR2_AssetState.MISSING;
                        return;
                    }

                    var str = string.Empty;
                    foreach (var t in buffer)
                    {
                        str += (char) t;
                    }

                    if (str != "%YAML")
                    {
                        type = FR2_AssetType.BINARY_ASSET;
                        //var assetData = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                        //type = assetData is TerrainData ? FR2_AssetType.TERRAIN : FR2_AssetType.BINARY_ASSET;
                        //assetData = null;
                        //FR2_Unity.UnloadUnusedAssets();
                    }
                }
            }
            else if (extension == ".fbx")
            {
                type = FR2_AssetType.MODEL;
            }
            else if (extension == ".dll")
            {
                type = FR2_AssetType.DLL;
            }
            else
            {
                type = FR2_AssetType.NON_READABLE;
            }
        }

        internal void LoadAssetInfo()
        {
            assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
#if FR2_DEBUG
            Debug.LogWarning("Something wrong ! Should never be here !\n"  + assetPath + "\n" + guid);
#endif
                return;
            }

            var info = new FileInfo(assetPath);
            assetName = info.Name;
            extension = info.Extension.ToLower();
            assetFolder = assetPath.Substring(7, Mathf.Max(0, assetPath.Length - assetName.Length - 7));
            // 7 = "Assets/".Length
            loaded = stamp == FR2_Unity.Epoch(info.LastWriteTime);

            if (Directory.Exists(info.FullName))
            {
                type = FR2_AssetType.FOLDER;
            }
            else if (File.Exists(info.FullName))
            {
                if (type == FR2_AssetType.UNKNOWN) GuessAssetType();

                fileSize = info.Length;
                inEditor = assetPath.Contains("/Editor/");
                inResources = assetPath.Contains("/Resources/");
                inStreamingAsset = assetPath.Contains("/StreamingAsset/");
                inPlugins = assetPath.StartsWith("Assets/Plugins/", StringComparison.Ordinal);
                fileInfoHash = info.Length + info.Extension;
            }
            else
            {
                state = FR2_AssetState.MISSING;
            }
        }

        internal void LoadContent(bool force)
        {
            if (state == FR2_AssetState.NEW)
            {
                LoadAssetInfo();
            }

            if (IsMissing || type == FR2_AssetType.NON_READABLE) return;

            if (type == FR2_AssetType.DLL)
            {
#if FR2_DEBUG
            Debug.LogWarning("Parsing DLL not yet supportted ");
#endif
                return;
            }

            if (loaded && !force) return;

            // Check for file / folder changes & validate if file / folder exist
            var newStamp = stamp;
            var exist = true;

            if (IsFolder)
            {
                var info = new DirectoryInfo(assetPath);
                exist = info.Exists;
                newStamp = FR2_Unity.Epoch(info.LastWriteTime);
            }
            else
            {
                var info = new FileInfo(assetPath);
                exist = info.Exists;
                newStamp = FR2_Unity.Epoch(info.LastWriteTime);
            }

            if (!exist)
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            loaded = true;
            if (newStamp == stamp && !force)
            {
#if FR2_DEBUG
            Debug.Log("Unchanged : " + stamp + ":" + assetName + ":" + type);
#endif
                return; // nothing changed
            }

            stamp = newStamp;

            UseGUIDs.Clear();

            if (IsFolder)
            {
                LoadFolder();
            }
            else if (IsReferencable)
            {
                LoadYAML();
            }
            else if (IsBinaryAsset)
            {
                LoadBinaryAsset();
            }
            else if (IsScript)
            {
                LoadScript();
            }
        }

        internal void AddUseGUID(string fguid, bool checkExist = true)
        {
            if (checkExist && UseGUIDs.Contains(fguid)) return;
            if (!IsValidGUID(fguid)) return;
            UseGUIDs.Add(fguid);
        }

        // ----------------------------- STATIC  ---------------------------------------

        internal static int SortByExtension(FR2_Asset a1, FR2_Asset a2)
        {
            if (a1 == null) return -1;
            if (a2 == null) return 1;

            var result = a1.extension.CompareTo(a2.extension);
            return result == 0 ? a1.assetName.CompareTo(a2.assetName) : result;
        }

        internal static List<FR2_Asset> FindUsage(FR2_Asset asset)
        {
            if (asset == null) return null;

            var refs = FR2_Cache.Api.FindAssets(asset.UseGUIDs.ToArray(), true);

            if (asset.ScriptUsage != null)
            {
                for (var i = 0; i < asset.ScriptUsage.Count; i++)
                {
                    var symbolList = FR2_Cache.Api.FindAllSymbol(asset.ScriptUsage[i]);
                    if (symbolList.Contains(asset)) continue;

                    var symbol = symbolList[0];
                    if (symbol == null || refs.Contains(symbol)) continue;
                    refs.Add(symbol);
                }
            }

            return refs;
        }

        internal static List<FR2_Asset> FindUsedBy(FR2_Asset asset)
        {
            return asset.UsedByMap.Values.ToList();
        }

        internal static List<string> FindUsageGUIDs(FR2_Asset asset, bool includeScriptSymbols)
        {
            var result = new HashSet<string>();
            if (asset == null)
            {
                Debug.LogWarning("Asset invalid : " + asset.assetName);
                return result.ToList();
            }

            for (var i = 0; i < asset.UseGUIDs.Count; i++)
            {
                result.Add(asset.UseGUIDs[i]);
            }

            if (!includeScriptSymbols) return result.ToList();

            if (asset.ScriptUsage != null)
            {
                for (var i = 0; i < asset.ScriptUsage.Count; i++)
                {
                    var symbolList = FR2_Cache.Api.FindAllSymbol(asset.ScriptUsage[i]);
                    if (symbolList.Contains(asset)) continue;

                    var symbol = symbolList[0];
                    if (symbol == null || result.Contains(symbol.guid)) continue;

                    result.Add(symbol.guid);
                }
            }

            return result.ToList();
        }

        internal static List<string> FindUsedByGUIDs(FR2_Asset asset)
        {
            return asset.UsedByMap.Keys.ToList();
        }

        // ----------------------------- DRAW  ---------------------------------------

        internal float Draw(Rect r, bool highlight, bool drawPath = true)
        {
//, bool hasMouse
            var singleLine = r.height <= 18f;
            var rw = r.width;

            r.height = 16f;
            var hasMouse = Event.current.type == EventType.MouseUp && r.Contains(Event.current.mousePosition);

            if (hasMouse && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                if (extension == ".prefab")
                {
                    menu.AddItem(new GUIContent("Edit in Scene"), false, EditPrefab);
                }

                menu.AddItem(new GUIContent("Open"), false, Open);
                menu.AddItem(new GUIContent("Ping"), false, Ping);
                menu.AddItem(new GUIContent(guid), false, CopyGUID);
                //menu.AddItem(new GUIContent("Reload"), false, Reload);

                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Add to Selection"), false, AddToSelection);
                menu.AddItem(new GUIContent("Copy path"), false, CopyAssetPath);
                menu.AddItem(new GUIContent("Copy full path"), false, CopyAssetPathFull);

                if (IsScript)
                {
                    menu.AddSeparator(string.Empty);
                    AddArray(menu, ScriptSymbols, "+ ", "Definitions", "No Definition", false);

                    menu.AddSeparator(string.Empty);
                    AddArray(menu, ScriptUsage, "-> ", "Depends", "No Dependency", true);
                }

                menu.ShowAsContext();
                Event.current.Use();
            }

            if (IsMissing)
            {
                if (!singleLine) r.y += 16f;
                GUI.Label(r, "(missing) " + guid, EditorStyles.whiteBoldLabel);
                return 0;
            }

            //if (IsScript)
            //{
            //    var w = 40f;
            //    var rr = r;
            //    rr.x += r.width - w;
            //    rr.width = w;

            //    GUI.Label(rr,
            //        (ScriptSymbols != null ? "" + ScriptSymbols.Count : "-") + "|" +
            //        (ScriptUsage != null ? "" + ScriptUsage.Count : "-"));
            //}

            //var usageRect = LeftRect(20f, ref r);
            var icon = AssetDatabase.GetCachedIcon(assetPath);
            var iconRect = LeftRect(16f, ref r);
            if (icon != null) GUI.DrawTexture(iconRect, icon);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                var pingRect = FR2_Setting.PingRow ? new Rect(0, r.y, r.x + r.width, r.height) : iconRect;
                if (pingRect.Contains(Event.current.mousePosition))
                {
                    Ping();
                    Event.current.Use();
                }
            }

            //if (UsedByMap != null && UsedByMap.Count > 0)
            //{
            //    var str = new GUIContent(UsedByMap.Count.ToString());

            //    usageRect.y += 1f;
            //    usageRect.xMin = usageRect.xMax - EditorStyles.miniLabel.CalcSize(str).x + 2f;
            //    usageRect.xMax += 2f;
            //    GUI.Label(usageRect, str);
            //}

            var pathW = drawPath ? EditorStyles.miniLabel.CalcSize(new GUIContent(assetFolder)).x : 0;
            var nameW = EditorStyles.boldLabel.CalcSize(new GUIContent(assetName)).x;

            if (singleLine)
            {
                var lbRect = LeftRect(pathW + nameW, ref r);
                if (highlight)
                {
                    var c = GUI.color;
                    GUI.color = new Color(0, 0f, 1f, 0.5f);
                    GUI.DrawTexture(lbRect, EditorGUIUtility.whiteTexture);
                    GUI.color = c;
                }

                if (drawPath)
                {
                    GUI.Label(LeftRect(pathW, ref lbRect), assetFolder, EditorStyles.miniLabel);
                    lbRect.xMin -= 4f;
                    GUI.Label(lbRect, assetName, EditorStyles.boldLabel);
                }
                else
                {
                    GUI.Label(lbRect, assetName);
                }
            }
            else
            {
                if (drawPath)
                    GUI.Label(new Rect(r.x, r.y + 16f, r.width, r.height), assetFolder, EditorStyles.miniLabel);

                var lbRect = LeftRect(nameW, ref r);
                if (highlight)
                {
                    var c = GUI.color;
                    GUI.color = new Color(0, 0f, 1f, 0.5f);
                    GUI.DrawTexture(lbRect, EditorGUIUtility.whiteTexture);
                    GUI.color = c;
                }

                GUI.Label(lbRect, assetName, EditorStyles.boldLabel);
            }

            if (Event.current.type == EventType.Repaint)
            {
                return rw < pathW + nameW ? 32f : 18f;
            }

            return r.height;
        }

        private Rect LeftRect(float w, ref Rect rect)
        {
            rect.x += w;
            rect.width -= w;
            return new Rect(rect.x - w, rect.y, w, rect.height);
        }

        internal GenericMenu AddArray(GenericMenu menu, List<string> list, string prefix, string title,
            string emptyTitle, bool showAsset, int max = 10)
        {
            if (list.Count > 0)
            {
                if (list.Count > max)
                {
                    prefix = string.Format("{0} _{1}/", title, list.Count) + prefix;
                }

                for (var i = 0; i < list.Count; i++)
                {
                    var def = list[i];
                    var suffix = showAsset ? "/" + FR2_Cache.Api.FindSymbol(def).assetName : string.Empty;
                    menu.AddItem(new GUIContent(prefix + def + suffix), false, () => OpenScript(def));
                }
            }
            else
            {
                menu.AddItem(new GUIContent(emptyTitle), true, null);
            }

            return menu;
        }

        internal void CopyGUID()
        {
            EditorGUIUtility.systemCopyBuffer = guid;
            Debug.Log(guid);
        }

        internal void CopyName()
        {
            EditorGUIUtility.systemCopyBuffer = assetName;
            Debug.Log(assetName);
        }

        internal void CopyAssetPath()
        {
            EditorGUIUtility.systemCopyBuffer = assetPath;
            Debug.Log(assetPath);
        }

        internal void CopyAssetPathFull()
        {
            var fullName = new FileInfo(assetPath).FullName;
            EditorGUIUtility.systemCopyBuffer = fullName;
            Debug.Log(fullName);
        }

        internal void AddToSelection()
        {
            var list = Selection.objects.ToList();
            var obj = FR2_Unity.LoadAssetAtPath<Object>(assetPath);
            if (!list.Contains(obj))
            {
                list.Add(obj);
                Selection.objects = list.ToArray();
            }
        }

        internal void Ping()
        {
            EditorGUIUtility.PingObject(
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object))
            );
        }

        internal void Open()
        {
            AssetDatabase.OpenAsset(
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object))
            );
        }

        internal void EditPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
            Object.Instantiate(prefab);
        }

        internal void OpenScript(string definition)
        {
            var asset = FR2_Cache.Api.FindSymbol(definition);
            if (asset == null) return;

            EditorGUIUtility.PingObject(
                AssetDatabase.LoadAssetAtPath(asset.assetPath, typeof(Object))
            );
        }

        internal void Reload()
        {
            LoadAssetInfo();
            LoadContent(true);
        }

        // ----------------------------- SERIALIZED UTILS ---------------------------------------

        private static SerializedProperty[] xGetSerializedProperties(Object go, bool processArray)
        {
            var so = new SerializedObject(go);
            so.Update();
            var result = new List<SerializedProperty>();

            var iterator = so.GetIterator();
            while (iterator.NextVisible(true))
            {
                var copy = iterator.Copy();

                if (processArray && iterator.isArray)
                {
                    result.AddRange(xGetSOArray(copy));
                }
                else
                {
                    result.Add(copy);
                }
            }

            return result.ToArray();
        }

        internal static List<SerializedProperty> xGetSOArray(SerializedProperty prop)
        {
            var size = prop.arraySize;
            var result = new List<SerializedProperty>();

            for (var i = 0; i < size; i++)
            {
                var p = prop.GetArrayElementAtIndex(i);

                if (p.isArray)
                {
                    result.AddRange(xGetSOArray(p.Copy()));
                }
                else
                {
                    //Debug.Log(p.name + ":" + p.propertyType + ":" + p.isArray);
                    result.Add(p.Copy());
                }
            }

            return result;
        }

        // ----------------------------- LOAD ASSETS ---------------------------------------

        internal void LoadGameObject(GameObject go)
        {
            var compList = go.GetComponentsInChildren<Component>();
            for (var i = 0; i < compList.Length; i++)
            {
                LoadSerialized(compList[i]);
            }
        }

        internal void LoadSerialized(Object target)
        {
            var props = xGetSerializedProperties(target, true);

            for (var i = 0; i < props.Length; i++)
            {
                if (props[i].propertyType != SerializedPropertyType.ObjectReference) continue;

                var refObj = props[i].objectReferenceValue;
                if (refObj == null) continue;

                var refGUID = AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(refObj)
                );

                //Debug.Log("Found Reference BinaryAsset <" + assetPath + "> : " + refGUID + ":" + refObj);
                AddUseGUID(refGUID);
            }
        }

        internal void LoadTerrainData(TerrainData terrain)
        {
            var arr = terrain.detailPrototypes;

            for (var i = 0; i < arr.Length; i++)
            {
                var aPath = AssetDatabase.GetAssetPath(arr[i].prototypeTexture);
                var refGUID = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGUID(refGUID);
            }

            var arr2 = terrain.treePrototypes;
            for (var i = 0; i < arr2.Length; i++)
            {
                var aPath = AssetDatabase.GetAssetPath(arr2[i].prefab);
                var refGUID = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGUID(refGUID);
            }

            var arr3 = terrain.splatPrototypes;
            for (var i = 0; i < arr3.Length; i++)
            {
                var aPath = AssetDatabase.GetAssetPath(arr3[i].texture);
                var refGUID = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGUID(refGUID);

                var bPath = AssetDatabase.GetAssetPath(arr3[i].normalMap);
                refGUID = AssetDatabase.AssetPathToGUID(bPath);
                AddUseGUID(refGUID);
            }
        }

        internal void LoadBinaryAsset()
        {
            UseGUIDs.Clear();

            //var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            //for (var i = 0;i < assets.Length; i++){
            //	Debug.Log(i + " : "+ assets[i].name + ":" + assets[i].GetType() + "\n" + 
            //		//EditorUtility.GetAssetPath(assets[i]) + "\n"
            //		assets[i].GetHashCode()
            //	);
            //}

            var assetData = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
            if (assetData is GameObject)
            {
                type = FR2_AssetType.MODEL;
                LoadGameObject(assetData as GameObject);
            }
            else if (assetData is TerrainData)
            {
                type = FR2_AssetType.TERRAIN;
                LoadTerrainData(assetData as TerrainData);
            }

            //Debug.Log("LoadBinaryAsset :: " + assetData + ":" + type);

            assetData = null;
            FR2_Unity.UnloadUnusedAssets();
        }

        internal void LoadYAML()
        {
            if (!File.Exists(assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            var text = string.Empty;
            try
            {
                text = File.ReadAllText(assetPath);
            }
#if FR2_DEBUG
            catch (Exception e)
            {
                Debug.LogWarning("Guess Asset Type error :: " + e + "\n" + assetPath);
#else
            catch
            {
#endif
                state = FR2_AssetState.MISSING;
                return;
            }

            // PERFORMANCE HOG!
            var matches = Regex.Matches(text, @"\bguid: [a-f0-9]{32}\b");

            foreach (Match match in matches)
            {
                var refGUID = match.Value.Replace("guid: ", string.Empty);
                if (UseGUIDs.Contains(refGUID)) continue;
                AddUseGUID(refGUID);
            }

            //var idx = text.IndexOf("guid: ");
            //var counter=0;
            //while (idx != -1)
            //{
            //	var guid = text.Substring(idx + 6, 32);
            //	if (UseGUIDs.Contains(guid)) continue;
            //	AddUseGUID(guid);

            //	idx += 39;
            //	if (idx > text.Length-40) break;

            //	//Debug.Log(assetName + ":" +  guid);
            //	idx = text.IndexOf("guid: ", idx + 39);
            //	if (counter++ > 100) break;
            //}

            //if (counter > 100){
            //	Debug.LogWarning("Never finish on " + assetName);
            //}
        }

        internal void LoadFolder()
        {
            if (!Directory.Exists(assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            try
            {
                var files = Directory.GetFiles(assetPath);
                var dirs = Directory.GetDirectories(assetPath);

                foreach (var f in files)
                {
                    if (f.EndsWith(".meta", StringComparison.Ordinal)) continue;

                    var fguid = AssetDatabase.AssetPathToGUID(f);
                    if (string.IsNullOrEmpty(fguid)) continue;
                    AddUseGUID(fguid, true);
                }

                foreach (var d in dirs)
                {
                    var fguid = AssetDatabase.AssetPathToGUID(d);
                    if (string.IsNullOrEmpty(fguid)) continue;
                    AddUseGUID(fguid, true);
                }
            }
#if FR2_DEBUG
            catch (Exception e)
            {
                Debug.LogWarning("LoadFolder() error :: " + e + "\n" + assetPath);
#else
            catch
            {
#endif
                state = FR2_AssetState.MISSING;
            }

            //Debug.Log("Load Folder :: " + assetName + ":" + type + ":" + UseGUIDs.Count);
        }

        internal void LoadScript()
        {
            ScriptSymbols.Clear();
            ScriptTokens.Clear();

            var text = string.Empty;

            if (!File.Exists(assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            try
            {
                text = File.ReadAllText(assetPath);
            }
#if FR2_DEBUG
            catch (Exception e)
            {
                Debug.LogWarning("LoadScript() error :: " + e + "\n" + assetPath);
#else
            catch
            {
#endif
                state = FR2_AssetState.MISSING;
                return;
            }

            var idx = -1;
            var l = text.Length;

            int matchIdx;
            int matchCount;
            //bool isSymbol = false;

            //Debug.Log("loading ... " + assetName);
            //string lastKeyword = null;
            //string lastWord = null;
            var currentScope = new List<string>();
            var braceLevel = 0;

            var stMap = new Dictionary<string, string>();

            while (++idx < l)
            {
                var c = text[idx];

                // Skip comments
                if (c == '/' && idx < l - 1)
                {
                    var c1 = text[idx + 1];

                    if (c1 == '/')
                    {
                        //line comment
                        idx++;

                        while (++idx < l)
                        {
                            c1 = text[idx];
                            if (c1 == '\r' || c1 == '\n') break;
                        }
                    }
                    else if (c1 == '*')
                    {
                        //block comment
                        idx++;

                        while (++idx < l)
                        {
                            c1 = text[idx];
                            if (c1 != '*' || idx == l - 1) continue;

                            c1 = text[idx + 1];
                            if (c1 == '/') break;
                        }
                    }
                }

                // Skip strings
                if (c == '"' && idx < l - 2)
                {
                    //var fromIdx = idx;

                    while (++idx < l)
                    {
                        var c1 = text[idx];
                        if (c1 == '"' && text[idx - 1] != '\\') break;
                    }

                    //Debug.Log("Skip string \n" + text.Substring(fromIdx, idx-fromIdx));
                    continue;
                }

                if (c == '{')
                {
                    //if (braceLevel == currentScope.Count && lastWord != null && (lastKeyword == "class" || lastKeyword == "namespace")) {
                    //	currentScope.Add(lastKeyword);
                    //	lastWord = null;

                    //}

                    braceLevel++;
#if FR2_DEBUG_BRACE_LEVEL
				Debug.Log("------->" + braceLevel + "\n" + text.Substring(idx, Mathf.Min(l-idx, 20))); //
#endif
                }
                else if (c == '}')
                {
#if FR2_DEBUG_BRACE_LEVEL
				Debug.Log("<--------" + braceLevel + ":" + currentScope[currentScope.Count - 1] + ":" + "\n" + text.Substring(idx, Mathf.Min(l-idx, 20))); //
#endif

                    if ((currentScope.Count > 0) && (braceLevel == currentScope.Count))
                    {
#if FR2_DEBUG_BRACE_LEVEL
					Debug.Log("out scope : " + currentScope[currentScope.Count - 1]);
#endif

                        currentScope.RemoveAt(currentScope.Count - 1);
                    }

                    braceLevel--;
                }

                if (!char.IsLetter(c) && c != '_') continue;

                matchIdx = idx;
                matchCount = 1;

                while (++idx < l && char.IsLetterOrDigit(c = text[idx]) || c == '_')
                {
                    matchCount++;
                }

                if (matchIdx > 0 && text[matchIdx - 1] == '.') continue; //skip function / method names
                if (text[matchIdx] == '_')
                {
                    //Debug.Log("Skipping name " + text.Substring(matchIdx, matchCount));
                    continue; // skip names starts with _
                }

                var word = text.Substring(matchIdx, matchCount);

                //skip using and var
                if ((word == "using") && char.IsWhiteSpace(text[matchIdx + matchCount]))
                {
                    while (idx++ < l - 2)
                    {
                        c = text[idx];
                        if (c == '\n' || c == '\r') break;
                    }

                    //Debug.Log("skip using ... " + text.Substring(matchIdx, idx-matchIdx));
                    //isSymbol = false;
                    continue;
                }

                if (word == "var" && char.IsWhiteSpace(text[matchIdx + matchCount]))
                {
                    while (idx++ < l - 2)
                    {
                        c = text[idx];
                        if (c == ';' || c == '=' || char.IsLetterOrDigit(c)) break;
                    }

                    //Debug.Log("skip var ... " + text.Substring(matchIdx, idx-matchIdx));
                    //isSymbol = false;
                }

                if (SCRIPT_KEYWORDS.Contains(word))
                {
                    var isSymbol = SCRIPT_SYMBOL.Contains(word);
                    var isScope = word == "namespace" || word == "class";
                    var hasBrace = false;

                    if (isSymbol || isScope)
                    {
                        var fromIdx = idx;
                        var hasChar = false;

                        while (idx++ < l - 2)
                        {
                            c = text[idx];
                            if (c == '{' || c == ':' || c == '<' || (hasChar && char.IsWhiteSpace(c))) break;
                            if (!hasChar) hasChar = char.IsLetter(c) || c == '_';
                        }

                        while (char.IsWhiteSpace(c))
                        {
                            //fast forward to first non-whitespace character
                            idx++;
                            c = text[idx];
                        }

                        hasBrace = c == '{';

                        var nextWord = text.Substring(fromIdx, idx - fromIdx).Trim();

                        if (word == "delegate")
                        {
                            fromIdx = idx;
                            while (idx++ < l - 2)
                            {
                                c = text[idx];
                                if (!char.IsLetterOrDigit(c) && c != '_') break;
                            }

                            nextWord = text.Substring(fromIdx, idx - fromIdx).Trim();

//#if FR2_DEBUG_SYMBOL
//						Debug.Log("Delegate detected ! " + nextWord);
//#endif
                        }

                        if (isSymbol)
                        {
                            string symb;
                            var ns = currentScope.Count > 0
                                ? string.Join(".", currentScope.ToArray()) + "."
                                : string.Empty;
                            if (stMap.TryGetValue(nextWord, out symb))
                            {
                                if (symb == "@token")
                                {
                                    stMap[nextWord] = ns + nextWord;
                                }
                                else
                                {
#if FR2_DEV
								Debug.LogWarning("Not yet support same symbol name definitions <" + ns + nextWord + "> : " + symb);
#endif
                                }
                            }
                            else
                            {
                                stMap.Add(nextWord, ns + nextWord);
                            }

//#if FR2_DEBUG_SYMBOL
//						Debug.LogWarning(word + " : " + (ns + nextWord));
//#endif
                        }

                        if (isScope)
                        {
                            currentScope.Add(nextWord);

//#if FR2_DEBUG_SYMBOL
//						Debug.LogWarning("Add scope : " + nextWord + ":" + braceLevel);
//#endif
                        }

                        if (c == ':' && isSymbol)
                        {
                            //extends
                            fromIdx = idx + 1;
                            while (idx++ < l - 2)
                            {
                                c = text[idx];
                                if (c == '{') break;
                            }

                            hasBrace = true;
                            nextWord = text.Substring(fromIdx, idx - fromIdx).Trim();

                            if (!stMap.ContainsKey(nextWord))
                            {
                                stMap.Add(nextWord, "@token");
                            }
                        }

                        if (hasBrace)
                        {
                            braceLevel++;
#if FR2_DEBUG_BRACE_LEVEL
						Debug.Log("------->" + braceLevel + "\n" + text.Substring(idx, Mathf.Min(l-idx, 20))); //
#endif
                        }
                    }

                    continue;
                }

                if (matchCount < 2)
                {
                    //Debug.Log("Skipping name " + text.Substring(matchIdx, matchCount));
                    continue; // skip short names
                }

                if (char.ToLower(text[matchIdx]) == text[matchIdx])
                {
                    //Debug.Log("Skipping name " + text.Substring(matchIdx, matchCount));
                    continue; //starts with lowercase character
                }

                //if (isSymbol){
                //	if (!ScriptSymbols.Contains(word)){
                //		//Debug.Log("Symbol --------- " + word);

                //		ScriptSymbols.Add(word);
                //		if (ScriptTokens.Contains(word)) ScriptTokens.Remove(word);	
                //	}
                //} else 

                if (!stMap.ContainsKey(word))
                {
                    var isFuncCall = idx < l - 2 && text[idx + 1] == '(';
                    if (isFuncCall) continue; //skip funtion calls

                    stMap.Add(word, "@token");
                }

                //isSymbol = false;
            }

            foreach (var item in stMap)
            {
                if (item.Value == "@token")
                {
                    ScriptTokens.Add(item.Key);

#if FR2_DEBUG_SYMBOL
				Debug.Log("Add Token : " + item.Key);
#endif
                }
                else
                {
                    ScriptSymbols.Add(item.Value);

#if FR2_DEBUG_SYMBOL
				Debug.Log("Add Symbol : " + item.Value);
#endif
                }
            }
        }

        // ----------------------------- REPLACE GUIDS ---------------------------------------

        internal bool ReplaceReference(string fromGUID, string toGUID)
        {
            if (IsMissing) return false;

            if (IsReferencable)
            {
                var text = string.Empty;

                if (!File.Exists(assetPath))
                {
                    state = FR2_AssetState.MISSING;
                    return false;
                }

                try
                {
                    text = File.ReadAllText(assetPath).Replace("\r", "\n");
                    File.WriteAllText(assetPath, text.Replace(fromGUID, toGUID));
                    return true;
                }
                catch (Exception e)
                {
                    state = FR2_AssetState.MISSING;
//#if FR2_DEBUG
                    Debug.LogWarning("Replace Reference error :: " + e + "\n" + assetPath);
//#endif
                }

                return false;
            }

            if (type == FR2_AssetType.TERRAIN)
            {
                var fromObj = FR2_Unity.LoadAssetWithGUID<Object>(fromGUID);
                var toObj = FR2_Unity.LoadAssetWithGUID<Object>(toGUID);
                var found = 0;
                var terrain = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)) as TerrainData;

                if (fromObj is Texture2D)
                {
                    var arr = terrain.detailPrototypes;
                    for (var i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].prototypeTexture == (Texture2D) fromObj)
                        {
                            found++;
                            arr[i].prototypeTexture = (Texture2D) toObj;
                        }
                    }

                    terrain.detailPrototypes = arr;

                    var arr3 = terrain.splatPrototypes;
                    for (var i = 0; i < arr3.Length; i++)
                    {
                        if (arr3[i].texture == (Texture2D) fromObj)
                        {
                            found++;
                            arr3[i].texture = (Texture2D) toObj;
                        }

                        if (arr3[i].normalMap == (Texture2D) fromObj)
                        {
                            found++;
                            arr3[i].normalMap = (Texture2D) toObj;
                        }
                    }

                    terrain.splatPrototypes = arr3;
                }

                if (fromObj is GameObject)
                {
                    var arr2 = terrain.treePrototypes;
                    for (var i = 0; i < arr2.Length; i++)
                    {
                        if (arr2[i].prefab == (GameObject) fromObj)
                        {
                            found++;
                            arr2[i].prefab = (GameObject) toObj;
                        }
                    }

                    terrain.treePrototypes = arr2;
                }

                EditorUtility.SetDirty(terrain);
                AssetDatabase.SaveAssets();

                fromObj = null;
                toObj = null;
                terrain = null;
                FR2_Unity.UnloadUnusedAssets();

                return found > 0;
            }

            Debug.LogWarning("Something wrong, should never be here - Ignored <" + assetPath +
                             "> : not a readable type, can not replace ! " + type);
            return false;
        }
    }
}