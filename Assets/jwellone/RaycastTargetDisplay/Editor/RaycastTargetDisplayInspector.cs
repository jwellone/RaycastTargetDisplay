using UnityEditor;
using UnityEditor.UI;
using jwellone;

#nullable enable

namespace jwelloneEditor
{
    [CustomEditor(typeof(RaycastTargetDisplay))]
    class RaycastTargetDisplayInspector : CanvasScalerEditor
    {
        static readonly string[] _propertyNames = new[]
        {
            "_displayType",
            "_lineWidth",
            "_colors",
            "_selectColor",
        };

        SerializedProperty? _isDontDestroyOnLoadProperty;
        SerializedProperty? _gizmoColorProperty;
        SerializedObject? _diplaySerializeObject;
        readonly SerializedProperty?[] _serializedProperties = new SerializedProperty[_propertyNames.Length];

        protected override void OnEnable()
        {
            base.OnEnable();

            _isDontDestroyOnLoadProperty = serializedObject.FindProperty("_isDontDestroyOnLoad");

            var instance = (RaycastTargetDisplay)target;
            var graphic = instance.gameObject.GetComponentInChildren<RaycastTargetDisplayGraphic>();

            _diplaySerializeObject = new SerializedObject(graphic);

            _gizmoColorProperty = _diplaySerializeObject.FindProperty("_gizmoColor");

            for (var i = 0; i < _propertyNames.Length; ++i)
            {
                var propertyName = _propertyNames[i];
                _serializedProperties[i] = _diplaySerializeObject.FindProperty(propertyName);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            _diplaySerializeObject!.Update();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_isDontDestroyOnLoadProperty);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[GIZMO]");
            ++EditorGUI.indentLevel;
            if (GizmoUtility.TryGetGizmoInfo(typeof(RaycastTargetDisplayGraphic), out var info))
            {
                var gizmoEnabled = EditorGUILayout.Toggle("Gizmo Enabled", info.gizmoEnabled);
                if (info.gizmoEnabled != gizmoEnabled)
                {
                    GizmoUtility.SetGizmoEnabled(typeof(RaycastTargetDisplayGraphic), gizmoEnabled);
                }
            }

            EditorGUILayout.PropertyField(_gizmoColorProperty);
            --EditorGUI.indentLevel;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[DISPLAY]");
            ++EditorGUI.indentLevel;
            foreach (var property in _serializedProperties)
            {
                EditorGUILayout.PropertyField(property);
            }
            --EditorGUI.indentLevel;

            _diplaySerializeObject!.ApplyModifiedProperties();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
