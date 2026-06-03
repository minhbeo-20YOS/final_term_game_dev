using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DoorToDay1 : MonoBehaviour
{
    public TMP_Text pressEText;

    private bool playerNear = false;

    void Start()
    {
        if (pressEText != null)
        {
            pressEText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadScene("Day1");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;

            if (pressEText != null)
            {
                pressEText.text = "Nhấn E để đi làm";
                pressEText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;

            if (pressEText != null)
            {
                pressEText.gameObject.SetActive(false);
            }
        }
    }
}