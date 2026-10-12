using UnityEngine;
using UnityEngine.UI;

namespace GeometryDash.UI
{
    /// <summary>
    /// Represents a single Cube skin card inside the Skin Selection Panel.
    /// Exposes inspector references for preview image, selection highlight border, and button.
    /// </summary>
    public class SkinBoxUI : MonoBehaviour
    {
        [Header("CONFIGURATION")]
        public int skinIndex = 0;

        [Header("UI REFERENCES")]
        public Image previewImage;
        public GameObject selectionHighlight;
        public Button button;

        public void Setup(int index, Sprite skinSprite, bool isSelected, System.Action<int> onClicked)
        {
            skinIndex = index;

            if (previewImage != null && skinSprite != null)
            {
                previewImage.sprite = skinSprite;
            }

            SetSelected(isSelected);

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (onClicked != null)
                {
                    button.onClick.AddListener(() => onClicked(skinIndex));
                }
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(isSelected);
            }
        }
    }
}
