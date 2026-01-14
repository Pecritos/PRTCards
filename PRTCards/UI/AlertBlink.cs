using UnityEngine;

namespace PRT.UI
{
    public class AlertBlink : MonoBehaviour
    {
        public float blinkInterval = 0.3f;         private SpriteRenderer spriteRenderer;
        private float timer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= blinkInterval)
            {
                timer = 0f;
                spriteRenderer.enabled = !spriteRenderer.enabled;             }
        }

        public void Show()
        {
            spriteRenderer.enabled = true;
            timer = 0f;
        }

        public void Hide()
        {
            spriteRenderer.enabled = false;
        }
    }
}
