/*
Copyright(c) 2020 Tomoshibi/Tomoya
https://tomo-shi-vi.hateblo.jp/
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Sayabeans.KiseteNeForMA.Editor
{
	internal partial class KisekaeEditor : EditorWindow
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
		[SerializeField] private FloatUndoState upperArmRotateZ;
		[SerializeField] private FloatUndoState upperArmRotateY;
		[SerializeField] private FloatUndoState hipsPosY;
		[SerializeField] private FloatUndoState hipsPosZ;
		[SerializeField] private FloatUndoState upperLegRotateZ;
		[SerializeField] private FloatUndoState upperLegRotateY;
		[SerializeField] private FloatUndoState spineRotate;
		[SerializeField] private FloatUndoState upperArmScaleY;
		[SerializeField] private FloatUndoState upperArmScaleX;
		[SerializeField] private FloatUndoState hipScale;
		[SerializeField] private FloatUndoState upperLegScaleX;
		[SerializeField] private FloatUndoState upperLegScaleY;

		//初期値保持
		[SerializeField] private Vector3 defaultHipsPos;
		[SerializeField] private Vector3 defaultUArmScale;
		[SerializeField] private Vector3 defaultHipsScale;
		[SerializeField] private Vector3 defaultULegScale;
		[SerializeField] private Quaternion defaultULArmQuat;
		[SerializeField] private Quaternion defaultURArmQuat;
		[SerializeField] private Quaternion defaultSpineQuat;
		[SerializeField] private Quaternion defaultULLegQuat;
		[SerializeField] private Quaternion defaultURLegQuat;

		private void OnEnable()
		{
			// Only add callback once.
			Undo.undoRedoPerformed -= Repaint;
			Undo.undoRedoPerformed += Repaint;
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= Repaint;
		}

		private const string UndoGroupName = "KiseteNe for MA";

		[MenuItem("Tools/KiseteNe for MA")]
		private static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(KisekaeEditor), false, "KiseteNe for MA");
		}

		private void OnGUI()
		{
			Undo.RecordObject(this, UndoGroupName);
			GUILayout.Label("MA向け衣装調整支援ツール「キセテネ for MA」", EditorStyles.boldLabel);

			GUILayout.Label("服をセットしてください", EditorStyles.largeLabel);

			EditorGUI.BeginChangeCheck();
			dress = EditorGUILayout.ObjectField("服", dress, typeof(GameObject), true) as GameObject;
			if (EditorGUI.EndChangeCheck())
			{
				dressBoneError = false;
				dressBoneWarn = false;
				isHair = false;
				UpdateBoneList();
			}

			if (dress == null)
				return;

			if (dressBoneError)
			{
				EditorGUILayout.HelpBox("服のボーンを取得することができませんでした\n上の欄にArmatureやメッシュを設定している場合は、服のルートオブジェクトを設定してください", MessageType.Error, true);
				return;
			}

			if (dressBoneWarn)
				EditorGUILayout.HelpBox("服のボーンを一部取得することができませんでした。\n調整時にうまく動かない場合、ボーン詳細設定を確認してください", MessageType.Warning, true);

			boneDetail = GUILayout.Toggle(boneDetail, "ボーン詳細設定");
			if (boneDetail)
			{
				CreateBoneSettingsUI();
			}

			GUILayout.Space(20);
			LazyCollapseUndoOperations collapse = default;

			if (isHair)
			{
				GUILayout.Label("髪の調整です", EditorStyles.miniLabel);
				CreateHeadUI(ref collapse);
			}
			else
			{
				selectedTabNumber = GUILayout.Toolbar(selectedTabNumber, new string[] { "全体", "上半身", "下半身" }, EditorStyles.toolbarButton);
				switch (selectedTabNumber)
				{
					case 0:
						GUILayout.Label("全体の調整です", EditorStyles.miniLabel);
						CreateFullBodyUI(ref collapse);
						break;
					case 1:
						GUILayout.Label("腕周りの調整です", EditorStyles.miniLabel);
						CreateTopBodyUI(ref collapse);
						break;
					case 2:
						GUILayout.Label("足周りの調整です。スカートには影響がないものもあります", EditorStyles.miniLabel);
						CreateBottomBodyUI(ref collapse);
						break;
				}
			}

			GUILayout.Space(20);
			CreateFileUI(ref collapse);

			collapse.CollapseIfRequested();
		}

		private void CreateFullBodyUI(ref LazyCollapseUndoOperations collapse)
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Label("上下");
			hipsPosY.ButtonAndSliderGui(ref collapse, 0.0f, -1, 1);

			GUILayout.Space(5);

			GUILayout.Label("前後");
			hipsPosZ.ButtonAndSliderGui(ref collapse, 0.0f, -1, 1);

			if (EditorGUI.EndChangeCheck())
			{
				var hips = GetTransform(HumanBodyBones.Hips);
				Undo.RecordObject(hips, UndoGroupName);
				hips.position = defaultHipsPos + new Vector3(0, hipsPosY.Value, hipsPosZ.Value);
			}

			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();

			GUILayout.Label("拡大縮小");
			hipScale.ButtonAndSliderGui(ref collapse, 1.0f, 0.5f, 1.5f);
			if (EditorGUI.EndChangeCheck())
			{
				var hips = GetTransform(HumanBodyBones.Hips);
				Undo.RecordObject(hips, UndoGroupName);
				hips.localScale = defaultHipsScale * hipScale.Value;
			}

			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();
			GUILayout.Label("お辞儀");
			spineRotate.ButtonAndSliderGui(ref collapse, 0.0f, -20, 20, paramRatio: 10);
			if (EditorGUI.EndChangeCheck())
			{
				var spine = GetTransform(HumanBodyBones.Spine);
				if (spine != null)
				{
					Undo.RecordObject(spine, UndoGroupName);
					spine.rotation = defaultSpineQuat;
					spine.Rotate(spine.right, spineRotate.Value);
				}
			}
		}

		private void CreateTopBodyUI(ref LazyCollapseUndoOperations collapse)
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Label("腕を上げる");
			upperArmRotateZ.ButtonAndSliderGui(ref collapse, 0.0f, -50, 50, paramRatio: 10);

			GUILayout.Space(5);

			GUILayout.Label("腕を前に出す");
			upperArmRotateY.ButtonAndSliderGui(ref collapse, 0.0f, -15, 15, paramRatio: 10);

			if (EditorGUI.EndChangeCheck())
			{
				var left = GetTransform(HumanBodyBones.LeftUpperArm);
				if (left != null)
				{
					Undo.RecordObject(left, UndoGroupName);
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					left.rotation = defaultULArmQuat;

					left.Rotate(new Vector3(0, 0, 1), upperArmRotateZ.Value * -1, Space.World);
					left.Rotate(new Vector3(0, 1, 0), upperArmRotateY.Value, Space.World);
				}

				var right = GetTransform(HumanBodyBones.RightUpperArm);
				if (right != null)
				{
					Undo.RecordObject(right, UndoGroupName);
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					right.rotation = defaultURArmQuat;

					right.Rotate(new Vector3(0, 0, 1), upperArmRotateZ.Value, Space.World);
					right.Rotate(new Vector3(0, 1, 0), upperArmRotateY.Value * -1, Space.World);
				}
			}

			EditorGUI.BeginChangeCheck();

			GUILayout.Space(5);
			GUILayout.Label("袖を伸ばす");
			upperArmScaleY.ButtonAndSliderGui(ref collapse, 1.0f, 0.5f, 1.5f);

			GUILayout.Space(5);
			GUILayout.Label("袖を太くする");
			upperArmScaleX.ButtonAndSliderGui(ref collapse, 1.0f, 0.5f, 1.5f);

			if (EditorGUI.EndChangeCheck())
			{
				var left = GetTransform(HumanBodyBones.LeftUpperArm);
				var right = GetTransform(HumanBodyBones.RightUpperArm);
				var armScale = Vector3.Scale(defaultUArmScale, new Vector3(upperArmScaleX.Value, upperArmScaleY.Value, upperArmScaleX.Value));
				if (left != null)
				{
					Undo.RecordObject(left, UndoGroupName);
					left.localScale = armScale;
				}

				if (right != null)
				{
					Undo.RecordObject(right, UndoGroupName);
					right.localScale = armScale;
				}
			}
		}

		private void CreateBottomBodyUI(ref LazyCollapseUndoOperations collapse)
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Label("足を開く");
			upperLegRotateZ.ButtonAndSliderGui(ref collapse, 0.0f, -10, 10, paramRatio: 10);

			GUILayout.Space(5);
			GUILayout.Label("足を前に出す");
			upperLegRotateY.ButtonAndSliderGui(ref collapse, 0.0f, -10, 10, paramRatio: 10);

			if (EditorGUI.EndChangeCheck())
			{
				var left = GetTransform(HumanBodyBones.LeftUpperLeg);
				if (left != null)
				{
					Undo.RecordObject(left, UndoGroupName);
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					left.rotation = defaultULLegQuat;

					left.Rotate(left.forward, upperLegRotateZ.Value * -1);
					left.Rotate(left.right, upperLegRotateY.Value * -1);
				}

				var right = GetTransform(HumanBodyBones.RightUpperLeg);
				if (right != null)
				{
					Undo.RecordObject(right, UndoGroupName);
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					right.rotation = defaultURLegQuat;

					right.Rotate(right.forward, upperLegRotateZ.Value);
					right.Rotate(right.right, upperLegRotateY.Value * -1);
				}
			}

			EditorGUI.BeginChangeCheck();

			GUILayout.Space(5);
			GUILayout.Label("裾を伸ばす");
			upperLegScaleY.ButtonAndSliderGui(ref collapse, 1.0f, 0.5f, 1.5f);

			GUILayout.Space(5);
			GUILayout.Label("裾を太くする");
			upperLegScaleX.ButtonAndSliderGui(ref collapse, 1.0f, 0.5f, 1.5f);

			if (EditorGUI.EndChangeCheck())
			{
				var left = GetTransform(HumanBodyBones.LeftUpperLeg);
				var right = GetTransform(HumanBodyBones.RightUpperLeg);
				var legScale = Vector3.Scale(defaultULegScale, new Vector3(upperLegScaleX.Value, upperLegScaleY.Value, upperLegScaleX.Value));
				if (left != null)
				{
					Undo.RecordObject(left, UndoGroupName);
					left.localScale = legScale;
				}

				if (right != null)
				{
					Undo.RecordObject(right, UndoGroupName);
					right.localScale = legScale;
				}
			}
		}

		private void CreateHeadUI(ref LazyCollapseUndoOperations collapse)
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Label("上下");
			hipsPosY.ButtonAndSliderGui(ref collapse, 0.0f, -2, 2);

			GUILayout.Space(5);

			GUILayout.Label("前後");
			hipsPosZ.ButtonAndSliderGui(ref collapse, 0.0f, -1, 1);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(armature, UndoGroupName);
				armature.position = defaultHipsPos + new Vector3(0, hipsPosY.Value, hipsPosZ.Value);
			}

			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();

			GUILayout.Label("拡大縮小");
			hipScale.ButtonAndSliderGui(ref collapse, 1.0f, 0.5f, 2.0f);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(armature, UndoGroupName);
				armature.localScale = defaultHipsScale * hipScale.Value;
			}
		}

		private void CreateFileUI(ref LazyCollapseUndoOperations collapse)
		{
			GUILayout.Label("ファイル保存/読み込み");
			if (GUILayout.Button("ファイルに調整値を保存する"))
			{
				SaveToJson();
			}

			if (GUILayout.Button("ファイルから調整値を(現在の状態として)読み込む"))
			{
				LoadFromJson(ref collapse);
			}

			if (GUILayout.Button("ファイルから調整値を読み込み、新たに適用する"))
			{
				ApplyFromJson(ref collapse);
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
		private void UpdateBoneList()
		{
			boneList.Clear();

			if (dress == null)
				return;

			armature = FindBone(HumanBodyBones.Hips, dress.transform, ArmatureRegexPattern);
			if (armature == null)
			{
				dressBoneError = true;
				return;
			}

			//Humanoidなら取れる限りとってみる
			var dressAnim = dress.GetComponent<Animator>();
			if (dressAnim != null && dressAnim.isHuman)
			{
				for (int i = (int)HumanBodyBones.Hips; i <= (int)HumanBodyBones.RightToes; i++)
					boneList[(HumanBodyBones)i] = dressAnim.GetBoneTransform((HumanBodyBones)i);
			}

			if(boneList[HumanBodyBones.Hips] == null)
				boneList[HumanBodyBones.Hips] = FindBone(HumanBodyBones.Hips,armature, HipsPattern);

			if (boneList[HumanBodyBones.Hips] == null)
			{
				//頭すげ替えか髪の毛用
				if (FindBone(HumanBodyBones.Neck, armature, NeckPattern))
				{
					boneList[HumanBodyBones.Neck] = FindBone(HumanBodyBones.Neck, armature, NeckPattern);
					boneList[HumanBodyBones.Head] = FindBone(HumanBodyBones.Head, boneList[HumanBodyBones.Neck], HeadPattern);
					isHair = true;
				}
				else if (FindBone(HumanBodyBones.Head, armature, HeadPattern))
				{
					boneList[HumanBodyBones.Head] = FindBone(HumanBodyBones.Head, armature, HeadPattern);
					isHair = true;
				}
				else
				{
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

			SetDefault();
		}

		private void SetDefault()
		{
			upperArmRotateZ.Value = 0;
			upperArmRotateY.Value = 0;
			hipsPosY.Value = 0;
			hipsPosZ.Value = 0;
			upperLegRotateZ.Value = 0;
			upperLegRotateY.Value = 0;
			spineRotate.Value = 0;
			upperArmScaleY.Value = 1;
			upperArmScaleX.Value = 1;
			hipScale.Value = 1;
			upperLegScaleX.Value = 1;
			upperLegScaleY.Value = 1;

			var leftUpperArm = GetTransform(HumanBodyBones.LeftUpperArm);
			if (leftUpperArm != null)
			{
				defaultUArmScale = leftUpperArm.localScale;
				defaultULArmQuat = leftUpperArm.rotation;
			}

			var rightUpperArm = GetTransform(HumanBodyBones.RightUpperArm);
			if (rightUpperArm != null)
			{
				if (defaultUArmScale == Vector3.zero)
					defaultUArmScale = rightUpperArm.localScale;
				defaultURArmQuat = rightUpperArm.rotation;
			}

			var hips = GetTransform(HumanBodyBones.Hips);
			if (hips != null)
			{
				defaultHipsPos = hips.position;
				defaultHipsScale = hips.localScale;
			}

			if (GetTransform(HumanBodyBones.Spine) != null)
				defaultSpineQuat = GetTransform(HumanBodyBones.Spine).rotation;

			var leftUpperLeg = GetTransform(HumanBodyBones.LeftUpperLeg);
			if (leftUpperLeg != null)
			{
				defaultULegScale = leftUpperLeg.localScale;
				defaultULLegQuat = leftUpperLeg.rotation;
			}

			var rightUpperLeg = GetTransform(HumanBodyBones.RightUpperLeg);
			if (rightUpperLeg != null)
			{
				if (defaultULegScale == Vector3.zero)
					defaultULegScale = rightUpperLeg.localScale;
				defaultURLegQuat = rightUpperLeg.rotation;
			}
		}

		private void SaveToJson()
		{
			string filePath = EditorUtility.SaveFilePanel("[KiseteNe for MA] 調整値を保存", "Assets/", "edits.json", "json");
			if (filePath == "")
				return;

			var data = new SaveableData();
			data.upperArmRotateZ = upperArmRotateZ.Value;
			data.upperArmRotateY = upperArmRotateY.Value;
			data.hipsPosY = hipsPosY.Value;
			data.hipsPosZ = hipsPosZ.Value;
			data.upperLegRotateZ = upperLegRotateZ.Value;
			data.upperLegRotateY = upperLegRotateY.Value;
			data.spineRotate = spineRotate.Value;
			data.upperArmScaleY = upperArmScaleY.Value;
			data.upperArmScaleX = upperArmScaleX.Value;
			data.hipScale = hipScale.Value;
			data.upperLegScaleX = upperLegScaleX.Value;
			data.upperLegScaleY = upperLegScaleY.Value;

			try
			{
				using (StreamWriter sw = new StreamWriter(filePath))
				{
					sw.Write(JsonUtility.ToJson(data));
				}
			}
			catch (Exception)
			{
				EditorUtility.DisplayDialog("[KiseteNe for MA] 調整値を保存", "ファイル書き込み時にエラーが発生しました。\n詳細はConsoleを確認してください。", "OK");
				throw;
			}
		}

		private void LoadFromJson(ref LazyCollapseUndoOperations collapse)
		{
			var filePath = EditorUtility.OpenFilePanel("[KiseteNe for MA] 調整値を読み込む", "Assets/", "json");
			if (filePath == "")
				return;

			var collapseGroupId = Undo.GetCurrentGroup();
			SaveableData data = new SaveableData();

			try
			{
				using (StreamReader sr = new StreamReader(filePath))
				{
					JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), data);
				}
			}
			catch (Exception)
			{
				EditorUtility.DisplayDialog("[KiseteNe for MA] 調整値を読み込む", "ファイル読み込み時にエラーが発生しました。\n詳細はConsoleを確認してください。", "OK");
				throw;
			}

			if (!float.IsNaN(data.upperArmRotateZ))
				upperArmRotateZ.Value = data.upperArmRotateZ;
			if (!float.IsNaN(data.upperArmRotateY))
				upperArmRotateY.Value = data.upperArmRotateY;
			if (!float.IsNaN(data.hipsPosY))
				hipsPosY.Value = data.hipsPosY;
			if (!float.IsNaN(data.hipsPosZ))
				hipsPosZ.Value = data.hipsPosZ;
			if (!float.IsNaN(data.upperLegRotateZ))
				upperLegRotateZ.Value = data.upperLegRotateZ;
			if (!float.IsNaN(data.upperLegRotateY))
				upperLegRotateY.Value = data.upperLegRotateY;
			if (!float.IsNaN(data.spineRotate))
				spineRotate.Value = data.spineRotate;
			if (!float.IsNaN(data.upperArmScaleY))
				upperArmScaleY.Value = data.upperArmScaleY;
			if (!float.IsNaN(data.upperArmScaleX))
				upperArmScaleX.Value = data.upperArmScaleX;
			if (!float.IsNaN(data.hipScale))
				hipScale.Value = data.hipScale;
			if (!float.IsNaN(data.upperLegScaleX))
				upperLegScaleX.Value = data.upperLegScaleX;
			if (!float.IsNaN(data.upperLegScaleY))
				upperLegScaleY.Value = data.upperLegScaleY;

			if (isHair)
			{
				defaultHipsPos = armature.position - new Vector3(0, hipsPosY.Value, hipsPosZ.Value);;
				defaultHipsScale = armature.localScale / hipScale.Value;
			}
			else
			{
				var leftUpperArm = GetTransform(HumanBodyBones.LeftUpperArm);
				if (leftUpperArm != null)
				{
					defaultUArmScale = new Vector3(leftUpperArm.localScale.x / upperArmScaleX.Value, leftUpperArm.localScale.y / upperArmScaleY.Value, leftUpperArm.localScale.z / upperArmScaleX.Value);
					defaultULArmQuat = leftUpperArm.rotation * Quaternion.AngleAxis(upperArmRotateY.Value * -1, leftUpperArm.InverseTransformDirection(new Vector3(0, 1, 0))) * Quaternion.AngleAxis(upperArmRotateZ.Value, leftUpperArm.InverseTransformDirection(new Vector3(0, 0, 1)));
				}

				var rightUpperArm = GetTransform(HumanBodyBones.RightUpperArm);
				if (rightUpperArm != null)
				{
					if (defaultUArmScale == Vector3.zero)
						defaultUArmScale = new Vector3(rightUpperArm.localScale.x / upperArmScaleX.Value, rightUpperArm.localScale.y / upperArmScaleY.Value, rightUpperArm.localScale.z / upperArmScaleX.Value);
					defaultURArmQuat = rightUpperArm.rotation * Quaternion.AngleAxis(upperArmRotateY.Value, rightUpperArm.InverseTransformDirection(new Vector3(0, 1, 0))) * Quaternion.AngleAxis(upperArmRotateZ.Value * -1, rightUpperArm.InverseTransformDirection(new Vector3(0, 0, 1)));
				}

				var hips = GetTransform(HumanBodyBones.Hips);
				if (hips != null)
				{
					defaultHipsPos = hips.position - new Vector3(0, hipsPosY.Value, hipsPosZ.Value);;
					defaultHipsScale = hips.localScale / hipScale.Value;
				}

				var spine = GetTransform(HumanBodyBones.Spine);
				if (spine != null)
					defaultSpineQuat = spine.rotation * Quaternion.AngleAxis(spineRotate.Value * -1, spine.right);

				var leftUpperLeg = GetTransform(HumanBodyBones.LeftUpperLeg);
				if (leftUpperLeg != null)
				{
					defaultULegScale = new Vector3(leftUpperLeg.localScale.x / upperLegScaleX.Value, leftUpperLeg.localScale.y / upperLegScaleY.Value, leftUpperLeg.localScale.z / upperLegScaleX.Value);
					defaultULLegQuat = leftUpperLeg.rotation * Quaternion.AngleAxis(upperLegRotateY.Value, leftUpperLeg.right) * Quaternion.AngleAxis(upperLegRotateZ.Value, leftUpperLeg.forward);
				}

				var rightUpperLeg = GetTransform(HumanBodyBones.RightUpperLeg);
				if (rightUpperLeg != null)
				{
					if (defaultUArmScale == Vector3.zero)
						defaultULegScale = new Vector3(rightUpperLeg.localScale.x / upperLegScaleX.Value, rightUpperLeg.localScale.y / upperLegScaleY.Value, rightUpperLeg.localScale.z / upperLegScaleX.Value);
					defaultURLegQuat = rightUpperLeg.rotation * Quaternion.AngleAxis(upperLegRotateY.Value, rightUpperLeg.right) * Quaternion.AngleAxis(upperLegRotateZ.Value * -1, rightUpperLeg.forward);
				}
			}

			collapse.RequestCollapse(collapseGroupId);
		}

		private void ApplyFromJson(ref LazyCollapseUndoOperations collapse)
		{
			var filePath = EditorUtility.OpenFilePanel("[KiseteNe for MA] 調整値を読み込んで適用", "Assets/", "json");
			if (filePath == "")
				return;

			var collapseGroupId = Undo.GetCurrentGroup();
			SaveableData data = new SaveableData();

			try
			{
				using (StreamReader sr = new StreamReader(filePath))
				{
					JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), data);
				}
			}
			catch (Exception)
			{
				EditorUtility.DisplayDialog("[KiseteNe for MA] 調整値を読み込む", "ファイル読み込み時にエラーが発生しました。\n詳細はConsoleを確認してください。", "OK");
				throw;
			}

			if (!float.IsNaN(data.upperArmRotateZ))
				upperArmRotateZ.Value = data.upperArmRotateZ;
			if (!float.IsNaN(data.upperArmRotateY))
				upperArmRotateY.Value = data.upperArmRotateY;
			if (!float.IsNaN(data.hipsPosY))
				hipsPosY.Value = data.hipsPosY;
			if (!float.IsNaN(data.hipsPosZ))
				hipsPosZ.Value = data.hipsPosZ;
			if (!float.IsNaN(data.upperLegRotateZ))
				upperLegRotateZ.Value = data.upperLegRotateZ;
			if (!float.IsNaN(data.upperLegRotateY))
				upperLegRotateY.Value = data.upperLegRotateY;
			if (!float.IsNaN(data.spineRotate))
				spineRotate.Value = data.spineRotate;
			if (!float.IsNaN(data.upperArmScaleY))
				upperArmScaleY.Value = data.upperArmScaleY;
			if (!float.IsNaN(data.upperArmScaleX))
				upperArmScaleX.Value = data.upperArmScaleX;
			if (!float.IsNaN(data.hipScale))
				hipScale.Value = data.hipScale;
			if (!float.IsNaN(data.upperLegScaleX))
				upperLegScaleX.Value = data.upperLegScaleX;
			if (!float.IsNaN(data.upperLegScaleY))
				upperLegScaleY.Value = data.upperLegScaleY;

			if (isHair)
			{
				Undo.RecordObject(armature, UndoGroupName);
				armature.position = defaultHipsPos + new Vector3(0, hipsPosY.Value, hipsPosZ.Value);
				armature.localScale = defaultHipsScale * hipScale.Value;
			}
			else
			{
				var hips = GetTransform(HumanBodyBones.Hips);
				Undo.RecordObject(hips, UndoGroupName);
				hips.position = defaultHipsPos + new Vector3(0, hipsPosY.Value, hipsPosZ.Value);
				hips.localScale = defaultHipsScale * hipScale.Value;

				var spine = GetTransform(HumanBodyBones.Spine);
				if (spine != null)
				{
					Undo.RecordObject(spine, UndoGroupName);
					spine.rotation = defaultSpineQuat;
					spine.Rotate(spine.right, spineRotate.Value);
				}

				var leftUpperArm = GetTransform(HumanBodyBones.LeftUpperArm);
				if (leftUpperArm != null)
				{
					Undo.RecordObject(leftUpperArm, UndoGroupName);
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					leftUpperArm.rotation = defaultULArmQuat;
					leftUpperArm.Rotate(new Vector3(0, 0, 1), upperArmRotateZ.Value * -1, Space.World);
					leftUpperArm.Rotate(new Vector3(0, 1, 0), upperArmRotateY.Value, Space.World);
					leftUpperArm.localScale = Vector3.Scale(defaultUArmScale, new Vector3(upperArmScaleX.Value, upperArmScaleY.Value, upperArmScaleX.Value));
				}

				var rightUpperArm = GetTransform(HumanBodyBones.RightUpperArm);
				if (rightUpperArm != null)
				{
					Undo.RecordObject(rightUpperArm, UndoGroupName);
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					rightUpperArm.rotation = defaultURArmQuat;
					rightUpperArm.Rotate(new Vector3(0, 0, 1), upperArmRotateZ.Value, Space.World);
					rightUpperArm.Rotate(new Vector3(0, 1, 0), upperArmRotateY.Value * -1, Space.World);
					rightUpperArm.localScale = Vector3.Scale(defaultUArmScale, new Vector3(upperArmScaleX.Value, upperArmScaleY.Value, upperArmScaleX.Value));
				}

				var leftUpperLeg = GetTransform(HumanBodyBones.LeftUpperLeg);
				if (leftUpperLeg != null)
				{
					Undo.RecordObject(leftUpperLeg, UndoGroupName);
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					leftUpperLeg.rotation = defaultULLegQuat;
					leftUpperLeg.Rotate(leftUpperLeg.forward, upperLegRotateZ.Value * -1);
					leftUpperLeg.Rotate(leftUpperLeg.right, upperLegRotateY.Value * -1);
					leftUpperLeg.localScale = Vector3.Scale(defaultULegScale, new Vector3(upperLegScaleX.Value, upperLegScaleY.Value, upperLegScaleX.Value));
				}

				var rightUpperLeg = GetTransform(HumanBodyBones.RightUpperLeg);
				if (rightUpperLeg != null)
				{
					Undo.RecordObject(rightUpperLeg, UndoGroupName);
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					rightUpperLeg.rotation = defaultURLegQuat;
					rightUpperLeg.Rotate(rightUpperLeg.forward, upperLegRotateZ.Value);
					rightUpperLeg.Rotate(rightUpperLeg.right, upperLegRotateY.Value * -1);
					rightUpperLeg.localScale = Vector3.Scale(defaultULegScale, new Vector3(upperLegScaleX.Value, upperLegScaleY.Value, upperLegScaleX.Value));
				}
			}

			collapse.RequestCollapse(collapseGroupId);
		}
	}
}
