using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEditor;

public static class SerializationAnalysis
{

    private static Predicate<Type> isBasicType = new HashSet<Type> {
        typeof(bool),    typeof(byte),
        typeof(sbyte),   typeof(char),
        typeof(decimal), typeof(double),
        typeof(float),   typeof(int),
        typeof(uint),    typeof(long),
        typeof(ulong),   typeof(short),
        typeof(ushort),  typeof(string),
    }.Contains;

    private static Predicate<Type> isKnownStruct = new HashSet<Type> {
        typeof(Color),      typeof(Vector2),
        typeof(Vector3),    typeof(Vector4),
        typeof(Rect),       typeof(AnimationCurve),
        typeof(Bounds),     typeof(Gradient),
        typeof(Quaternion), typeof(Vector2Int),
        typeof(Vector3Int), typeof(RectInt),
        typeof(BoundsInt),
    }.Contains;

    private static bool isSingleValueType(Type type)
    {
        return typeof(UnityEngine.Object).IsAssignableFrom(type) ||
               type.IsEnum ||
               isBasicType(type);
    }

    private static bool isSerializableType(Type type) => Attribute.IsDefined(type, typeof(SerializableAttribute));
    private static bool hasSerializableAttribute(FieldInfo fi) => Attribute.IsDefined(fi, typeof(SerializeField));
    private static bool hasNonSerializedAttribute(FieldInfo fi) => Attribute.IsDefined(fi, typeof(NonSerializedAttribute));
    private static bool fieldDeclarationWillSerialize(FieldInfo fi) => fi.IsPublic && !hasNonSerializedAttribute(fi) || hasSerializableAttribute(fi);

    private static bool isSerializableStruct(Type type) => isKnownStruct(type) || isSerializableType(type);
    private static bool isCompatibleType(Type type) => isSingleValueType(type) || isSerializableStruct(type);


    private static bool fieldWillSerialize(FieldInfo fi) => fieldDeclarationWillSerialize(fi) && isCompatibleType(fi.FieldType);
    private static FieldInfo[] getSerializableFields(Type t) => Array.FindAll(GetFieldInfosIncludingBaseClasses(t), fieldWillSerialize);

    private const BindingFlags instanceFieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private static Type[] reservedTypes = new[] { typeof(MonoBehaviour), typeof(Component),
                                                  typeof(Behaviour),     typeof(ValueType),
                                                  typeof(System.Object) };

    public static FieldInfo[] GetFieldInfosIncludingBaseClasses(Type type)
    {
        FieldInfo[] fieldInfos = type.GetFields(instanceFieldFlags);

        // If this class doesn't have a base, don't waste any time
        if (reservedTypes.Contains(type.BaseType) || type == typeof(System.Object))
            return fieldInfos;

        var fieldInfoList = new List<FieldInfo>(fieldInfos);
        do
        {
            fieldInfoList.AddRange(type.GetFields(instanceFieldFlags));
            type = type.BaseType;
        } while (!reservedTypes.Contains(type));

        return fieldInfoList.ToArray();
    }

    private static bool tryGetArrayType(Type t, out Type elementType)
    {
        if (t.IsArray) { elementType = t.GetElementType(); return true; }
        else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) { elementType = t.GetGenericArguments()[0]; return true; }

