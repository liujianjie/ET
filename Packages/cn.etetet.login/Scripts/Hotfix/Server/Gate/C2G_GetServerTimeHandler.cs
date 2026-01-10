using System;
using ET;

[MessageSessionHandler(SceneType.Gate)]
public class C2G_GetServerTimeHandler : MessageSessionHandler<C2G_GetServerTime, G2C_GetServerTime>
{
    protected override async ETTask Run(Session session, C2G_GetServerTime request, G2C_GetServerTime response)
    {
        using C2G_GetServerTime _ = request; 
        long serverTime = TimeInfo.Instance.ServerNow();
        response.ServerTime = serverTime;
        Log.Error($"C2G_GetServerTimeHandler: {serverTime}");
        await ETTask.CompletedTask;
    }
}
