using UnityEngine;
using System.Collections.Generic;

namespace CR
{
	public class FPSFingerPresetRotator : MonoBehaviour
	{
		public Animator m_Animator;

		[System.Serializable]
		public class FingerBoneData
		{
			public string m_FingerName;
			public Transform m_Bone;
			public Vector3 m_LocalEulerRotation;
		}

		public bool m_IsActive = true;
		public bool m_ShowHandles = true;
		public List<FingerBoneData> m_FingerBones = new List<FingerBoneData>();

		private void LateUpdate()
		{
			if (!m_IsActive) return;

			foreach (var finger in m_FingerBones)
			{
				if (finger.m_Bone != null)
				{
					finger.m_Bone.localRotation = Quaternion.Euler(finger.m_LocalEulerRotation);
				}
			}
		}

		[ContextMenu("Setup Default 10 Fingers")]
		private void SetupDefaultFingers()
		{
			m_FingerBones.Clear();
			string[] names = { 
				"Left Thumb", "Left Index", "Left Middle", "Left Ring", "Left Pinky",
				"Right Thumb", "Right Index", "Right Middle", "Right Ring", "Right Pinky" 
			};
			
			foreach (var n in names)
			{
				m_FingerBones.Add(new FingerBoneData { m_FingerName = n });
			}
		}

		#region Persistence
		[System.Serializable]
		public class FingerBoneJsonData
		{
			public string finger_name;
			public Vector3 local_euler_rotation;
		}

		[System.Serializable]
		public class FingerPresetJsonData
		{
			public List<FingerBoneJsonData> finger_bones;
		}

		public string ExportToJson()
		{
			FingerPresetJsonData data = new FingerPresetJsonData { finger_bones = new List<FingerBoneJsonData>() };
			foreach (var bone in m_FingerBones)
			{
				data.finger_bones.Add(new FingerBoneJsonData
				{
					finger_name = bone.m_FingerName,
					local_euler_rotation = bone.m_LocalEulerRotation
				});
			}
			return JsonUtility.ToJson(data, true);
		}

		public void ImportFromJson(string json)
		{
			if (string.IsNullOrEmpty(json)) return;
			try
			{
				FingerPresetJsonData data = JsonUtility.FromJson<FingerPresetJsonData>(json);
				if (data != null && data.finger_bones != null)
				{
					foreach (var boneJson in data.finger_bones)
					{
						var target = m_FingerBones.Find(x => x.m_FingerName == boneJson.finger_name);
						if (target != null)
						{
							target.m_LocalEulerRotation = boneJson.local_euler_rotation;
						}
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError("FPSFingerPresetRotator: Failed to import JSON: " + e.Message);
			}
		}
		#endregion
	}
}
