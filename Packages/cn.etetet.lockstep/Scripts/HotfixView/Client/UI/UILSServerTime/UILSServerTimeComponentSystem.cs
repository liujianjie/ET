using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UILSServerTimeComponent))]
    [FriendOf(typeof(UILSServerTimeComponent))]
    public static partial class UILSServerTimeComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILSServerTimeComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            self.serverTimeBtn = rc.Get<GameObject>("ServerTimeBtn");
            self.serverTimeBtn.GetComponent<Button>().onClick.AddListener(() => { _ = self.OnServerTime(); });
        }


        public static async ETTask OnServerTime(this UILSServerTimeComponent self)
        {
            try
            {
                C2G_GetServerTime request = C2G_GetServerTime.Create();
                ClientSenderComponent sender = self.Root().GetComponent<ClientSenderComponent>();
                if (sender == null)
                {
                    Log.Error("ClientSenderComponent不存在!");
                    return;
                }
                G2C_GetServerTime response = await sender.Call(request) as G2C_GetServerTime;
                if (response.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"GetServerTime error: {response.Error}");
                    return;
                }
                Log.Error($"回消息了");
                long serverTime = response.ServerTime;
                Log.Info($"[客户端] 收到服务器时间: {serverTime}");
                
                System.DateTime dateTime = TimeInfo.Instance.ToDateTime(serverTime);
                Log.Info($"[客户端] 服务器时间: {dateTime:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception e)
            {
                Log.Error($"调用失败: {e}");
            }
        }
    }
}
