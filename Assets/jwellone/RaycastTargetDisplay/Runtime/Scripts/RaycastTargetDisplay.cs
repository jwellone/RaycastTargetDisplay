using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable enable

namespace jwellone
{
    [RequireComponent(typeof(CanvasRenderer)), DisallowMultipleComponent, DefaultExecutionOrder(32000)]
    public sealed class RaycastTargetDisplay : Graphic
    {
#if UNITY_EDITOR
        const int SELECTED_MAX_COUNT = 1;
#else
        const int SELECTED_MAX_COUNT = 5;
#endif

        [SerializeField] RaycastTargetDisplayType.Type _displayType;
        [SerializeField] float _lineWidth = 2f;
        [SerializeField] Color _selectColor = Color.yellow;

        IReadOnlyList<BaseRaycaster>? _cacheRaycasters;
        readonly Vector3[] _corners = new Vector3[4];
        readonly UIVertex[] _vertices = new UIVertex[4];
        readonly GameObject?[] _selectedObjects = new GameObject[SELECTED_MAX_COUNT];
        readonly List<Component> _components = new List<Component>();
        readonly List<Graphic> _graphics = new List<Graphic>();
        readonly List<Vector2> _touchPoints = new List<Vector2>();

        bool _isPlaying
        {
            get
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    return true;
                }
#endif
                return Application.isPlaying;
            }
        }

        int touchCount
        {
            get
            {
#if UNITY_EDITOR
                return (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) ? 1 : 0;
#else
                return Input.touchCount;
#endif
            }
        }

        IReadOnlyList<BaseRaycaster> _raycasters
        {
            get
            {
#if UNITY_EDITOR
                if (!_isPlaying)
                {
                    return FindObjectsOfType<BaseRaycaster>().Where(x => x.isActiveAndEnabled).ToList();
                }
#endif

                _cacheRaycasters ??= typeof(BaseRaycaster)
                    .Assembly
                    .GetType("UnityEngine.EventSystems.RaycasterManager")
                    .GetMethod("GetRaycasters", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod)
                    .Invoke(null, Array.Empty<object>()) as IReadOnlyList<BaseRaycaster>;

                return _cacheRaycasters!;
            }
        }

        public RaycastTargetDisplayType.Type displayType
        {
            get => _displayType;
            set => _displayType = value;
        }

        public float lineWidth
        {
            get => _lineWidth;
            set => _lineWidth = Mathf.Max(0.01f, value);
        }

        public Color selectColor
        {
            get => _selectColor;
            set => _selectColor = value;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var corners = _corners;
            foreach (var graphic in _graphics)
            {
                var setColor = _selectedObjects.Any(o => o == graphic.gameObject) ? _selectColor : color;
                for (var i = 0; i < _vertices.Length; ++i)
                {
                    _vertices[i].color = setColor;
                }

                var targetCamera = graphic.canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : graphic.canvas.worldCamera;
                GetCorners(graphic, ref corners, targetCamera);

                if (_displayType == RaycastTargetDisplayType.Type.Fill)
                {
                    _vertices[0].position = corners[0];
                    _vertices[1].position = corners[1];
                    _vertices[2].position = corners[2];
                    _vertices[3].position = corners[3];
                    vh.AddUIVertexQuad(_vertices);
                    continue;
                }

                var cornerOffsets = new Vector3[4]
                {
                    ((corners[1] - corners[0]).normalized + (corners[3] - corners[0]).normalized) * _lineWidth,
                    ((corners[0] - corners[1]).normalized + (corners[2] - corners[1]).normalized) * _lineWidth,
                    ((corners[1] - corners[2]).normalized + (corners[3] - corners[2]).normalized) * _lineWidth,
                    ((corners[2] - corners[3]).normalized + (corners[0] - corners[3]).normalized) * _lineWidth
                };

                for (var k = 0; k < corners.Length; ++k)
                {
                    var nextIndex = (k + 1) % corners.Length;
                    var offset0 = cornerOffsets[k];
                    var offset1 = cornerOffsets[nextIndex];
                    _vertices[0].position = corners[k] - offset0;
                    _vertices[1].position = corners[k] + offset1;
                    _vertices[2].position = corners[nextIndex] + offset1;
                    _vertices[3].position = corners[nextIndex] - offset0;
                    vh.AddUIVertexQuad(_vertices);
                }
            }
        }

        void LateUpdate()
        {
            if (!_isPlaying)
            {
                ResetSelectObjects();

                _touchPoints.Clear();

                if (_graphics.Count > 0)
                {
                    _graphics.Clear();
                    SetVerticesDirty();
                }
                return;
            }

            UpdateGraphics();

            SetVerticesDirty();

            var eventSystem = EventSystem.current;
            var count = Math.Min(touchCount, _selectedObjects.Length);
            if (eventSystem == null || count == 0)
            {
                ResetSelectObjects();
            }

            var pointer = new PointerEventData(eventSystem);
            var rayCastRults = new List<RaycastResult>();
            for (var i = 0; i < count; ++i)
            {
                rayCastRults.Clear();
                var touch = GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    pointer.position = touch.position;
                    eventSystem!.RaycastAll(pointer, rayCastRults);

                    if (rayCastRults.Count > 0)
                    {
                        if (rayCastRults.Any(t => t.gameObject == eventSystem.currentSelectedGameObject))
                        {
                            _selectedObjects[touch.fingerId] = eventSystem.currentSelectedGameObject;
                        }
                        else
                        {
                            _selectedObjects[touch.fingerId] = rayCastRults[0].gameObject;
                        }
                    }
                }
                else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                }
                else
                {
                    _selectedObjects[touch.fingerId] = null;
                }
            }
        }

        void UpdateGraphics()
        {
            _graphics.Clear();

            foreach (var raycaster in _raycasters)
            {
                if (!raycaster.TryGetComponent<Canvas>(out var canvas))
                {
                    continue;
                }

                if (Culling(canvas.planeDistance, canvas.worldCamera))
                {
                    continue;
                }

                var graphics = GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas);
                if (graphics == null)
                {
                    continue;
                }

                for (var i = 0; i < graphics.Count; ++i)
                {
                    var graphic = graphics[i];
                    if (CanInteractable(graphic))
                    {
                        _graphics.Add(graphic);
                    }
                }
            }

            _graphics.Sort((a, b) => b.depth.CompareTo(a.depth));
        }

        bool CanInteractable(Graphic target)
        {
            if (!target.raycastTarget || target.canvasRenderer.cull || target.depth == -1)
            {
                return false;
            }

            var parent = target.transform.parent;
            while (parent != null)
            {
                _components.Clear();
                parent.GetComponents(_components);
                for (var i = 0; i < _components.Count; ++i)
                {
                    var component = _components[i];
                    if (component.TryGetComponent<CanvasGroup>(out var canvasGroup))
                    {
                        if (!canvasGroup.enabled || !canvasGroup.interactable || !canvasGroup.blocksRaycasts)
                        {
                            return false;
                        }
                    }
                    else if (component.TryGetComponent<Selectable>(out var selectable))
                    {
                        if (!selectable.enabled || !selectable.interactable)
                        {
                            return false;
                        }
                    }
                }

                parent = parent.parent;
            }

            _components.Clear();

            return !Culling(target.transform, target.canvas.worldCamera);
        }

        bool Culling(Transform target, Camera camera)
        {
            if (camera == null)
            {
                return false;
            }

            var screenPoint = camera.WorldToScreenPoint(target.transform.position);
            return screenPoint.z < camera.nearClipPlane || screenPoint.z > camera.farClipPlane;
        }

        bool Culling(float z, Camera camera)
        {
            return camera == null ? false : z < camera.nearClipPlane || z > camera.farClipPlane;
        }

        void ResetSelectObjects()
        {
            for (var i = 0; i < _selectedObjects.Length; ++i)
            {
                _selectedObjects[i] = null;
            }
        }

        Touch GetTouch(int index)
        {
#if UNITY_EDITOR
            var touch = new Touch();

            if (Input.GetMouseButtonDown(index))
            {
                touch.phase = TouchPhase.Began;
                touch.position = Input.mousePosition;
                touch.fingerId = index;
            }
            else if (Input.GetMouseButton(index))
            {
                touch.phase = TouchPhase.Stationary;
                touch.position = Input.mousePosition;
                touch.fingerId = index;
            }
            else if (Input.GetMouseButtonUp(index))
            {
                touch.phase = TouchPhase.Ended;
                touch.position = Input.mousePosition;
                touch.fingerId = index;
            }

            return touch;
#else
            return Input.GetTouch(index);
#endif
        }

        void GetCorners(Graphic graphic, ref Vector3[] corners, Camera? targetCamera)
        {
            var targetRectTransform = graphic.rectTransform;
            var padding = graphic.raycastPadding;

            targetRectTransform.GetLocalCorners(corners);
            corners[0].x += padding.x;
            corners[0].y += padding.y;
            corners[1].x += padding.x;
            corners[1].y -= padding.w;
            corners[2].x -= padding.z;
            corners[2].y -= padding.w;
            corners[3].x -= padding.z;
            corners[3].y += padding.y;

            var canvasRectTransform = graphic.canvas.gameObject.GetComponent<RectTransform>();
            var localToWorldMatrix = targetRectTransform.localToWorldMatrix;
            for (var k = 0; k < corners.Length; ++k)
            {
                var point = RectTransformUtility.WorldToScreenPoint(
                    targetCamera,
                    localToWorldMatrix.MultiplyPoint(corners[k]));

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform,
                    point,
                    targetCamera,
                    out point);

                corners[k].x = point.x;
                corners[k].y = point.y;
            }
        }

