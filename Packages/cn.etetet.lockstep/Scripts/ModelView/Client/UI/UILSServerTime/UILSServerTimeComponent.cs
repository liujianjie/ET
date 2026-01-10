using UnityEngine;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILSServerTimeComponent: Entity, IAwake
	{
		public GameObject serverTimeBtn;
	}
}
