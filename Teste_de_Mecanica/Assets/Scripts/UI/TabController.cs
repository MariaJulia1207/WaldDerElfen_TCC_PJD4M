using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    [SerializeField] private Image[] tabImages;
    [SerializeField] private GameObject[] pages;

    void Start()
    {
        ActivateTab(0);
    }

    public void ActivateTab(int tabNo)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.grey;
        }
        pages[tabNo].SetActive(true);
        tabImages[tabNo].color = Color.white;

        // Register any SFX slider present in the activated page so it syncs with the manager
        if (pages != null && pages.Length > tabNo && pages[tabNo] != null)
        {
            Slider s = pages[tabNo].GetComponentInChildren<Slider>(true);
            if (s != null && SoundEffectManager.Instance != null)
            {
                SoundEffectManager.Instance.RegisterSlider(s);
            }
        }
    }
}
