using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject closedDoor;
    public GameObject openDoor;
    public bool doorOpen = false;

    public void OpenDoor()
    {
        closedDoor.SetActive(false);
        openDoor.SetActive(true);
        doorOpen = true;
    }
}
