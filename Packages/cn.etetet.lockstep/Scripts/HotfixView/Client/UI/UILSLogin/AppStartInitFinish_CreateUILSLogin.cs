namespace ET.Client
{
	[Event(SceneType.LockStep)]
	public class AppStartInitFinish_CreateUILSLogin: AEvent<Scene, AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
			// // 手动添加ClientSenderComponent并连接服务器（仅用于测试）
			// if (root.GetComponent<ClientSenderComponent>() == null)
			// {
			// 	ClientSenderComponent clientSender = root.AddComponent<ClientSenderComponent>();
			//
			// 	// 测试连接服务器（需要确保服务器已启动）
			// 	try
			// 	{
			// 		string address = "127.0.0.1:10002"; // Realm服务器地址，根据实际配置修改
			// 		await clientSender.LoginAsync(address, "test_account", "test_password");
			// 		Log.Debug("[测试] 已连接到服务器");
			// 	}
			// 	catch (System.Exception e)
			// 	{
			// 		Log.Error($"[测试] 连接服务器失败: {e}");
			// 	}
			// }
			// await UIHelper.Create(root, UIType.UILSServerTime, UILayer.Mid);
			await UIHelper.Create(root, UIType.UILSLogin, UILayer.Mid);
		}
	}
}
