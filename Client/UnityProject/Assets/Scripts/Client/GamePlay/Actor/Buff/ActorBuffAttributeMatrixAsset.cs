using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "角色Buff相克矩阵配置文件")]
public class ActorBuffAttributeMatrixAsset : SerializedScriptableObject
{
    public ActorBuffAttributeRelationship[,] ActorBuffAttributeMatrix;
}