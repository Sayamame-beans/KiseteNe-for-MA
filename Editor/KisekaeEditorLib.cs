/*
Copyright(c) 2020 Tomoshibi/Tomoya
https://tomo-shi-vi.hateblo.jp/
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Sayabeans.KiseteNeForMA.Editor
{
	public partial class KisekaeEditor : EditorWindow
	{
		Transform GetTransform(HumanBodyBones bone)
		{
			return boneList[bone];
		}

		Transform FindBone(HumanBodyBones bone, Transform parent, Regex matchPattern)
		{
			if (boneList[bone] != null)
				return boneList[bone];

			if (parent == null)
				return null;

			foreach (Transform child in parent) {
				if (matchPattern.IsMatch(child.name))
					return child;
			}
			return null;
		}

		Transform FindBone(HumanBodyBones bone, Transform parent, Regex matchPattern, Side side)
		{
			if (boneList[bone] != null)
				return boneList[bone];

			if (parent == null)
				return null;

			Transform hit1 = null;
			Transform hit2 = null;

			foreach (Transform child in parent)
			{
				if (matchPattern.IsMatch(child.name))
				{
					if (hit1 == null)
						hit1 = child;
					else
						hit2 = child;
				}
			}

			if (hit1 == null || hit2 == null)
				return null;

			switch (side)
			{
				case Side.Right:
					return hit1.position.x > hit2.position.x ? hit1 : hit2;
				case Side.Left:
					return hit1.position.x < hit2.position.x ? hit1 : hit2;
				default:
					throw new ArgumentOutOfRangeException(nameof(side), side, null);
			}

			return null;
		}

		enum Side
		{
			Right,
			Left,
		}
	}
}
