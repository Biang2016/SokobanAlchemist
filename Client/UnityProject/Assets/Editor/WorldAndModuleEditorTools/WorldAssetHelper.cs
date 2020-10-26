using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

public class WorldAssetHelper : UnityEditor.AssetModificationProcessor
{
    public static string[] OnWillSaveAssets(string[] paths)
    {
        //foreach (string path in paths)
        //{
        //    if (path.Contains("Designs/WorldModule"))
        //    {
        //        WorldModuleDesignHelper module = StageUtility.GetCurrentStageHandle().FindComponentOfType<WorldModuleDesignHelper>();
        //        if (module)
        //        {
        //            bool dirty = module.SortModule();
        //            if (dirty)
        //            {
        //                Transform[] allTrans = module.GetComponentsInChildren<Transform>();
        //                foreach (Transform trans in allTrans)
        //                {
        //                    EditorUtility.SetDirty(trans.gameObject);
        //                    EditorUtility.SetDirty(trans);
        //                }

        //                EditorSceneManager.MarkSceneDirty(PrefabStageUtility.GetPrefabStage(module.gameObject).scene);
        //                PrefabUtility.SaveAsPrefabAsset(module.gameObject, path);
        //            }
        //        }
        //    }

        //    if (path.Contains("Designs/Worlds"))
        //    {
        //        WorldDesignHelper world = StageUtility.GetCurrentStageHandle().FindComponentOfType<WorldDesignHelper>();
        //        if (world)
        //        {
        //            bool dirty = world.SortWorld();
        //            if (dirty)
        //            {
        //                PrefabUtility.SaveAsPrefabAsset(world.gameObject, path);
        //            }
        //        }
        //    }
        //}

        return paths;
    }
}