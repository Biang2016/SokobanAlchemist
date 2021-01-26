using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

[assembly: BindTypeNameToType("BuffAttributeMatrixAsset", typeof(EntityBuffAttributeMatrixAsset))]

[CreateAssetMenu(menuName = "Buff相克矩阵配置文件")]
public class EntityBuffAttributeMatrixAsset : SerializedScriptableObject
{
    [FormerlySerializedAs("BuffAttributeMatrix")]
    public EntityBuffAttributeRelationship[,] EntityBuffAttributeMatrix;
}