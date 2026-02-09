using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSMineViewComponent))]
    public static partial class LSMineViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSMineViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LSMineViewComponent self)
        {
        }

        public static void Init(this LSMineViewComponent self)
        {
            Room room = self.Room();
            LSMineComponent lsMineComponent = room.LSWorld.GetComponent<LSMineComponent>();

            foreach (var kv in lsMineComponent.Children)
            {
                LSMine lsMine = kv.Value as LSMine;
                if (lsMine == null) continue;

                // 创建地雷 GameObject
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.localScale = Vector3.one * 0.5f;

                Renderer renderer = go.GetComponent<Renderer>();
                renderer.material.color = lsMine.IsExploded ? Color.gray : Color.red;

                Vector3 pos = new Vector3((float)lsMine.Position.x, (float)lsMine.Position.y, (float)lsMine.Position.z);
                go.transform.position = pos;

                self.AddChildWithId<LSMineView, GameObject>(lsMine.Id, go);
            }
        }
    }
}
