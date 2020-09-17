using BiangStudio.Singleton;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    public Camera MainCamera => FieldCamera.Camera;
    public Camera BattleUICamera;
    public FieldCamera FieldCamera;
}