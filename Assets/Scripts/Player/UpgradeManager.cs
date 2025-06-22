using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using TMPro;
using System.Collections.Generic;
using static GameUIController;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject upgradePanel;
    public Button[] optionButtons; // 3 adet buton
    public Image[] optionIcons;
    public TextMeshProUGUI[] optionTexts;

    [Header("Silah Listesi")]
    public List<WeaponUpgradeOption> allUpgrades;

    private EntityManager _entityManager;
    private Entity _weaponManagerEntity;
    private WeaponUpgradeOption[] currentOptions;
    private WeaponSelectionUI _weaponSelectionUI;
    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _weaponManagerEntity = _entityManager.CreateEntityQuery(typeof(WeaponPrefabBuffer), typeof(SelectedWeapon)).GetSingletonEntity();
        _weaponSelectionUI = GetComponent<WeaponSelectionUI>();
        // UI baþlangýçta kapalý olsun
        //upgradePanel.SetActive(false);
    }

    public void ShowUpgradePanel()
    {
        upgradePanel.SetActive(true);
        currentOptions = new WeaponUpgradeOption[3];
        var shuffled = new List<WeaponUpgradeOption>(allUpgrades);
        Shuffle(shuffled);

        for (int i = 0; i < 3; i++)
        {
            currentOptions[i] = shuffled[i];

            optionIcons[i].sprite = currentOptions[i].Icon;
            optionTexts[i].text = $"{currentOptions[i].Description}\n {currentOptions[i].Price}";

            int optionIndex = i; // Closure problemi olmamasý için
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => TrySelectUpgrade(optionIndex));
        }
        GameUIController.Instance.ToggleGameUpgrade();

    }

    private void TrySelectUpgrade(int index)
    {
        var selectedUpgrade = currentOptions[index];

        if (GameUIController.Instance.currentCoins < selectedUpgrade.Price)
        {
            Debug.Log("Yetersiz para! Upgrade alýnamadý.");
            return;
        }

        GameUIController.Instance.currentCoins -= selectedUpgrade.Price;
        SetWeaponIndex(selectedUpgrade.WeaponIndex);

        Debug.Log($"Upgrade alýndý: {selectedUpgrade.Description}");
        upgradePanel.SetActive(false);
    }

    private void SetWeaponIndex(int index)
    {
        GameUIController.Instance.ToggleGameUpgrade();
        _weaponSelectionUI.SelectWeapon(index);

    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public void CloseButton()
    {
        GameUIController.Instance.ToggleGameUpgrade();
        gameObject.SetActive(false);
    }
}
