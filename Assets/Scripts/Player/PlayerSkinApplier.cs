using UnityEngine;

namespace GeometryDash.Player
{
    /// <summary>
    /// Applies the selected Cube visual skin to the Player's SpriteRenderer.
    /// Operates purely on the visual sprite without touching colliders, physics, or movement controllers.
    /// Re-applies reliably on respawn, death, and scene loading.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerSkinApplier : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureApplierInScene()
        {
            GameObject cube = GameObject.Find("CubePlayer");
            if (cube != null)
            {
                Transform visual = cube.transform.Find("Visual");
                if (visual != null && visual.GetComponent<PlayerSkinApplier>() == null)
                {
                    visual.gameObject.AddComponent<PlayerSkinApplier>();
                }
            }
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            ApplySelectedSkin();
        }

        private void Start()
        {
            ApplySelectedSkin();
        }

        private void OnEnable()
        {
            PlayerSkinDatabase.OnSkinSelected += HandleSkinSelected;
            ApplySelectedSkin();
        }

        private void OnDisable()
        {
            PlayerSkinDatabase.OnSkinSelected -= HandleSkinSelected;
        }

        private void HandleSkinSelected(int newSkinIndex)
        {
            ApplySelectedSkin();
        }

        /// <summary>
        /// Reads the selected skin from PlayerSkinDatabase and sets the SpriteRenderer's sprite.
        /// </summary>
        public void ApplySelectedSkin()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_spriteRenderer == null) return;

            Sprite skinSprite = PlayerSkinDatabase.GetSelectedSkinGameplaySprite();
            if (skinSprite != null)
            {
                _spriteRenderer.sprite = skinSprite;
            }
        }
    }
}
