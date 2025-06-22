using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

// PlayerTag ve EnemyTag gibi bileþenlerin baþka bir dosyada tanýmlandýðýný varsayýyorum.
// Eðer tanýmlý deðillerse, buraya eklemeniz gerekecektir:
// public struct PlayerTag : IComponentData { }
// public struct EnemyTag : IComponentData { }

public struct EnemySpawnData : IComponentData
{
    public Entity EnemyPrefab;
    // public Entity ReaperPrefab; // Reaper kaldýrýldýðý için bu satýr silindi
    public float SpawnInterval;
    public float SpawnDistance;
}

public struct EnemySpawnState : IComponentData
{
    public float SpawnTimer;
    // public float ReaperSpawnTimer; // Reaper kaldýrýldýðý için bu satýr silindi
    public Random Random;
}

public class EnemySpawnerAuthoring : MonoBehaviour
{
    public GameObject[] EnemyPrefabs;
    public float[] SpawnWeights;
    public float SpawnInterval;
    public float SpawnDistance;
    public uint RandomSeed;
    public float WeightMultiplierPerLevel = 0.25f;
    public float LevelIncreaseInterval = 30f;

    private class Baker : Baker<EnemySpawnerAuthoring>
    {
        public override void Bake(EnemySpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new EnemySpawnControllerData
            {
                SpawnInterval = authoring.SpawnInterval,
                SpawnDistance = authoring.SpawnDistance
            });

            AddComponent(entity, new EnemySpawnState
            {
                SpawnTimer = 0f,
                Random = Random.CreateFromIndex(authoring.RandomSeed)
            });

            AddComponent(entity, new EnemyLevelScalingData
            {
                WeightMultiplierPerLevel = authoring.WeightMultiplierPerLevel
            });

            AddComponent(entity, new GameLevel
            {
                Value = 1,
                Timer = 0f,
                Interval = authoring.LevelIncreaseInterval
            });

            var buffer = AddBuffer<EnemyWaveData>(entity);
            for (int i = 0; i < authoring.EnemyPrefabs.Length; i++)
            {
                buffer.Add(new EnemyWaveData
                {
                    Prefab = GetEntity(authoring.EnemyPrefabs[i], TransformUsageFlags.Dynamic),
                    Weight = i < authoring.SpawnWeights.Length ? authoring.SpawnWeights[i] : 1f
                });
            }
        }
    }
}


public struct EnemySpawnControllerData : IComponentData
{
    public float SpawnInterval;
    public float SpawnDistance;
}

public partial struct EnemySpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var ecbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        if (!SystemAPI.HasSingleton<PlayerTag>())
            return;

        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;

        foreach (var (spawnState, spawnData, entity) in SystemAPI.Query<RefRW<EnemySpawnState>, EnemySpawnControllerData>().WithEntityAccess())
        {
            DynamicBuffer<EnemyWaveData> enemyBuffer = SystemAPI.GetBuffer<EnemyWaveData>(entity);
            if (enemyBuffer.Length == 0) continue;

            spawnState.ValueRW.SpawnTimer -= deltaTime;
            if (spawnState.ValueRO.SpawnTimer > 0f) continue;
            spawnState.ValueRW.SpawnTimer = spawnData.SpawnInterval;

            float totalWeight = 0f;
            foreach (var enemy in enemyBuffer)
                totalWeight += enemy.Weight;

            float choice = spawnState.ValueRW.Random.NextFloat(0f, totalWeight);
            Entity chosenPrefab = Entity.Null;
            float cumulative = 0f;

            foreach (var enemy in enemyBuffer)
            {
                cumulative += enemy.Weight;
                if (choice <= cumulative)
                {
                    chosenPrefab = enemy.Prefab;
                    break;
                }
            }

            if (chosenPrefab == Entity.Null)
            {
                Debug.LogError("No valid enemy prefab selected.");
                continue;
            }

            var newEnemy = ecb.Instantiate(chosenPrefab);
            var spawnAngle = spawnState.ValueRW.Random.NextFloat(0f, math.TAU);
            var spawnPoint = new float3(math.sin(spawnAngle), math.cos(spawnAngle), 0f) * spawnData.SpawnDistance + playerPosition;

            ecb.SetComponent(newEnemy, LocalTransform.FromPosition(spawnPoint));
        }
    }
}


//different enemies
[System.Serializable]
public struct EnemyWaveEntry
{
    public Entity Prefab;
    public float SpawnWeight; // Oran
}

public struct EnemyWaveData : IBufferElementData
{
    public Entity Prefab;
    public float Weight;
}
public struct GameLevel : IComponentData
{
    public int Value;
    public float Timer;
    public float Interval; // örn: her 30 saniyede bir seviye artýþý
}

public struct EnemyLevelScalingData : IComponentData
{
    public float WeightMultiplierPerLevel; // her seviye için çarpan katsayýsý
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LevelProgressionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (gameLevel, scalingData, entity) in SystemAPI
                     .Query<RefRW<GameLevel>, EnemyLevelScalingData>()
                     .WithEntityAccess())
        {
            gameLevel.ValueRW.Timer += deltaTime;

            if (gameLevel.ValueRO.Timer >= gameLevel.ValueRO.Interval)
            {
                gameLevel.ValueRW.Value += 1;
                gameLevel.ValueRW.Timer = 0f;

                var buffer = SystemAPI.GetBuffer<EnemyWaveData>(entity);

                for (int i = 0; i < buffer.Length; i++)
                {
                    var entry = buffer[i];
                    entry.Weight *= (1f + scalingData.WeightMultiplierPerLevel);
                    buffer[i] = entry;
                }

                Debug.Log($"Level up! New level: {gameLevel.ValueRO.Value}");
                GameUIController.Instance.UpdateLevelText(gameLevel.ValueRO.Value);
            }
        }
    }
}



