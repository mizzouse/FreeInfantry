﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using InfServer.Protocol;
using InfServer.Data;

namespace InfServer.Logic
{	// Logic_Stats Class
	/// Handles statistics functionality
	///////////////////////////////////////////////////////
	class Logic_Stats
	{
		/// <summary>
		/// Writes a scorechart element to a memory stream
		/// </summary>
		static private void writeElementToBuffer(Data.DB.stats stat, MemoryStream stream)
		{
			BinaryWriter bw = new BinaryWriter(stream);

			bw.Write(stat.players[0].alias1.name.ToCharArray());
			bw.Write((byte)0);

			Data.DB.squad squad = stat.players[0].squad1;
			string squadname = "";
			if (squad != null)
				squadname = squad.name;

			bw.Write(squadname.ToCharArray());
			bw.Write((byte)0);

			bw.Write((short)2);
			bw.Write(stat.vehicleDeaths);
			bw.Write(stat.vehicleKills);
			bw.Write(stat.killPoints);
			bw.Write(stat.deathPoints);
			bw.Write(stat.assistPoints);
			bw.Write(stat.bonusPoints);
			bw.Write(stat.kills);
			bw.Write(stat.deaths);
			bw.Write((int)0);
			bw.Write(stat.playSeconds);
			bw.Write(stat.zonestat1);
			bw.Write(stat.zonestat2);
			bw.Write(stat.zonestat3);
			bw.Write(stat.zonestat4);
			bw.Write(stat.zonestat5);
			bw.Write(stat.zonestat6);
			bw.Write(stat.zonestat7);
			bw.Write(stat.zonestat8);
			bw.Write(stat.zonestat9);
			bw.Write(stat.zonestat10);
			bw.Write(stat.zonestat11);
			bw.Write(stat.zonestat12);
		}

		/// <summary>
		/// Handles a player update request
		/// </summary>
		static public void Handle_CS_PlayerStatsRequest(CS_PlayerStatsRequest<Zone> pkt, Zone zone)
		{	//Attempt to find the player in question
			Zone.Player player = zone.getPlayer(pkt.player.id);
			if (player == null)
			{	//Make a note
				Log.write(TLog.Warning, "Ignoring player stats request for #{0}, not present in zone mirror.", pkt.player.id);
				return;
			}

			using (InfantryDataContext db = zone._server.getContext())
			{	//What sort of request are we dealing with?
				switch (pkt.type)
				{
					case CS_PlayerStatsRequest<Zone>.ChartType.ScoreLifetime:
						{	//Get the top100 stats sorted by points
							var stats = (from st in db.stats
										 where st.zone1 == zone._zone
										 orderby st.assistPoints + st.bonusPoints + st.killPoints descending
										 select st).Take(100);
							MemoryStream stream = new MemoryStream();

							foreach (Data.DB.stats stat in stats)
								writeElementToBuffer(stat, stream);

							SC_PlayerStatsResponse<Zone> response = new SC_PlayerStatsResponse<Zone>();

							response.player = pkt.player;
							response.type = CS_PlayerStatsRequest<Zone>.ChartType.ScoreLifetime;
							response.columns = "Top100 Lifetime Score,Name,Squad";
							response.data = stream.ToArray();

							zone._client.sendReliable(response, 1);
						}
						break;
				}
			}
		}

		/// <summary>
		/// Registers all handlers
		/// </summary>
		[RegistryFunc]
		static public void Register()
		{
			CS_PlayerStatsRequest<Zone>.Handlers += Handle_CS_PlayerStatsRequest;
		}
	}
}