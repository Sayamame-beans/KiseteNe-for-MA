/*
Copyright(c) 2020 Tomoshibi/Tomoya
https://tomo-shi-vi.hateblo.jp/
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Sayabeans.KiseteNeForMA.Editor
{
	public partial class KisekaeEditor : EditorWindow
	{
		Transform GetTransform(HumanBodyBones bone)
		{
			return m_boneList[bone];
		}

		Transform FindBone(HumanBodyBones bone, Transform parent, string matchPattern)
		{
			if (m_boneList.ContainsKey(bone) && m_boneList[bone] != null)
				return m_boneList[bone];

			if (parent == null)
				return null;

			foreach (Transform child in parent) {
				if (Regex.IsMatch(child.name, matchPattern, RegexOptions.IgnoreCase))
					return child;
			}
			return null;
		}

		Transform FindBone(HumanBodyBones bone, Transform parent, string matchPattern, int side)
		{
			if (m_boneList[bone] != null)
				return m_boneList[bone];

			if (parent == null)
				return null;

			Transform hit1 = null;
			Transform hit2 = null;

			foreach (Transform child in parent) {
				if (Regex.IsMatch(child.name, matchPattern, RegexOptions.IgnoreCase)) {
					if (hit1 == null)
						hit1 = child;
					else
						hit2 = child;
				}
			}

			if (hit1 == null || hit2 == null)
				return null;

			if (side == RIGHT) {
				if (hit1.position.x > hit2.position.x)
					return hit1;
				else
					return hit2;
			} else if (side == LEFT) {
				if (hit1.position.x < hit2.position.x)
					return hit1;
				else
					return hit2;
			}

			return null;
		}
	}
}
