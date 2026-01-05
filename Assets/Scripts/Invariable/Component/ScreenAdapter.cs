using UnityEngine;



namespace Invariable
{
    [ExecuteInEditMode]
    public class ScreenAdapter : MonoBehaviour
    {
        private RectTransform m_tsPanel;
        private int m_lastOrientation;



        private void Awake()
        {
            m_tsPanel = GetComponent<RectTransform>();
            m_lastOrientation = -1;
        }

        private void Update()
        {
            if (m_lastOrientation != (int)Screen.orientation)
            {
                ApplySafeArea();
            }
        }



        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea; // 原点在左下角

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            m_tsPanel.anchorMin = anchorMin;
            m_tsPanel.anchorMax = anchorMax;

            m_lastOrientation = (int)Screen.orientation;
        }
    }
}