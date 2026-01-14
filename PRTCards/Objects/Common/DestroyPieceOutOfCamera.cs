using UnityEngine;

public class DestroyPieceOutOfCamera : MonoBehaviour
{
    private Camera cam;
    private float zDist;

    void Start()
    {
        cam = Camera.main;
        zDist = Mathf.Abs(transform.position.z - cam.transform.position.z);
    }

    void Update()
    {
        if (cam == null) return;

        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);

                Vector3 bottomWorldPos = cam.ViewportToWorldPoint(new Vector3(viewportPos.x, 0, zDist));
        if (transform.position.y < bottomWorldPos.y - 20f)
        {
            Destroy(gameObject);
        }
    }
}
