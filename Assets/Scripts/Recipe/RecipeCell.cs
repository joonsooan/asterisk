using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeCell : MonoBehaviour
{
    public Image comboIcon;
    public TMP_Text comboNameText;

    private ComboCardData _comboData;

    public void Initialize(ComboCardData data)
    {
        _comboData = data;
        comboIcon.sprite = data.comboIcon;
        comboNameText.text = data.comboName;
    }

    public void OnClickCell()
    {
        GameManager.Instance.recipeManager.recipeInfo.UpdateRecipeInfo(_comboData);
    }
}