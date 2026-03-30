using UnityEngine;
using System.Collections;

namespace CR
{
    public class FPSADSParameterController : MonoBehaviour
    {
        [Header("Settings")]
        public Animator m_TargetAnimator;
        public string m_AdsParameterName = "ADS";
        public float m_TransitionDuration = 0.3f; 

        [Header("State Names")]
        public string m_AimInStateName = "A_FP_PCH_Aim_In_Transition";
        public string m_AimOutStateName = "A_FP_PCH_Aim_Out_Transition";

        private Coroutine m_TransitionCoroutine;
        [Header("Debug")]
        [Range(0, 1)]
        public float m_CurrentValue = 0f;

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

            if (cell.m_MainAnimatorState == m_AimInStateName)
            {
                StartAdsTransition(1f);
            }
            else if (cell.m_MainAnimatorState == m_AimOutStateName)
            {
                StartAdsTransition(0f);
            }
        }

        private void StartAdsTransition(float targetValue)
        {
            if (m_TransitionCoroutine != null)
            {
                StopCoroutine(m_TransitionCoroutine);
            }
            m_TransitionCoroutine = StartCoroutine(DoTransition(targetValue));
        }

        private IEnumerator DoTransition(float targetValue)
        {
            float startValue = m_CurrentValue;
            float timer = 0f;

            while (timer < m_TransitionDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / m_TransitionDuration);

                float easedT = -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;

                m_CurrentValue = Mathf.Lerp(startValue, targetValue, easedT);
                
                m_TargetAnimator.SetFloat(m_AdsParameterName, m_CurrentValue);
                yield return null;
            }

            m_CurrentValue = targetValue;
            m_TargetAnimator.SetFloat(m_AdsParameterName, m_CurrentValue);
            m_TransitionCoroutine = null;
        }
    }
}
