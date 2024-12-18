using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatUIManager : MonoBehaviour
{
    public static ChatUIManager Instance; // Static instance for global access.

    public TMP_InputField ChatInputField; // Reference to the input field for typing messages.
    public TMP_Text ChatDisplay; // Reference to the text display for showing chat messages.
                                 // Start is called before the first frame update
    private void Awake()
    {
        // Ensure there’s only one instance of ChatUIManager.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates.
        }
    }
}
