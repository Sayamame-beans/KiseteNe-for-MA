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
		[SerializeField] private GameObject dress;

		[SerializeField] private Transform armature;
		[SerializeField] private HumanBodyBonesToDictionaryMapping boneList = new HumanBodyBonesToDictionaryMapping();

		[SerializeField] private bool boneDetail = false;
		[SerializeField] private int selectedTabNumber = 0;
		[SerializeField] private Vector2 scrollPosition;
		[SerializeField] private bool isHair = false;
		[SerializeField] private bool dressBoneError = false;
		[SerializeField] private bool dressBoneWarn = false;

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

		//初期値保持
		[SerializeField] private Vector3 defaultHipsPos;
		[SerializeField] private Quaternion defaultLArmQuat;
		[SerializeField] private Quaternion defaultRArmQuat;
		[SerializeField] private Quaternion defaultSpineQuat;
		[SerializeField] private Quaternion defaultLLegQuat;
		[SerializeField] private Quaternion defaultRLegQuat;

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

		[MenuItem("Tools/KiseteNe for MA")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(KisekaeEditor), false, "KiseteNe for MA");
		}

		void OnGUI()
		{
			Undo.RecordObject(this, "KiseteNe for MA");
			GUILayout.Label("MA向け衣装調整支援ツール「キセテネ for MA」", EditorStyles.boldLabel);

			GUILayout.Label("服をセットしてください", EditorStyles.largeLabel);

			EditorGUI.BeginChangeCheck();
			dress = EditorGUILayout.ObjectField("服", dress, typeof(GameObject), true) as GameObject;
			if (EditorGUI.EndChangeCheck()) {
				dressBoneError = false;
				dressBoneWarn = false;
				isHair = false;
				UpdateBoneList();
			}

			if (dress == null)
				return;

			if (dressBoneError) {
				EditorGUILayout.HelpBox("服のボーンを取得することができませんでした\n上の欄にArmatureやメッシュを設定している場合は、服のルートオブジェクトを設定してください", MessageType.Error, true);
				return;
			}

			if (dressBoneWarn)
				EditorGUILayout.HelpBox("服のボーンを一部取得することができませんでした。\n調整時にうまく動かない場合、ボーン詳細設定を確認してください", MessageType.Warning, true);

			boneDetail = GUILayout.Toggle(boneDetail, "ボーン詳細設定");
			if (boneDetail) {
				CreateBoneSettingsUI();
			}

			GUILayout.Space(20);

			if (isHair) {
				GUILayout.Label("髪の調整です", EditorStyles.miniLabel);
				CreateHeadUI();
			} else {
				selectedTabNumber = GUILayout.Toolbar(selectedTabNumber, new string[] { "全体", "上半身", "下半身" }, EditorStyles.toolbarButton);
				switch (selectedTabNumber) {
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
				hips.position = defaultHipsPos + new Vector3(0, hipsPosY.Value, hipsPosZ.Value);
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
					spine.rotation = defaultSpineQuat;
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
					left.rotation = defaultLArmQuat;

					left.Rotate(new Vector3(0, 0, 1), armRotateZ.Value*-1, Space.World);
					left.Rotate(new Vector3(0, 1, 0), armRotateY.Value, Space.World);
				}

				var right = GetTransform(HumanBodyBones.RightUpperArm);
				if (right != null) {
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					right.rotation = defaultRArmQuat;

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
					left.rotation = defaultLLegQuat;

					left.Rotate(left.forward, legRotateZ.Value * -1);
					left.Rotate(left.right, legRotate.Value * -1);
				}

				var right = GetTransform(HumanBodyBones.RightUpperLeg);
				if (right != null) {
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					right.rotation = defaultRLegQuat;

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
				armature.transform.position = new Vector3(0, hipsPosY.Value, hipsPosZ.Value);
			}

			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();

			GUILayout.Label("拡大縮小");
			hipScaleX.ButtonAndSliderGui(1.0f, 0.5f, 2.0f);
			if (EditorGUI.EndChangeCheck()) {
				armature.localScale = new Vector3(hipScaleX.Value, hipScaleX.Value, hipScaleX.Value);
			}
		}

		private static readonly Regex ArmatureRegexPattern = new Regex("armature|root|skelton", RegexOptions.IgnoreCase);
		private static readonly Regex HipsPattern = new Regex("hip", RegexOptions.IgnoreCase);
		private static readonly Regex NeckPattern = new Regex("neck", RegexOptions.IgnoreCase);
		private static readonly Regex HeadPattern = new Regex("head", RegexOptions.IgnoreCase);
		private static readonly Regex SplinePattern = new Regex("spine", RegexOptions.IgnoreCase);
		private static readonly Regex ChestPattern = new Regex("chest", RegexOptions.IgnoreCase);
		private static readonly Regex UpperChestPattern = new Regex("upper", RegexOptions.IgnoreCase);
		private static readonly Regex ShoulderPattern = new Regex("shoulder", RegexOptions.IgnoreCase);
		private static readonly Regex UpperArmPattern = new Regex("upper|arm", RegexOptions.IgnoreCase);
		private static readonly Regex LowerArmPattern = new Regex("lower|elbow", RegexOptions.IgnoreCase);
		private static readonly Regex HandPattern = new Regex("hand|wrist", RegexOptions.IgnoreCase);
		private static readonly Regex UpperLegPattern = new Regex("upper|leg", RegexOptions.IgnoreCase);
		private static readonly Regex LowerLegPattern = new Regex("lower|knee", RegexOptions.IgnoreCase);
		private static readonly Regex FootPattern = new Regex("foot|ankle", RegexOptions.IgnoreCase);
		private static readonly Regex ToesPattern = new Regex("toe", RegexOptions.IgnoreCase);

		//セットされたものからボーン構造を作る
		void UpdateBoneList()
		{
			boneList.Clear();

			if (dress == null)
				return;

			armature = FindBone(HumanBodyBones.Hips, dress.transform, ArmatureRegexPattern);
			if (armature == null) {
				dressBoneError = true;
				return;
			}

			//Humanoidなら取れる限りとってみる
			var dressAnim = dress.GetComponent<Animator>();
			if (dressAnim != null && dressAnim.isHuman) {
				for (int i = (int)HumanBodyBones.Hips; i <= (int)HumanBodyBones.RightToes; i++)
					boneList[(HumanBodyBones)i] = dressAnim.GetBoneTransform((HumanBodyBones)i);
			}

			if(boneList[HumanBodyBones.Hips] == null)
				boneList[HumanBodyBones.Hips] = FindBone(HumanBodyBones.Hips,armature, HipsPattern);

			if (boneList[HumanBodyBones.Hips] == null) {
				//頭すげ替えか髪の毛用
				if (FindBone(HumanBodyBones.Neck, armature, NeckPattern)) {
					boneList[HumanBodyBones.Neck] = FindBone(HumanBodyBones.Neck, armature, NeckPattern);
					boneList[HumanBodyBones.Head] = FindBone(HumanBodyBones.Head, boneList[HumanBodyBones.Neck], HeadPattern);
					isHair = true;
				} else if (FindBone(HumanBodyBones.Head, armature, HeadPattern)) {
					boneList[HumanBodyBones.Head] = FindBone(HumanBodyBones.Head, armature, HeadPattern);
					isHair = true;
				} else {
					dressBoneError = true;
					return;
				}
			}

			dressBoneError = false;

			boneList[HumanBodyBones.Spine] = FindBone(HumanBodyBones.Spine, boneList[HumanBodyBones.Hips], SplinePattern);
			boneList[HumanBodyBones.Chest] = FindBone(HumanBodyBones.Chest, boneList[HumanBodyBones.Spine], ChestPattern);

			//UpperChestあったらHeadとShoulderはそっちから拾う
			var upperChest = FindBone(HumanBodyBones.UpperChest, boneList[HumanBodyBones.Chest], UpperChestPattern);
			boneList[HumanBodyBones.Neck] = FindBone(HumanBodyBones.Neck,
				(upperChest) ? upperChest : boneList[HumanBodyBones.Chest], NeckPattern);
			boneList[HumanBodyBones.Head] = FindBone(HumanBodyBones.Head, boneList[HumanBodyBones.Neck], HeadPattern);

			//左腕
			boneList[HumanBodyBones.LeftShoulder] = FindBone(HumanBodyBones.LeftShoulder,
				(upperChest) ? upperChest : boneList[HumanBodyBones.Chest], ShoulderPattern, Side.Left);
			boneList[HumanBodyBones.LeftUpperArm] = FindBone(HumanBodyBones.LeftUpperArm, boneList[HumanBodyBones.LeftShoulder], UpperArmPattern);
			boneList[HumanBodyBones.LeftLowerArm] = FindBone(HumanBodyBones.LeftLowerArm, boneList[HumanBodyBones.LeftUpperArm], LowerArmPattern);
			boneList[HumanBodyBones.LeftHand] = FindBone(HumanBodyBones.LeftHand, boneList[HumanBodyBones.LeftLowerArm], HandPattern);

			//右腕
			boneList[HumanBodyBones.RightShoulder] = FindBone(HumanBodyBones.RightShoulder,
				(upperChest) ? upperChest : boneList[HumanBodyBones.Chest], ShoulderPattern, Side.Right);
			boneList[HumanBodyBones.RightUpperArm] = FindBone(HumanBodyBones.RightUpperArm, boneList[HumanBodyBones.RightShoulder], UpperArmPattern);
			boneList[HumanBodyBones.RightLowerArm] = FindBone(HumanBodyBones.RightLowerArm, boneList[HumanBodyBones.RightUpperArm], LowerArmPattern);
			boneList[HumanBodyBones.RightHand] = FindBone(HumanBodyBones.RightHand, boneList[HumanBodyBones.RightLowerArm], HandPattern);

			//左足
			boneList[HumanBodyBones.LeftUpperLeg] = FindBone(HumanBodyBones.LeftUpperLeg, boneList[HumanBodyBones.Hips], UpperLegPattern, Side.Left);
			boneList[HumanBodyBones.LeftLowerLeg] = FindBone(HumanBodyBones.LeftLowerLeg, boneList[HumanBodyBones.LeftUpperLeg], LowerLegPattern);
			boneList[HumanBodyBones.LeftFoot] = FindBone(HumanBodyBones.LeftFoot, boneList[HumanBodyBones.LeftLowerLeg], FootPattern);
			boneList[HumanBodyBones.LeftToes] = FindBone(HumanBodyBones.LeftToes, boneList[HumanBodyBones.LeftFoot], ToesPattern);

			//右足
			boneList[HumanBodyBones.RightUpperLeg] = FindBone(HumanBodyBones.RightUpperLeg, boneList[HumanBodyBones.Hips], UpperLegPattern, Side.Right);
			boneList[HumanBodyBones.RightLowerLeg] = FindBone(HumanBodyBones.RightLowerLeg, boneList[HumanBodyBones.RightUpperLeg], LowerLegPattern);
			boneList[HumanBodyBones.RightFoot] = FindBone(HumanBodyBones.RightFoot, boneList[HumanBodyBones.RightLowerLeg], FootPattern);
			boneList[HumanBodyBones.RightToes] = FindBone(HumanBodyBones.RightToes, boneList[HumanBodyBones.RightFoot], ToesPattern);

			if (boneList[HumanBodyBones.Spine] == null ||
				boneList[HumanBodyBones.Chest] == null ||
				boneList[HumanBodyBones.LeftShoulder] == null ||
				boneList[HumanBodyBones.RightShoulder] == null ||
				boneList[HumanBodyBones.LeftUpperLeg] == null ||
				boneList[HumanBodyBones.RightUpperLeg] == null)
				dressBoneWarn = true && !isHair;

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
				defaultLArmQuat = GetTransform(HumanBodyBones.LeftUpperArm).rotation;
			if (GetTransform(HumanBodyBones.RightUpperArm) != null)
				defaultRArmQuat = GetTransform(HumanBodyBones.RightUpperArm).rotation;

			if (GetTransform(HumanBodyBones.Hips) != null)
				defaultHipsPos = GetTransform(HumanBodyBones.Hips).position;
			if (GetTransform(HumanBodyBones.Spine) != null)
				defaultSpineQuat = GetTransform(HumanBodyBones.Spine).rotation;

			if (GetTransform(HumanBodyBones.LeftUpperLeg) != null)
				defaultLLegQuat = GetTransform(HumanBodyBones.LeftUpperLeg).rotation;
			if (GetTransform(HumanBodyBones.RightUpperLeg) != null)
				defaultRLegQuat = GetTransform(HumanBodyBones.RightUpperLeg).rotation;
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
