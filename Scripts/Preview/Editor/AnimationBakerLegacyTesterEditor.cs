using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace CR
{
    [CustomEditor(typeof(AnimationBakerLegacyTester))]
    public class AnimationBakerLegacyTesterEditor : Editor
    {
       private Dictionary<int, bool> m_LayerFoldouts = new Dictionary<int, bool>();
       private readonly Color m_HighlightColor = new Color(0f, 1f, 0f, 1f); 
       private readonly HashSet<string> m_ImportantStates = new HashSet<string> 
       { 
           "A_FP_PCH_Aim_In_Transition", 
           "A_FP_PCH_Aim_Out_Transition" 
       };
       
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AnimationBakerLegacyTester script = (AnimationBakerLegacyTester)target;

            if (script.m_SourceHands == null)
            {
                EditorGUILayout.HelpBox("Assign SourceFPSHands", MessageType.Warning);
                return;
            }
            
            var adsController = script.m_SourceHands.GetComponent<FPSADSParameterController>();
            if (adsController != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Live ADS Parameter (Base Layer BlendShape)", EditorStyles.boldLabel);
                
                Rect rect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.ProgressBar(rect, adsController.m_CurrentValue, $"ADS: {adsController.m_CurrentValue:F2}");
                
                EditorGUILayout.EndVertical();
                GUILayout.Space(10);
                
                if (Application.isPlaying || AnimationMode.InAnimationMode())
                {
                    Repaint();
                }
            }

            var cells = script.m_SourceHands.m_AnimationCellList;
            var groupedCells = new Dictionary<int, List<int>>();

            for (int i = 0; i < cells.Count; i++)
            {
                int layer = cells[i].m_Layer;
                if (!groupedCells.ContainsKey(layer))
                    groupedCells[layer] = new List<int>();
                
                groupedCells[layer].Add(i);
            }

            var sortedLayers = groupedCells.Keys.OrderBy(x => x).ToList();

            foreach (int layer in sortedLayers)
            {
                if (!m_LayerFoldouts.ContainsKey(layer))
                    m_LayerFoldouts[layer] = true;

                m_LayerFoldouts[layer] = EditorGUILayout.BeginFoldoutHeaderGroup(m_LayerFoldouts[layer], $"Layer {layer}");

                if (m_LayerFoldouts[layer])
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    List<int> indices = groupedCells[layer];
                    int buttonsPerRow = 2;

                    for (int j = 0; j < indices.Count; j++)
                    {
                        if (j % buttonsPerRow == 0) EditorGUILayout.BeginHorizontal();

                        int cellIndex = indices[j];
                        string cellName = cells[cellIndex].m_CellName;
                        
                        bool isImportant = m_ImportantStates.Contains(cellName);
                        if (isImportant)
                        {
                            GUI.color = m_HighlightColor; 
                        }

                        GUIStyle buttonStyle = isImportant ? new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold } : GUI.skin.button;

                        if (GUILayout.Button(cellName, buttonStyle, GUILayout.Height(30)))
                        {
                            SimulateSelectAndPreview(cellIndex, cellName);
                        }
                        if (isImportant)
                        {
                            GUI.color = Color.white; 
                        }

                        if ((j + 1) % buttonsPerRow == 0 || j == indices.Count - 1) EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                GUILayout.Space(5);
            }
            
        }

        private void SimulateSelectAndPreview(int index, string cellName)
        {
            AnimationBakerWindow window = EditorWindow.GetWindow<AnimationBakerWindow>("Animation Baker");
            if (window == null) return;

            System.Type windowType = typeof(AnimationBakerWindow);
            
            FieldInfo indexField = windowType.GetField("m_SelectAnimtionCellIndex", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo nameField = windowType.GetField("m_SelectAnimtionCell", BindingFlags.Instance | BindingFlags.NonPublic);
            
            MethodInfo previewMethod = windowType.GetMethod("PreviewAnimation", 
                BindingFlags.Instance | BindingFlags.NonPublic, 
                null, 
                System.Type.EmptyTypes, 
                null);

            if (indexField != null) indexField.SetValue(window, index);
            if (nameField != null) nameField.SetValue(window, cellName);

            if (previewMethod != null)
            {
                previewMethod.Invoke(window, null);
                window.Repaint();
            }
            else
            {
                Debug.LogError("反射失败：未在 AnimationBakerWindow 中找到 PreviewAnimation() 方法");
            }
        }
    }
}
