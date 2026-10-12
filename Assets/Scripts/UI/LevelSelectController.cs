using UnityEngine;
using UnityEngine.SceneManagement;
using GeometryDash.Level;

namespace GeometryDash.UI
{
    /// <summary>
    /// Controls the Level Select Panel.
    /// Manages unlocking states, preview card updates, and starting gameplay at the selected level.
    /// </summary>
    public class LevelSelectController : MonoBehaviour
    {
        [Header("LEVEL BOXES (15 LEVELS)")]
        [Tooltip("Direct references to all 15 LevelBoxUI cards in the scene hierarchy.")]
        public LevelBoxUI[] levelBoxes = new LevelBoxUI[15];

        [Header("PREVIEWS")]
        [Tooltip("Direct references to the 15 level preview thumbnail sprites.")]
        public Sprite[] levelPreviews = new Sprite[15];

        [Header("SETTINGS")]
        [SerializeField] private string gameplaySceneName = "TestScene";

        private UIScaleAnimator _animator;

        private void Awake()
        {
            _animator = GetComponent<UIScaleAnimator>();
            AutoPopulateIfEmpty();
        }

        private void OnEnable()
        {
            AutoPopulateIfEmpty();
            RefreshLevelGrid();
        }

        /// <summary>
        /// Automatically populates references from child hierarchy if not already assigned in Inspector.
        /// </summary>
        private void AutoPopulateIfEmpty()
        {
            if (levelBoxes == null || levelBoxes.Length < 15 || levelBoxes[0] == null)
            {
                LevelBoxUI[] foundBoxes = GetComponentsInChildren<LevelBoxUI>(true);
                if (foundBoxes != null && foundBoxes.Length > 0)
                {
                    System.Array.Sort(foundBoxes, (a, b) => a.levelNumber.CompareTo(b.levelNumber));
                    levelBoxes = foundBoxes;
                }
            }

            // Fallback preview sprite loading if not assigned
            if (levelPreviews == null || levelPreviews.Length < 15 || levelPreviews[0] == null)
            {
                levelPreviews = new Sprite[15];
                for (int i = 0; i < 15; i++)
                {
                    string num = (i + 1).ToString("D2");
#if UNITY_EDITOR
                    levelPreviews[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/UI/LevelPreviews/LevelPreview_{num}.png");
#endif
                    if (levelPreviews[i] == null)
                    {
                        levelPreviews[i] = Resources.Load<Sprite>($"LevelPreviews/LevelPreview_{num}");
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes all 15 level cards based on saved progression in LevelProgressManager.
        /// </summary>
        public void RefreshLevelGrid()
        {
            int highestUnlocked = LevelProgressManager.GetHighestUnlockedLevel();

            for (int i = 0; i < levelBoxes.Length && i < 15; i++)
            {
                if (levelBoxes[i] == null) continue;

                int levelNum = i + 1;
                bool isUnlocked = (levelNum <= highestUnlocked);
                Sprite preview = (levelPreviews != null && i < levelPreviews.Length) ? levelPreviews[i] : null;

                levelBoxes[i].Setup(levelNum, preview, isUnlocked, OnLevelSelected);
            }
        }

        /// <summary>
        /// Called when an unlocked level box is clicked.
        /// Sets the starting level in LevelSequenceManager and loads gameplay.
        /// </summary>
        public void OnLevelSelected(int levelNum)
        {
            CheckCpAd.instance.CallAd();
            if (!LevelProgressManager.IsLevelUnlocked(levelNum))
            {
                Debug.LogWarning($"[LevelSelectController] Level {levelNum} is locked. Cannot start.");
                return;
            }

            Debug.Log($"[LevelSelectController] Starting gameplay at Level {levelNum}.");
            // Set 0-based start index in LevelSequenceManager
            LevelSequenceManager.savedLevelIndex = levelNum - 1;
            SceneManager.LoadScene(gameplaySceneName);
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
