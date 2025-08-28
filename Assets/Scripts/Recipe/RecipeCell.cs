using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeCell : InfoDisplayTrigger
{
    public Image comboIcon;
    public TMP_Text comboNameText;

    private ComboCardData _comboData;

    public void Initialize(ComboCardData data)
    {
        _comboData = data;
        comboIcon.sprite = data.icon;
        comboNameText.text = data.displayName;
    }

    public void OnClickCell()
    {
        GameManager.Instance?.uiManager.PinRecipeInfo(_comboData);
    }

    protected override DisplayableData GetData() => _comboData;
    protected override void ShowInfo() => GameManager.Instance?.uiManager.DisplayRecipeInfo(_comboData);
    protected override void HideInfo() => GameManager.Instance?.uiManager.HideRecipeInfo();
}