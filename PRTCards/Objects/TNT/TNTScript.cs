using Photon.Pun;
using PRT;
using PRT.Objects.TNT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class TNTScript : MonoBehaviour
{
    public Animator animator;
    public string animationName = "Explosion";
    public AudioSource audioSource;
    public Rigidbody2D rb;
    public GameObject effectPrefab;
    public Player player_spawner; public float tntScale = 1f;
    private Collider2D col;


    public int numberOfCopies = 0;
    public bool isCopy = false;
    public bool copiesCreated = false;

    private int loopCount = 0;
    private float maxLoops = 3;
    private bool isPlaying = false;
    private bool touchedGround = false;
    private bool isInvisible = true;
    private List<Collider2D> copyColliders = new List<Collider2D>();

    private SpriteRenderer sr;

    private List<AudioClip> explosoes = new List<AudioClip>();
    private AudioClip fuseClip;


    void Start()
    {
        Explosion exp = GetComponent<Explosion>();
        SpawnedAttack atk = GetComponent<SpawnedAttack>();
        if (atk == null) atk = gameObject.AddComponent<SpawnedAttack>();
        if (exp == null) exp = gameObject.AddComponent<Explosion>();

        if (player_spawner != null)
            atk.spawner = player_spawner.GetComponent<Player>();
        else
            exp.auto = false;
        exp.ignoreTeam = true;
        exp.range = 5f;
        atk.attackLevel = 1;

        if (!isCopy) Initialize();
    }





    void Initialize()
    {
        sr = transform.Find("SpriteRenderer")?.GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); animator = transform.Find("SpriteRenderer")?.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        col = GetComponent<Collider2D>();

        if (sr != null) sr.enabled = true;
        if (animator != null) animator.enabled = true;

        gameObject.layer = LayerMask.NameToLayer("PlayerObjectCollider");
        sr.gameObject.layer = LayerMask.NameToLayer("Default");
        isInvisible = false;
        copiesCreated = false;
    }


    public void StartTNT(int amount, float loops, bool activateNow = true)
    {


        numberOfCopies = amount;
        maxLoops = loops;
        isInvisible = false;

        transform.localScale = Vector3.one * tntScale;

        LoadAudioFromSharedBundle();

        float zDist = Mathf.Abs(transform.position.z - Camera.main.transform.position.z);
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, zDist));
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, zDist));
        float cameraTop = topRight.y;
        float maxTop = cameraTop + 3f;

        float randomX = UnityEngine.Random.Range(bottomLeft.x, topRight.x);
        float spawnY = isCopy ? UnityEngine.Random.Range(cameraTop + 0.5f, maxTop) : maxTop + 1f;

        transform.position = new Vector3(randomX, spawnY, transform.position.z);

        sr = transform.Find("SpriteRenderer")?.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;

        rb = GetComponent<Rigidbody2D>();
        animator = transform.Find("SpriteRenderer")?.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        col = GetComponent<BoxCollider2D>();

        rb.simulated = activateNow || isCopy;
        animator.enabled = false;
        isPlaying = false;
    }

    void LoadAudioFromSharedBundle()
    {
        AssetBundle bundle = Assets.Bundle;

        if (bundle == null)
        {
            return;
        }

        explosoes.Clear();
        for (int i = 1; i <= 4; i++)
        {
            var clip = bundle.LoadAsset<AudioClip>($"Explode{i}");
            if (clip != null) explosoes.Add(clip);
        }

        fuseClip = bundle.LoadAsset<AudioClip>("fuse");

        if (effectPrefab == null)
        {
            var loadedPrefab = bundle.LoadAsset<GameObject>("explosion");
            if (loadedPrefab != null)
            {
                effectPrefab = loadedPrefab;
            }
        }

    }

    void Update()
    {
        if (isInvisible) return;

        float zDist = Mathf.Abs(transform.position.z - Camera.main.transform.position.z);
        float cameraTop = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, zDist)).y;

        if (transform.position.y <= cameraTop && !isPlaying && !touchedGround)
        {
            PlayAnimation();
        }

        if (!isPlaying) return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName(animationName) && state.normalizedTime >= 1f)
        {
            if (touchedGround)
            {
                loopCount++;
                if (loopCount >= maxLoops)
                {
                    StartCoroutine(ScaleUpAndDestroySprite());
                    isPlaying = false;
                }
                else
                {
                    animator.Play(animationName, 0, 0f);
                }
            }
            else
            {
                animator.Play(animationName, 0, 0f);
            }
        }

        float cameraBottom = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0f, zDist)).y;
        float limitY = cameraBottom - 0.2f; 
        if (transform.position.y < limitY)
        {
            PhotonView pv = GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)             {
                PhotonNetwork.Destroy(gameObject);
            }
            else if (pv == null)             {
                Destroy(gameObject);
            }
        }
    }


    public void PlayAnimation()
    {
        if (!copiesCreated && !isCopy)
        {
            copiesCreated = true;
            CreateCopies();
        }

        loopCount = 0;
        isPlaying = true;
        touchedGround = false;

        animator.enabled = true;
        animator.Play(animationName, 0, 0f);

        if (audioSource != null && fuseClip != null)
        {
            audioSource.clip = fuseClip;
            audioSource.Play();
        }

        float horizontalDir = UnityEngine.Random.value < 0.5f ? -0.2f : 0.2f;
        float verticalImpulse = 1f;

        if (rb != null)
            rb.velocity = new Vector2(horizontalDir * 2f, verticalImpulse);
    }

    void CreateCopies()
    {
        if (!PhotonNetwork.IsMasterClient) return;

                int[] cIDs = new int[numberOfCopies];
                int pID = PhotonNetwork.AllocateViewID(false);

        Vector2[] positions = new Vector2[numberOfCopies];

        float zDist = Mathf.Abs(transform.position.z - Camera.main.transform.position.z);
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, zDist));
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, zDist));
        float cameraTop = topRight.y;
        float maxTop = cameraTop + 3f;

        for (int i = 0; i < numberOfCopies; i++)
        {
            cIDs[i] = PhotonNetwork.AllocateViewID(false);
            float randomX = UnityEngine.Random.Range(bottomLeft.x, topRight.x);
            float randomY = UnityEngine.Random.Range(cameraTop + 0.5f, maxTop);

            positions[i] = new Vector2(randomX, randomY);
        }

                if (TNTNetworkProxy.Instance != null)
        {
            TNTNetworkProxy.Instance.RequestNetworkSync(
                pID,
                cIDs,
                positions,
                gameObject.GetInstanceID(),
                maxLoops,
                tntScale,
                player_spawner.playerID
            );
        }

        Destroy(gameObject);
    }

    IEnumerator ScaleUpAndDestroySprite()
    {
        yield return new WaitForSeconds(0.25f);

        Vector3 originalScale = transform.localScale;
        Vector3 scaleStep = new Vector3(0.4f, 0.4f, 1f) * tntScale;
        Vector3 targetScale = originalScale + scaleStep;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        if (explosoes.Count > 0 && audioSource != null)
        {
            AudioClip chosenClip = explosoes[UnityEngine.Random.Range(0, explosoes.Count)];
            audioSource.clip = chosenClip;
            audioSource.Play();
        }

        Explosion exp = GetComponent<Explosion>();
        if (exp != null)
        {
            exp.Explode();
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (sr != null)
            sr.enabled = false;
        if (col != null)
            col.enabled = false;

        SpawnVisualEffect();
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<TNTScript>() != null)
            return;

        if (!touchedGround)
        {
            touchedGround = true;

            if (maxLoops == 0)
            {
                ExplodeInstantly();
                isPlaying = false;
            }
        }
    }

    void ExplodeInstantly()
    {


        if (explosoes.Count > 0 && audioSource != null)
        {
            AudioClip chosenClip = explosoes[UnityEngine.Random.Range(0, explosoes.Count)];
            audioSource.clip = chosenClip;
            audioSource.Play();
        }

        Explosion exp = GetComponent<Explosion>();
        if (exp != null)
        {
            exp.Explode();
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (sr != null)
            sr.enabled = false;

        if (col != null)
            col.enabled = false;

        SpawnVisualEffect();
    }

    void SpawnVisualEffect()
    {
        int totalExplosions = 10;

        for (int i = 0; i < totalExplosions; i++)
        {
            StartCoroutine(SpawnExplosionWithDelay(UnityEngine.Random.Range(0f, 0.05f * i)));
        }


        Destroy(gameObject, 0.3f * totalExplosions + 0.5f);
    }


    IEnumerator SpawnExplosionWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject newExplosion = ExplosionPool.Instance.GetExplosion();
        if (newExplosion == null)
        {
            yield break;
        }

        Vector3 offset = new Vector3(
    UnityEngine.Random.Range(-0.3f, 0.3f),
    UnityEngine.Random.Range(-0.3f, 0.3f),
    0f
);



        Transform srTransform = transform.Find("SpriteRenderer");
        if (srTransform != null)
        {
            newExplosion.transform.SetParent(srTransform);
        }
        else
        {
        }

        newExplosion.transform.localPosition = offset;
        newExplosion.layer = 31;
        newExplosion.SetActive(true);

        SpriteRenderer explosionSR = newExplosion.GetComponent<SpriteRenderer>();
        if (explosionSR != null)
        {
            Color minColor = HexToColor("777777");
            Color maxColor = HexToColor("FFFFFF");
            explosionSR.color = Color.Lerp(minColor, maxColor, UnityEngine.Random.value);
        }

        Animator anim = newExplosion.GetComponent<Animator>();
        float animLength = 0.5f;
        if (anim != null)
        {
            anim.Play("Explosion", -1, 0);
            if (anim.runtimeAnimatorController != null)
            {
                var clips = anim.runtimeAnimatorController.animationClips;
                var clip = Array.Find(clips, c => c.name == "Explosion");
                if (clip != null)
                    animLength = clip.length;
            }
        }

        AudioSource audio = newExplosion.GetComponent<AudioSource>();
        if (audio != null)
            audio.Play();

        StartCoroutine(DisableAfterDelay(newExplosion, animLength));
    }









    IEnumerator DisableAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        obj.SetActive(false);

        if (ExplosionPool.Instance != null)
            ExplosionPool.Instance.ReturnToContainer(obj);
    }



    Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color color))
            return color;
        return Color.white;
    }
}
