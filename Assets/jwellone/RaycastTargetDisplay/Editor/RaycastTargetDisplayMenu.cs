using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using jwellone;

#nullable enable

namespace jwelloneEditor
{
    static class RaycastTargetDisplayMenu
    {
        [MenuItem("GameObject/UI/jwellone/RaycastTargetDisplayCanvas")]
        static void OnCreate()
        {
            if (GameObject.FindObjectOfType<RaycastTargetDisplayCanvas>() != null)
            {
                Debug.LogWarning("RaycastTargetDisplayCanvas already exists.");
                return;
            }

            var owner = new GameObject("RaycastTargetDisplayCanvas").AddComponent<RaycastTargetDisplayCanvas>();
            var canvas = owner.gameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;

            var display = new GameObject("Display").AddComponent<RaycastTargetDisplay>();
            display.transform.SetParent(owner.transform, false);
            display.raycastTarget = false;
            display.color = Color.green;

            var rectTransform = display.rectTransform;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Undo.RegisterCreatedObjectUndo(owner.gameObject, "Create RaycastTargetDisplayCanvas");
        }
    }
}