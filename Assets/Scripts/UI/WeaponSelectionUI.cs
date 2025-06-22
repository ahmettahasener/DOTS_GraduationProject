using UnityEngine;
using Unity.Entities;
using System;

public class WeaponSelectionUI : MonoBehaviour
{
    private EntityManager _entityManager;
    private Entity _weaponManagerEntity;
    public bool selectFirst;

    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var query = _entityManager.CreateEntityQuery(typeof(WeaponPrefabBuffer), typeof(SelectedWeapon));
        _weaponManagerEntity = query.GetSingletonEntity();
    }

    public void SelectWeapon(int index)
    {
        var buffer = _entityManager.GetBuffer<WeaponPrefabBuffer>(_weaponManagerEntity);

        if (index < 0 || index >= buffer.Length)
        {
            Debug.LogWarning($"Invalid weapon index: {index}");
            return;
        }

        var selected = _entityManager.GetComponentData<SelectedWeapon>(_weaponManagerEntity);
        selected.Index = index;
        _entityManager.SetComponentData(_weaponManagerEntity, selected);
    }
}
