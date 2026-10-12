using UnityEngine;

namespace GeometryDash.Level
{
    public enum LiquidType
    {
        Lava,
        Water
    }

    public class LevelLiquidController : MonoBehaviour
    {
        [Header("LIQUID SETTINGS")]
        public LiquidType liquidType;
        public float liquidWidth = 100f;
        public float liquidHeight = 10f;
        
        public bool enableBubbles = true;

        [Header("ASSETS")]
        [SerializeField] private Material lavaMaterial;
        [SerializeField] private Material waterMaterial;
        [SerializeField] private GameObject bubbleParticlePrefab;

        private SpriteRenderer spriteRenderer;
        private ParticleSystem bubbleSystem;
        private Transform spriteTransform;

        private void Awake()
        {
            ApplySettings();
        }

        public void ApplySettings()
        {
            // Setup Sprite Child
            if (spriteTransform == null)
            {
                Transform existing = transform.Find("SpriteVisual");
                if (existing != null)
                {
                    spriteTransform = existing;
                    spriteRenderer = existing.GetComponent<SpriteRenderer>();
                }
                else
                {
                    GameObject obj = new GameObject("SpriteVisual");
                    obj.transform.SetParent(transform, false);
                    spriteTransform = obj.transform;
                    spriteRenderer = obj.AddComponent<SpriteRenderer>();
                    spriteRenderer.sortingOrder = -5;
                }
            }
            
            // Fix missing base sprite (set from editor but just in case)
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = Resources.Load<Sprite>("LiquidBase"); // Optional fallback
            }

            spriteTransform.localScale = new Vector3(liquidWidth, liquidHeight, 1f);
            spriteTransform.localPosition = Vector3.zero;

            // Apply material
            if (liquidType == LiquidType.Lava && lavaMaterial != null)
                spriteRenderer.sharedMaterial = lavaMaterial;
            else if (liquidType == LiquidType.Water && waterMaterial != null)
                spriteRenderer.sharedMaterial = waterMaterial;

            // Setup Bubbles
            if (enableBubbles && bubbleParticlePrefab != null)
            {
                if (bubbleSystem == null)
                {
                    Transform existingBubbles = transform.Find("BubbleParticles");
                    if (existingBubbles != null)
                    {
                        bubbleSystem = existingBubbles.GetComponent<ParticleSystem>();
                    }
                    else
                    {
                        GameObject bubbleObj = Instantiate(bubbleParticlePrefab, transform);
                        bubbleObj.name = "BubbleParticles";
                        bubbleSystem = bubbleObj.GetComponent<ParticleSystem>();
                    }
                }
                
                bubbleSystem.transform.localPosition = new Vector3(0, -liquidHeight * 0.4f, 0);
                
                var shape = bubbleSystem.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(liquidWidth, 1f, 1f);
                
                var main = bubbleSystem.main;
                if (liquidType == LiquidType.Lava)
                    main.startColor = new Color(1f, 0.8f, 0.1f, 1f);
                else
                    main.startColor = new Color(0.8f, 0.95f, 1f, 0.8f);
                    
                if (!bubbleSystem.isPlaying) bubbleSystem.Play();
            }
            else
            {
                if (bubbleSystem != null) bubbleSystem.Stop();
            }
        }

        private void OnValidate()
        {
            ApplySettings();
        }
    }
}
