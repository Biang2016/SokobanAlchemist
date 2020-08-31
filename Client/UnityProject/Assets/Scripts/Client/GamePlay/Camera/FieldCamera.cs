using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class FieldCamera : MonoBehaviour
{
    [SerializeField]
    private Camera Camera;

    [SerializeField]
    private Camera BattleUICamera;

    [SerializeField]
    private Transform target;

    [LabelText("水平角")]
    public float HorizontalAngle;

    [LabelText("竖直角")]
    public float VerticalAngle;

    [LabelText("距离")]
    public float Distance;

    void Awake()
    {
        Distance_Level = 2;
    }

    void Start()
    {
        ClientGameManager.Instance.BattleMessenger.AddListener<Actor>((uint) Enum_Events.OnPlayerLoaded, SetTarget);
    }

    private void SetTarget(Actor actor)
    {
        target = actor.transform;
    }

    public float GetScaleForBattleUI()
    {
        return DistanceLevels_ScaleForBattleUI[Distance_Level];
    }

    public bool NeedLerp = true;
    public float SmoothTime = 0.05f;
    Vector3 curVelocity = Vector3.zero;

    private void Update()
    {
        UpdateFOVLevel();
    }

    private void LateUpdate()
    {
        if (target)
        {
            Vector3 diff = Vector3.zero;
            diff.x = Distance * Mathf.Cos(VerticalAngle * Mathf.Deg2Rad) * Mathf.Sin(HorizontalAngle * Mathf.Deg2Rad);
            diff.y = Distance * Mathf.Sin(VerticalAngle * Mathf.Deg2Rad);
            diff.z = -Distance * Mathf.Cos(VerticalAngle * Mathf.Deg2Rad) * Mathf.Cos(HorizontalAngle * Mathf.Deg2Rad);
            Vector3 destination = target.position + diff;
            if (NeedLerp)
            {
                transform.position = Vector3.SmoothDamp(transform.position, destination, ref curVelocity, SmoothTime);
            }
            else
            {
                transform.position = destination;
            }

            transform.forward = target.position - destination;
        }
    }

    #region Distance Levels

    private int _distance_Level = 0;

    internal int Distance_Level
    {
        get { return _distance_Level; }
        set
        {
            if (_distance_Level != value)
            {
                _distance_Level = Mathf.Clamp(value, 0, DistanceLevels.Length - 1);
                Distance = DistanceLevels[_distance_Level];
                Camera.orthographicSize = OrthographicSizeLevels[_distance_Level];
            }
        }
    }

    [LabelText("距离等级表")]
    public float[] DistanceLevels = new float[] {10, 15, 25, 35, 50, 75};

    [LabelText("距离等级表-战斗飘字UI")]
    public float[] DistanceLevels_ScaleForBattleUI = new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f};

    [LabelText("正交大小等级表")]
    public float[] OrthographicSizeLevels = new float[] {5, 7, 9, 11, 13, 15};

    void UpdateFOVLevel()
    {
        if (ControlManager.Instance.Battle_MouseWheel.y < 0)
        {
            Distance_Level++;
        }

        if (ControlManager.Instance.Battle_MouseWheel.y > 0)
        {
            Distance_Level--;
        }

        if (ControlManager.Instance.Building_MouseWheel.y < 0)
        {
            if (Distance_Level <= 1)
            {
                Distance_Level++;
            }
        }

        if (ControlManager.Instance.Building_MouseWheel.y > 0)
        {
            Distance_Level--;
        }
    }

    #endregion
}