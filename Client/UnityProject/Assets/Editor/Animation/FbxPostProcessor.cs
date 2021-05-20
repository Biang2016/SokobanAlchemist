using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetPostProcessors
{
	/// <summary>
	/// 把此文件放到项目内的Editor文件夹下面即可。
	/// </summary>
	public class FbxPostProcessor : AssetPostprocessor
	{

		public void OnPostprocessModel(GameObject gameObject)
		{
			AnimationClip[] animationClipList = UnityEngine.Object.FindObjectsOfType(typeof(AnimationClip)) as AnimationClip[];
			foreach (AnimationClip theAnimation in animationClipList)
			{
				//exclude scale curve
				foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(theAnimation))
				{
					string name = theCurveBinding.propertyName.ToLower();
					if (name.Contains("scale"))
					{
						AnimationUtility.SetEditorCurve(theAnimation, theCurveBinding, null);
					}
				}
			}

			ModelImporter model = (ModelImporter)assetImporter;
			if (model != null)
			{
				//set AnimationCompression.Optimal
				model.animationCompression = ModelImporterAnimationCompression.Optimal;

				ModelImporterClipAnimation[] clipAnims = model.clipAnimations;

				//Floating point precision is compressed to f3
				for (int ii = 0; ii < clipAnims.Length; ++ii)
				{
					ClipAnimationInfoCurve[] newCurves = clipAnims[ii].curves;
					//Debug.LogFormat("OnPostprocessFBX : newCurves Clip Count : {0}", newCurves.Length);
					if (newCurves == null)
					{
						continue;
					}

					foreach (ClipAnimationInfoCurve animCurve in newCurves)
					{
						if (animCurve.curve == null)
						{
							continue;
						}

						Keyframe[] keyFrames = animCurve.curve.keys;

						for (int i = 0; i < keyFrames.Length; ++i)
						{
							Keyframe key = animCurve.curve.keys[i];
							key.value = float.Parse(key.value.ToString("f3"));
							key.inTangent = float.Parse(key.inTangent.ToString("f3"));
							key.outTangent = float.Parse(key.outTangent.ToString("f3"));
							keyFrames[i] = key;
						}

						animCurve.curve.keys = keyFrames;
					}
					clipAnims[ii].curves = newCurves;
				}

				if (clipAnims.Length > 0)
					AssetDatabase.Refresh();
			}
		}
	}
}