using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.UI;
using TMG.Survivors;

public struct PlayerTag : IComponentData { }

public struct CameraTarget : IComponentData
{
    public UnityObjectRef<Transform> CameraTransform;
}

public struct InitializeCameraTargetTag : IComponentData { }

[MaterialProperty("_AnimationIndex")]
public struct AnimationIndexOverride : IComponentData
{
    public float Value;
}

public enum PlayerAnimationIndex : byte
{
    Movement = 0,
    Idle = 1,

    None = byte.MaxValue
}

public struct PlayerAttackData : IComponentData
{
    public Entity AttackPrefab;
    public float CooldownTime;
    public float3 DetectionSize;
    public CollisionFilter CollisionFilter;
}

public struct PlayerCooldownExpirationTimestamp : IComponentData
{
    public double Value;
}

public struct GemsCollectedCount : IComponentData
{
    public int Value;
}

public struct UpdateGemUIFlag : IComponentData, IEnableableComponent { }

public struct PlayerWorldUI : ICleanupComponentData
{
    public UnityObjectRef<Transform> CanvasTransform;
    public UnityObjectRef<Slider> HealthBarSlider;
}

public struct PlayerWorldUIPrefab : IComponentData
{
    public UnityObjectRef<GameObject> Value;
}

public class PlayerAuthoring : MonoBehaviour
{
    public GameObject AttackPrefab;
    public float CooldownTime;
    public float DetectionSize;
    public GameObject WorldUIPrefab;

    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerTag>(entity);
            AddComponent<InitializeCameraTargetTag>(entity);
            AddComponent<CameraTarget>(entity);
            AddComponent<AnimationIndexOverride>(entity);

            var enemyLayer = LayerMask.NameToLayer("Enemy");
            var enemyLayerMask = (uint)math.pow(2, enemyLayer);

            var attackCollisionFilter = new CollisionFilter
            {
                BelongsTo = uint.MaxValue,
                CollidesWith = enemyLayerMask
            };

            AddComponent(entity, new PlayerAttackData
            {
                AttackPrefab = GetEntity(authoring.AttackPrefab, TransformUsageFlags.Dynamic),
                CooldownTime = authoring.CooldownTime,
                DetectionSize = new float3(authoring.DetectionSize),
                CollisionFilter = attackCollisionFilter
            });
            AddComponent<PlayerCooldownExpirationTimestamp>(entity);
            AddComponent<GemsCollectedCount>(entity);
            AddComponent<UpdateGemUIFlag>(entity);
            AddComponent(entity, new PlayerWorldUIPrefab
            {
                Value = authoring.WorldUIPrefab
            });
        }
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct CameraInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitializeCameraTargetTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (CameraTargetSingleton.Instance == null) return;
        var cameraTargetTransform = CameraTargetSingleton.Instance.transform;

        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        foreach (var (cameraTarget, entity) in SystemAPI.Query<RefRW<CameraTarget>>().WithAll<InitializeCameraTargetTag, PlayerTag>().WithEntityAccess())
        {
            cameraTarget.ValueRW.CameraTransform = cameraTargetTransform;
            ecb.RemoveComponent<InitializeCameraTargetTag>(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}

[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct MoveCameraSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (transform, cameraTarget) in SystemAPI.Query<LocalToWorld, CameraTarget>().WithAll<PlayerTag>().WithNone<InitializeCameraTargetTag>())
        {
            cameraTarget.CameraTransform.Value.position = transform.Position;
        }
    }
}

public partial class PlayerInputSystem : SystemBase
{
    private SurvivorsInput _input;

    protected override void OnCreate()
    {
        _input = new SurvivorsInput();
        _input.Enable();
    }

    protected override void OnUpdate()
    {
        var currentInput = (float2)_input.Player.Move.ReadValue<Vector2>();
        foreach (var direction in SystemAPI.Query<RefRW<CharacterMoveDirection>>().WithAll<PlayerTag>())
        {
            direction.ValueRW.Value = currentInput;
        }
    }
}

