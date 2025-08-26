using UnityEngine;
using TMPro;

public class RecipeInfo : MonoBehaviour
{
    public TMP_Text recipeNameText;
    public TMP_Text recipeDescriptionText;

    public void UpdateRecipeInfo(ComboCardData data)
    {
        recipeNameText.text = data.comboName;
        recipeDescriptionText.text = data.comboDescription;
    }
}