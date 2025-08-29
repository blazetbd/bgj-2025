using System.Collections;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    public float hideLength = 1f;
    public float showLength = 5f;
    public GameObject platform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            platform.SetActive(false);
            yield return new WaitForSeconds(hideLength);

            platform.SetActive(true);
            yield return new WaitForSeconds(showLength);  
        }
    }
}
