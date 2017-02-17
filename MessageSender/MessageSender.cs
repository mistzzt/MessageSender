using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace MessageSender
{
	[ApiVersion(2, 0)]
	public class MessageSender : TerrariaPlugin
	{
		public override string Name => GetType().Name;

		public override string Author => "MistZZT";

		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		public override string Description => "Send message to players";

		public MessageSender(Main game) : base(game)
		{
			Order = 10;
		}

		public override void Initialize()
		{
			Commands.ChatCommands.Add(new Command("messagesender.sendmsg", SendMessage, "sendmsg")
			{
				AllowServer = false,
				HelpText = "Send message to player"
			});

			Commands.ChatCommands.Add(new Command("messagesender.sendother", SendMessageToOther, "sendother")
			{
				AllowServer = true,
				HelpText = "Send message to other players"
			});

			Commands.ChatCommands.Add(new Command("messagesender.sendct", SendCombatText, "sendct")
			{
				AllowServer = false,
				HelpText = "Send combat text to player"
			});

			Commands.ChatCommands.Add(new Command("messagesender.sendctother", SendCombatTextToOther, "sendctother")
			{
				AllowServer = true,
				HelpText = "Send combat text to other players"
			});

			Commands.ChatCommands.Add(new Command("messagesender.sendctpos", SendCombatTextToPosition, "sendctpos")
			{
				AllowServer = true,
				HelpText = "Send combat text to a specific position"
			});
		}

		private static void SendMessage(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /sendmessage <Messages> [r] [g] [b]");
				return;
			}

			if (args.Parameters.Count >= 4)
			{
				byte r, g, b;
				var rgbs = args.Parameters.Skip(args.Parameters.Count - 3).ToArray();
				if (!byte.TryParse(rgbs[0], out r) || !byte.TryParse(rgbs[1], out g) || !byte.TryParse(rgbs[2], out b))
				{
					args.Player.SendInfoMessage(string.Join(" ", args.Parameters));
				}
				else
				{
					args.Player.SendMessage(string.Join(" ", args.Parameters.GetRange(0, args.Parameters.Count - 3)), r, g, b);
				}
			}
			else
			{
				args.Player.SendInfoMessage(string.Join(" ", args.Parameters));
			}
		}

		private static void SendMessageToOther(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /sendother <Player> <Messages> [r] [g] [b]");
				return;
			}

			var players = TShock.Utils.FindPlayer(args.Parameters[0]);
			if (players.Count > 1)
			{
				TShock.Utils.SendMultipleMatchError(args.Player, players.Select(p => p.Name));
				return;
			}
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player name!");
				return;
			}

			var player = players[0];
			if (args.Parameters.Count >= 5)
			{
				byte r, g, b;
				var rgbs = args.Parameters.Skip(args.Parameters.Count - 3).ToArray();
				if (!byte.TryParse(rgbs[0], out r) || !byte.TryParse(rgbs[1], out g) || !byte.TryParse(rgbs[2], out b))
				{
					player.SendInfoMessage(string.Join(" ", args.Parameters.Skip(1)));
				}
				else
				{
					player.SendMessage(string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 4)), r, g, b);
				}
			}
			else
			{
				player.SendInfoMessage(string.Join(" ", args.Parameters.Skip(1)));
			}
		}

		private static void SendCombatText(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /sendct [-b/--broadcast] <Messages> [r] [g] [b]");
				return;
			}

			var broadcast = false;
			if (string.Equals(args.Parameters[0], "-b", StringComparison.OrdinalIgnoreCase) ||
			    string.Equals(args.Parameters[0], "--broadcast", StringComparison.OrdinalIgnoreCase))
			{
				broadcast = true;
				args.Parameters.RemoveAt(0);
			}

			Color color;
			string text;

			if (args.Parameters.Count >= 4)
			{
				byte r, g, b;
				var rgbs = args.Parameters.Skip(args.Parameters.Count - 3).ToArray();
				
				if (!byte.TryParse(rgbs[0], out r) || !byte.TryParse(rgbs[1], out g) || !byte.TryParse(rgbs[2], out b))
				{
					text = string.Join(" ", args.Parameters);
					color = Color.Yellow;
				}
				else
				{
					text = string.Join(" ", args.Parameters.GetRange(0, args.Parameters.Count - 3));
					color = new Color(r, g, b);
				}
			}
			else
			{
				text = string.Join(" ", args.Parameters);
				color = Color.Yellow;
			}

			var position = GetPosition(args.Player.TPlayer.getRect());

			if (broadcast)
				TSPlayer.All.SendData(PacketTypes.CreateCombatText, text, (int)color.PackedValue, position.X, position.Y);
			else
				args.Player.SendData(PacketTypes.CreateCombatText, text, (int)color.PackedValue, position.X, position.Y);
		}

		private static void SendCombatTextToOther(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /sendctother [-b/--broadcast] <Player> <Messages> [r] [g] [b]");
				return;
			}

			var broadcast = false;
			if (string.Equals(args.Parameters[0], "-b", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(args.Parameters[0], "--broadcast", StringComparison.OrdinalIgnoreCase))
			{
				broadcast = true;
				args.Parameters.RemoveAt(0);
			}

			var players = TShock.Utils.FindPlayer(args.Parameters[0]);
			if (players.Count > 1)
			{
				TShock.Utils.SendMultipleMatchError(args.Player, players.Select(p => p.Name));
				return;
			}
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player name!");
				return;
			}
			var player = players[0];

			Color color;
			string text;

			if (args.Parameters.Count >= 5)
			{
				byte r, g, b;
				var rgbs = args.Parameters.Skip(args.Parameters.Count - 3).ToArray();
				if (!byte.TryParse(rgbs[0], out r) || !byte.TryParse(rgbs[1], out g) || !byte.TryParse(rgbs[2], out b))
				{
					color = Color.Yellow;
					text = string.Join(" ", args.Parameters.Skip(1));
				}
				else
				{
					color = new Color(r, g, b);
					text = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 4));
				}
			}
			else
			{
				color = Color.Yellow;
				text = string.Join(" ", args.Parameters.Skip(1));
			}

			var position = GetPosition(player.TPlayer.getRect());

			if (broadcast)
				TSPlayer.All.SendData(PacketTypes.CreateCombatText, text, (int)color.PackedValue, position.X, position.Y);
			else
				player.SendData(PacketTypes.CreateCombatText, text, (int)color.PackedValue, position.X, position.Y);
		}

		private static void SendCombatTextToPosition(CommandArgs args)
		{
			if (args.Parameters.Count < 4)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /sendctpos [-b/--broadcast] <tileX> <tileY> <Player> <Messages> [r] [g] [b]");
				return;
			}

			var broadcast = false;
			if (string.Equals(args.Parameters[0], "-b", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(args.Parameters[0], "--broadcast", StringComparison.OrdinalIgnoreCase))
			{
				broadcast = true;
				args.Parameters.RemoveAt(0);
			}

			int x, y;
			if (!int.TryParse(args.Parameters[0], out x) || !int.TryParse(args.Parameters[1], out y))
			{
				args.Player.SendErrorMessage("Invalid position!");
				return;
			}
			for(var i = 0; i < 2; i++)
				args.Parameters.RemoveAt(0);

			var players = TShock.Utils.FindPlayer(args.Parameters[0]);
			if (players.Count > 1)
			{
				TShock.Utils.SendMultipleMatchError(args.Player, players.Select(p => p.Name));
				return;
			}
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player name!");
				return;
			}
			var player = players[0];

			Color color;
			string text;

			if (args.Parameters.Count >= 5)
			{
				byte r, g, b;
				var rgbs = args.Parameters.Skip(args.Parameters.Count - 3).ToArray();
				if (!byte.TryParse(rgbs[0], out r) || !byte.TryParse(rgbs[1], out g) || !byte.TryParse(rgbs[2], out b))
				{
					color = Color.Yellow;
					text = string.Join(" ", args.Parameters.Skip(1));
				}
				else
				{
					color = new Color(r, g, b);
					text = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 4));
				}
			}
			else
			{
				color = Color.Yellow;
				text = string.Join(" ", args.Parameters.Skip(1));
			}

			var position = GetPosition(new Rectangle(x * 16, y * 16, 0, 0));

			if (broadcast)
				TSPlayer.All.SendData(PacketTypes.CreateCombatText, text, (int)color.PackedValue, position.X, position.Y);
			else
				player.SendData(PacketTypes.CreateCombatText, text, (int)color.PackedValue, position.X, position.Y);
		}

		private static Vector2 GetPosition(Rectangle location)
		{
			var vector = Vector2.Zero;

			var position =  new Vector2(
				location.X + location.Width * 0.5f - vector.X * 0.5f,
				location.Y + location.Height * 0.25f - vector.Y * 0.5f
			);

			position.X += Main.rand.Next(-(int)(location.Width * 0.5), (int)(location.Width * 0.5) + 1);
			position.Y += Main.rand.Next(-(int)(location.Height * 0.5), (int)(location.Height * 0.5) + 1);

			return position;
		}
	}
}
