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
    public GameObject EnemyPrefab;
    // public GameObject ReaperPrefab; // Reaper kaldýrýldýðý için bu satýr silindi
    // public float ReaperSpawnTime; // Reaper kaldýrýldýðý için bu satýr silindi
    public float SpawnInterval;
    public float SpawnDistance;
    public uint RandomSeed;

    private class Baker : Baker<EnemySpawnerAuthoring>
    {
        public override void Bake(EnemySpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new EnemySpawnData
            {
                EnemyPrefab = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic),
                // ReaperPrefab = GetEntity(authoring.ReaperPrefab, TransformUsageFlags.Dynamic), // Reaper kaldýrýldýðý için bu satýr silindi
                SpawnInterval = authoring.SpawnInterval,
                SpawnDistance = authoring.SpawnDistance
            });
            AddComponent(entity, new EnemySpawnState
            {
                SpawnTimer = 0f,
                // ReaperSpawnTimer = authoring.ReaperSpawnTime, // Reaper kaldýrýldýðý için bu satýr silindi
                Random = Random.CreateFromIndex(authoring.RandomSeed)
            });
        }
    }
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

        // Player'ýn var olduðundan emin olun
        if (!SystemAPI.HasSingleton<PlayerTag>())
        {
            // Eðer oyuncu yoksa düþman spawn etmeyin
            return;
        }

        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;

        foreach (var (spawnState, spawnData, entity) in SystemAPI.Query<RefRW<EnemySpawnState>, EnemySpawnData>().WithEntityAccess())
        {
            // ReaperSpawnTimer ve Reaper ile ilgili tüm mantýk kaldýrýldý
            /*
            spawnState.ValueRW.ReaperSpawnTimer -= deltaTime;
            if (spawnState.ValueRO.ReaperSpawnTimer <= 0f)
            {
                if (spawnData.ReaperPrefab == Entity.Null)
                {
                    Debug.LogError("ReaperPrefab is null in EnemySpawnData. Cannot instantiate Reaper.");
                    ecb.DestroyEntity(entity); 
                    continue;
                }

                var reaper = ecb.Instantiate(spawnData.ReaperPrefab);
                var reaperSpawnPoint = playerPosition + new float3(15f, 10f, 0f);
                ecb.SetComponent(reaper, LocalTransform.FromPositionRotationScale(reaperSpawnPoint, quaternion.identity, 4f));

                var enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag>().Build();
                var enemies = enemyQuery.ToEntityArray(state.WorldUpdateAllocator);
                
                if (enemies.Length > 0)
                {
                    foreach (var enemy in enemies)
                    {
                        ecb.DestroyEntity(enemy); 
                    }
                }
                enemies.Dispose(); 

                ecb.DestroyEntity(entity); 
                continue; 
            }
            */

            spawnState.ValueRW.SpawnTimer -= deltaTime;
            if (spawnState.ValueRO.SpawnTimer > 0f) continue;
            spawnState.ValueRW.SpawnTimer = spawnData.SpawnInterval;

            // Enemy prefab'ýnýn geçerli olduðundan emin olun
            if (spawnData.EnemyPrefab == Entity.Null)
            {
                Debug.LogError("EnemyPrefab is null in EnemySpawnData. Cannot instantiate Enemy.");
                continue;
            }

            var newEnemy = ecb.Instantiate(spawnData.EnemyPrefab);
            var spawnAngle = spawnState.ValueRW.Random.NextFloat(0f, math.TAU);
            var spawnPoint = new float3
            {
                x = math.sin(spawnAngle),
                y = math.cos(spawnAngle),
                z = 0f
            };
            spawnPoint *= spawnData.SpawnDistance;
            spawnPoint += playerPosition;

            ecb.SetComponent(newEnemy, LocalTransform.FromPosition(spawnPoint));
        }
    }
}