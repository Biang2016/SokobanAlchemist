using UnityEngine;
using Random = UnityEngine.Random;

public class BoxArtHelper : EntityArtHelper
{
    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        if (UseModelVariants)
        {
            int showIndex = Random.Range(0, ModelVariants.Length);
            for (int i = 0; i < ModelVariants.Length; i++)
            {
                ModelVariants[i].SetActive(i == showIndex);
            }
        }
    }

    public bool UseModelVariants = false;
    public GameObject[] ModelVariants = new GameObject[1];
}