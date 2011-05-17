using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using InfServer.Network;
using InfServer.Protocol;
using InfServer.Logic;

using Assets;

namespace InfServer.Game
{
	// Team Class
	/// Represents a single team in an arena
	///////////////////////////////////////////////////////
	public class Team : IChatTarget
	{	// Member variables
		///////////////////////////////////////////////////
		public Arena _arena;					//The arena we belong to
		public ZoneServer _server;				//The server we belong to

		private List<Player> _players;			//The list of players in this team

		public string _name;					//The name of the team
		public string _password;				//The password necessary to join the team
		public short _id;						//The ID of the team

		public bool _isPrivate;					//Is this a private team?

		public int _calculatedKills;			//The amount of kills for the team, since the last calculation
		public int _calculatedDeaths;			//The amoutn of deaths for the team, since the last calculation

		public CfgInfo.TeamInfo _info;		//The team-specific info

		//Events
		public event Action<Team> Empty;	//Called when an team runs out of players and is empty


		///////////////////////////////////////////////////
		// Accessors
		///////////////////////////////////////////////////
		/// <summary>
		/// Returns the number of unspecced players in the team
		/// </summary>
		public int ActivePlayerCount
		{
			get
			{
				return _players.Count(player => (!player.IsSpectator));
			}
		}

		/// <summary>
		/// Returns a list of the unspecced players in the team
		/// </summary>
		public IEnumerable<Player> ActivePlayers
		{
			get
			{
				return _players.Where(player => (!player.IsSpectator));
			}
		}

		/// <summary>
		/// Returns whether the team is a public team
		/// </summary>
		public bool IsPublic
		{
			get
			{
				return !_isPrivate;
			}
		}

		/// <summary>
		/// Returns whether the team is a public team
		/// </summary>
		public bool IsSpec
		{
			get
			{
				return _name.Equals("spec", StringComparison.OrdinalIgnoreCase);
			}
		}

		/// <summary>
		/// Gives a short summary of this team
		/// </summary>
		public override string ToString()
		{	//Return the player credentials
			return String.Format("{0} ({1})", _name, _id);
		}


		///////////////////////////////////////////////////
		// Member Functions
		///////////////////////////////////////////////////
		/// <summary>
		/// Generic constructor
		/// </summary>
		public Team(Arena owner, ZoneServer server)
		{
			_arena = owner;
			_server = server;

			_players = new List<Player>();
		}

		#region State
		/// <summary>
		/// Called when the team is empty of players 
		/// </summary>
		private void empty()
		{	//Let everyone know
			if (Empty != null)
				Empty(this);
		}

		/// <summary>
		/// Adds the given player to the team and notifies others
		/// </summary>
		public void addPlayer(Player player)
		{	//Sanity checks
			if (player._bIngame && player._team == this)
			{
				Log.write(TLog.Warning, "Attempted to add player to his own team. {0}", player);
				return;
			}

			//Have him lose his current team
			if (player._team != null)
				player._team.lostPlayer(player);

			//Set his new team!
			player._team = this;

			//Welcome him in
			_players.Add(player);

			//Do we need to notify, or is this an initial team?
			if (player._bIngame)
				//Let everyone know
				Helpers.Player_SetTeam(_arena.Players, player, this);

			player.sendMessage(0, "Team joined: " + _name);

			//If he isn't a spectator, trigger the event too
			if (!player.IsSpectator)
				Logic_Assets.RunEvent(player, _server._zoneConfig.EventInfo.joinTeam);
		}

		/// <summary>
		/// Handles the loss of a player
		/// </summary>
		public void lostPlayer(Player player)
		{	//Sob, let him go
			_players.Remove(player);

			//Do we have any players left?
			if (_players.Count == 0)
				//Nope. It's closing time.
				empty();

			//Reset any team-related state the player might have had
			_arena.flagResetPlayer(player);

			//We don't need to inform others of the loss of team,
			//as the player will either be leaving or joining another
			//team - both of which infer leaving the old team.
		}
		#endregion

		#region Social
        /// <summary>
        /// Returns the list of players to for team chat (' or ")
        /// </summary>
        public IEnumerable<Player> getChatTargets()
        {
            return _players;
        }

		/// <summary>
		/// Triggered when a player has sent chat to his current team
		/// </summary>
		public void playerTeamChat(Player from, CS_Chat chat)
		{	//Route it to our entire player list!
			Helpers.Player_RouteChat(this, from, chat);
		}

		/// <summary>
		/// Triggered when a player has sent chat to an enemy team
		/// </summary>
		public void playerEnemyTeamChat(Player from, CS_Chat chat)
		{	//Route it to both the enemy team and our team
		}
		#endregion

		#region Stats
		/// <summary>
		/// Pre-calculates the statistics for the team, and stores them in the _calculated* variables
		/// </summary>
		public void precalculateStats(bool bCurrent)
		{
			int kills = 0;
			int deaths = 0;

			//Add up the stats for each player
			foreach (Player player in ActivePlayers)
			{	
				if (bCurrent)
				{
					kills = kills + player.StatsCurrentGame.kills;
					deaths = deaths + player.StatsCurrentGame.deaths;
				}
				else
				{
					kills = kills + player.StatsLastGame.kills;
					deaths = deaths + player.StatsLastGame.deaths;
				}
			}

			_calculatedKills = kills;
			_calculatedDeaths = deaths;
		}
		#endregion
	}
}