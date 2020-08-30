using UnityEngine;
using System.Collections;

public class BornPoint : MonoBehaviour
{
    public BornPointType BornPointType;
}

public enum BornPointType
{
    None,
    Player,
    Enemy,
}