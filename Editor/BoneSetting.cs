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

			boneList[HumanBodyBones.Hips] = EditorGUILayout.ObjectField("Hips", boneList[HumanBodyBones.Hips], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.Spine] = EditorGUILayout.ObjectField("Spine", boneList[HumanBodyBones.Spine], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.Chest] = EditorGUILayout.ObjectField("Chest", boneList[HumanBodyBones.Chest], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.Neck] = EditorGUILayout.ObjectField("Neck", boneList[HumanBodyBones.Neck], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.Head] = EditorGUILayout.ObjectField("Head", boneList[HumanBodyBones.Head], typeof(Transform), true) as Transform;

			GUILayout.Label("左腕", EditorStyles.boldLabel);

			boneList[HumanBodyBones.LeftShoulder] = EditorGUILayout.ObjectField("LeftShoulder", boneList[HumanBodyBones.LeftShoulder], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.LeftUpperArm] = EditorGUILayout.ObjectField("LeftUpperArm", boneList[HumanBodyBones.LeftUpperArm], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.LeftLowerArm] = EditorGUILayout.ObjectField("LeftLowerArm", boneList[HumanBodyBones.LeftLowerArm], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.LeftHand] = EditorGUILayout.ObjectField("LeftHand", boneList[HumanBodyBones.LeftHand], typeof(Transform), true) as Transform;

			GUILayout.Label("右腕", EditorStyles.boldLabel);

			boneList[HumanBodyBones.RightShoulder] = EditorGUILayout.ObjectField("RightShoulder", boneList[HumanBodyBones.RightShoulder], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.RightUpperArm] = EditorGUILayout.ObjectField("RightUpperArm", boneList[HumanBodyBones.RightUpperArm], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.RightLowerArm] = EditorGUILayout.ObjectField("RightLowerArm", boneList[HumanBodyBones.RightLowerArm], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.RightHand] = EditorGUILayout.ObjectField("RightHand", boneList[HumanBodyBones.RightHand], typeof(Transform), true) as Transform;

			GUILayout.Label("左足", EditorStyles.boldLabel);

			boneList[HumanBodyBones.LeftUpperLeg] = EditorGUILayout.ObjectField("LeftUpperLeg", boneList[HumanBodyBones.LeftUpperLeg], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.LeftLowerLeg] = EditorGUILayout.ObjectField("LeftLowerLeg", boneList[HumanBodyBones.LeftLowerLeg], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.LeftFoot] = EditorGUILayout.ObjectField("LeftFoot", boneList[HumanBodyBones.LeftFoot], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.LeftToes] = EditorGUILayout.ObjectField("LeftToes", boneList[HumanBodyBones.LeftToes], typeof(Transform), true) as Transform;

			GUILayout.Label("右足", EditorStyles.boldLabel);

			boneList[HumanBodyBones.RightUpperLeg] = EditorGUILayout.ObjectField("RightUpperLeg", boneList[HumanBodyBones.RightUpperLeg], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.RightLowerLeg] = EditorGUILayout.ObjectField("RightLowerLeg", boneList[HumanBodyBones.RightLowerLeg], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.RightFoot] = EditorGUILayout.ObjectField("RightFoot", boneList[HumanBodyBones.RightFoot], typeof(Transform), true) as Transform;
			boneList[HumanBodyBones.RightToes] = EditorGUILayout.ObjectField("RightToes", boneList[HumanBodyBones.RightToes], typeof(Transform), true) as Transform;

			GUILayout.EndScrollView();
		}
	}
}
