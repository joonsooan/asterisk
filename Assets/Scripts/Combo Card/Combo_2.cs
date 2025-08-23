using UnityEngine;

public class Combo_2 : MonoBehaviour, ICombo
{
    private void Awake()
    {
        ActivateComboCard();
    }

    public void ActivateComboCard()
    {
        GameManager.Instance.expansionPanel.TogglePanelVisibility();
    }

    public void DeactivateComboCard()
    {
    }
}
