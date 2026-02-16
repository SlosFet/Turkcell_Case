using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class NavigationManager : MonoBehaviour
{
    [Header("Screens & UI")]
    public List<GameObject> screens; // Listeye panellerini (Leaderboard, Detail, Sim vb.) sürükle býrak
    public TMP_Text headerTitle;

    [Header("Navigation Names")]
    public List<string> screenTitles; // Her ekranýn tepede yazacak baþlýðý (sýrayla)

    // Bu tek fonksiyon tüm butonlar için yeterli!
    public void SwitchToScreen(int index)
    {
        // Güvenlik kontrolü
        if (index < 0 || index >= screens.Count) return;

        // Tüm ekranlarý kapat
        for (int i = 0; i < screens.Count; i++)
        {
            screens[i].SetActive(false);
        }

        // Seçilen ekraný aç
        screens[index].SetActive(true);

        // Baþlýðý güncelle
        if (index < screenTitles.Count)
        {
            headerTitle.text = screenTitles[index];
        }
    }
}