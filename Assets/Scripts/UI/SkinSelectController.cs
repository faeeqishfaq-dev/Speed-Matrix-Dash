using UnityEngine;
using GeometryDash.Player;

namespace GeometryDash.UI
{
    /// <summary>
    /// Controls the Skin Selection Panel.
    /// Displays a grid of 10 Cube skin cards, manages selection state, and updates highlights.
    /// </summary>
    public class SkinSelectController : MonoBehaviour
    {
        [Header("SKIN BOXES (10 SKINS)")]
        [Tooltip("Direct references to all 10 SkinBoxUI cards in the scene hierarchy.")]
        public SkinBoxUI[] skinBoxes = new SkinBoxUI[10];

        [Header("SKIN SPRITES")]
        [Tooltip("Direct references to the 10 Cube skin sprites.")]
        public Sprite[] skinSprites = new Sprite[10];

        private UIScaleAnimator _animator;

        private void Awake()
        {
            _animator = GetComponent<UIScaleAnimator>();
            AutoPopulateIfEmpty();
        }

        private void OnEnable()
        {
            AutoPopulateIfEmpty();
            RefreshSkinGrid();
        }

        /// <summary>
        /// Automatically populates references from child hierarchy if not already assigned in Inspector.
        /// </summary>
        private void AutoPopulateIfEmpty()
        {
            if (skinBoxes == null || skinBoxes.Length < 10 || skinBoxes[0] == null)
            {
                SkinBoxUI[] foundBoxes = GetComponentsInChildren<SkinBoxUI>(true);
                if (foundBoxes != null && foundBoxes.Length > 0)
                {
                    System.Array.Sort(foundBoxes, (a, b) => a.skinIndex.CompareTo(b.skinIndex));
                    skinBoxes = foundBoxes;
                }
            }

            // Fallback sprite loading if not assigned
            if (skinSprites == null || skinSprites.Length < 10 || skinSprites[0] == null)
            {
                skinSprites = new Sprite[10];
                for (int i = 0; i < 10; i++)
                {
                    skinSprites[i] = PlayerSkinDatabase.GetSkinSprite(i);
                }
            }
        }

        /// <summary>
        /// Refreshes all 10 skin cards to display the current selection highlight.
        /// </summary>
        public void RefreshSkinGrid()
        {
            int selectedIndex = PlayerSkinDatabase.GetSelectedSkinIndex();

            for (int i = 0; i < skinBoxes.Length && i < 10; i++)
            {
                if (skinBoxes[i] == null) continue;

                Sprite sprite = (skinSprites != null && i < skinSprites.Length) ? skinSprites[i] : null;
                if (sprite == null) sprite = PlayerSkinDatabase.GetSkinSprite(i);

                bool isSelected = (i == selectedIndex);
                skinBoxes[i].Setup(i, sprite, isSelected, OnSkinClicked);
            }
        }

        /// <summary>
        /// Called when a skin card is clicked.
        /// Saves the selected skin index and updates highlights across all cards.
        /// </summary>
        public void OnSkinClicked(int index)
        {
            PlayerSkinDatabase.SetSelectedSkinIndex(index);
            CheckCpAd.instance.CallAd();

            for (int i = 0; i < skinBoxes.Length && i < 10; i++)
            {
                if (skinBoxes[i] == null) continue;
                skinBoxes[i].SetSelected(i == index);
            }
        }

        public void ClosePanel()
        {
            if (_animator != null)
            {
                _animator.Hide();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
