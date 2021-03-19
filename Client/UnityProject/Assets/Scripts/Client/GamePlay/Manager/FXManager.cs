using BiangLibrary.Singleton;
using UnityEngine;

public class FXManager : TSingletonBaseManager<FXManager>
{
    private Transform Root;

    public void Init(Transform root)
    {
        Root = root;
    }

    public FX PlayFX(FXConfig fxConfig, Vector3 position, float evaluator = 0f)
    {
        if (fxConfig == null || fxConfig.Empty) return null;
        ushort fxTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.FX, fxConfig.TypeName);
        if (GameObjectPoolManager.Instance.FXDict.ContainsKey(fxTypeIndex))
        {
            FX fx = GameObjectPoolManager.Instance.FXDict[fxTypeIndex].AllocateGameObject<FX>(Root);
            fx.transform.position = position;
            fx.transform.localScale = Vector3.one * fxConfig.GetScale(evaluator);
            fx.transform.rotation = Quaternion.identity;
            fx.Play();
            return fx;
        }

        return null;
    }
}