using BiangStudio.Singleton;
using UnityEngine;

public class FXManager : TSingletonBaseManager<FXManager>
{
    private Transform Root;

    public void Init(Transform root)
    {
        Root = root;
    }

    //public FX PlayFX(string fxPath, Vector3 from)
    //{
    //    FX fx = GameObjectPoolManager.Instance.FXDict[fx_Type].AllocateGameObject<FX>(Root);
    //    fx.transform.position = from;
    //    fx.Play();
    //    return fx;
    //}
}