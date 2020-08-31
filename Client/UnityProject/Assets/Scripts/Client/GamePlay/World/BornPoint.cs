using UnityEngine;
using System.Collections;

public class BornPoint : MonoBehaviour
{
    public BornPointType BornPointType;
    public PlayerNumber PlayerNumber;
}

public enum BornPointType
{
    None,
    Player,
    Enemy,
}