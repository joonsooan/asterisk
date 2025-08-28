public class Combo_7 : Damageable
{
    protected override void Awake()
    {
        base.Awake();
        ActivateComboCard();
    }

    private void ActivateComboCard()
    {
        GameManager.Instance.expansionPanel.TogglePanelVisibility();
    }
}