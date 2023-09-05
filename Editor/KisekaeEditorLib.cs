﻿/*
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

		[Serializable]
		class HumanBodyBonesToDictionaryMapping
		{
			[SerializeField] private Transform[] backedArray = new Transform[(int)HumanBodyBones.LastBone];

			public ref Transform this[HumanBodyBones humanBodyBones] => ref backedArray[(int)humanBodyBones];

			public void Clear()
			{
				for (var i = 0; i < backedArray.Length; i++)
					backedArray[i] = null;
			}
		}

		[Serializable]
		struct FloatUndoState
		{
			public float Value
			{
				get => value;
				set => _knownValue = this.value = value;
			}

			[SerializeField] private float value;
			[NonSerialized] private float _knownValue;
			[SerializeField] private int collapseGroupId;
			[SerializeField] private int prevGroupId;

			public void ButtonAndSliderGui(float paramDefault, float leftValue, float rightValue, float paramRatio = 1.0f)
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				// When we detected external changes (mostly by undo/redo), GUI.changed = true
				if (_knownValue != value) GUI.changed = true;

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("RESET"))
					_knownValue = value = paramDefault;

				if (GUILayout.Button("--", EditorStyles.miniButtonLeft, GUILayout.Height(20), GUILayout.Width(50)))
					SliderButton(-0.01f * paramRatio);

				if (GUILayout.Button("-", EditorStyles.miniButtonMid, GUILayout.Height(20), GUILayout.Width(50)))
					SliderButton(-0.001f * paramRatio);

				if (GUILayout.Button("+", EditorStyles.miniButtonMid, GUILayout.Height(20), GUILayout.Width(50)))
					SliderButton(+0.001f * paramRatio);

				if (GUILayout.Button("++", EditorStyles.miniButtonRight, GUILayout.Height(20), GUILayout.Width(50)))
					SliderButton(+0.01f * paramRatio);

				GUILayout.EndHorizontal();

				_knownValue = value = EditorGUILayout.Slider(value, leftValue, rightValue);
			}

			public void SliderButton(float diff)
			{
				_knownValue = value += diff;
				var currentId = Undo.GetCurrentGroup();
				if (prevGroupId + 1 == currentId)
					Undo.CollapseUndoOperations(collapseGroupId);
				else
					collapseGroupId = currentId;
				prevGroupId = currentId;
			}
		}
	}
}
