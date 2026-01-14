using UnityEngine;
using System.Collections.Generic;

namespace PRT.Objects.Train
{
    public class TrainObject : MonoBehaviour
    {
        private readonly HashSet<Player> playersAcertados = new HashSet<Player>();
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<Player>();
            if (player != null && !playersAcertados.Contains(player))
            {
                playersAcertados.Add(player);

                var healthHandler = player.GetComponent<HealthHandler>();
                if (healthHandler != null)
                {
                    float velocidade = rb.velocity.magnitude;
                    if (velocidade < 1f) return;

                    float damageMultiplier = 0.3f;
                    float forceMultiplier = 300f;

                    Vector2 hitDirection = (player.transform.position - transform.position).normalized;

                    Vector2 damageVector = hitDirection * velocidade * damageMultiplier;
                    healthHandler.CallTakeDamage(damageVector, transform.position, gameObject, null, false);

                    Vector2 forceVector = hitDirection * velocidade * forceMultiplier;
                    healthHandler.TakeForce(forceVector, ForceMode2D.Impulse, forceIgnoreMass: true, ignoreBlock: false, setFlying: 1f);
                }
            }
        }
    }
}