        elementType = default(Type); return false;
    }

    private static IEnumerable<string> getGeneralizedPropertyPaths(FieldInfo fi)
    {
        var fieldType = fi.FieldType;
        if (tryGetArrayType(fieldType, out var elementType))
        {
            yield return $"{fi.Name}.Array.size";
            if (isSingleValueType(elementType))
                yield return $"{fi.Name}.Array.data[*]";
            else
                foreach (var elementField in getSerializableFields(elementType))
                    foreach (var aboutString in getGeneralizedPropertyPaths(elementField))
                        yield return $"{fi.Name}.Array.data[*].{aboutString}";
        }

        else if (isSingleValueType(fieldType))
            yield return fi.Name;

        else if (isSerializableStruct(fieldType))
        {
            foreach (var elementField in getSerializableFields(fieldType))
                foreach (var aboutString in getGeneralizedPropertyPaths(elementField))
                    yield return $"{fi.Name}.{aboutString}";
        }

        else
        {
            Debug.LogWarning($"Unexpected field: {fi.Name}");
            yield return $"UNEXPECTED: {fi.Name}";
        }
    }

    static HashSet<string> getExpectedPropertyPaths(Type type)
    {
        var result = new HashSet<string>(getSerializableFields(type).SelectMany(getGeneralizedPropertyPaths));
        if (typeof(Behaviour).IsAssignableFrom(type))
            result.Add("m_Enabled");
        return result;
    }

    static IEnumerable<MonoBehaviour> targetsOfInstance(GameObject root)
    {
        // root is always included, but other prefab instance roots aren't
        var parents = new Stack<Transform>(); parents.Push(root.transform);
        var nextChild = new Stack<int>(); nextChild.Push(0);
        while (parents.Count != 0)
        {
            var current = parents.Peek();
            var nextIndex = nextChild.Pop();

            foreach (var b in current.GetComponents<MonoBehaviour>())
                if (!PrefabUtility.IsAddedComponentOverride(b))
                    yield return b;

            if (nextIndex == current.childCount)
            {
                parents.Pop();
                continue;
            }
            var child = current.GetChild(nextIndex);

            nextChild.Push(nextIndex + 1);

            if (!PrefabUtility.IsAnyPrefabInstanceRoot(child.gameObject) &&
                !PrefabUtility.IsAddedGameObjectOverride(child.gameObject))
            {
                parents.Push(child);
                nextChild.Push(0);
            }
        }
    }

    public static (Dictionary<MonoScript, HashSet<string>> mods, IEnumerable<GameObject> changedInstances) GetEliminatedPropertyPaths(IEnumerable<GameObject> instanceRoots, bool debug = false)
    {
        var prefabRoots = new HashSet<GameObject>(instanceRoots.Select(PrefabUtility.GetCorrespondingObjectFromSource));
        var types = instanceRoots.SelectMany(targetsOfInstance).Select(mb => mb.GetType()).Distinct();

        var mods = new Dictionary<MonoScript, HashSet<string>>();
        var acceptedPropertyPaths = types.ToDictionary(t => t, getExpectedPropertyPaths);

        var changedInstances = new List<GameObject>();

        foreach (var ir in instanceRoots)
        {
            foreach (var mb in targetsOfInstance(ir))
            {

                var originalPrefabModifications = PrefabUtility.GetPropertyModifications(ir);
                var targetObjectInAsset = PrefabUtility.GetCorrespondingObjectFromSource(mb);

                var generalizedProperties = new HashSet<string>(originalPrefabModifications.Where(mod => mod.target == targetObjectInAsset)
                                                                                            .Select(mod => Regex.Replace(mod.propertyPath, @"(?<=\.Array.data\[)[0-9]+(?=\])", "*")));
                generalizedProperties.ExceptWith(acceptedPropertyPaths[mb.GetType()]);

                if (generalizedProperties.Count != 0)
                {
                    changedInstances.Add(ir);

                    HashSet<string> changeSet;
                    var monoScript = MonoScript.FromMonoBehaviour(mb);
                    if (!mods.TryGetValue(monoScript, out changeSet))
                        mods[monoScript] = changeSet = new HashSet<string>();

                    changeSet.UnionWith(generalizedProperties);
                }
            }
        }

        return (mods, changedInstances);
    }

    public static void EliminatePropertyModifications(GameObject instanceRoot, Dictionary<Type, HashSet<string>> removedMods)
    {
        if (!PrefabUtility.IsAnyPrefabInstanceRoot(instanceRoot))
        {
            Debug.LogWarning($"game object {instanceRoot} is not a prefab instance root", instanceRoot);
            return;
        }
        if (!PrefabUtility.HasPrefabInstanceAnyOverrides(instanceRoot, false))
        {
            Debug.LogWarning($"no overrides in {instanceRoot}", instanceRoot);
            return;
        }

        var prefabHandle = PrefabUtility.GetPrefabInstanceHandle(instanceRoot);
        var mods_with_index = PrefabUtility.GetPropertyModifications(instanceRoot).Select((mod, i) => (mod: mod, index: i)).ToList();
        var numOriginalMods = mods_with_index.Count;

        var elementsRemoved = 0;

        foreach (var mb in targetsOfInstance(instanceRoot))
            if (removedMods.TryGetValue(mb.GetType(), out var pathsForType))
                elementsRemoved += mods_with_index.RemoveAll(mwi => mwi.mod.target == PrefabUtility.GetCorrespondingObjectFromSource(mb) && pathsForType.Contains(mwi.mod.propertyPath));

        if (elementsRemoved != 0)
        {
            var modsToRemove = new List<int>();

            for (int i = 0, mod_i = 0; i < numOriginalMods; i++)
                if (mod_i >= mods_with_index.Count || i != mods_with_index[mod_i].index)
                    modsToRemove.Add(i);
                else
                    mod_i++;

            var so = new SerializedObject(prefabHandle);
            var prop = so.FindProperty("m_Modification.m_Modifications");

            for (int i = modsToRemove.Count - 1; i >= 0; i--)
                prop.DeleteArrayElementAtIndex(modsToRemove[i]);

            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(instanceRoot);
        }
    }
}