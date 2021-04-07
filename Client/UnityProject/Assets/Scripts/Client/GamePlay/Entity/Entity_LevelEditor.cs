using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Entity_LevelEditor : MonoBehaviour
{
    public GameObject ModelRoot;

    public GameObject IndicatorHelperGO;

    public EntityData EntityData = new EntityData();

    public abstract bool RefreshOrientation();
}