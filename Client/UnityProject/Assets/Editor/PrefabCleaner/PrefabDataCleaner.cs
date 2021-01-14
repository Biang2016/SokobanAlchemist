using Type = System.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PrefabDataCleaner : EditorWindow
{
    [MenuItem("Window/Prefab data cleaner")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<PrefabDataCleaner>("Prefab data cleaner");
    }

    void OnEnable()
    {
        if (scenes == null) scenes = new SceneAssetList();
        if (parentPrefabs == null) parentPrefabs = new GameObjectList();
        if (monoScripts == null) monoScripts = new MonoScriptList();
        if (sourcePrefabs == null) sourcePrefabs = new GameObjectList();
        if (propertyPaths == null) propertyPaths = new ModList();

        scenes.AlsoClears(propertyPaths, sourcePrefabs, monoScripts, parentPrefabs);
        parentPrefabs.AlsoClears(propertyPaths, sourcePrefabs, monoScripts);
        sourcePrefabs.AlsoClears(propertyPaths, monoScripts);
        monoScripts.AlsoClears(propertyPaths);
        propertyPaths.AlsoClears();

        MakeMonoScriptIndexes();

        EditorSceneManager.sceneOpened += NewSceneList;
        EditorSceneManager.sceneClosed += NewSceneList;
    }

    void OnDisable()
    {
        EditorSceneManager.sceneOpened -= NewSceneList;
        EditorSceneManager.sceneClosed -= NewSceneList;
    }

    void NewSceneList(Scene scene)
    {
        scenes.Clear();
    }

    void NewSceneList(Scene scene, OpenSceneMode mode)
    {
        NewSceneList(scene);
    }

    enum CheckStatus
    {
        None,
        All,
        Some,
    };

    interface iobjectList
    {
        void Clear();
    }

    class objectList<T> : iobjectList, ISerializationCallbackReceiver
    {
        public Vector2 scrollPosition;
        public CheckStatus[] checkmarks;
        public bool hasContent;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (!hasContent)
            {
                checkmarks = null;
                elements = null;
            }
        }

        private T[] elements;

        public T[] Elements
        {
            get { return elements; }
            set
            {
                this.elements = value;
                this.checkmarks = new CheckStatus[value.Length];
                this.hasContent = true;
            }
        }

        public void SetElements(IEnumerable<T> elements)
        {
            Elements = elements.ToArray();
        }

        public IEnumerable<T> SelectedElements =>
            elements == null
                ? Enumerable.Empty<T>()
                : Enumerable.Range(0, elements.Length)
                    .Where(i => checkmarks[i] != CheckStatus.None)
                    .Select(i => elements[i]);

        private iobjectList[] children;

        public void AlsoClears(params iobjectList[] children)
        {
            this.children = children;
        }

        public void Clear()
        {
            elements = null;
            checkmarks = null;
            hasContent = false;

            Array.ForEach(children, c => c.Clear());
        }
    }

    [Serializable]
    class mod
    {
        public MonoScript monoScript;
        public string propertyPath;
    }

    [Serializable]
    class SceneAssetList : objectList<SceneAsset>
    {
    };

    [Serializable]
    class GameObjectList : objectList<GameObject>
    {
    };

    [Serializable]
    class MonoScriptList : objectList<MonoScript>
    {
    };

    [Serializable]
    class ModList : objectList<mod>
    {
    };

    [SerializeField]
    SceneAssetList scenes;

    [SerializeField]
    GameObjectList parentPrefabs;

    [SerializeField]
    MonoScriptList monoScripts;

    [SerializeField]
    GameObjectList sourcePrefabs;

    [SerializeField]
    ModList propertyPaths;

    private Dictionary<MonoScript, List<int>> propertyPathsPerMonoScript;
    private Dictionary<mod, int> monoScriptIndexPerPropertyPath;

    void MakeMonoScriptIndexes()
    {
        var monoScriptIndexes = new Dictionary<MonoScript, int>();
        if (monoScripts.Elements != null)
            for (int i = 0; i < monoScripts.Elements.Length; i++)
                monoScriptIndexes[monoScripts.Elements[i]] = i;

        propertyPathsPerMonoScript = new Dictionary<MonoScript, List<int>>();
        monoScriptIndexPerPropertyPath = new Dictionary<mod, int>();
        if (propertyPaths.Elements != null)
            for (int i = 0; i < propertyPaths.Elements.Length; i++)
            {
                List<int> ppIndexes;
                var mod = propertyPaths.Elements[i];
                var monoScript = mod.monoScript;
                if (!propertyPathsPerMonoScript.TryGetValue(monoScript, out ppIndexes))
                    propertyPathsPerMonoScript[monoScript] = ppIndexes = new List<int>();
                ppIndexes.Add(i);

                monoScriptIndexPerPropertyPath[mod] = monoScriptIndexes[monoScript];
            }
    }

    private Dictionary<Type, HashSet<string>> getDeniedPropertyPaths()
    {
        var result = new Dictionary<Type, HashSet<string>>();
        foreach (var mod in propertyPaths.SelectedElements)
        {
            HashSet<string> pPaths;
            var type = mod.monoScript.GetClass();
            if (!result.TryGetValue(type, out pPaths))
                result[type] = pPaths = new HashSet<string>();
            pPaths.Add(mod.propertyPath);
        }

        return result;
    }

    private IEnumerable<SceneAsset> Scenes
    {
        set { scenes.SetElements(value); }
    }

    private IEnumerable<GameObject> ParentPrefabs
    {
        set { parentPrefabs.SetElements(value); }
    }

    private IEnumerable<MonoScript> MonoScripts
    {
        set { monoScripts.SetElements(value); }
    }

    private IEnumerable<GameObject> SourcePrefabs
    {
        set { sourcePrefabs.SetElements(value); }
    }

    private IEnumerable<mod> PropertyPaths
    {
        set { propertyPaths.SetElements(value); }
    }

    Vector2 prefabScrollPosition, monoScriptScrollPosition;

    private bool analysisRequested;

    void OnGUI()
    {
        if (scenes.Elements == null && GUILayout.Button("Load Scenes"))
        {
            var loadedScenes = Enumerable.Range(0, EditorSceneManager.sceneCount).Select(i => EditorSceneManager.GetSceneAt(i)).Where(s => s.isLoaded);
            Scenes = loadedScenes.Select(s => AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path));
        }

        var numActiveScenes = scenes.checkmarks == null ? 0 : scenes.checkmarks.Count(m => m != CheckStatus.None);

        if (scenes.Elements != null &&
            parentPrefabs.Elements == null)
        {
            GUI.enabled = numActiveScenes != 0;
            if (GUILayout.Button("Load Related Nested Prefabs"))
            {
                var sceneDepPaths = new HashSet<string>();
                for (int i = 0, c = 0; i < scenes.Elements.Length; i++)
                    if (scenes.checkmarks[i] != CheckStatus.None)
                    {
                        EditorUtility.DisplayProgressBar("Collecting dependencies", scenes.Elements[i].name, c / (float) numActiveScenes);
                        sceneDepPaths.UnionWith(AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(scenes.Elements[i])));
                        c++;
                    }

                EditorUtility.ClearProgressBar();
                var allPrefabsFromDependencies = new HashSet<GameObject>(
                    sceneDepPaths.Where(p => AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(GameObject))
                        .Select(p => (GameObject) AssetDatabase.LoadMainAssetAtPath(p))
                        .Where(o =>
                        {
                            var type = PrefabUtility.GetPrefabAssetType(o);
                            var isRegularPrefab = type == PrefabAssetType.Regular ||
                                                  type == PrefabAssetType.Variant;
                            return isRegularPrefab && hasNestedPrefabs(o);
                        }));

                ParentPrefabs = allPrefabsFromDependencies;
            }

            GUI.enabled = true;
        }

        if ((scenes.Elements != null ||
             parentPrefabs.Elements != null) && analysisRequested)
        {
            analysisRequested = false;

            IEnumerable<GameObject> instanceRoots = null;
            try
            {
                instanceRoots = getConnectedInstanceRoots();
            }
            catch (NoSceneOrPrefabSelectedException)
            {
                //Debug.LogException(e);
                sourcePrefabs.Clear();
            }

            if (instanceRoots != null)
            {
                var (scriptToPropertyPathChange, changedInstances) = SerializationAnalysis.GetEliminatedPropertyPaths(instanceRoots);

                SourcePrefabs = changedInstances.Select(PrefabUtility.GetCorrespondingObjectFromOriginalSource).Distinct().ToList();
                MonoScripts = scriptToPropertyPathChange.Keys;
                PropertyPaths = scriptToPropertyPathChange.SelectMany(kvp => kvp.Value.Select(p => new mod {propertyPath = p, monoScript = kvp.Key}));

                MakeMonoScriptIndexes();
            }
        }

        using (var h = new EditorGUILayout.HorizontalScope())
        {
            using (var v = new EditorGUILayout.VerticalScope())
            {
                listArray(scenes, checkChanged: (scene, checkStatus) => analysisRequested = true, showAll: true);
                listArray(parentPrefabs, checkChanged: (scene, checkStatus) => analysisRequested = true, showClear: true);
            }

            listArray(sourcePrefabs, showCheckmarks: false);
            listArray(monoScripts, checkChanged: (monoScript, checkStatus) =>
            {
                var pp = propertyPathsPerMonoScript[monoScript];
                for (int i = 0; i < pp.Count; i++)
                    propertyPaths.checkmarks[pp[i]] = checkStatus;
            });
            listModArray(propertyPaths, checkChanged: (mod, checkStatus) =>
            {
                var i = monoScriptIndexPerPropertyPath[mod];
                var pp = propertyPathsPerMonoScript[monoScripts.Elements[i]];
                var numPathsForScript = pp.Count;
                var numPathsActive = pp.Count(p_i => propertyPaths.checkmarks[p_i] != CheckStatus.None);
                monoScripts.checkmarks[i] = numPathsActive == 0 ? CheckStatus.None : numPathsActive < numPathsForScript ? CheckStatus.Some : CheckStatus.All;
            });
        }

        if (propertyPaths.Elements != null && GUILayout.Button("Clean prefab instances"))
        {
            IEnumerable<GameObject> instanceRoots = null;
            try
            {
                instanceRoots = getConnectedInstanceRoots();
            }
            catch (NoSceneOrPrefabSelectedException)
            {
                // No scene or prefab selected
            }

            if (instanceRoots != null)
            {
                var removedMods = getDeniedPropertyPaths();

                foreach (var instanceRoot in instanceRoots)
                    SerializationAnalysis.EliminatePropertyModifications(instanceRoot, removedMods);
            }
        }

        return;

        IEnumerable<GameObject> getConnectedInstanceRoots()
        {
            var numActiveScenesAndPrefabs = (scenes.Elements == null ? 0 : scenes.checkmarks.Count(m => m != CheckStatus.None)) +
                                            (parentPrefabs.Elements == null ? 0 : scenes.checkmarks.Count(m => m != CheckStatus.None));
            if (numActiveScenesAndPrefabs == 0)
                throw new NoSceneOrPrefabSelectedException();

            var validScenes = scenes.SelectedElements.Select(s => EditorSceneManager.GetSceneByPath(AssetDatabase.GetAssetPath(s))).Where(s => s.IsValid() && s.isLoaded);
            var allSceneGameObjects = validScenes.SelectMany(s => s.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Transform>().Select(t => t.gameObject)));
            var allSceneInstanceRoots = allSceneGameObjects.Where(go => isInstanceWithOverrides(go));

            var allPrefabGameObjects = parentPrefabs.SelectedElements.SelectMany(p => p.GetComponentsInChildren<Transform>().Select(t => t.gameObject));
            var allPrefabInstanceRoots = allPrefabGameObjects.Where(go => isInstanceWithOverrides(go));

            return allSceneInstanceRoots.Concat(allPrefabInstanceRoots);
        }

        bool hasNestedPrefabs(GameObject gameObject)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
                return true;

            var parents = new Stack<Transform>();
            parents.Push(gameObject.transform);
            var nextChild = new Stack<int>();
            nextChild.Push(0);

            while (parents.Count != 0)
            {
                var current = parents.Peek();
                var nextIndex = nextChild.Pop();
                if (nextIndex == current.childCount)
                {
                    parents.Pop();
                    continue;
                }

                var child = current.GetChild(nextIndex);
                if (PrefabUtility.IsPartOfPrefabInstance(child))
                    return true;

                nextChild.Push(nextIndex + 1);

                parents.Push(child);
                nextChild.Push(0);
            }

            return false;
        }

        void listArray<T>(objectList<T> l, Action<T, CheckStatus> checkChanged = null, bool showCheckmarks = true, bool showAll = false, bool showClear = false) where T : UnityEngine.Object
        {
            listArrayImpl(l, checkChanged, e => EditorGUILayout.ObjectField(e, typeof(T), true), showCheckmarks, showAll, showClear);
        }

        void listModArray(objectList<mod> l, Action<mod, CheckStatus> checkChanged = null, bool showCheckmarks = true, bool showAll = false, bool showClear = false)
        {
            listArrayImpl(l, checkChanged, e => EditorGUILayout.LabelField(e.propertyPath), showCheckmarks, showAll, showClear);
        }

        void listArrayImpl<T>(objectList<T> l, Action<T, CheckStatus> checkChanged, Action<T> showField, bool showCheckmarks, bool showAll, bool showClear)
        {
            if (l.Elements == null) return;
            var layoutOptions = (showAll ? new[] {GUILayout.Height(18f * l.Elements.Length + 24f)} : new GUILayoutOption[0]);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(l.scrollPosition, layoutOptions))
            {
                l.scrollPosition = scrollView.scrollPosition;

                if (l.Elements != null)
                {
                    if (showClear && GUILayout.Button("Clear"))
                        l.Clear();
                    else
                    {
                        for (int i = 0; i < l.Elements.Length; i++)
                        {
                            using (var h = new EditorGUILayout.HorizontalScope())
                            {
                                if (showCheckmarks)
                                    using (var check = new EditorGUI.ChangeCheckScope())
                                    {
                                        EditorGUI.showMixedValue = l.checkmarks[i] == CheckStatus.Some;
                                        var newToggleValue = EditorGUILayout.Toggle(l.checkmarks[i] != CheckStatus.None, GUILayout.Width(18f));
                                        if (check.changed)
                                        {
                                            var newCheckStatus = newToggleValue ? CheckStatus.All : CheckStatus.None;
                                            l.checkmarks[i] = newCheckStatus;
                                            checkChanged?.Invoke(l.Elements[i], newCheckStatus);
                                        }

                                        EditorGUI.showMixedValue = false;
                                    }

                                GUI.enabled = false;
                                showField(l.Elements[i]);
                                GUI.enabled = true;
                            }
                        }
                    }
                }
            }
        }

        bool isInstanceWithOverrides(GameObject o, bool onlyPrefabs = true)
        {
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(o))
                return false;

            var assetType = PrefabUtility.GetPrefabAssetType(o);
            if (!(assetType == PrefabAssetType.Regular ||
                  assetType == PrefabAssetType.Variant ||
                  (assetType == PrefabAssetType.Model && !onlyPrefabs)))
                return false;

            return PrefabUtility.HasPrefabInstanceAnyOverrides(o, false);
        }
    }
}

public class NoSceneOrPrefabSelectedException : Exception
{
    public NoSceneOrPrefabSelectedException() : base("No scenes or prefabs selected")
    {
    }

    public NoSceneOrPrefabSelectedException(string message) : base(message)
    {
    }

    public NoSceneOrPrefabSelectedException(string message, Exception inner) : base(message, inner)
    {
    }
}