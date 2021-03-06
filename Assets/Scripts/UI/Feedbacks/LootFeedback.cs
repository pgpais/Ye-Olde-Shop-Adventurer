using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LootFeedback : MonoBehaviour
{
    [SerializeField] TMP_Text feedbackTitle;
    [SerializeField] TMP_Text feedbackTitleItem;
    [SerializeField] TMP_Text feedbackSubtitle;
    [SerializeField] TMP_Text feedbackText;
    [SerializeField] Image feedbackImage;
    [SerializeField] float feedbackTime;
    [SerializeField] Transform panel;

    public void Initialize(string itemName, int amount, Sprite itemSprite)
    {
        panel.gameObject.SetActive(true);

        if (feedbackTitle != null)
        {
            feedbackTitle.text = Localisation.Get(StringKey.HUD_NewLootFeedback_Title);

            if (feedbackSubtitle != null)
            {
                feedbackSubtitle.text = Localisation.Get(StringKey.HUD_NewLootFeedback_Subtitle);
            }

            if (feedbackTitleItem != null)
            {
                feedbackTitleItem.text = itemName;
            }
        }
        else
        {
            string feedbackTextString = Localisation.Get(StringKey.HUD_LootFeedback);
            feedbackTextString = feedbackTextString.Replace("$AMOUNT$", amount.ToString());
            feedbackTextString = feedbackTextString.Replace("$ITEM$", itemName);
            feedbackText.text = feedbackTextString;
        }

        feedbackImage.sprite = itemSprite;

        var stuffToDisableAndReenable = GetComponentsInChildren<RectTransform>();
        foreach (var transform in stuffToDisableAndReenable)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            // transform.enabled = false;
        }
        // foreach (var transform in stuffToDisableAndReenable)
        // {
        //     transform.enabled = true;
        // }

        StartCoroutine(DisableAfterSeconds(feedbackTime));

    }

    private IEnumerator DisableAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}
