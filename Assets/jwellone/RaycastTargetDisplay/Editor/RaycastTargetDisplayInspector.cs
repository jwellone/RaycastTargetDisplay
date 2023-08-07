using System.Reflection;
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

#if !UNITY_2022_1_OR_NEWER
        const BindingFlags BINDING_FLAGS = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
        const int CLASS_ID = 114;
        const string SCRIPT_CLASS_NAME = "RaycastTargetDisplayGraphic";
        const string EDITOR_USER_SETTINGS_KEY_FOR_GIZMO_ENABLED = "EDITOR_USER_SETTINGS_KEY_FOR_GIZMO_ENABLED";
        MethodInfo? _cacheSetGizmoEnabledMethodInfo;

        bool _gizmoEnabled
        {
	        get => EditorUserSettings.GetConfigValue(EDITOR_USER_SETTINGS_KEY_FOR_GIZMO_ENABLED) == "true";
	        set => EditorUserSettings.SetConfigValue(EDITOR_USER_SETTINGS_KEY_FOR_GIZMO_ENABLED, value ? "true" : "false");
        }

        MethodInfo? _setGizmoEnabledMethodInfo
        {
	        get
	        {
		        _cacheSetGizmoEnabledMethodInfo ??= Assembly.GetAssembly(typeof(Editor))
			        ?.GetType("UnityEditor.AnnotationUtility")
			        ?.GetMethod("SetGizmoEnabled", BINDING_FLAGS);
		        return _cacheSetGizmoEnabledMethodInfo;
	        }
        }
#endif

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

            if (TryGetGizmoEnabled(out var gizmoEnabled))
            {
                var result = EditorGUILayout.Toggle("Gizmo Enabled", gizmoEnabled);
                if (result != gizmoEnabled)
                {
                    SetGizmoEnabled(result);
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

        bool TryGetGizmoEnabled(out bool value)
        {
#if UNITY_2022_1_OR_NEWER
            if (GizmoUtility.TryGetGizmoInfo(typeof(RaycastTargetDisplayGraphic), out var info))
            {
                value = info.gizmoEnabled;
                return true;
            }

            value = false;
            return false;
#else
		    value = _gizmoEnabled;
		    return true;
#endif
        }

        void SetGizmoEnabled(bool value)
        {
#if UNITY_2022_1_OR_NEWER
            GizmoUtility.SetGizmoEnabled(typeof(RaycastTargetDisplayGraphic), value);
#else
		    _gizmoEnabled = value;
		    _setGizmoEnabledMethodInfo?.Invoke(
			    null,
			    new object[]
			    {
				    CLASS_ID,
				    SCRIPT_CLASS_NAME,
				    value ? 1 : 0,
				    false
			    });
#endif
        }
    }
}
