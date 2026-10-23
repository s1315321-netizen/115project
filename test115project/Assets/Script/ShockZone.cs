using UnityEngine;

public class ShockZone : MonoBehaviour
{
    public WindowInteraction windowInteraction;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (windowInteraction != null)
            {
                windowInteraction.TriggerShockAtWindow();
            }
        }
    }
}