using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BiangStudio.GameDataFormat;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace BiangStudio
{
    public static class CommonUtils
    {
        public static Color ChangeColorToWhite(this Color color, float whiteRatio)
        {
            float r = color.r;
            float g = color.g;
            float b = color.b;

            float max = Mathf.Max(r, g, b);

            if (max - r < 0.2f && max - g < 0.2f && max - b < 0.2f) //本来就是灰色
            {
                max = max + 0.3f;
                Color res = Color.Lerp(color, new Color(max, max, max, color.a), 1f);
                return res;
            }
            else
            {
                max = max + 0.3f;
                Color res = Color.Lerp(color, new Color(max, max, max, color.a), whiteRatio);
                return res;
            }
        }

        public static Color ChangeColorToBlack(this Color color, float blackRatio)
        {
            float r = color.r;
            float g = color.g;
            float b = color.b;

            float min = Mathf.Min(r, g, b);

            if (r - min < 0.2f && g - min < 0.2f && b - min < 0.2f) //本来就是灰色
            {
                min = min - 0.3f;
                Color res = Color.Lerp(color, new Color(min, min, min, color.a), 1f);
                return res;
            }
            else
            {
                min = min - 0.3f;
                Color res = Color.Lerp(color, new Color(min, min, min, color.a), blackRatio);
                return res;
            }
        }

        public static Color HSL_2_RGB(float H, float S, float L)
        {
            //H, S and L input range = 0 ÷ 1.0
            //R, G and B output range = 0 ÷ 255
            float R;
            float G;
            float B;
            if (S.Equals(0))
            {
                R = L;
                G = L;
                B = L;
            }
            else
            {
                float var_1 = 0;
                float var_2 = 0;
                if (L < 0.5)
                {
                    var_2 = L * (1 + S);
                }
                else
                {
                    var_2 = (L + S) - (S * L);
                }

                var_1 = 2 * L - var_2;

                R = Hue_2_RGB(var_1, var_2, H + (1.0f / 3));
                G = Hue_2_RGB(var_1, var_2, H);
                B = Hue_2_RGB(var_1, var_2, H - (1.0f / 3));
            }

            return new Color(R, G, B);
        }

        static float Hue_2_RGB(float v1, float v2, float vH) //Function Hue_2_RGB
        {
            if (vH < 0) vH += 1;
            if (vH > 1) vH -= 1;
            if ((6 * vH) < 1) return (v1 + (v2 - v1) * 6 * vH);
            if ((2 * vH) < 1) return (v2);
            if ((3 * vH) < 2) return (v1 + (v2 - v1) * ((2.0f / 3.0f) - vH) * 6);
            return v1;
        }

        public static Vector3 GenerateRandomPosInsideCollider(BoxCollider bc)
        {
            float x = Random.Range(bc.center.x - bc.size.x * 0.5f, bc.center.x + bc.size.x * 0.5f);
            float y = Random.Range(bc.center.y - bc.size.y * 0.5f, bc.center.y + bc.size.y * 0.5f);
            float z = Random.Range(bc.center.z - bc.size.z * 0.5f, bc.center.z + bc.size.z * 0.5f);
            Vector3 localPos = new Vector3(x, y, z);
            Vector3 worldPos = bc.transform.TransformPoint(localPos);
            return worldPos;
        }

        public static SortedDictionary<int, List<T>> GetRank<T>(this IEnumerable<T> value, IComparer<T> comparer)
        {
            List<T> list = value.ToList<T>();
            list.Sort(comparer); // 排序  Sort方法排序后的结果是升序
            list.Reverse(); //反转List中的数据  就变成降序了

            SortedDictionary<int, List<T>> rank = new SortedDictionary<int, List<T>>();

            int order = 0;
            int pcount = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (!rank.ContainsKey(order))
                {
                    rank.Add(order, new List<T> { });
                }

                if (i < list.Count && i > 0)
                {
                    int result = comparer.Compare(list[i], list[i - 1]);
                    if (result != 0)
                    {
                        if (pcount == 0)
                        {
                            order++;
                        }
                        else
                        {
                            order = order + 1 + pcount;
                            pcount = 0;
                        }
                    }
                    else
                    {
                        pcount++;
                    }
                }

                if (!rank.ContainsKey(order))
                {
                    rank.Add(order, new List<T> { });
                }

                rank[order].Add(list[i]);
            }

            for (int i = 0; i < 4; i++)
            {
                if (!rank.ContainsKey(i))
                {
                    rank.Add(i, new List<T> { });
                }
            }

            return rank;
        }

        public static float DistanceBetweenPointToSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
        {
            Vector3 S_P = point - segmentStart;
            Vector3 S_E = segmentEnd - segmentStart;
            Vector3 E_P = point - segmentEnd;
            Vector3 E_S = -S_E;

            if (Vector3.Dot(S_P, S_E) <= 0)
            {
                return S_P.magnitude;
            }

            if (Vector3.Dot(E_P, E_S) <= 0)
            {
                return E_P.magnitude;
            }

            return (Vector3.Dot(S_P, S_E) / S_E.magnitude * S_E.normalized - S_P).magnitude;
        }

        public static string TimeToString(float timeTick)
        {
            return Mathf.FloorToInt(timeTick / 60f) + ":" + Mathf.FloorToInt(timeTick % 60).ToString().PadLeft(2, '0');
        }

        public static string TimeToString_Milisecond(float timeTick)
        {
            return "." + Mathf.CeilToInt(timeTick % 1 * 10f);
        }

        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static float GetClipLength(Animator animator, string clip)
        {
            if (null == animator || String.IsNullOrEmpty(clip) || null == animator.runtimeAnimatorController)
                return 0;
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            AnimationClip[] tAnimationClips = ac.animationClips;
            if (null == tAnimationClips || tAnimationClips.Length <= 0) return 0;
            AnimationClip tAnimationClip;
            for (int tCounter = 0, tLen = tAnimationClips.Length; tCounter < tLen; tCounter++)
            {
                tAnimationClip = ac.animationClips[tCounter];
                if (null != tAnimationClip && tAnimationClip.name == clip)
                    return tAnimationClip.length;
            }

            return 0F;
        }

        public static string GetIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            return localIP;
        }

        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static float Remap(this int value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static IEnumerator UpdateLayout(RectTransform rect, UnityAction callBack = null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            yield return null;
            callBack?.Invoke();
        }

        public static Color HTMLColorToColor(string htmlColor)
        {
            Color cl = new Color();
            ColorUtility.TryParseHtmlString(htmlColor, out cl);
            return cl;
        }

        public static List<Type> GetClassesByNameSpace(string nameSpace, Assembly assembly)
        {
            List<Type> res = new List<Type>();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.Namespace == nameSpace)
                    res.Add(type);
            }

            return res;
        }

        public static List<Type> GetClassesByBaseClass(Type baseType, Assembly assembly)
        {
            List<Type> res = new List<Type>();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract) continue;
                if (type.BaseType == baseType)
                {
                    res.Add(type);
                }
            }

            return res;
        }

        public static List<Type> GetClassesByGenericClass(Type baseType)
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            List<Type> res = new List<Type>();

            foreach (Type type in asm.GetTypes())
            {
                if (type.IsAbstract) continue;
                if (IsBaseType(type, baseType))
                {
                    res.Add(type);
                }
            }

            return res;
        }

        public static bool IsBaseType(Type type, Type baseType)
        {
            if (type == null || baseType == null || type == baseType || type.BaseType == null)
            {
                return false;
            }

            if (baseType.IsInterface)
            {
                foreach (Type t in type.GetInterfaces())
                {
                    if (t == baseType)
                    {
                        return true;
                    }
                }
            }
            else
            {
                do
                {
                    if (type.BaseType == baseType)
                    {
                        return true;
                    }

                    type = type.BaseType;
                } while (type != null);
            }

            return false;
        }

        public static string TextToVertical(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ch in text)
            {
                sb.Append(ch);
                sb.Append("\n");
            }

            return sb.ToString().Trim('\n');
        }

        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public static List<T> GetRandomFromList<T>(List<T> OriList, int number, SRandom random, List<T> exceptList = null)
        {
            return GetRandomFromListCore(OriList, number, random, exceptList);
        }

        public static List<T> GetRandomFromList<T>(List<T> OriList, int number, List<T> exceptList = null)
        {
            return GetRandomFromListCore(OriList, number, null, exceptList);
        }

        private static List<T> GetRandomFromListCore<T>(List<T> OriList, int number, SRandom random = null, List<T> exceptList = null)
        {
            if (OriList == null || OriList.Count == 0) return new List<T>();

            List<T> ori = OriList.ToArray().ToList();
            if (exceptList != null)
            {
                List<T> remove = new List<T>();
                foreach (T t in ori)
                {
                    if (exceptList.Contains(t))
                    {
                        remove.Add(t);
                    }
                }

                foreach (T removeT in remove)
                {
                    ori.Remove(removeT);
                }
            }

            if (number > ori.Count) number = ori.Count;

            HashSet<int> indices = new HashSet<int>();
            if (random == null)
            {
                System.Random rd = new System.Random(DateTime.Now.Millisecond * number);
                while (indices.Count < number)
                {
                    int index = rd.Next(0, ori.Count);
                    if (!indices.Contains(index))
                    {
                        indices.Add(index);
                    }
                }
            }
            else
            {
                while (indices.Count < number)
                {
                    int index = random.Range(0, ori.Count);
                    if (!indices.Contains(index))
                    {
                        indices.Add(index);
                    }
                }
            }

            List<T> res = new List<T>();
            foreach (int i in indices)
            {
                res.Add(ori[i]);
            }

            return res;
        }

        public static List<T> GetRandomWithProbabilityFromList<T>(List<T> OriList, int number) where T : Probability
        {
            if (OriList == null || OriList.Count == 0) return new List<T>();

            int accu = 0;
            SortedDictionary<int, T> resDict = new SortedDictionary<int, T>();
            foreach (T probability in OriList)
            {
                if (probability.Probability > 0)
                {
                    accu += probability.Probability;
                    resDict.Add(accu, probability);
                }
            }

            System.Random rd = new System.Random(DateTime.Now.Millisecond * number);
            HashSet<T> res = new HashSet<T>();
            while (res.Count < number)
            {
                int index = rd.Next(0, accu);
                foreach (int key in resDict.Keys)
                {
                    if (key >= index)
                    {
                        T pr = resDict[key];
                        if (!res.Contains(pr))
                        {
                            res.Add(pr);
                        }
                        else
                        {
                            if (!pr.IsSingleton)
                            {
                                res.Add((T) pr.ProbabilityClone());
                            }
                        }

                        break;
                    }
                }
            }

            return res.ToList();
        }

        public static void CopyDirectory(string srcPath, string destPath)
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos(); //获取目录下（不包含子目录）的文件和子目录
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo) //判断是否文件夹
                {
                    if (!Directory.Exists(destPath + "/" + i.Name))
                    {
                        Directory.CreateDirectory(destPath + "/" + i.Name); //目标目录下不存在此文件夹即创建子文件夹
                    }

                    CopyDirectory(i.FullName, destPath + "/" + i.Name); //递归调用复制子文件夹
                }
                else
                {
                    File.Copy(i.FullName, destPath + "/" + i.Name, true); //不是文件夹即复制文件，true表示可以覆盖同名文件
                }
            }
        }

        public static string HighlightStringFormat(string src, string color, params object[] args)
        {
            string[] coloredStrings = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                coloredStrings[i] = "<" + color + ">" + args[i] + "</color>";
            }

            return String.Format(src, coloredStrings);
        }

        public static string HighlightStringFormat(string src, string color, bool[] needTint, params object[] args)
        {
            string[] coloredStrings = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                if (needTint[i])
                {
                    coloredStrings[i] = "<" + color + ">" + args[i] + "</color>";
                }
                else
                {
                    coloredStrings[i] = args[i].ToString();
                }
            }

            return String.Format(src, coloredStrings);
        }

        public static string AddHighLightColorToText(string highLightText, string color)
        {
            return "<" + color + ">" + highLightText + "</color>";
        }

        private static string colorStringPattern = @"(.*)(<#)([0-9a-fA-F]{6,8}>.*</color>)(.*)";

        public static string TextMeshProColorStringConvertToText(string colorString)
        {
            Regex rg = new Regex(colorStringPattern);
            if (rg.IsMatch(colorString))
            {
                string replace = colorString;
                while (rg.IsMatch(replace))
                {
                    replace = rg.Replace(replace, "$1<color=#$3$4");
                }

                return replace;
            }

            return colorString;
        }

        public static Vector3 GetIntersectWithLineAndPlane(Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint)
        {
            float d = Vector3.Dot(planePoint - point, planeNormal) / Vector3.Dot(direct.normalized, planeNormal);
            return d * direct.normalized + point;
        }

        public static string ConvertProjectPathToAbsolutePath(string projectPath)
        {
            if (projectPath.StartsWith("/"))
            {
                return Application.dataPath + projectPath;
            }
            else
            {
                return $"{Application.dataPath}/{projectPath}";
            }
        }

        public static string ConvertAbsolutePathToProjectPath(string absolutePath)
        {
            if (absolutePath.Contains("\\"))
            {
                absolutePath = absolutePath.Replace("\\", "/");
            }

            if (absolutePath.StartsWith(Application.dataPath))
            {
                return "Assets" + absolutePath.Replace(Application.dataPath, "");
            }
            else
            {
                return null;
            }
        }
    }
}