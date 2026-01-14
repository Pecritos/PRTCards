using UnityEngine;

public class TrainCooldown : MonoBehaviour
{
    public float lastUseTime = -999f;
    public float cooldown = 2f;

    public bool CanUse()
    {
        return Time.time >= lastUseTime + cooldown;
    }

    public void Trigger()
    {
        lastUseTime = Time.time;
    }
}
