using UnityEngine;
using System.Collections.Generic;

namespace CR
{
	public class FPSAnimationBakerEventSubscriber : MonoBehaviour
	{
		public SourceFPSHands m_SourceHands;
		[SerializeField] public Animator m_TargetAnimator;
		
		[System.Serializable]
		public class AnimationMapping
		{
			public string m_SourceStateName;
			public string m_TargetStateName;
		}

		public List<AnimationMapping> m_Mappings = new List<AnimationMapping>();

		private void OnEnable()
		{
			AnimationBakerEvents.OnPreviewAnimation += OnAnimationPlayed;
			AnimationBakerEvents.OnPlayAndRecord += OnAnimationPlayed;
		}

		private void OnDisable()
		{
			AnimationBakerEvents.OnPreviewAnimation -= OnAnimationPlayed;
			AnimationBakerEvents.OnPlayAndRecord -= OnAnimationPlayed;
		}

		private void OnAnimationPlayed(AnimationCellData cell)
		{
			if (m_TargetAnimator == null || cell == null) return;

			Debug.Log("FPSAnimationBakerEventSubscriber: Received event for " + cell.m_MainAnimatorState);

			foreach (var mapping in m_Mappings)
			{
				if (mapping.m_SourceStateName == cell.m_MainAnimatorState)
				{
					if (!string.IsNullOrEmpty(mapping.m_TargetStateName))
					{
						m_TargetAnimator.Play(mapping.m_TargetStateName, 0, 0);
						Debug.Log("FPSAnimationBakerEventSubscriber: Playing additional action " + mapping.m_TargetStateName);
					}
					else
					{
						Debug.Log($"FPSAnimationBakerEventSubscriber: {mapping.m_TargetStateName} Target state is empty, skipping.");
					}
					break;
				}
			}
		}
	}
}
