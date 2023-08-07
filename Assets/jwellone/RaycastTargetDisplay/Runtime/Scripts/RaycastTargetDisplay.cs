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
            get
            {
                return _instance == null ? RaycastTargetDisplayType.Type.Line : _instance!._graphic!.displayType;
            }
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
            get
            {
                return _instance == null ? 0f : _instance!._graphic!.lineWidth;
            }
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
            get
            {
                return _instance == null ? Color.white : _instance!._graphic!.color;
            }
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
            get
            {
                return _instance == null ? Color.white : _instance!._graphic!.selectColor;
            }
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
            get
            {
                return _instance == null ? false : _instance!._graphic!.enabled;
            }
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