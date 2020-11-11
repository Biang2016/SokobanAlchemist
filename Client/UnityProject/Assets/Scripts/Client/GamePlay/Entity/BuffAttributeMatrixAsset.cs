using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Buff相克矩阵配置文件")]
public class BuffAttributeMatrixAsset : SerializedScriptableObject
{
    public BuffAttributeRelationship[,] BuffAttributeMatrix;
}