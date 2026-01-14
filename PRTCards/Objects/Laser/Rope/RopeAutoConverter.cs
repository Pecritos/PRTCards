using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RopeAutoConverter : MonoBehaviour
{
    private Coroutine autoRoutine;
    private float interval = 2f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

                SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartLoop();
    }

    private void StartLoop()
    {
        if (autoRoutine == null)
            autoRoutine = StartCoroutine(AutoCheckLoop());
    }

    private IEnumerator AutoCheckLoop()
    {
        while (true)
        {
            bool stillMissing = ConvertAllRopes();

            if (!stillMissing)
            {
                autoRoutine = null;
                yield break;
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private bool ConvertAllRopes()
    {
        bool missing = false;
        LineRenderer[] lines = GameObject.FindObjectsOfType<LineRenderer>();

        foreach (var line in lines)
        {
            if (!line.name.ToLower().Contains("rope")) continue;

            if (line.GetComponent<RopeColliderGenerator>() == null)
            {
                missing = true;
                line.gameObject.AddComponent<RopeColliderGenerator>();
            }
        }

        return missing;
    }

    private void Update()
    {
        if (autoRoutine == null && HasUnconvertedRopes())
            StartLoop();
    }

    private bool HasUnconvertedRopes()
    {
        foreach (var line in GameObject.FindObjectsOfType<LineRenderer>())
        {
            if (line.name.ToLower().Contains("rope") &&
                line.GetComponent<RopeColliderGenerator>() == null)
                return true;
        }
        return false;
    }
}
