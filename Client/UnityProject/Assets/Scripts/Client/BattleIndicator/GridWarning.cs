using UnityEngine;

public class GridWarning : BattleIndicator
{
    [SerializeField]
    private MeshRenderer Fill;

    [SerializeField]
    private MeshRenderer BorderDim;

    [SerializeField]
    private MeshRenderer BorderHighlight;

    public GridWarning SetFillColor(Color color)
    {
        Fill.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", color);
        Fill.SetPropertyBlock(mpb);
        return this;
    }

    public GridWarning SetBorderDimColor(Color color)
    {
        BorderDim.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", color);
        BorderDim.SetPropertyBlock(mpb);
        return this;
    }

    public GridWarning SetBorderHighlightColor(Color color)
    {
        BorderHighlight.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", color);
        BorderHighlight.SetPropertyBlock(mpb);
        return this;
    }
}