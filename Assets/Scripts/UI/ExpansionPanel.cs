using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class RoomButton
{
    public Button button;
    public Vector2Int roomCoords;
}

public class ExpansionPanel : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public Color expandableColor = Color.green;
    public Color defaultColor = Color.white;
    public Color unlockedColor = Color.gray;

    private readonly RoomButton[,] _roomButtons = new RoomButton[5, 5];
    private bool _isPanelActive;

    public void InitiateExpansionPanel()
    {
        if (mapGenerator.mapGridSize.x != _roomButtons.GetLength(0) ||
            mapGenerator.mapGridSize.y != _roomButtons.GetLength(1)) {
            return;
        }

        Button[] allButtons = GetComponentsInChildren<Button>();

        int buttonIndex = 0;
        for (int y = mapGenerator.mapGridSize.y - 1; y >= 0; y--) {
            for (int x = 0; x < mapGenerator.mapGridSize.x; x++) {
                Button btn = allButtons[buttonIndex];

                _roomButtons[x, y] = new RoomButton();
                _roomButtons[x, y].button = btn;
                _roomButtons[x, y].roomCoords = new Vector2Int(x, y);

                btn.image.color = defaultColor;

                int tempX = x;
                int tempY = y;
                btn.onClick.AddListener(() => OnRoomButtonClick(tempX, tempY));

                buttonIndex++;
            }
        }

        gameObject.SetActive(false);
    }

    public void TogglePanelVisibility()
    {
        _isPanelActive = !_isPanelActive;
        gameObject.SetActive(_isPanelActive);

        if (_isPanelActive) {
            ShowExpandableRooms();
        }
    }

    private void ShowExpandableRooms()
    {
        ResetButtonColors();

        List<Vector2Int> expandableRooms = mapGenerator.FindExpandableRooms();

        foreach (Vector2Int coords in expandableRooms) {
            if (coords.x >= 0 && coords.x < mapGenerator.mapGridSize.x &&
                coords.y >= 0 && coords.y < mapGenerator.mapGridSize.y) {
                Button btn = _roomButtons[coords.x, coords.y].button;
                if (btn != null) {
                    btn.image.color = expandableColor;
                    btn.interactable = true;
                }
            }
        }

        for (int x = 0; x < mapGenerator.mapGridSize.x; x++) {
            for (int y = 0; y < mapGenerator.mapGridSize.y; y++) {
                if (mapGenerator.IsRoomUnlocked(x, y)) {
                    _roomButtons[x, y].button.interactable = false;
                    _roomButtons[x, y].button.image.color = unlockedColor;
                }
            }
        }

        gameObject.SetActive(true);
    }

    private void ResetButtonColors()
    {
        for (int x = 0; x < mapGenerator.mapGridSize.x; x++) {
            for (int y = 0; y < mapGenerator.mapGridSize.y; y++) {
                Button btn = _roomButtons[x, y].button;
                if (btn != null) {
                    btn.image.color = defaultColor;
                    btn.interactable = false;
                }
            }
        }
    }

    private void OnRoomButtonClick(int x, int y)
    {
        mapGenerator.UnlockRoom(x, y);
        TogglePanelVisibility();
    }
}