#if UNITY_EDITOR
        [SerializeField] bool _showSceneView = true;

        void OnDrawGizmos()
        {
            if (_isPlaying || !_showSceneView)
            {
                return;
            }

            var targetCamera = UnityEditor.SceneView.lastActiveSceneView.camera;
            var tmpColor = Gizmos.color;
            Gizmos.color = color;

            UpdateGraphics();

            var corners = _corners;
            foreach (var graphic in _graphics)
            {
                GetCorners(graphic, ref corners, targetCamera);

                var canvasRectTransform = graphic.canvas.gameObject.GetComponent<RectTransform>();
                if (graphic.canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    var offsetX = canvasRectTransform.sizeDelta.x / 2f;
                    var offsetY = canvasRectTransform.sizeDelta.y / 2f;
                    for (var k = 0; k < corners.Length; ++k)
                    {
                        corners[k].x += offsetX;
                        corners[k].y += offsetY;
                    }
                }
                else
                {
                    var localScale = canvasRectTransform.localScale;
                    for (var k = 0; k < corners.Length; ++k)
                    {
                        corners[k].x *= localScale.x;
                        corners[k].y *= localScale.y;
                        corners[k].z *= localScale.z;
                    }
                }

                Gizmos.DrawLineStrip(corners, true);
            }

            _graphics.Clear();

            Gizmos.color = tmpColor;
        }
#endif
    }
}