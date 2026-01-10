namespace ET.Client
{
	[Event(SceneType.LockStep)]
	public class LoginFinish_CreateUILSServerTime: AEvent<Scene, LoginFinish>
	{
		protected override async ETTask Run(Scene scene, LoginFinish args)
		{
			await UIHelper.Create(scene, UIType.UILSServerTime, UILayer.Mid);
		}
	}
}
