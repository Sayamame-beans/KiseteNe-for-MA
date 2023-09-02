/*
Copyright(c) 2020 Tomoshibi/Tomoya
https://tomo-shi-vi.hateblo.jp/
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using System;
using UnityEditor;
using UnityEngine;

namespace Sayabeans.KiseteNeForMA.Editor
{
	public partial class KisekaeEditor : EditorWindow
	{
		GameObject m_dress;

		Transform m_armature;
		HumanBodyBonesToDictionaryMapping m_boneList = new HumanBodyBonesToDictionaryMapping();

		bool m_boneDetail = false;
		int m_selectedTabNumber = 0;
		Vector2 scrollPosition;
		bool m_isHair = false;
		bool m_dressBoneError = false;
		bool m_dressBoneWarn = false;

		const int RIGHT = 1;
		const int LEFT = 2;

		//各所調整用
		[SerializeField] private FloatUndoState armRotateZ;
		[SerializeField] private FloatUndoState armRotateY;
		[SerializeField] private FloatUndoState armScaleY;
		[SerializeField] private FloatUndoState armScaleX;
		[SerializeField] private FloatUndoState hipsPosY;
		[SerializeField] private FloatUndoState hipsPosZ;
		[SerializeField] private FloatUndoState hipScaleX;
		[SerializeField] private FloatUndoState legRotateZ;
		[SerializeField] private FloatUndoState legRotate;
		[SerializeField] private FloatUndoState legScaleX;
		[SerializeField] private FloatUndoState legScaleY;

		[SerializeField] private FloatUndoState spineRotate;

		public void OnEnable()
		{
			// Only add callback once.
			Undo.undoRedoPerformed -= Repaint;
			Undo.undoRedoPerformed += Repaint;
		}

		public void OnDisable()
		{
			Undo.undoRedoPerformed -= Repaint;
		}

		//初期値保持
		Vector3 m_defaultHipsPos;
		Quaternion m_defaultLArmQuat;
		Quaternion m_defaultRArmQuat;
		Quaternion m_defaultSpineQuat;
		Quaternion m_defaultLLegQuat;
		Quaternion m_defaultRLegQuat;

		[MenuItem("Tools/KiseteNe for MA")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(KisekaeEditor), false, "KiseteNe for MA");
		}

		void OnGUI()
		{
			Undo.RecordObject(this, "Kisetene");
			GUILayout.Label("MA向け衣装調整支援ツール「キセテネ for MA」", EditorStyles.boldLabel);

			GUILayout.Label("服をセットしてください", EditorStyles.largeLabel);

			EditorGUI.BeginChangeCheck();
			m_dress = EditorGUILayout.ObjectField("服", m_dress, typeof(GameObject), true) as GameObject;
			if (EditorGUI.EndChangeCheck()) {
				m_dressBoneError = false;
				m_dressBoneWarn = false;
				m_isHair = false;
				UpdateBoneList();
			}

			if (m_dress == null)
				return;

			if (m_dressBoneError) {
				EditorGUILayout.HelpBox("服のボーンを取得することができませんでした\n上の欄にArmatureやメッシュを設定している場合は、服のルートオブジェクトを設定してください", MessageType.Error, true);
				return;
			}

			if (m_dressBoneWarn)
				EditorGUILayout.HelpBox("服のボーンを一部取得することができませんでした。\n調整時にうまく動かない場合、ボーン詳細設定を確認してください", MessageType.Warning, true);

			m_boneDetail = GUILayout.Toggle(m_boneDetail, "ボーン詳細設定");
			if (m_boneDetail) {
				CreateBoneSettingsUI();
			}

			GUILayout.Space(20);

			if (m_isHair) {
				GUILayout.Label("髪の調整です", EditorStyles.miniLabel);
				CreateHeadUI();
			} else {
				m_selectedTabNumber = GUILayout.Toolbar(m_selectedTabNumber, new string[] { "全体", "上半身", "下半身" }, EditorStyles.toolbarButton);
				switch (m_selectedTabNumber) {
					case 0:
						GUILayout.Label("全体の調整です", EditorStyles.miniLabel);
						CreateFUllBodyUI();
						break;
					case 1:
						GUILayout.Label("腕周りの調整です", EditorStyles.miniLabel);
						CreateTopBodyUI();
						break;
					case 2:
						GUILayout.Label("足周りの調整です。スカートには影響がないものもあります", EditorStyles.miniLabel);
						CreateBottomBodyUI();
						break;
				}
			}
		}

		void CreateFUllBodyUI()
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Label("上下");
			hipsPosY.ButtonAndSliderGui(0.0f, -1, 1);

			GUILayout.Space(5);

			GUILayout.Label("前後");
			hipsPosZ.ButtonAndSliderGui(0.0f, -1, 1);

			if (EditorGUI.EndChangeCheck()) {
				var hips = GetTransform(HumanBodyBones.Hips);
				hips.position = m_defaultHipsPos + new Vector3(0, hipsPosY.Value, hipsPosZ.Value);
			}

			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();

			GUILayout.Label("拡大縮小");
			hipScaleX.ButtonAndSliderGui(1.0f, 0.5f, 1.5f);
			if (EditorGUI.EndChangeCheck()) {
				var hips = GetTransform(HumanBodyBones.Hips);
				hips.localScale = new Vector3(hipScaleX.Value, hipScaleX.Value, hipScaleX.Value);
			}

			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();
			GUILayout.Label("お辞儀");
			spineRotate.ButtonAndSliderGui(0.0f, -20, 20, paramRatio: 10);
			if (EditorGUI.EndChangeCheck()) {
				var spine = GetTransform(HumanBodyBones.Spine);
				if (spine != null) {
					spine.rotation = m_defaultSpineQuat;
					spine.Rotate(spine.right, spineRotate.Value);
				}
			}
		}

		void CreateTopBodyUI()
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Label("腕を上げる");
			armRotateZ.ButtonAndSliderGui(0.0f, -50, 50, paramRatio: 10);

			GUILayout.Space(5);

			GUILayout.Label("腕を前に出す");
			armRotateY.ButtonAndSliderGui(0.0f, -15, 15, paramRatio: 10);

			if (EditorGUI.EndChangeCheck()) {
				var left = GetTransform(HumanBodyBones.LeftUpperArm);
				if (left != null) {
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					left.rotation = m_defaultLArmQuat;

					left.Rotate(new Vector3(0, 0, 1), armRotateZ.Value*-1, Space.World);
					left.Rotate(new Vector3(0, 1, 0), armRotateY.Value, Space.World);
				}

				var right = GetTransform(HumanBodyBones.RightUpperArm);
				if (right != null) {
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					right.rotation = m_defaultRArmQuat;

					right.Rotate(new Vector3(0, 0, 1), armRotateZ.Value, Space.World);
					right.Rotate(new Vector3(0, 1, 0), armRotateZ.Value * -1, Space.World);
				}
			}

			EditorGUI.BeginChangeCheck();

			GUILayout.Space(5);
			GUILayout.Label("袖を伸ばす");
			armScaleY.ButtonAndSliderGui(1.0f, 0.5f, 1.5f);

			GUILayout.Space(5);
			GUILayout.Label("袖を太くする");
			armScaleX.ButtonAndSliderGui(1.0f, 0.5f, 1.5f);

			if (EditorGUI.EndChangeCheck()) {
				var left = GetTransform(HumanBodyBones.LeftUpperArm);
				if (left != null) {
					if (Mathf.Abs(left.forward.y) > Mathf.Abs(left.forward.z)) {
						left.localScale = new Vector3(armScaleX.Value, armScaleY.Value, armScaleX.Value);
					} else {
						//軸が違うのでxyを入れ替える
						if (left.forward.z > 0) {
							left.localScale = new Vector3(armScaleY.Value, armScaleX.Value, armScaleX.Value);
						} else {
							left.localScale = new Vector3(armScaleX.Value, armScaleY.Value, armScaleX.Value);
						}
					}
				}

				var right = GetTransform(HumanBodyBones.RightUpperArm);
				if (right != null) {
					if (Mathf.Abs(right.forward.y) > Mathf.Abs(right.forward.z)) {
						right.localScale = new Vector3(armScaleX.Value, armScaleY.Value, armScaleX.Value);
					} else {
						//軸が違うのでxyを入れ替える
						if (right.forward.z > 0) {
							right.localScale = new Vector3(armScaleY.Value, armScaleX.Value, armScaleX.Value);
						} else {
							right.localScale = new Vector3(armScaleX.Value, armScaleY.Value, armScaleX.Value);
						}
					}
				}
			}
		}

		void CreateBottomBodyUI()
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Label("足を開く");
			legRotateZ.ButtonAndSliderGui(0.0f, -10, 10, paramRatio: 10);

			GUILayout.Space(5);
			GUILayout.Label("足を前に出す");
			legRotate.ButtonAndSliderGui(0.0f, -10, 10, paramRatio: 10);

			if (EditorGUI.EndChangeCheck()) {
				var left = GetTransform(HumanBodyBones.LeftUpperLeg);
				if (left != null) {
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					left.rotation = m_defaultLLegQuat;

					left.Rotate(left.forward, legRotateZ.Value * -1);
					left.Rotate(left.right, legRotate.Value * -1);
				}

				var right = GetTransform(HumanBodyBones.RightUpperLeg);
				if (right != null) {
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					right.rotation = m_defaultRLegQuat;

					right.Rotate(right.forward, legRotateZ.Value);
					right.Rotate(right.right, legRotate.Value * -1);
				}
			}

			EditorGUI.BeginChangeCheck();

			GUILayout.Space(5);
			GUILayout.Label("裾を伸ばす");
			legScaleY.ButtonAndSliderGui(1.0f, 0.5f, 1.5f);

			GUILayout.Space(5);
			GUILayout.Label("裾を太くする");
			legScaleX.ButtonAndSliderGui(1.0f, 0.5f, 1.5f);

			if (EditorGUI.EndChangeCheck())
			{
				var left = GetTransform(HumanBodyBones.LeftUpperLeg);
				var right = GetTransform(HumanBodyBones.RightUpperLeg);
				if (left != null)
				{
					left.localScale = new Vector3(legScaleX.Value, 1, legScaleX.Value);
				}

				if (right != null)
				{
					right.localScale = new Vector3(legScaleX.Value, 1, legScaleX.Value);
				}
			}
		}

		void CreateHeadUI()
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Label("上下");
			hipsPosY.ButtonAndSliderGui(0.0f, -2, 2);

			GUILayout.Space(5);

			GUILayout.Label("前後");
			hipsPosZ.ButtonAndSliderGui(0.0f, -1, 1);

			if (EditorGUI.EndChangeCheck()) {
				m_armature.transform.position = new Vector3(0, hipsPosY.Value, hipsPosZ.Value);
			}

			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();

			GUILayout.Label("拡大縮小");
			hipScaleX.ButtonAndSliderGui(1.0f, 0.5f, 2.0f);
			if (EditorGUI.EndChangeCheck()) {
				m_armature.localScale = new Vector3(hipScaleX.Value, hipScaleX.Value, hipScaleX.Value);
			}
		}

		//セットされたものからボーン構造を作る
		void UpdateBoneList()
		{
			m_boneList.Clear();

			if (m_dress == null)
				return;

			m_armature = FindBone(HumanBodyBones.Hips, m_dress.transform, "armature|root|skelton");
			if (m_armature == null) {
				m_dressBoneError = true;
				return;
			}

			//Humanoidなら取れる限りとってみる
			var dressAnim = m_dress.GetComponent<Animator>();
			if (dressAnim != null && dressAnim.isHuman) {
				for (int i = (int)HumanBodyBones.Hips; i <= (int)HumanBodyBones.RightToes; i++)
					m_boneList[(HumanBodyBones)i] = dressAnim.GetBoneTransform((HumanBodyBones)i);
			}

			if(m_boneList[HumanBodyBones.Hips] == null)
				m_boneList[HumanBodyBones.Hips] = FindBone(HumanBodyBones.Hips,m_armature, "hip");

			if (m_boneList[HumanBodyBones.Hips] == null) {
				//頭すげ替えか髪の毛用
				if (FindBone(HumanBodyBones.Neck, m_armature, "neck")) {
					m_boneList[HumanBodyBones.Neck] = FindBone(HumanBodyBones.Neck, m_armature, "neck");
					m_boneList[HumanBodyBones.Head] = FindBone(HumanBodyBones.Head, m_boneList[HumanBodyBones.Neck], "head");
					m_isHair = true;
				} else if (FindBone(HumanBodyBones.Head, m_armature, "head")) {
					m_boneList[HumanBodyBones.Head] = FindBone(HumanBodyBones.Head, m_armature, "head");
					m_isHair = true;
				} else {
					m_dressBoneError = true;
					return;
				}
			}

			m_dressBoneError = false;

			m_boneList[HumanBodyBones.Spine] = FindBone(HumanBodyBones.Spine, m_boneList[HumanBodyBones.Hips], "spine");
			m_boneList[HumanBodyBones.Chest] = FindBone(HumanBodyBones.Chest, m_boneList[HumanBodyBones.Spine], "chest");

			//UpperChestあったらHeadとShoulderはそっちから拾う
			var upperChest = FindBone(HumanBodyBones.UpperChest, m_boneList[HumanBodyBones.Chest], "upper");
			m_boneList[HumanBodyBones.Neck] = FindBone(HumanBodyBones.Neck,
				(upperChest) ? upperChest : m_boneList[HumanBodyBones.Chest], "neck");
			m_boneList[HumanBodyBones.Head] = FindBone(HumanBodyBones.Head, m_boneList[HumanBodyBones.Neck], "head");

			//左腕
			m_boneList[HumanBodyBones.LeftShoulder] = FindBone(HumanBodyBones.LeftShoulder,
				(upperChest) ? upperChest : m_boneList[HumanBodyBones.Chest], "shoulder", LEFT);
			m_boneList[HumanBodyBones.LeftUpperArm] = FindBone(HumanBodyBones.LeftUpperArm, m_boneList[HumanBodyBones.LeftShoulder], "upper|arm");
			m_boneList[HumanBodyBones.LeftLowerArm] = FindBone(HumanBodyBones.LeftLowerArm, m_boneList[HumanBodyBones.LeftUpperArm], "lower|elbow");
			m_boneList[HumanBodyBones.LeftHand] = FindBone(HumanBodyBones.LeftHand, m_boneList[HumanBodyBones.LeftLowerArm], "hand|wrist");

			//右腕
			m_boneList[HumanBodyBones.RightShoulder] = FindBone(HumanBodyBones.RightShoulder,
				(upperChest) ? upperChest : m_boneList[HumanBodyBones.Chest], "shoulder", RIGHT);
			m_boneList[HumanBodyBones.RightUpperArm] = FindBone(HumanBodyBones.RightUpperArm, m_boneList[HumanBodyBones.RightShoulder], "upper|arm");
			m_boneList[HumanBodyBones.RightLowerArm] = FindBone(HumanBodyBones.RightLowerArm, m_boneList[HumanBodyBones.RightUpperArm], "lower|elbow");
			m_boneList[HumanBodyBones.RightHand] = FindBone(HumanBodyBones.RightHand, m_boneList[HumanBodyBones.RightLowerArm], "hand|wrist");

			//左足
			m_boneList[HumanBodyBones.LeftUpperLeg] = FindBone(HumanBodyBones.LeftUpperLeg, m_boneList[HumanBodyBones.Hips], "upper|leg", LEFT);
			m_boneList[HumanBodyBones.LeftLowerLeg] = FindBone(HumanBodyBones.LeftLowerLeg, m_boneList[HumanBodyBones.LeftUpperLeg], "lower|knee");
			m_boneList[HumanBodyBones.LeftFoot] = FindBone(HumanBodyBones.LeftFoot, m_boneList[HumanBodyBones.LeftLowerLeg], "foot|ankle");
			m_boneList[HumanBodyBones.LeftToes] = FindBone(HumanBodyBones.LeftToes, m_boneList[HumanBodyBones.LeftFoot], "toe");

			//右足
			m_boneList[HumanBodyBones.RightUpperLeg] = FindBone(HumanBodyBones.RightUpperLeg, m_boneList[HumanBodyBones.Hips], "upper|leg", RIGHT);
			m_boneList[HumanBodyBones.RightLowerLeg] = FindBone(HumanBodyBones.RightLowerLeg, m_boneList[HumanBodyBones.RightUpperLeg], "lower|knee");
			m_boneList[HumanBodyBones.RightFoot] = FindBone(HumanBodyBones.RightFoot, m_boneList[HumanBodyBones.RightLowerLeg], "foot|ankle");
			m_boneList[HumanBodyBones.RightToes] = FindBone(HumanBodyBones.RightToes, m_boneList[HumanBodyBones.RightFoot], "toe");

			if (m_boneList[HumanBodyBones.Spine] == null ||
				m_boneList[HumanBodyBones.Chest] == null ||
				m_boneList[HumanBodyBones.LeftShoulder] == null ||
				m_boneList[HumanBodyBones.RightShoulder] == null ||
				m_boneList[HumanBodyBones.LeftUpperLeg] == null ||
				m_boneList[HumanBodyBones.RightUpperLeg] == null)
				m_dressBoneWarn = true && !m_isHair;

			SetDefaultQuaternion();
		}

		void SetDefaultQuaternion()
		{
			armRotateZ.Value = 0;
			armRotateY.Value = 0;
			hipsPosY.Value = 0;
			hipsPosZ.Value = 0;
			legRotateZ.Value = 0;
			legRotate.Value = 0;
			armScaleX.Value = 1;
			armScaleY.Value = 1;
			hipScaleX.Value = 1;
			legScaleX.Value = 1;
			legScaleY.Value = 1;
			spineRotate.Value = 0;

			if (GetTransform(HumanBodyBones.LeftUpperArm) != null)
				m_defaultLArmQuat = GetTransform(HumanBodyBones.LeftUpperArm).rotation;
			if (GetTransform(HumanBodyBones.RightUpperArm) != null)
				m_defaultRArmQuat = GetTransform(HumanBodyBones.RightUpperArm).rotation;

			if (GetTransform(HumanBodyBones.Hips) != null)
				m_defaultHipsPos = GetTransform(HumanBodyBones.Hips).position;
			if (GetTransform(HumanBodyBones.Spine) != null)
				m_defaultSpineQuat = GetTransform(HumanBodyBones.Spine).rotation;

			if (GetTransform(HumanBodyBones.LeftUpperLeg) != null)
				m_defaultLLegQuat = GetTransform(HumanBodyBones.LeftUpperLeg).rotation;
			if (GetTransform(HumanBodyBones.RightUpperLeg) != null)
				m_defaultRLegQuat = GetTransform(HumanBodyBones.RightUpperLeg).rotation;
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
