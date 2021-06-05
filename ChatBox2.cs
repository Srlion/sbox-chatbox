using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System;


partial class ChatEntry : Panel
{
	public Label NameLabel { get; internal set; }
	public Label Message { get; internal set; }
	public Image Avatar { get; internal set; }

	private RealTimeSince _timeSinceBorn = 0;
	private bool _canhide = true;


	public ChatEntry()
	{
		Avatar = Add.Image();
		NameLabel = Add.Label( "Name", "name" );
		Message = Add.Label( "Message", "message" );

		AddEvent( "show", Show );

		AddEvent( "hide", () =>
		{
			_canhide = true;
			_timeSinceBorn = 2;
		} );

}

public override void Tick()
	{
		base.Tick();

		if ( _canhide &&  _timeSinceBorn >= 5 )
		{
			AddClass( "hidden" );
			_canhide = false;
		}
	}

	public void Show()
	{
		_canhide = false;
		RemoveClass( "hidden" );
	}
}

public partial class ChatBox2 : Panel
{
	static ChatBox2 Current;
	public TextEntry Entry { get; protected set; }
	public Button EmojisButton { get; set; }
	public Panel Canvas { get; protected set; }

	public RealTimeSince TimeSinceBorn = 1;
	private string[] Emojis = {
		"😡",
		"😳",
		"😋",
		"🥰",
		"🥳",
		"🤩",
		"🤪"
	};

	public ChatBox2()
	{
		Current = this;
		SetTemplate( "/ui/chatbox2.html" );
		Canvas.PreferScrollToBottom = true;
		//Sandbox.UI.Emoji.Entries
		Entry.AddEvent( "onsubmit", () => Submit() );
		Entry.AddEvent( "onblur", () => Close() );
		Entry.AddEvent( "onchange", () => {
			Entry.Style.Height = 36;
			Entry.Style.Dirty();
			Entry.Style.Height = Entry.ScrollSize.y + 36;
			Entry.Style.Dirty();
		} );
		Entry.AcceptsFocus = true;
		Entry.AllowEmojiReplace = true;

		Sandbox.Hooks.Chat2.OnOpenChat += Open;
	}

	public override void Tick()
	{
		base.Tick();

		if ( !HasFocus ) return;
		if ( TimeSinceBorn <= 4 ) return;
		TimeSinceBorn = 0;

		var rand = new Random();
		EmojisButton.Text = Emojis[ rand.Next( 0, Emojis.Length ) ];
	}

	void Open()
	{
		AddClass( "open" );
		Canvas.BroadcastEvent( "show" );
		Entry.Focus();
		Entry.AllowEmojiReplace = true;
	}

	void Close()
	{
		RemoveClass( "open" );
		Canvas.BroadcastEvent( "hide" );
		Entry.Blur();
	}

	void Submit()
	{
		Close();

		var msg = Entry.Text.Trim();
		Entry.Text = "";


		if ( string.IsNullOrWhiteSpace( msg ) )
			return;

		Say( msg );
	}

	public void AddEntry( string name, string message, string avatar )
	{
		var e = Canvas.AddChild<ChatEntry>();
		e.Message.Text = message;
		e.NameLabel.Text = name;
		e.Avatar.SetTexture( avatar );

		if ( HasClass( "open" ) )
		{
			e.Show();
		}
		//e.SetFirstSibling();
		//Log.Info( Canvas.ScrollSize.ToString() );
		//Canvas.TryScroll(1);
		//Canvas.IsScrollAtBottom

		e.SetClass( "noname", string.IsNullOrEmpty( name ) );
		e.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );
	}

	[ClientCmd( "chat_add", CanBeCalledFromServer = true )]
	public static void AddChatEntry( string name, string message, string avatar = null )
	{
		Current?.AddEntry( name, message, avatar );

		if ( !Global.IsListenServer )
		{
			Log.Info( $"{name}: {message}" );
		}
	}

	[ClientCmd( "chat_addinfo", CanBeCalledFromServer = true )]
	public static void AddInformation( string message, string avatar = null )
	{
		Current?.AddEntry( null, message, avatar );
	}

	[ServerCmd( "say" )]
	public static void Say( string message )
	{
		Assert.NotNull( ConsoleSystem.Caller );

		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		Log.Info( $"{ConsoleSystem.Caller}: {message}" );
		AddChatEntry( To.Everyone, ConsoleSystem.Caller.Name, message, $"avatar:{ConsoleSystem.Caller.SteamId}" );
	}
}

namespace Sandbox.Hooks
{
	public static partial class Chat2
	{
		public static event Action OnOpenChat;

		[ClientCmd( "openchat" )]
		internal static void MessageMode()
		{
			OnOpenChat?.Invoke();
		}

	}
}
