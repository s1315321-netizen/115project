using UnityEngine;

public class GhostAttack : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SimpleDeathSystem.Instance.TriggerDeath();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SimpleDeathSystem.Instance.TriggerDeath();
        }
    }
}