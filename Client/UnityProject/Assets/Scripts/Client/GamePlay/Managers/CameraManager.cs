using BiangStudio.Singleton;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    public Camera MainCamera;
    public Camera BattleUICamera;
    public FieldCamera FieldCamera;
}