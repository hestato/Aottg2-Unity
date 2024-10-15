using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LinkHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        TextMeshProUGUI textbox = GetComponent<TextMeshProUGUI>();
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int linkno = TMP_TextUtilities.FindIntersectingLink(textbox, Input.mousePosition, null);
            if (linkno != -1)
            {
                TMP_LinkInfo linkdata = textbox.textInfo.linkInfo[linkno];
                //Can use the linkdata object for future in-game popups, or link related stuff here.
                //For now, executing all links as website URLs
                string url = linkdata.GetLinkText();
                Application.OpenURL(url);
                return;
            }
            else return;
        }
    }
}
