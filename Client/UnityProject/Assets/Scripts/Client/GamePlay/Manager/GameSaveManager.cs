using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BiangLibrary.Singleton;
using Sirenix.Serialization;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class GameSaveManager : TSingletonBaseManager<GameSaveManager>
{
    public enum SaveDataType
    {
        GameProgress,
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void ShutDown()
    {
        base.ShutDown();
        MDict.Clear();
    }

    private string GetFilePath(string dataGroup, string dataKey, SaveDataType saveDataType)
    {
        string filePath;
        string folderPath;
        //if (Application.platform == RuntimePlatform.WindowsPlayer)
        //{
        //    filePath = $"{Application.persistentDataPath}/{saveDataType}/{dataKey}.save";
        //}
        //else if (Application.platform == RuntimePlatform.WindowsEditor)
        //{
        folderPath = $"{Application.streamingAssetsPath}/{saveDataType}/{dataGroup}";
        filePath = $"{folderPath}/{dataKey}.save";
        //}

        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        return filePath;
    }

    public Dictionary<string, WorldModuleDataModification> MDict = new Dictionary<string, WorldModuleDataModification>();

    public void SaveData(string dataGroup, string dataKey, SaveDataType saveDataType, WorldModuleDataModification data, DataFormat dataFormat = DataFormat.Binary)
    {
        if (MDict.ContainsKey(dataGroup + dataKey))
        {
            MDict.Remove(dataGroup + dataKey);
        }

        MDict.Add(dataGroup + dataKey, data.Clone());
        //string filePath = GetFilePath(dataGroup, dataKey, saveDataType);
        //if (File.Exists(filePath))
        //{
        //    File.Delete(filePath);
        //}

        //byte[] bytes = SerializationUtility.SerializeValue(data, dataFormat);
        //using (FileStream stream = File.Open(filePath, FileMode.Create))
        //{
        //    await stream.WriteAsync(bytes, 0, bytes.Length);
        //}
    }

    //public async Task<T> Async_LoadData<T>(string dataGroup, string dataKey, SaveDataType saveDataType, DataFormat dataFormat = DataFormat.Binary)
    //{
    //    string filePath = GetFilePath(dataGroup, dataKey, saveDataType);
    //    if (File.Exists(filePath))
    //    {
    //        using (FileStream stream = File.Open(filePath, FileMode.Open))
    //        {
    //            byte[] bytes = new byte[stream.Length];
    //            await stream.ReadAsync(bytes, 0, bytes.Length);
    //            T data = SerializationUtility.DeserializeValue<T>(bytes, dataFormat);
    //            return data;
    //        }
    //    }

    //    return default;
    //}

    public WorldModuleDataModification LoadData(string dataGroup, string dataKey, SaveDataType saveDataType, DataFormat dataFormat = DataFormat.Binary)
    {
        if (MDict.TryGetValue(dataGroup + dataKey, out WorldModuleDataModification data))
        {
            return data.Clone();
        }
        else
        {
            return null;
        }

        //string filePath = GetFilePath(dataGroup, dataKey, saveDataType);
        //if (File.Exists(filePath))
        //{
        //    byte[] bytes = File.ReadAllBytes(filePath);
        //    T data = SerializationUtility.DeserializeValue<T>(bytes, dataFormat);
        //    return data;
        //}

        //return default;
    }
}