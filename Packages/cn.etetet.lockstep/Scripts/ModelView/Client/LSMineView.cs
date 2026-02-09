using UnityEngine;

namespace ET
{
    [ChildOf(typeof(LSMineViewComponent))]
    public class LSMineView : Entity, IAwake<GameObject>, IUpdate, ILSRollback
    {
        public GameObject GameObject { get; set; }
        public Transform Transform { get; set; }
        public EntityRef<LSMine> Mine;
    }
}
