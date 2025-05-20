using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEditModeManager : MonoBehaviour
{
    public List<UIDraggableButton> draggableButtons;  // Assign draggable buttons in Inspector
    public Button editToggleButton;                   // Assign the "Edit UI" button
    private bool isEditing = false;

    private float previousTimeScale = 1f;

    void Start()
    {
        LoadLayout();

        if (editToggleButton != null)
        {
            editToggleButton.onClick.AddListener(ToggleEditMode);
            UpdateButtonText();
        }
        else
        {
            Debug.LogWarning("Edit toggle button not assigned!");
        }
    }

    void ToggleEditMode()
    {
        isEditing = !isEditing;

        // Toggle draggable state on all buttons
        foreach (var btn in draggableButtons)
        {
            btn.isEditMode = isEditing;
        }

        // Freeze or resume game time
        if (isEditing)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = previousTimeScale;
            SaveLayout();
        }

        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        if (editToggleButton != null)
        {
            Text label = editToggleButton.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = isEditing ? "Done" : "Edit UI";
            }
        }
    }

    void SaveLayout()
    {
        foreach (var btn in draggableButtons)
        {
            string key = btn.name;
            Vector2 pos = ((RectTransform)btn.transform).anchoredPosition;
            PlayerPrefs.SetFloat(key + "_x", pos.x);
            PlayerPrefs.SetFloat(key + "_y", pos.y);
        }
        PlayerPrefs.Save();
    }

    void LoadLayout()
    {
        foreach (var btn in draggableButtons)
        {
            string key = btn.name;
            RectTransform rt = (RectTransform)btn.transform;

            float x = PlayerPrefs.GetFloat(key + "_x", rt.anchoredPosition.x);
            float y = PlayerPrefs.GetFloat(key + "_y", rt.anchoredPosition.y);

            rt.anchoredPosition = new Vector2(x, y);
        }
    }
}
