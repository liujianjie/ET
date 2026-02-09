using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSMineView))]
    [LSEntitySystemOf(typeof(LSMineView))]
    [FriendOf(typeof(LSMineView))]
    public static partial class LSMineViewSystem
    {
        [EntitySystem]
        private static void Awake(this LSMineView self, GameObject go)
        {
            self.GameObject = go;
            self.Transform = go.transform;
        }

        [EntitySystem]
        private static void Update(this LSMineView self)
        {
            LSMine mine = self.GetMine();
            if (mine == null) return;

            // 同步位置
            Vector3 pos = new Vector3((float)mine.Position.x, (float)mine.Position.y, (float)mine.Position.z);
            self.Transform.position = pos;

            // 同步颜色（爆炸后变灰）
            Renderer renderer = self.GameObject.GetComponent<Renderer>();
            renderer.material.color = mine.IsExploded ? Color.gray : Color.red;

            // 爆炸后隐藏，重生后显示
            self.GameObject.SetActive(!mine.IsExploded);
        }

        [LSEntitySystem]
        private static void LSRollback(this LSMineView self)
        {
            LSMine mine = self.GetMine();
            if (mine == null) return;

            // 同步位置
            Vector3 pos = new Vector3((float)mine.Position.x, (float)mine.Position.y, (float)mine.Position.z);
            self.Transform.position = pos;

            // 同步颜色（爆炸后变灰）
            Renderer renderer = self.GameObject.GetComponent<Renderer>();
            renderer.material.color = mine.IsExploded ? Color.gray : Color.red;

            // 爆炸后隐藏，重生后显示
            self.GameObject.SetActive(!mine.IsExploded);
        }

        private static LSMine GetMine(this LSMineView self)
        {
            LSMine mine = self.Mine;
            if (mine != null)
            {
                return mine;
            }

            self.Mine = (self.IScene as Room).LSWorld.GetComponent<LSMineComponent>().GetChild<LSMine>(self.Id);
            return self.Mine;
        }
    }
}
