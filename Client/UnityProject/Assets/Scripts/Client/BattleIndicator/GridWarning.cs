using UnityEngine;

public class GridWarning : BattleIndicator
{
    public MeshRenderer Fill;
    public MeshRenderer BorderDim;
    public MeshRenderer BorderHighlight;

    public GridWarning SetFillColor(Color color)
    {
        Fill.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", color);
        mpb.SetColor("_EmissionColor", color);
        Fill.SetPropertyBlock(mpb);
        return this;
    }

    public GridWarning SetBorderDimColor(Color color)
    {
        BorderDim.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", color);
        mpb.SetColor("_EmissionColor", color);
        BorderDim.SetPropertyBlock(mpb);
        return this;
    }

    public GridWarning SetBorderHighlightColor(Color color)
    {
        BorderHighlight.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", color);
        mpb.SetColor("_EmissionColor", color);
        BorderHighlight.SetPropertyBlock(mpb);
        return this;
    }
}