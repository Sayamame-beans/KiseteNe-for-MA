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
		Vector3 m_armRotate = Vector3.zero;
		Vector3 m_armScale = Vector3.one;
		Vector3 m_hipsPos = Vector3.zero;
		Vector3 m_hipScale = Vector3.one;
		Vector3 m_legRotate = Vector3.zero;
		Vector3 m_legScale = Vector3.one;

		float m_SpineRotate = 0;

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
			CreateButtonUI(ref m_hipsPos.y, 0.0f);
			m_hipsPos.y = EditorGUILayout.Slider(m_hipsPos.y, -1, 1);

			GUILayout.Space(5);

			GUILayout.Label("前後");
			CreateButtonUI(ref m_hipsPos.z, 0.0f);
			m_hipsPos.z = EditorGUILayout.Slider(m_hipsPos.z, -1, 1);

			if (EditorGUI.EndChangeCheck()) {
				var hips = GetTransform(HumanBodyBones.Hips);
				hips.position = m_defaultHipsPos + m_hipsPos;
			}

			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();

			GUILayout.Label("拡大縮小");
			CreateButtonUI(ref m_hipScale.x, 1.0f);
			m_hipScale.x = EditorGUILayout.Slider(m_hipScale.x, 0.5f, 1.5f);
			if (EditorGUI.EndChangeCheck()) {
				m_hipScale.y = m_hipScale.z = m_hipScale.x;
				var hips = GetTransform(HumanBodyBones.Hips);
				hips.localScale = m_hipScale;
			}

			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();
			GUILayout.Label("お辞儀");
			CreateButtonUI(ref m_SpineRotate, 0.0f, 10);
			m_SpineRotate = EditorGUILayout.Slider(m_SpineRotate, -20, 20);
			if (EditorGUI.EndChangeCheck()) {
				var spine = GetTransform(HumanBodyBones.Spine);
				if (spine != null) {
					spine.rotation = m_defaultSpineQuat;
					spine.Rotate(spine.right, m_SpineRotate);
				}
			}
		}

		void CreateTopBodyUI()
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Label("腕を上げる");
			CreateButtonUI(ref m_armRotate.z, 0.0f, 10);
			m_armRotate.z = EditorGUILayout.Slider(m_armRotate.z, -50, 50);

			GUILayout.Space(5);

			GUILayout.Label("腕を前に出す");
			CreateButtonUI(ref m_armRotate.y, 0.0f, 10);
			m_armRotate.y = EditorGUILayout.Slider(m_armRotate.y, -15, 15);

			if (EditorGUI.EndChangeCheck()) {
				var left = GetTransform(HumanBodyBones.LeftUpperArm);
				if (left != null) {
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					left.rotation = m_defaultLArmQuat;

					left.Rotate(new Vector3(0, 0, 1), m_armRotate.z*-1, Space.World);
					left.Rotate(new Vector3(0, 1, 0), m_armRotate.y, Space.World);
				}

				var right = GetTransform(HumanBodyBones.RightUpperArm);
				if (right != null) {
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					right.rotation = m_defaultRArmQuat;

					right.Rotate(new Vector3(0, 0, 1), m_armRotate.z, Space.World);
					right.Rotate(new Vector3(0, 1, 0), m_armRotate.y * -1, Space.World);
				}
			}

			EditorGUI.BeginChangeCheck();

			GUILayout.Space(5);
			GUILayout.Label("袖を伸ばす");
			CreateButtonUI(ref m_armScale.y, 1.0f);
			m_armScale.y = EditorGUILayout.Slider(m_armScale.y, 0.5f, 1.5f);

			GUILayout.Space(5);
			GUILayout.Label("袖を太くする");
			CreateButtonUI(ref m_armScale.x, 1.0f);
			m_armScale.x = EditorGUILayout.Slider(m_armScale.x, 0.5f, 1.5f);

			if (EditorGUI.EndChangeCheck()) {
				var left = GetTransform(HumanBodyBones.LeftUpperArm);
				if (left != null) {
					if (Mathf.Abs(left.forward.y) > Mathf.Abs(left.forward.z)) {
						m_armScale.z = m_armScale.x;
						left.localScale = m_armScale;
					} else {
						//軸が違うのでxyを入れ替える
						if (left.forward.z > 0) {
							Vector3 tmpScale = new Vector3(m_armScale.y, m_armScale.x, m_armScale.x);
							left.localScale = tmpScale;
						} else {
							Vector3 tmpScale = new Vector3(m_armScale.x, m_armScale.y, m_armScale.x);
							left.localScale = tmpScale;
						}
					}
				}

				var right = GetTransform(HumanBodyBones.RightUpperArm);
				if (right != null) {
					if (Mathf.Abs(right.forward.y) > Mathf.Abs(right.forward.z)) {
						m_armScale.z = m_armScale.x;
						right.localScale = m_armScale;
					} else {
						//軸が違うのでxyを入れ替える
						if (right.forward.z > 0) {
							Vector3 tmpScale = new Vector3(m_armScale.y, m_armScale.x, m_armScale.x);
							right.localScale = tmpScale;
						} else {
							Vector3 tmpScale = new Vector3(m_armScale.x, m_armScale.y, m_armScale.x);
							right.localScale = tmpScale;
						}
					}
				}
			}
		}

		void CreateBottomBodyUI()
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Label("足を開く");
			CreateButtonUI(ref m_legRotate.z, 0.0f, 10);
			m_legRotate.z = EditorGUILayout.Slider(m_legRotate.z, -10, 10);

			GUILayout.Space(5);
			GUILayout.Label("足を前に出す");
			CreateButtonUI(ref m_legRotate.y, 0.0f, 10);
			m_legRotate.y = EditorGUILayout.Slider(m_legRotate.y, -10, 10);

			if (EditorGUI.EndChangeCheck()) {
				var left = GetTransform(HumanBodyBones.LeftUpperLeg);
				if (left != null) {
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					left.rotation = m_defaultLLegQuat;

					left.Rotate(left.forward, m_legRotate.z * -1);
					left.Rotate(left.right, m_legRotate.y * -1);
				}

				var right = GetTransform(HumanBodyBones.RightUpperLeg);
				if (right != null) {
					//0で0に戻りたいので、回す前にいったん初期値を入れる
					right.rotation = m_defaultRLegQuat;

					right.Rotate(right.forward, m_legRotate.z);
					right.Rotate(right.right, m_legRotate.y * -1);
				}
			}

			EditorGUI.BeginChangeCheck();

			GUILayout.Space(5);
			GUILayout.Label("裾を伸ばす");
			CreateButtonUI(ref m_legScale.y, 1.0f);
			m_legScale.y = EditorGUILayout.Slider(m_legScale.y, 0.5f, 1.5f);

			GUILayout.Space(5);
			GUILayout.Label("裾を太くする");
			CreateButtonUI(ref m_legScale.x, 1.0f);
			m_legScale.x = EditorGUILayout.Slider(m_legScale.x, 0.5f, 1.5f);

			if (EditorGUI.EndChangeCheck()) {
				m_legScale.z = m_legScale.x;
				var left = GetTransform(HumanBodyBones.LeftUpperLeg);
				var right = GetTransform(HumanBodyBones.RightUpperLeg);
				if (left != null)
					left.localScale = m_legScale;

				if (right != null)
					right.localScale = m_legScale;
			}
		}

		void CreateButtonUI(ref float setParam, float paramDefault, float paramRatio = 1.0f)
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("RESET"))
				setParam = paramDefault;

			if (GUILayout.Button("--", EditorStyles.miniButtonLeft, GUILayout.Height(20), GUILayout.Width(50)))
				setParam -= 0.01f * paramRatio;

			if (GUILayout.Button("-", EditorStyles.miniButtonMid, GUILayout.Height(20), GUILayout.Width(50)))
				setParam -= 0.001f * paramRatio;

			if (GUILayout.Button("+", EditorStyles.miniButtonMid, GUILayout.Height(20), GUILayout.Width(50)))
				setParam += 0.001f * paramRatio;

			if (GUILayout.Button("++", EditorStyles.miniButtonRight, GUILayout.Height(20), GUILayout.Width(50)))
				setParam += 0.01f * paramRatio;

			GUILayout.EndHorizontal();
		}

		void CreateHeadUI()
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Label("上下");
			CreateButtonUI(ref m_hipsPos.y, 0.0f);
			m_hipsPos.y = EditorGUILayout.Slider(m_hipsPos.y, -2, 2);

			GUILayout.Space(5);

			GUILayout.Label("前後");
			CreateButtonUI(ref m_hipsPos.z, 0.0f);
			m_hipsPos.z = EditorGUILayout.Slider(m_hipsPos.z, -1, 1);

			if (EditorGUI.EndChangeCheck()) {
				m_armature.transform.position = m_hipsPos;
			}

			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();

			GUILayout.Label("拡大縮小");
			CreateButtonUI(ref m_hipScale.x, 1.0f);
			m_hipScale.x = EditorGUILayout.Slider(m_hipScale.x, 0.5f, 2.0f);
			if (EditorGUI.EndChangeCheck()) {
				m_hipScale.y = m_hipScale.z = m_hipScale.x;
				m_armature.localScale = m_hipScale;
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
			m_armRotate = Vector3.zero;
			m_hipsPos = Vector3.zero;
			m_legRotate = Vector3.zero;
			m_armScale = Vector3.one;
			m_hipScale = Vector3.one;
			m_legScale = Vector3.one;
			m_SpineRotate = 0;

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
	}
}
