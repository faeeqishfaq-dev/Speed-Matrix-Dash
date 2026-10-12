using UnityEngine;
using System.Collections.Generic;

namespace GeometryDash.Player
{
    [System.Serializable]
    public class SkinDefinition
    {
        public int id;
        public string skinName;
        public Sprite sprite;
    }

    [CreateAssetMenu(fileName = "PlayerSkinDatabase", menuName = "Geometry Dash/Player Skin Database")]
    public class PlayerSkinDatabase : ScriptableObject
    {
        private const string KEY_SELECTED_SKIN = "SelectedSkinIndex";
        public const int TOTAL_SKINS = 10;

        [Header("SKIN CONFIGURATIONS")]
        [Tooltip("List of 10 available Cube player skins.")]
        public List<SkinDefinition> skins = new List<SkinDefinition>();

        private static PlayerSkinDatabase _cachedInstance;
        public static event System.Action<int> OnSkinSelected;

        public static PlayerSkinDatabase Instance
        {
            get
            {
                if (_cachedInstance == null)
                {
                    _cachedInstance = Resources.Load<PlayerSkinDatabase>("PlayerSkinDatabase");
                }
                return _cachedInstance;
            }
            set => _cachedInstance = value;
        }

        public static int GetSelectedSkinIndex()
        {
            int index = PlayerPrefs.GetInt(KEY_SELECTED_SKIN, 0);
            return Mathf.Clamp(index, 0, TOTAL_SKINS - 1);
        }

        public static void SetSelectedSkinIndex(int index)
        {
            index = Mathf.Clamp(index, 0, TOTAL_SKINS - 1);
            PlayerPrefs.SetInt(KEY_SELECTED_SKIN, index);
            PlayerPrefs.Save();
            Debug.Log($"[PlayerSkinDatabase] Selected skin index set to: {index}");
            OnSkinSelected?.Invoke(index);
        }

        public static Sprite GetSelectedSkinGameplaySprite()
        {
            return GetSkinSprite(GetSelectedSkinIndex());
        }

        public static Sprite GetSkinSprite(int index)
        {
            PlayerSkinDatabase db = Instance;
            if (db != null && db.skins != null && index >= 0 && index < db.skins.Count)
            {
                if (db.skins[index] != null && db.skins[index].sprite != null)
                {
                    return db.skins[index].sprite;
                }
            }

            // Fallback: try loading from Resources directly
            string skinNum = (index + 1).ToString("D2");
            Sprite fallback = Resources.Load<Sprite>($"Skins/CubeSkin_{skinNum}");
            if (fallback != null) return fallback;

#if UNITY_EDITOR
            // Editor fallback: load from AssetDatabase directly
            string assetPath = $"Assets/UI/Skins/CubeSkin_{skinNum}.png";
            fallback = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (fallback != null) return fallback;
#endif

            return null;
        }
    }
}
