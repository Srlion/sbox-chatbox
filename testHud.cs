using Sandbox;
using Sandbox.UI;

[Library]
public partial class TestHud : HudEntity<RootPanel>
{
	public TestHud()
	{
		if ( !IsClient )
			return;

		RootPanel.AddChild<ChatBox2>();
	}
}
