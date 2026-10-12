using UnityEngine;
using UnityEngine.UI;

namespace GeometryDash.UI
{
    /// <summary>
    /// Represents a single level card inside the Level Select Panel.
    /// Exposes inspector fields for preview image, level number label, lock overlay, lock icon, and button.
    /// </summary>
    public class LevelBoxUI : MonoBehaviour
    {
        [Header("CONFIGURATION")]
        public int levelNumber = 1;

        [Header("UI REFERENCES")]
        public Image previewImage;
        public Text levelNumberText;
        public GameObject lockOverlay;
        public GameObject lockIcon;
        public Button button;

        public void Setup(int number, Sprite previewSprite, bool isUnlocked, System.Action<int> onClicked)
        {
            levelNumber = number;

            if (previewImage != null && previewSprite != null)
            {
                previewImage.sprite = previewSprite;
                previewImage.color = isUnlocked ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f);
            }

            if (levelNumberText != null)
            {
                levelNumberText.text = $"LEVEL {number}";
            }

            if (lockOverlay != null)
            {
                lockOverlay.SetActive(!isUnlocked);
            }

            if (lockIcon != null)
            {
                lockIcon.SetActive(!isUnlocked);
            }

            if (button != null)
            {
                button.interactable = isUnlocked;
                button.onClick.RemoveAllListeners();
                if (isUnlocked && onClicked != null)
                {
                    button.onClick.AddListener(() => onClicked(levelNumber));
                }
            }
        }
    }
}
