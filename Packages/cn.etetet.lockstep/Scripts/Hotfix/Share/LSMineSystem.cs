using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSMineComponent))]
    [LSEntitySystemOf(typeof(LSMineComponent))]
    [FriendOf(typeof(LSMine))]
    [FriendOf(typeof(LSUnit))]
    public static partial class LSMineComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSMineComponent self)
        {
            
        }

        public static void Init(this LSMineComponent self)
        {
            for (int i = 0; i < 10; i++)
            {
                TSVector randomPos = new TSVector(RandomGenerator.RandomNumber(-20, 20), 0, RandomGenerator.RandomNumber(-20, 20));

                LSMine mine = self.AddChild<LSMine, TSVector>(randomPos);
                mine.Position = randomPos;
                mine.IsExploded = false;
            }
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSMineComponent self)
        {
            LSWorld world = self.GetParent<LSWorld>();
            LSUnitComponent unitComponent = world.GetComponent<LSUnitComponent>();
            
            foreach (LSMine mine in self.Children.Values)
            {
                // 已爆炸的地雷检查重生
                if (mine.IsExploded)
                {
                    if (world.Frame >= mine.RespawnFrame)
                    {
                        mine.Position = new TSVector(RandomGenerator.RandomNumber(-20, 20), 0, RandomGenerator.RandomNumber(-20, 20));
                        mine.IsExploded = false;
                    }
                    continue;
                }
                // 检测玩家碰撞
                foreach (LSUnit unit in unitComponent.Children.Values)
                {
                    FP distance = TSVector.Distance(mine.Position, unit.Position);
                    if (distance < FP.One)
                    {
                        mine.Explode(unit);
                        break;
                    }
                }
            }
        }
        
        //爆炸逻辑
        private static void Explode(this LSMine self, LSUnit triggerdUnit)
        {
            // 计算推力方向（从地雷执行玩家）
            TSVector direction = (triggerdUnit.Position - self.Position).normalized;
            
            // 应用推力：让玩家位移5米
            triggerdUnit.Position += direction * 5;
            
            // 标记地雷已爆炸，5秒后重生（5000ms / 50ms每帧 = 100帧）
            self.IsExploded = true;
            self.RespawnFrame = self.LSWorld().Frame + 100;
        }
    }

    [EntitySystemOf(typeof(LSMine))]
    public static partial class LSMineSystem
    {
        [EntitySystem]
        private static void Awake(this LSMine self, TSVector position)
        {
            self.Position = position;
        }

        [EntitySystem]
        private static void Destroy(this LSMine self)
        {
        }
    }
}