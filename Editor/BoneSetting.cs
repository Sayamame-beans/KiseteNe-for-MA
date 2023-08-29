/*
Copyright(c) 2020 Tomoshibi/Tomoya
https://tomo-shi-vi.hateblo.jp/
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UnityEditor;
using UnityEngine;

namespace Sayabeans.KiseteNeForMA.Editor
{
	public partial class KisekaeEditor : EditorWindow
	{
		///服のボーンを手動設定するためのUIを作る
		///Humanoidではなくボーンの名前がイレギュラーな服用
		void CreateBoneSettingsUI()
		{
			EditorGUILayout.HelpBox("この設定は調整がうまく動かないときに確認してください。\nすべて埋める必要はありません。該当するボーンが存在しない場合はnoneにしてください。", MessageType.Warning, true);

			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));

			m_boneList[HumanBodyBones.Hips] = EditorGUILayout.ObjectField("Hips", m_boneList[HumanBodyBones.Hips], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.Spine] = EditorGUILayout.ObjectField("Spine", m_boneList[HumanBodyBones.Spine], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.Chest] = EditorGUILayout.ObjectField("Chest", m_boneList[HumanBodyBones.Chest], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.Neck] = EditorGUILayout.ObjectField("Neck", m_boneList[HumanBodyBones.Neck], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.Head] = EditorGUILayout.ObjectField("Head", m_boneList[HumanBodyBones.Head], typeof(Transform), true) as Transform;

			GUILayout.Label("左腕", EditorStyles.boldLabel);

			m_boneList[HumanBodyBones.LeftShoulder] = EditorGUILayout.ObjectField("LeftShoulder", m_boneList[HumanBodyBones.LeftShoulder], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.LeftUpperArm] = EditorGUILayout.ObjectField("LeftUpperArm", m_boneList[HumanBodyBones.LeftUpperArm], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.LeftLowerArm] = EditorGUILayout.ObjectField("LeftLowerArm", m_boneList[HumanBodyBones.LeftLowerArm], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.LeftHand] = EditorGUILayout.ObjectField("LeftHand", m_boneList[HumanBodyBones.LeftHand], typeof(Transform), true) as Transform;

			GUILayout.Label("右腕", EditorStyles.boldLabel);

			m_boneList[HumanBodyBones.RightShoulder] = EditorGUILayout.ObjectField("RightShoulder", m_boneList[HumanBodyBones.RightShoulder], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.RightUpperArm] = EditorGUILayout.ObjectField("RightUpperArm", m_boneList[HumanBodyBones.RightUpperArm], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.RightLowerArm] = EditorGUILayout.ObjectField("RightLowerArm", m_boneList[HumanBodyBones.RightLowerArm], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.RightHand] = EditorGUILayout.ObjectField("RightHand", m_boneList[HumanBodyBones.RightHand], typeof(Transform), true) as Transform;

			GUILayout.Label("左足", EditorStyles.boldLabel);

			m_boneList[HumanBodyBones.LeftUpperLeg] = EditorGUILayout.ObjectField("LeftUpperLeg", m_boneList[HumanBodyBones.LeftUpperLeg], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.LeftLowerLeg] = EditorGUILayout.ObjectField("LeftLowerLeg", m_boneList[HumanBodyBones.LeftLowerLeg], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.LeftFoot] = EditorGUILayout.ObjectField("LeftFoot", m_boneList[HumanBodyBones.LeftFoot], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.LeftToes] = EditorGUILayout.ObjectField("LeftToes", m_boneList[HumanBodyBones.LeftToes], typeof(Transform), true) as Transform;

			GUILayout.Label("右足", EditorStyles.boldLabel);

			m_boneList[HumanBodyBones.RightUpperLeg] = EditorGUILayout.ObjectField("RightUpperLeg", m_boneList[HumanBodyBones.RightUpperLeg], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.RightLowerLeg] = EditorGUILayout.ObjectField("RightLowerLeg", m_boneList[HumanBodyBones.RightLowerLeg], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.RightFoot] = EditorGUILayout.ObjectField("RightFoot", m_boneList[HumanBodyBones.RightFoot], typeof(Transform), true) as Transform;
			m_boneList[HumanBodyBones.RightToes] = EditorGUILayout.ObjectField("RightToes", m_boneList[HumanBodyBones.RightToes], typeof(Transform), true) as Transform;

			GUILayout.EndScrollView();
		}
	}
}
