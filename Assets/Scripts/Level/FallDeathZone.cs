using UnityEngine;
using GeometryDash.Player;

namespace GeometryDash.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class FallDeathZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"[FallDeathZone] Trigger entered by: {collision.gameObject.name}");
            
            CubePlayerController player = collision.GetComponentInParent<CubePlayerController>();
            if (player != null)
            {
                PlayerDeathController deathController = player.GetComponent<PlayerDeathController>();
                if (deathController != null)
                {
                    Debug.Log("[FallDeathZone] PlayerDeathController found");
                    if (player.IsAlive)
                    {
                        Debug.Log("[FallDeathZone] Death triggered");
                        deathController.Die();
                    }
                }
                else
                {
                    Debug.Log("[FallDeathZone] PlayerDeathController NOT found");
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.bounds.size);
            }
        }
    }
}
