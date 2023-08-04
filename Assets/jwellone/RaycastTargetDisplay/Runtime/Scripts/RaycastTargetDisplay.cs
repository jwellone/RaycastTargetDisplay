using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace jwellone
{
    [AddComponentMenu("")]
    public sealed class RaycastTargetDisplay : CanvasScaler
    {
        static RaycastTargetDisplay? _instance;

        [SerializeField] bool _isDontDestroyOnLoad = true;
        [SerializeField] RaycastTargetDisplayGraphic? _graphic;

        public static RaycastTargetDisplayType.Type displayType
        {
            get => _instance?._graphic!.displayType ?? RaycastTargetDisplayType.Type.Line;
            set
            {
                if (_instance != null)
                {
                    _instance!._graphic!.displayType = value;
                }
            }
        }

        public static float lineWidth
        {
            get => _instance?._graphic?.lineWidth ?? 0f;
            set
            {
                if (_instance != null)
                {
                    _instance!._graphic!.lineWidth = value;
                }
            }
        }

        public static Color color
        {
            get => _instance?._graphic!.color ?? Color.white;
            set
            {
                if (_instance != null)
                {
                    _instance!._graphic!.color = value;
                }
            }
        }

        public static Color selectColor
        {
            get => _instance?._graphic!.selectColor ?? Color.white;
            set
            {
                if (_instance != null)
                {
                    _instance!._graphic!.selectColor = value;
                }
            }
        }

        public static bool enabledDisp
        {
            get => _instance?._graphic!.enabled ?? false;
            set
            {
                if (_instance != null)
                {
                    _instance!._graphic!.enabled = value;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (!Application.isPlaying)
            {
                return;
            }

            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _graphic ??= GetComponentInChildren<RaycastTargetDisplayGraphic>();

            if (_isDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

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