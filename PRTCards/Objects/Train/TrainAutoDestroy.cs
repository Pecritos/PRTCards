using System.Collections.Generic;
using UnityEngine;

public class TrainAutoDestroy : MonoBehaviour
{
    public GameObject alertaVisual;
    public List<GameObject> vagoes = new List<GameObject>();
    public bool bumerangueAtivo = false;

        public bool bornFromAsterisco = false;

    private bool alertaDestruido = false;
    private bool voltando = false;
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
        if (!bornFromAsterisco)
        {
                        AtualizaTremNormal();
        }
        else
        {
                        AtualizaTremAsterisco();
        }
    }

            
    private bool jaEntrouNaTela = false;

    void AtualizaTremAsterisco()
    {
        Transform marker = transform.Find("Marker");
        Vector3 baseLocal = marker != null ? marker.localPosition : transform.localPosition;

                for (int i = 0; i < vagoes.Count; i++)
        {
            var vagao = vagoes[i];
            if (vagao == null) continue;

            if (i == 0)
                vagao.transform.localPosition = baseLocal;
            else
                vagao.transform.localPosition = baseLocal + stepNormal * i;

            vagao.transform.localScale = Vector3.one;
        }

                if (!jaEntrouNaTela)
        {
            foreach (var v in vagoes)
            {
                if (v == null) continue;
                if (!ForaDaTela(v.transform.position))
                {
                    jaEntrouNaTela = true;
                    break;
                }
            }

                        if (!jaEntrouNaTela && !ForaDaTela(transform.position))
                jaEntrouNaTela = true;
        }

                if (jaEntrouNaTela && TodosForaDaTela())
        {
            outTimer += Time.deltaTime;
            if (outTimer >= maxOutTime)
            {
                foreach (var v in vagoes) if (v != null) Destroy(v);
                if (alertaVisual != null) Destroy(alertaVisual);
                Destroy(gameObject);
            }
        }
        else
        {
            outTimer = 0f;
        }
    }

        bool TodosForaDaTela()
    {
        if (!ForaDaTela(transform.position)) return false;

        foreach (var v in vagoes)
        {
            if (v == null) continue;
            if (!ForaDaTela(v.transform.position)) return false;
        }

        return true;
    }

        bool ForaDaTela(Vector3 pos)
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(pos);
        return (vp.x < -0.2f || vp.x > 1.2f || vp.y < -0.2f || vp.y > 1.2f);
    }


            
    void AtualizaTremNormal()
    {
        float angle = transform.eulerAngles.z;

        bool vertical =
            Mathf.Abs(Mathf.DeltaAngle(angle, 90f)) < 45f ||
            Mathf.Abs(Mathf.DeltaAngle(angle, 270f)) < 45f;

        Vector3 step = vertical ? stepTopToBottom : stepNormal;

        if (!alertaDestruido && alertaVisual != null)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);

            if (!vertical)
            {
                if (!bumerangueAtivo)
                {
                    if (vp.x >= 0f)
                    {
                        Destroy(alertaVisual);
                        alertaVisual = null;
                        alertaDestruido = true;
                    }
                }
                else
                {
                    if (voltando && vp.x * Screen.width < -extraOffset)
                    {
                        Destroy(alertaVisual);
                        alertaVisual = null;
                        alertaDestruido = true;
                    }
                }
            }
            else
            {
                if (!bumerangueAtivo)
                {
                    if (vp.y <= 1f)
                    {
                        Destroy(alertaVisual);
                        alertaVisual = null;
                        alertaDestruido = true;
                    }
                }
                else
                {
                    if (voltando && vp.y * Screen.height > Screen.height + extraOffset)
                    {
                        Destroy(alertaVisual);
                        alertaVisual = null;
                        alertaDestruido = true;
                    }
                }
            }
        }

        Transform anchor = transform.Find("Marker");
        Vector3 baseLocal = anchor != null ? anchor.localPosition : transform.localPosition;

        for (int i = 0; i < vagoes.Count; i++)
        {
            var vagao = vagoes[i];
            if (vagao == null) continue;

            Vector3 localOffset = (vertical ? stepTopToBottom : stepNormal) * i;
            vagao.transform.localPosition = baseLocal + localOffset;
            vagao.transform.localScale = Vector3.one;
        }

        bool todosFora = true;

        foreach (var vagao in vagoes)
        {
            if (vagao == null) continue;

            Vector3 vp = Camera.main.WorldToViewportPoint(vagao.transform.position);

            if (!vertical)
            {
                float px = vp.x * Screen.width;
                if (voltando) { if (px >= -extraOffset) { todosFora = false; break; } }
                else { if (px <= Screen.width + extraOffset) { todosFora = false; break; } }
            }
            else
            {
                float py = vp.y * Screen.height;
                if (voltando) { if (py <= Screen.height + extraOffset) { todosFora = false; break; } }
                else { if (py >= -extraOffset) { todosFora = false; break; } }
            }
        }

        Vector3 tvp = Camera.main.WorldToViewportPoint(transform.position);

        if (!vertical)
        {
            float px2 = tvp.x * Screen.width;
            if (voltando) { if (px2 >= -extraOffset) todosFora = false; }
            else { if (px2 <= Screen.width + extraOffset) todosFora = false; }
        }
        else
        {
            float py2 = tvp.y * Screen.height;
            if (voltando) { if (py2 <= Screen.height + extraOffset) todosFora = false; }
            else { if (py2 >= -extraOffset) todosFora = false; }
        }

        if (todosFora)
        {
            if (bumerangueAtivo && !voltando)
            {
                voltando = true;

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
                foreach (var v in vagoes)
                    if (v != null) Destroy(v);

                if (alertaVisual != null)
                    Destroy(alertaVisual);

                Destroy(gameObject);
            }
        }
    }
}
