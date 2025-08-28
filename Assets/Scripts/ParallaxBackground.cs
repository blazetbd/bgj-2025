using UnityEngine;
using UnityEngine.UIElements;

public class ParallaxBackground : MonoBehaviour
{
    private float startPos, length;
    public Camera cam;
    public float parallaxEffect;

    void Start()
    {
        cam = Camera.main;
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float distance = cam.transform.position.x * parallaxEffect;
        float movement = cam.transform.position.x * (1 - parallaxEffect);

        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

        if (movement > startPos + length)
        {
            startPos += length;
        }
        else if (movement < startPos - length)
        {
            startPos -= length;
        }
    }
}
