using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Animations; 

namespace CR
{
	[CustomEditor(typeof(FPSAnimationBakerEventSubscriber))]
	public class FPSAnimationBakerEventSubscriberEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SourceHands"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TargetAnimator"));
			GUILayout.Space(10);

			FPSAnimationBakerEventSubscriber script = (FPSAnimationBakerEventSubscriber)target;

			SerializedProperty mappingsProp = serializedObject.FindProperty("m_Mappings");

			EditorGUILayout.LabelField("Animation Mappings", EditorStyles.boldLabel);
			for (int i = 0; i < mappingsProp.arraySize; i++)
			{
				SerializedProperty element = mappingsProp.GetArrayElementAtIndex(i);
				SerializedProperty sourceState = element.FindPropertyRelative("m_SourceStateName");
				SerializedProperty targetState = element.FindPropertyRelative("m_TargetStateName");

				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				
				EditorGUILayout.LabelField($"Source: {sourceState.stringValue}", EditorStyles.miniLabel);
				
				EditorGUILayout.BeginHorizontal();
				
				EditorGUILayout.PropertyField(targetState, new GUIContent("Target State"));

				if (GUILayout.Button("Select", GUILayout.Width(50)))
				{
					ShowStateSelector(script, targetState);
				}

				if (GUILayout.Button("X", GUILayout.Width(20)))
				{
					mappingsProp.DeleteArrayElementAtIndex(i);
				}

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}

			GUILayout.Space(10);

			if (GUILayout.Button("Generate Mappings From Source", GUILayout.Height(30)))
			{
				GenerateMappings(script);
			}
			
			if (GUILayout.Button("Clear Mappings By Empty Target", GUILayout.Height(30)))
			{
				for (int i = mappingsProp.arraySize - 1; i >= 0; i--)
				{
					SerializedProperty element = mappingsProp.GetArrayElementAtIndex(i);
					SerializedProperty targetState = element.FindPropertyRelative("m_TargetStateName");

					if (string.IsNullOrWhiteSpace(targetState.stringValue))
					{
						mappingsProp.DeleteArrayElementAtIndex(i);
					}
				}
				Debug.Log("FPSAnimationBakerEventSubscriber: Cleared all empty mappings.");
			}
			serializedObject.ApplyModifiedProperties();
		}

		private void ShowStateSelector(FPSAnimationBakerEventSubscriber script, SerializedProperty targetProperty)
		{
			if (script.m_TargetAnimator == null || script.m_TargetAnimator.runtimeAnimatorController == null)
			{
				Debug.LogWarning("FPSAnimationBakerEventSubscriber: TargetAnimator (or Controller) is missing!");
				return;
			}

			var runtimeController = script.m_TargetAnimator.runtimeAnimatorController;
			var editorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(runtimeController));

			if (editorController == null) return;

			GenericMenu menu = new GenericMenu();

			foreach (var layer in editorController.layers)
			{
				foreach (var stateInMachine in layer.stateMachine.states)
				{
					string stateName = stateInMachine.state.name;
					string displayName = $"{layer.name}/{stateName}";
					
					menu.AddItem(new GUIContent(displayName), targetProperty.stringValue == stateName, () =>
					{
						targetProperty.stringValue = stateName;
						targetProperty.serializedObject.ApplyModifiedProperties();
					});
				}
			}

			menu.ShowAsContext();
		}

		private void GenerateMappings(FPSAnimationBakerEventSubscriber script)
		{
			if (script.m_SourceHands == null)
			{
				Debug.LogWarning("FPSAnimationBakerEventSubscriber: SourceHands is not assigned!");
				return;
			}

			Undo.RecordObject(script, "Generate Animation Mappings");

			HashSet<string> existingStates = new HashSet<string>();
			foreach (var mapping in script.m_Mappings)
			{
				if (!string.IsNullOrEmpty(mapping.m_SourceStateName))
					existingStates.Add(mapping.m_SourceStateName);
			}

			foreach (var cell in script.m_SourceHands.m_AnimationCellList)
			{
				if (!existingStates.Contains(cell.m_MainAnimatorState))
				{
					script.m_Mappings.Add(new FPSAnimationBakerEventSubscriber.AnimationMapping
					{
						m_SourceStateName = cell.m_MainAnimatorState,
						m_TargetStateName = ""
					});
				}
			}

			EditorUtility.SetDirty(script);
			Debug.Log("FPSAnimationBakerEventSubscriber: Generated/Updated mappings from " + script.m_SourceHands.name);
		}
	}
}
