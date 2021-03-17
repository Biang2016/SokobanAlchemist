using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Buff相克矩阵配置文件")]
public class EntityBuffAttributeMatrixAsset : SerializedScriptableObject
{
    public EntityBuffAttributeRelationship[,] EntityBuffAttributeMatrix;
    public int version = 0;
}