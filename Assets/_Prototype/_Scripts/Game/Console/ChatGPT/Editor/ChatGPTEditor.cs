using UnityEditor;
using UnityEngine;

namespace ChatGPTWrapper {
  	[CustomEditor(typeof(ChatGPTConversation))]
  	public class ChatGPTEditor : Editor
  	{
		SerializedProperty _apiKey;
		SerializedProperty _model;
		SerializedProperty _maxTokens;
		SerializedProperty _temperature;
		SerializedProperty _chatbotName;
		SerializedProperty _initialPrompt;
		SerializedProperty chatGPTResponse;
		SerializedProperty _debugAPI;

		private void OnEnable() 
		{
			_apiKey = serializedObject.FindProperty("_apiKey");
			_model = serializedObject.FindProperty("_model");
			_maxTokens = serializedObject.FindProperty("_maxTokens");
			_temperature = serializedObject.FindProperty("_temperature");
			_chatbotName = serializedObject.FindProperty("_chatbotName");
			_initialPrompt = serializedObject.FindProperty("_initialPrompt");
			chatGPTResponse = serializedObject.FindProperty("chatGPTResponse");
			_debugAPI = serializedObject.FindProperty("_debugAPIFromFile");
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
			GUI.enabled = false;
			EditorGUILayout.PropertyField(_apiKey);
			GUI.enabled = true;
			EditorGUILayout.PropertyField(_model);

			if (_model.enumValueIndex != 0) {
				EditorGUILayout.PropertyField(_maxTokens);
				EditorGUILayout.PropertyField(_temperature);
			}
			EditorGUILayout.LabelField("Prompt", EditorStyles.boldLabel);
			if (_model.enumValueIndex != 0) {
				EditorGUILayout.PropertyField(_chatbotName);
			}
			GUI.enabled = false;
			EditorGUILayout.PropertyField(_initialPrompt);
			GUI.enabled = true;
			
			EditorGUILayout.Space(10);
			
			EditorGUILayout.PropertyField(chatGPTResponse);

			EditorGUILayout.PropertyField(_debugAPI);
			

			serializedObject.ApplyModifiedProperties();
		}
  	}
}