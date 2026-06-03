using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{
    public TMP_Text pressEText;

    private bool playerNear = false;
    private bool opened = false;

    private void Start()
    {
        pressEText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
        }
    }

    void ToggleDoor()
    {
        if (!opened)
        {
            transform.Rotate(0, 90, 0);
            opened = true;

            pressEText.text = "Press E to close";
        }
        else
        {
            transform.Rotate(0, -90, 0);
            opened = false;

            pressEText.text = "Press E to open";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;

            if (opened)
            {
                pressEText.text = "Press E to close";
            }
            else
            {
                pressEText.text = "Press E to open";
            }

            pressEText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            pressEText.gameObject.SetActive(false);
        }
    }
}