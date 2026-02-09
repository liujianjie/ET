
using MemoryPack;
using TrueSync;

namespace ET
{
    [ChildOf(typeof(LSMineComponent))]
    public class LSMine : LSEntity, IAwake<TSVector>, IDestroy
    {
        // 地雷位置
        public TSVector Position { get; set; }
        // 是否已爆炸（爆炸后等待重生）
        public bool IsExploded { get; set; }
        // 重生时间戳（LSWorld.Frame）
        public int RespawnFrame { get; set; }
    }
}


