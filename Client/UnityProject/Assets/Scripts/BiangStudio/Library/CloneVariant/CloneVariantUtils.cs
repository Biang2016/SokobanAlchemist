using System.Collections.Generic;

namespace BiangStudio.CloneVariant
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

        private static T GetOperationResult<T>(T src, OperationType operationType = OperationType.Clone)
        {
            T res_t = src;
            switch (operationType)
            {
                case OperationType.Clone:
                {
                    if (src is IClone<T> t_Clone)
                    {
                        res_t = t_Clone.Clone();
                    }

                    break;
                }
                case OperationType.Variant:
                {
                    if (src is IVariant<T> t_Variant)
                    {
                        res_t = t_Variant.Variant();
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

        public static HashSet<T> Clone<T>(this HashSet<T> src)
        {
            return src.Operate(OperationType.Clone);
        }

        public static HashSet<T> Variant<T>(this HashSet<T> src)
        {
            return src.Operate(OperationType.Variant);
        }

        private static HashSet<T> Operate<T>(this HashSet<T> src, OperationType operationType = OperationType.Clone)
        {
            HashSet<T> res = new HashSet<T>();
            if (src == null) return res;
            foreach (T t in src)
            {
                res.Add(GetOperationResult(t, operationType));
            }

            return res;
        }

        public static List<T> Clone<T>(this List<T> src)
        {
            return src.Operate(OperationType.Clone);
        }

        public static List<T> Variant<T>(this List<T> src)
        {
            return src.Operate(OperationType.Variant);
        }

        private static List<T> Operate<T>(this List<T> src, OperationType operationType = OperationType.Clone)
        {
            List<T> res = new List<T>();
            if (src == null) return res;
            foreach (T t in src)
            {
                res.Add(GetOperationResult(t, operationType));
            }

            return res;
        }

        public static Dictionary<T1, T2> Clone<T1, T2>(this Dictionary<T1, T2> src)
        {
            return src.Operate(OperationType.Clone);
        }

        public static Dictionary<T1, T2> Variant<T1, T2>(this Dictionary<T1, T2> src)
        {
            return src.Operate(OperationType.Variant);
        }

        private static Dictionary<T1, T2> Operate<T1, T2>(this Dictionary<T1, T2> src, OperationType operationType = OperationType.Clone)
        {
            Dictionary<T1, T2> res = new Dictionary<T1, T2>();
            if (src == null) return res;
            foreach (KeyValuePair<T1, T2> kv in src)
            {
                res.Add(GetOperationResult(kv.Key, operationType), GetOperationResult(kv.Value, operationType));
            }

            return res;
        }

        public static SortedDictionary<T1, T2> Clone<T1, T2>(this SortedDictionary<T1, T2> src)
        {
            return src.Operate(OperationType.Clone);
        }

        public static SortedDictionary<T1, T2> Variant<T1, T2>(this SortedDictionary<T1, T2> src)
        {
            return src.Operate(OperationType.Variant);
        }

        private static SortedDictionary<T1, T2> Operate<T1, T2>(this SortedDictionary<T1, T2> src, OperationType operationType = OperationType.Clone)
        {
            SortedDictionary<T1, T2> res = new SortedDictionary<T1, T2>();
            if (src == null) return res;
            foreach (KeyValuePair<T1, T2> kv in src)
            {
                res.Add(GetOperationResult(kv.Key, operationType), GetOperationResult(kv.Value, operationType));
            }

            return res;
        }
    }
}