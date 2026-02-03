namespace ET.Server
{
    [MessageHandler(SceneType.RoomRoot)]
    public class C2Room_CheckHashHandler: MessageHandler<Scene, C2Room_CheckHash>
    {
        protected override async ETTask Run(Scene root, C2Room_CheckHash message)
        {
            Room room = root.GetComponent<Room>();
            // 获取服务端哈希
            long hash = room.FrameBuffer.GetHash(message.Frame);
            if (message.Hash != hash)
            {
                byte[] bytes = room.FrameBuffer.Snapshot(message.Frame).ToArray();
                // 发送服务端快照给客户端调试
                Room2C_CheckHashFail room2CCheckHashFail = Room2C_CheckHashFail.Create();
                room2CCheckHashFail.Frame = message.Frame;
                room2CCheckHashFail.LSWorldBytes = bytes;
                room.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Send(message.PlayerId, room2CCheckHashFail);
            }
            await ETTask.CompletedTask;
        }
    }
}