public partial struct PlayerAttackSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var elapsedTime = SystemAPI.Time.ElapsedTime;

        var ecbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (var (expirationTimestamp, attackData, transform) in SystemAPI.Query<RefRW<PlayerCooldownExpirationTimestamp>, PlayerAttackData, LocalTransform>())
        {
            if (expirationTimestamp.ValueRO.Value > elapsedTime) continue;

            var spawnPosition = transform.Position;
            var minDetectPosition = spawnPosition - attackData.DetectionSize;
            var maxDetectPosition = spawnPosition + attackData.DetectionSize;

            var aabbInput = new OverlapAabbInput
            {
                Aabb = new Aabb
                {
                    Min = minDetectPosition,
                    Max = maxDetectPosition
                },
                Filter = attackData.CollisionFilter
            };

            var overlapHits = new NativeList<int>(state.WorldUpdateAllocator);
            if (!physicsWorldSingleton.OverlapAabb(aabbInput, ref overlapHits))
            {
                continue;
            }

            var maxDistanceSq = float.MaxValue;
            var closestEnemyPosition = float3.zero;
            foreach (var overlapHit in overlapHits)
            {
                var curEnemyPosition = physicsWorldSingleton.Bodies[overlapHit].WorldFromBody.pos;
                var distanceToPlayerSq = math.distancesq(spawnPosition.xy, curEnemyPosition.xy);
                if (distanceToPlayerSq < maxDistanceSq)
                {
                    maxDistanceSq = distanceToPlayerSq;
                    closestEnemyPosition = curEnemyPosition;
                }
            }

            var vectorToClosestEnemy = closestEnemyPosition - spawnPosition;
            var angleToClosestEnemy = math.atan2(vectorToClosestEnemy.y, vectorToClosestEnemy.x);
            var spawnOrientation = quaternion.Euler(0f, 0f, angleToClosestEnemy);

            var newAttack = ecb.Instantiate(attackData.AttackPrefab);
            ecb.SetComponent(newAttack, LocalTransform.FromPositionRotation(spawnPosition, spawnOrientation));

            expirationTimestamp.ValueRW.Value = elapsedTime + attackData.CooldownTime;
        }
    }
}

public partial struct UpdateGemUISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // GameUIController.Instance'ýn null olup olmadýðýný kontrol edin
        if (GameUIController.Instance == null)
        {
            //Debug.LogWarning("GameUIController.Instance is null. Skipping UpdateGemUISystem.");
            return; // UI Controller henüz hazýr deðilse iþlemi yapma
        }

        foreach (var (gemCount, shouldUpdateUI) in SystemAPI.Query<GemsCollectedCount, EnabledRefRW<UpdateGemUIFlag>>())
        {
            // Hata alýnan satýr burasý. GameUIController.Instance'ýn null olmadýðýna emin olduktan sonra çaðýr.
            GameUIController.Instance.UpdateGemsCollectedText(gemCount.Value);
            shouldUpdateUI.ValueRW = false;
        }
    }
}

public partial struct PlayerWorldUISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        // Yeni UI prefab'larýný instantiate etme
        foreach (var (uiPrefab, entity) in SystemAPI.Query<PlayerWorldUIPrefab>().WithNone<PlayerWorldUI>().WithEntityAccess())
        {
            // uiPrefab.Value.Value'nin null olmadýðýndan emin olun
            if (uiPrefab.Value.Value == null)
            {
                Debug.LogWarning($"PlayerWorldUIPrefab for entity {entity} is null. Cannot instantiate UI.");
                continue; // Bir sonraki entity'ye geç
            }

            var newWorldUI = Object.Instantiate(uiPrefab.Value.Value);
            ecb.AddComponent(entity, new PlayerWorldUI
            {
                CanvasTransform = newWorldUI.transform,
                HealthBarSlider = newWorldUI.GetComponentInChildren<Slider>()
            });
        }

        // Mevcut UI'larýn pozisyonunu ve saðlýk çubuðunu güncelleme
        foreach (var (transform, worldUI, currentHitPoints, maxHitPoints) in SystemAPI.Query<LocalToWorld, PlayerWorldUI, CharacterCurrentHitPoints, CharacterMaxHitPoints>())
        {
            // Null kontrolü ekle
            if (worldUI.CanvasTransform.Value == null)
            {
                Debug.LogWarning($"PlayerWorldUI for entity with transform at {transform.Position} has a null CanvasTransform. Skipping update.");
                continue;
            }

            worldUI.CanvasTransform.Value.position = transform.Position;
            var healthValue = (float)currentHitPoints.Value / maxHitPoints.Value;

            // Null kontrolü ekle
            if (worldUI.HealthBarSlider.Value == null)
            {
                Debug.LogWarning($"PlayerWorldUI for entity with transform at {transform.Position} has a null HealthBarSlider. Skipping update.");
                continue;
            }
            worldUI.HealthBarSlider.Value.value = healthValue;
        }

        // LocalToWorld bileþeni olmayan UI'larý temizleme
        foreach (var (worldUI, entity) in SystemAPI.Query<PlayerWorldUI>().WithNone<LocalToWorld>().WithEntityAccess())
        {
            // **Hata alýnan satýr burasý.**
            // Yok etmeden önce CanvasTransform.Value'nin null olup olmadýðýný kontrol edin.
            if (worldUI.CanvasTransform.Value != null)
            {
                Object.Destroy(worldUI.CanvasTransform.Value.gameObject);
            }
            // Eðer CanvasTransform.Value null ise zaten yok edilmiþtir veya hiç oluþturulmamýþtýr, sorun yok.

            ecb.RemoveComponent<PlayerWorldUI>(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}