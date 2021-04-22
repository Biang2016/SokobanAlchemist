using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BiangLibrary.CloneVariant
{
    public static class CloneVariantUtils
    {
        public enum OperationType
        {
            Clone,
            Variant,
            None,
        }

        public static T TryGetClone<T>(T src)
        {
            if (src is IClone<T> t_Clone)
            {
                return t_Clone.Clone();
            }

            return src;
        }

        private static T GetOperationResult<T, BT>(T src, OperationType operationType = OperationType.Clone) where T : BT
        {
            T res_t = src;
            switch (operationType)
            {
                case OperationType.Clone:
                {
                    if (src is IClone<BT> t_Clone)
                    {
                        res_t = (T) t_Clone.Clone();
                    }

                    break;
                }
                case OperationType.Variant:
                {
                    if (src is IVariant<BT> t_Variant)
                    {
                        res_t = (T) t_Variant.Variant();
                    }

                    break;
                }
                case OperationType.None:
                {
                    break;
                }
            }

            return res_t;
        }

        public static HashSet<T> Clone<T, BT>(this HashSet<T> src) where T : BT
        {
            return src.Operate<T, BT>(OperationType.Clone);
        }

        public static HashSet<T> Variant<T, BT>(this HashSet<T> src) where T : BT
        {
            return src.Operate<T, BT>(OperationType.Variant);
        }

        private static HashSet<T> Operate<T, BT>(this HashSet<T> src, OperationType operationType = OperationType.Clone) where T : BT
        {
            HashSet<T> res = new HashSet<T>();
            if (src == null) return res;
            foreach (T t in src)
            {
                res.Add(GetOperationResult<T, BT>(t, operationType));
            }

            return res;
        }

        public static List<T> Clone<T, BT>(this List<T> src) where T : BT
        {
            return src.Operate<T, BT>(OperationType.Clone);
        }

        public static List<T> Variant<T, BT>(this List<T> src) where T : BT
        {
            return src.Operate<T, BT>(OperationType.Variant);
        }

        private static List<T> Operate<T, BT>(this List<T> src, OperationType operationType = OperationType.Clone) where T : BT
        {
            if (src == null) return new List<T>();
            List<T> res = new List<T>(src.Count);
            foreach (T t in src)
            {
                res.Add(GetOperationResult<T, BT>(t, operationType));
            }

            return res;
        }

        public static Dictionary<T1, T2> Clone<T1, T2, BT1, BT2>(this Dictionary<T1, T2> src) where T1 : BT1 where T2 : BT2
        {
            return src.Operate<T1, T2, BT1, BT2>(OperationType.Clone);
        }

        public static Dictionary<T1, T2> Variant<T1, T2, BT1, BT2>(this Dictionary<T1, T2> src) where T1 : BT1 where T2 : BT2
        {
            return src.Operate<T1, T2, BT1, BT2>(OperationType.Variant);
        }

        private static Dictionary<T1, T2> Operate<T1, T2, BT1, BT2>(this Dictionary<T1, T2> src, OperationType operationType = OperationType.Clone) where T1 : BT1 where T2 : BT2
        {
            if (src == null) return new Dictionary<T1, T2>();
            Dictionary<T1, T2> res = new Dictionary<T1, T2>(src.Count);
            foreach (KeyValuePair<T1, T2> kv in src)
            {
                res.Add(GetOperationResult<T1, BT1>(kv.Key, operationType), GetOperationResult<T2, BT2>(kv.Value, operationType));
            }

            return res;
        }

        public static SortedDictionary<T1, T2> Clone<T1, T2, BT1, BT2>(this SortedDictionary<T1, T2> src) where T1 : BT1 where T2 : BT2
        {
            return src.Operate<T1, T2, BT1, BT2>(OperationType.Clone);
        }

        public static SortedDictionary<T1, T2> Variant<T1, T2, BT1, BT2>(this SortedDictionary<T1, T2> src) where T1 : BT1 where T2 : BT2
        {
            return src.Operate<T1, T2, BT1, BT2>(OperationType.Variant);
        }

        private static SortedDictionary<T1, T2> Operate<T1, T2, BT1, BT2>(this SortedDictionary<T1, T2> src, OperationType operationType = OperationType.Clone) where T1 : BT1 where T2 : BT2
        {
            SortedDictionary<T1, T2> res = new SortedDictionary<T1, T2>();
            if (src == null) return res;
            foreach (KeyValuePair<T1, T2> kv in src)
            {
                res.Add(GetOperationResult<T1, BT1>(kv.Key, operationType), GetOperationResult<T2, BT2>(kv.Value, operationType));
            }

            return res;
        }
    }
}