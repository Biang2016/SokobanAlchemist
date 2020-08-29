using BiangStudio.GameDataFormat.Grid;
using DG.Tweening;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    private Camera Camera;

    [SerializeField]
    private Camera BattleUICamera;

    private Transform target;
    private Vector3 targetingPoint;
    private Vector3 Offset_Fighting;

    void Awake()
    {
        RefreshTargetingPosition();
        FOV_Level = 2;
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
        transform.position = transform.position + target.position - targetingPoint;
        RefreshTargetingPosition();

        if (target)
        {
            Offset_Fighting = target.position - targetingPoint;
        }
    }

    private int _fov_Level = 0;

    internal int FOV_Level
    {
        get { return _fov_Level; }
        set
        {
            if (_fov_Level != value)
            {
                _fov_Level = Mathf.Clamp(value, 0, FOVs.Length - 1);
                Camera.DOFieldOfView(FOVs[_fov_Level], 0.2f);
                BattleUICamera.DOFieldOfView(FOVs[_fov_Level], 0.2f);
            }
        }
    }

    public float[] FOVs = new float[] {10, 15, 25, 35, 50, 75};
    public float[] FOVs_ScaleForBattleUI = new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f};

    public float GetScaleForBattleUI()
    {
        return FOVs_ScaleForBattleUI[FOV_Level];
    }

    private Vector3 offset_Manually;

    public static bool NeedLerp = true;
    public static float SmoothTime = 0.05f;

    private void Update()
    {
        RefreshTargetingPosition();
        if (target)
        {
            offset_Manually = Vector3.zero;
            Vector3 destination = transform.position + target.position - targetingPoint + Offset_Fighting;
            Vector3 curVelocity = Vector3.zero;
            if (NeedLerp)
            {
                transform.position = Vector3.SmoothDamp(transform.position, destination, ref curVelocity, SmoothTime);
            }
            else
            {
                transform.position = destination;
            }
        }

        float movement = 5f;
        offset_Manually = ControlManager.Instance.Building_Move.x * new Vector3(movement, 0, movement) + ControlManager.Instance.Building_Move.y * new Vector3(-movement, 0, movement);
        if (ControlManager.Instance.Battle_MouseWheel.y < 0)
        {
            FOV_Level++;
        }

        if (ControlManager.Instance.Battle_MouseWheel.y > 0)
        {
            FOV_Level--;
        }

        if (ControlManager.Instance.Building_MouseWheel.y < 0)
        {
            if (FOV_Level <= 1)
            {
                FOV_Level++;
            }
        }

        if (ControlManager.Instance.Building_MouseWheel.y > 0)
        {
            FOV_Level--;
        }
    }

    private void RefreshTargetingPosition()
    {
        Ray ray = CameraManager.Instance.MainCamera.ScreenPointToRay(new Vector3(CameraManager.Instance.MainCamera.pixelWidth / 2f, CameraManager.Instance.MainCamera.pixelHeight / 2f));
        targetingPoint = GridUtils.GetIntersectWithLineAndPlane(ray.origin, ray.direction, Vector3.up, Vector3.zero);
    }
}