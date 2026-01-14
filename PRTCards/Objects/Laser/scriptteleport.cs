using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportUIScript : MonoBehaviour
{
        public Vector3 anchorPosition = Vector3.zero;

        public Vector3 anchorRotation = Vector3.zero;

    private RectTransform rect;

    void Start()
    {
                rect = GetComponent<RectTransform>();
    }

    void Update()
    {
                if (anchorPosition != Vector3.zero)
        {
                        rect.anchoredPosition = anchorPosition;

                        rect.localEulerAngles = anchorRotation;

                        anchorPosition = Vector3.zero;
            anchorRotation = Vector3.zero;
        }
    }
}