//using UnityEngine;
//using Unity.Entities;
//using static EnemyAttackJob;

//public class EnemySpriteAuthoring : MonoBehaviour
//{
//    public SpriteRenderer TargetRenderer;
//    public Sprite[] AnimationFrames;
//    public float FrameRate = 6f;

//    public static Sprite[] GlobalEnemyFrames;

//    private void Awake()
//    {
//        GlobalEnemyFrames = AnimationFrames; // Her düþman ayný frame setini kullanacaksa bu yeterli
//    }

//    private class Baker : Baker<EnemySpriteAuthoring>
//    {
//        public override void Bake(EnemySpriteAuthoring authoring)
//        {
//            var entity = GetEntity(TransformUsageFlags.None);

//            AddComponent(entity, new SpriteAnimation
//            {
//                FrameRate = authoring.FrameRate,
//                TimeAccumulator = 0f,
//                CurrentFrameIndex = 0
//            });

//            AddComponent(entity, new SpriteRendererReference
//            {
//                Renderer = authoring.TargetRenderer
//            });
//        }
//    }
//}
