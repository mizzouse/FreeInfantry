﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Assets;

using InfServer.Bots;
using InfServer.Protocol;

namespace InfServer.Game.Commands.Mod
{
	/// <summary>
	/// Provides a series of functions for handling mod commands
	/// </summary>
	public class Game
	{
		/// <summary>
		/// Restarts the current game
		/// </summary>
        static public void restart(Player player, Player recipient, string payload, int bong)
		{	//End the current game and restart a new one
			player._arena.gameEnd();
            player._arena.gameStart();
		}

		/// <summary>
		/// Registers all handlers
		/// </summary>
		[Commands.RegistryFunc(HandlerType.ModCommand)]
		static public IEnumerable<Commands.HandlerDescriptor> Register()
		{
			yield return new HandlerDescriptor(restart, "restart",
				"Restarts the current game.",
				"*restart", InfServer.Data.PlayerPermission.ArenaMod, true);
		}
	}
}