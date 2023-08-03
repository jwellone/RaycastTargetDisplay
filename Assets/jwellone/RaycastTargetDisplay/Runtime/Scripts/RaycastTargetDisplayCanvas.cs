using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace jwellone
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
    public sealed class RaycastTargetDisplayCanvas : MonoBehaviour
    {
        static RaycastTargetDisplayCanvas? _instance;

        [SerializeField] bool _isDontDestroyOnLoad = true;

        RaycastTargetDisplay? _display;
        Canvas? _canvas;

        public static RaycastTargetDisplayType.Type displayType
        {
            get => _instance?._display!.displayType ?? RaycastTargetDisplayType.Type.Line;
            set
            {
                if (_instance != null)
                {
                    _instance!._display!.displayType = value;
                }
            }
        }

        public static float lineWidth
        {
            get => _instance?._display?.lineWidth ?? 0f;
            set
            {
                if (_instance != null)
                {
                    _instance!._display!.lineWidth = value;
                }
            }
        }

        public static Color color
        {
            get => _instance?._display!.color ?? Color.white;
            set
            {
                if (_instance != null)
                {
                    _instance!._display!.color = value;
                }
            }
        }

        public static Color selectColor
        {
            get => _instance?._display!.selectColor ?? Color.white;
            set
            {
                if (_instance != null)
                {
                    _instance!._display!.selectColor = value;
                }
            }
        }

        public static Canvas? canvas
        {
            get => _instance?._canvas;
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _display ??= GetComponentInChildren<RaycastTargetDisplay>();
            _canvas ??= GetComponent<Canvas>();

            if (_isDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        void OnApplicationQuit()
        {
            _instance = null;
        }
    }
}