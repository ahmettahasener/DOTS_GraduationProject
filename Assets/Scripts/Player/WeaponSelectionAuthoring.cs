using UnityEngine;
using Unity.Entities;

public class WeaponSelectionAuthoring : MonoBehaviour
{
    public GameObject[] WeaponPrefabs;

    public class Baker : Baker<WeaponSelectionAuthoring>
    {
        public override void Bake(WeaponSelectionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            var buffer = AddBuffer<WeaponPrefabBuffer>(entity);

            foreach (var prefab in authoring.WeaponPrefabs)
            {
                var prefabEntity = GetEntity(prefab, TransformUsageFlags.Dynamic);
                buffer.Add(new WeaponPrefabBuffer { PrefabEntity = prefabEntity });
            }

            AddComponent(entity, new SelectedWeapon { Index = 0 });
        }
    }
}

public struct WeaponPrefabBuffer : IBufferElementData
{
    public Entity PrefabEntity;
}

public struct SelectedWeapon : IComponentData
{
    public int Index;
}
