using UnityEditor;
using jwellone;

#nullable enable

namespace jwelloneEditor
{
    [CustomEditor(typeof(RaycastTargetDisplay))]
    class RaycastTargetDisplayInspector : Editor
    {
        static readonly string[] _propertyNames = new[]
        {
            "_displayType",
            "_showSceneView",
            "_lineWidth",
            "m_Color",
            "_selectColor",
        };

        readonly SerializedProperty?[] _serializedProperties = new SerializedProperty[_propertyNames.Length];

        void OnEnable()
        {
            for (var i = 0; i < _propertyNames.Length; ++i)
            {
                _serializedProperties[i] = serializedObject.FindProperty(_propertyNames[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            foreach (var property in _serializedProperties)
            {
                EditorGUILayout.PropertyField(property);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
