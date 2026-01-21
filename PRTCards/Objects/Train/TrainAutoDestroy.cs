using System.Collections.Generic;
using UnityEngine;

public class TrainAutoDestroy : MonoBehaviour
{
    public GameObject visualAlert;
    public List<GameObject> wagons = new List<GameObject>();
    public bool boomerangactive = false;

    public bool bornFromGodtrain = false;

    private bool DestroyedAlert = false;
    private bool returning = false;
    private Rigidbody2D rb;

    private Vector3 stepNormal = new Vector3(-3.45f, 0f, 0f);
    private Vector3 stepTopToBottom = new Vector3(-3.45f, 0f, 0f);

    private float extraOffset = 500f;

    private float outTimer = 0f;
    private float maxOutTime = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!bornFromGodtrain)
        {
            UpdateNormalTrain();
        }
        else
        {
            UpdateGodTrain();
        }
    }


    private bool IsOnScreen = false;

    void UpdateGodTrain()
    {
        Transform marker = transform.Find("Marker");
        Vector3 baseLocal = marker != null ? marker.localPosition : transform.localPosition;

        for (int i = 0; i < wagons.Count; i++)
        {
            var vagao = wagons[i];
            if (vagao == null) continue;

            if (i == 0)
                vagao.transform.localPosition = baseLocal;
            else
                vagao.transform.localPosition = baseLocal + stepNormal * i;

            vagao.transform.localScale = Vector3.one;
        }

        if (!IsOnScreen)
        {
            foreach (var v in wagons)
            {
                if (v == null) continue;
                if (!IsOutOfScreen(v.transform.position))
                {
                    IsOnScreen = true;
                    break;
                }
            }

            if (!IsOnScreen && !IsOutOfScreen(transform.position))
                IsOnScreen = true;
        }

        if (IsOnScreen && AreAllOutOfScreen())
        {
            outTimer += Time.deltaTime;
            if (outTimer >= maxOutTime)
            {
                foreach (var v in wagons) if (v != null) Destroy(v);
                if (visualAlert != null) Destroy(visualAlert);
                Destroy(gameObject);
            }
        }
        else
        {
            outTimer = 0f;
        }
    }

    bool AreAllOutOfScreen()
    {
        if (!IsOutOfScreen(transform.position)) return false;

        foreach (var v in wagons)
        {
            if (v == null) continue;
            if (!IsOutOfScreen(v.transform.position)) return false;
        }

        return true;
    }

    bool IsOutOfScreen(Vector3 pos)
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(pos);
        return (vp.x < -0.2f || vp.x > 1.2f || vp.y < -0.2f || vp.y > 1.2f);
    }



    void UpdateNormalTrain()
    {
        float angle = transform.eulerAngles.z;

        bool vertical =
            Mathf.Abs(Mathf.DeltaAngle(angle, 90f)) < 45f ||
            Mathf.Abs(Mathf.DeltaAngle(angle, 270f)) < 45f;

        Vector3 step = vertical ? stepTopToBottom : stepNormal;

        if (!DestroyedAlert && visualAlert != null)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);

            if (!vertical)
            {
                if (!boomerangactive)
                {
                    if (vp.x >= 0f)
                    {
                        Destroy(visualAlert);
                        visualAlert = null;
                        DestroyedAlert = true;
                    }
                }
                else
                {
                    if (returning && vp.x * Screen.width < -extraOffset)
                    {
                        Destroy(visualAlert);
                        visualAlert = null;
                        DestroyedAlert = true;
                    }
                }
            }
            else
            {
                if (!boomerangactive)
                {
                    if (vp.y <= 1f)
                    {
                        Destroy(visualAlert);
                        visualAlert = null;
                        DestroyedAlert = true;
                    }
                }
                else
                {
                    if (returning && vp.y * Screen.height > Screen.height + extraOffset)
                    {
                        Destroy(visualAlert);
                        visualAlert = null;
                        DestroyedAlert = true;
                    }
                }
            }
        }

        Transform anchor = transform.Find("Marker");
        Vector3 baseLocal = anchor != null ? anchor.localPosition : transform.localPosition;

        for (int i = 0; i < wagons.Count; i++)
        {
            var vagao = wagons[i];
            if (vagao == null) continue;

            Vector3 localOffset = (vertical ? stepTopToBottom : stepNormal) * i;
            vagao.transform.localPosition = baseLocal + localOffset;
            vagao.transform.localScale = Vector3.one;
        }

        bool todosFora = true;

        foreach (var vagao in wagons)
        {
            if (vagao == null) continue;

            Vector3 vp = Camera.main.WorldToViewportPoint(vagao.transform.position);

            if (!vertical)
            {
                float px = vp.x * Screen.width;
                if (returning) { if (px >= -extraOffset) { todosFora = false; break; } }
                else { if (px <= Screen.width + extraOffset) { todosFora = false; break; } }
            }
            else
            {
                float py = vp.y * Screen.height;
                if (returning) { if (py <= Screen.height + extraOffset) { todosFora = false; break; } }
                else { if (py >= -extraOffset) { todosFora = false; break; } }
            }
        }

        Vector3 tvp = Camera.main.WorldToViewportPoint(transform.position);

        if (!vertical)
        {
            float px2 = tvp.x * Screen.width;
            if (returning) { if (px2 >= -extraOffset) todosFora = false; }
            else { if (px2 <= Screen.width + extraOffset) todosFora = false; }
        }
        else
        {
            float py2 = tvp.y * Screen.height;
            if (returning) { if (py2 <= Screen.height + extraOffset) todosFora = false; }
            else { if (py2 >= -extraOffset) todosFora = false; }
        }

        if (todosFora)
        {
            if (boomerangactive && !returning)
            {
                returning = true;

                Vector3 s = transform.localScale;
                s.x *= -1f;
                transform.localScale = s;

                if (rb != null)
                {
                    if (!vertical) rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y);
                    else rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y);
                }
            }
            else
            {
                foreach (var v in wagons)
                    if (v != null) Destroy(v);

                if (visualAlert != null)
                    Destroy(visualAlert);

                Destroy(gameObject);
            }
        }
    }
}
