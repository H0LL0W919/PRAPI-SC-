using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;
using TMPro;


public class NameTag : MonoBehaviour
{

    [SerializeField] private FixedString32Bytes _name;

    //UI elements
    [SerializeField] private GameObject nameTag;
    [SerializeField] private GameObject hud;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button confirmBtn;

    void Start()
    {
        EnterName();
        confirmBtn.onClick.AddListener(HideTag);
    }

    private void EnterName()
    {
        nameTag.SetActive(true);
        hud.SetActive(false);
        inputField.characterLimit = 15;
    }

    private void HideTag()
    {
        if (!string.IsNullOrWhiteSpace(inputField.text))
        {
            _name = inputField.text;
            nameTag.SetActive(false);
            hud.SetActive(true);
        }
    }

    public FixedString32Bytes Name
    {
        get { return _name; }
    }
}