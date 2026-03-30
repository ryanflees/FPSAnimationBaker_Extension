using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CR
{
	[CustomEditor(typeof(FPSFingerPresetRotator))]
	public class FPSFingerPresetRotatorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			FPSFingerPresetRotator script = (FPSFingerPresetRotator)target;

			GUILayout.Space(10);
			if (GUILayout.Button("Auto-Set ALL Finger Bones (30 Bones)", GUILayout.Height(30)))
			{
				AutoSetAllFingers(script);
			}

			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Copy Data to Clipboard"))
			{
				EditorGUIUtility.systemCopyBuffer = script.ExportToJson();
				Debug.Log("FPSFingerPresetRotator: Data copied to clipboard.");
			}
			if (GUILayout.Button("Paste Data from Clipboard"))
			{
				Undo.RecordObject(script, "Paste Finger Data");
				script.ImportFromJson(EditorGUIUtility.systemCopyBuffer);
				EditorUtility.SetDirty(script);
				Debug.Log("FPSFingerPresetRotator: Data pasted from clipboard.");
			}
			EditorGUILayout.EndHorizontal();
		}

		private void OnSceneGUI()
		{
			FPSFingerPresetRotator script = (FPSFingerPresetRotator)target;
			if (!script.m_IsActive || !script.m_ShowHandles || script.m_FingerBones == null) return;

			foreach (var finger in script.m_FingerBones)
			{
				if (finger.m_Bone == null) continue;

				Vector3 pos = finger.m_Bone.position;
				Quaternion parentRot = finger.m_Bone.parent != null ? finger.m_Bone.parent.rotation : Quaternion.identity;
				Quaternion currentWorldRot = parentRot * Quaternion.Euler(finger.m_LocalEulerRotation);
				float handleSize = HandleUtility.GetHandleSize(pos) * 0.2f;

				EditorGUI.BeginChangeCheck();
				Quaternion newWorldRot = currentWorldRot;

				if (Tools.current == Tool.Rotate)
				{
					float discSize = handleSize * 1.2f;
					
					Handles.color = Color.red;
					newWorldRot = Handles.Disc(newWorldRot, pos, currentWorldRot * Vector3.right, discSize, true, 1f);
					Handles.color = Color.green;
					newWorldRot = Handles.Disc(newWorldRot, pos, currentWorldRot * Vector3.up, discSize, true, 1f);
					Handles.color = Color.blue;
					newWorldRot = Handles.Disc(newWorldRot, pos, currentWorldRot * Vector3.forward, discSize, true, 1f);
					Handles.color = Color.white;
				}
				else
				{
					newWorldRot = Handles.FreeRotateHandle(currentWorldRot, pos, handleSize);
				}

				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(script, "Rotate Finger Bone");
					
					Quaternion newLocalRot = Quaternion.Inverse(parentRot) * newWorldRot;
					finger.m_LocalEulerRotation = newLocalRot.eulerAngles;

					finger.m_Bone.localRotation = newLocalRot;
					EditorUtility.SetDirty(script);
				}
			}
		}

		private void AutoSetAllFingers(FPSFingerPresetRotator script)
		{
			if (script.m_Animator == null)
			{
				script.m_Animator = script.GetComponent<Animator>();
			}
			
			if (script.m_Animator == null)
			{
				Debug.LogWarning("FPSFingerPresetRotator: Animator is not assigned!");
				return;
			}

			if (!script.m_Animator.isHuman)
			{
				Debug.LogWarning("FPSFingerPresetRotator: Animator does not have a Humanoid avatar!");
				return;
			}

			Undo.RecordObject(script, "Auto Set All Finger Bones");

			script.m_FingerBones.Clear();

			AddFingerChain(script, "Left Thumb", HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal);
			AddFingerChain(script, "Left Index", HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal);
			AddFingerChain(script, "Left Middle", HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal);
			AddFingerChain(script, "Left Ring", HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal);
			AddFingerChain(script, "Left Pinky", HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal);

			AddFingerChain(script, "Right Thumb", HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal);
			AddFingerChain(script, "Right Index", HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal);
			AddFingerChain(script, "Right Middle", HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal);
			AddFingerChain(script, "Right Ring", HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal);
			AddFingerChain(script, "Right Pinky", HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal);

			EditorUtility.SetDirty(script);
			Debug.Log("FPSFingerPresetRotator: Successfully set 30 finger bones from " + script.m_Animator.name);
		}

		private void AddFingerChain(FPSFingerPresetRotator script, string fingerName, params HumanBodyBones[] bones)
		{
			string[] suffixes = { " Proximal", " Intermediate", " Distal" };
			for (int i = 0; i < bones.Length; i++)
			{
				Transform bone = script.m_Animator.GetBoneTransform(bones[i]);
				if (bone != null)
				{
					script.m_FingerBones.Add(new FPSFingerPresetRotator.FingerBoneData
					{
						m_FingerName = fingerName + suffixes[i],
						m_Bone = bone,
						m_LocalEulerRotation = bone.localEulerAngles
					});
				}
			}
		}
	}
}
