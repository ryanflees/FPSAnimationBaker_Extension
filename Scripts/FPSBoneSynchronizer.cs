using UnityEngine;
using System.Collections.Generic;

namespace CR
{
	[DefaultExecutionOrder(1010)] // Run after animations and basic IK
	public class FPSBoneSynchronizer : MonoBehaviour
	{
		[System.Serializable]
		public class BoneMapping
		{
			public Transform m_Source;
			public Transform m_Target;
		}

		public bool m_IsActive = true;
		public List<BoneMapping> m_Mappings = new List<BoneMapping>();

		private void LateUpdate()
		{
			if (!m_IsActive || m_Mappings == null) return;

			foreach (var mapping in m_Mappings)
			{
				if (mapping.m_Source != null && mapping.m_Target != null)
				{
					mapping.m_Target.localPosition = mapping.m_Source.localPosition;
					mapping.m_Target.localRotation = mapping.m_Source.localRotation;
				}
			}
		}

		public void AddMapping(Transform source, Transform target)
		{
			if (source == null || target == null) return;
			
			if (m_Mappings.Exists(m => m.m_Target == target)) return;

			m_Mappings.Add(new BoneMapping { m_Source = source, m_Target = target });
		}
	}
}
