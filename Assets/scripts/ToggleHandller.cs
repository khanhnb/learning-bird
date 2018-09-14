using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleHandller : MonoBehaviour
{

    // Use this for initialization
    Toggle m_Toggle;

    void Start()
    {
        m_Toggle = GetComponent<Toggle>();
        m_Toggle.onValueChanged.AddListener(delegate
        {
            ToggleValueChanged();
        });
    }

    void ToggleValueChanged()
    {
        if (m_Toggle.isOn)
        {
            var label = m_Toggle.GetComponentInChildren<Text>().text;
            Debug.Log(label + "is clicked");
            if (label.Equals("Normal")) GameManager.timeScale = 1f;
            if (label.Equals("x2")) GameManager.timeScale = 2f;
            if (label.Equals("x3")) GameManager.timeScale = 3f;

        }
    }
}
