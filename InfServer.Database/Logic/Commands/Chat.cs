﻿using System;
using System.Collections.Generic;
using System.Linq;

using InfServer.Protocol;
using InfServer.Data;

namespace InfServer.Logic
{
    class Logic_ChatCommands
    {
        /// <summary>
        /// Handles a query packet
        /// </summary>
        static public void Handle_CS_Query(CS_Query<Zone> pkt, Zone zone)
        {
            using (InfantryDataContext db = zone._server.getContext())
            {
                switch (pkt.queryType)
                {
                    case CS_Query<Zone>.QueryType.accountinfo:
                        Data.DB.alias from = db.alias.SingleOrDefault(a => a.name.Equals(pkt.sender));
                        var aliases = db.alias.Where(a => a.account == from.account);
                        zone._server.sendMessage(zone, pkt.sender, "Account Info");


                        Int64 total = 0;
                        int days = 0;
                        int hrs = 0;
                        int mins = 0;
                        //Loop through each alias to calculate time played
                        foreach (var alias in aliases)
                        {


                            //Does this alias even have any time played?
                            if (alias.timeplayed.HasValue)
                            {
                                TimeSpan timeplayed = TimeSpan.FromMinutes(alias.timeplayed.Value);
                                days = (int)timeplayed.Days;
                                hrs = (int)timeplayed.Hours;
                                mins = (int)timeplayed.Minutes;

                                total += alias.timeplayed.Value;
                            }

                            //Send it
                            zone._server.sendMessage(zone, pkt.sender, String.Format("~{0} ({1}d {2}h {3}m)", alias.name, days, hrs, mins));

                        }
                        //Calculate total time played across all aliases.
                        if (total != 0)
                        {
                            TimeSpan totaltime = TimeSpan.FromMinutes(total);
                            days = (int)totaltime.Days;
                            hrs = (int)totaltime.Hours;
                            mins = (int)totaltime.Minutes;
                            //Send it
                            zone._server.sendMessage(zone, pkt.sender, String.Format("!Grand Total: {0}d {1}h {2}m", days, hrs, mins));
                        }
                        break;

                    case CS_Query<Zone>.QueryType.whois:
                        zone._server.sendMessage(zone, pkt.sender, "&Whois Information");

                        //Query for an IP?
                        System.Net.IPAddress ip;

                        if (System.Net.IPAddress.TryParse(pkt.payload, out ip))
                        {
                            aliases = db.alias.Where(a => a.IPAddress.Equals(ip.ToString()));
                            zone._server.sendMessage(zone, pkt.sender, "*" + ip.ToString());
                        }
                        //Alias!
                        else
                        {
                            Data.DB.alias who = db.alias.SingleOrDefault(a => a.name.Equals(pkt.payload));
                            aliases = db.alias.Where(a => a.account.Equals(who.account));
                            zone._server.sendMessage(zone, pkt.sender, "*" + pkt.payload);
                        }

                        zone._server.sendMessage(zone, pkt.sender, "&Aliases: " + aliases.Count());
                        //Loop through them and display
                        foreach (var alias in aliases)
                            zone._server.sendMessage(zone, pkt.sender, String.Format("*[{0}] {1} (IP={2} Created={3} LastAccess={4})", alias.account, alias.name, alias.IPAddress, alias.creation.ToString(), alias.lastAccess.ToString()));
                        break;

                    case CS_Query<Zone>.QueryType.emailupdate:
                        zone._server.sendMessage(zone, pkt.sender, "&Email Update");

                        Data.DB.account account = db.alias.SingleOrDefault(a => a.name.Equals(pkt.sender)).account1;

                        //Update his email
                        account.email = pkt.payload;
                        db.SubmitChanges();
                        zone._server.sendMessage(zone, pkt.sender, "*Email updated to: " + pkt.payload);
                        break;

                    case CS_Query<Zone>.QueryType.find:
                        int minlength = 3;
                        var results = new List<KeyValuePair<string, Zone.Player>>();

                        foreach (KeyValuePair<string, Zone.Player> player in zone._server._players)
                        {
                            if (player.Key.ToLower() == pkt.payload.ToLower())
                            {
                                //Have they found the exact player they were looking for?
                                results.Add(player);
                                break;
                            }
                            else if (player.Key.ToLower().Contains(pkt.payload.ToLower()) && pkt.payload.Length >= minlength)
                                results.Add(player);
                        }

                        if (results.Count > 0)
                        {
                            zone._server.sendMessage(zone, pkt.sender, "&Search Results");
                            foreach (KeyValuePair<string, Zone.Player> result in results)
                            {
                                zone._server.sendMessage(zone, pkt.sender,
                                    String.Format("*Found: {0} (Zone: {1})", //TODO: Arena??
                                    result.Value.alias, result.Value.zone._zone.name, result.Value.arena));
                            }
                        }
                        else if (pkt.payload.Length < minlength)
                            zone._server.sendMessage(zone, pkt.sender, "Search query must contain at least " + minlength + " characters");
                        else
                            zone._server.sendMessage(zone, pkt.sender, "Sorry, we couldn't locate any players online by that alias");
                        break;

                    case CS_Query<Zone>.QueryType.online:
                        DBServer server = zone._server;

                        foreach (Zone z in zone._server._zones)
                        {
                            server.sendMessage(zone, pkt.sender, String.Format("~Server={0} Players={1}", z._zone.name, z._players.Count()));
                        }
                        zone._server.sendMessage(zone, pkt.sender, String.Format("Infantry (Total={0}) (Peak={1})", server._players.Count(), server.playerPeak));
                        break;

                    case CS_Query<Zone>.QueryType.zonelist:
                        //Collect the list of zones and send it over
                        List<ZoneInstance> zoneList = new List<ZoneInstance>();
                        foreach (Zone z in zone._server._zones.Where(zn => zn._zone.active == 1))
                        {
                            int playercount;
                            //Invert player count of our current zone
                            if (z._zone.port == Convert.ToInt32(pkt.payload))
                                playercount = -z._players.Count;
                            else
                                playercount = z._players.Count;
                            //Add it to our list
                            zoneList.Add(new ZoneInstance(0,
                                z._zone.name,
                                z._zone.ip,
                                Convert.ToInt16(z._zone.port),
                                playercount));
                        }
                        SC_Zones<Zone> zl = new SC_Zones<Zone>();
                        zl.requestee = pkt.sender;
                        zl.zoneList = zoneList;
                        zone._client.sendReliable(zl, 1);
                        break;

                    case CS_Query<Zone>.QueryType.history:
                        int page = Convert.ToInt32(pkt.payload);
                        int resultsperpage = 30;

                        zone._server.sendMessage(zone, pkt.sender, "!Command History (" + page + ")");

                        //Find all commands!
                        List<Data.DB.history> cmds = db.histories.Where(c =>
                            c.id >= (db.histories.Count() - (resultsperpage * (page + 1))) &&
                            c.id < (db.histories.Count() - (resultsperpage * page))).ToList();

                        //List them
                        foreach (Data.DB.history h in cmds)
                            zone._server.sendMessage(zone, pkt.sender, String.Format("!{0} [{1}:{2}] {3}> :{4}: {5}",
                                Convert.ToString(h.date), h.zone, h.arena, h.sender, h.recipient, h.command));

                        zone._server.sendMessage(zone, pkt.sender, "End of page, use ?history 1, ?history 2, etc to navigate previous pages");
                        break;

                    case CS_Query<Zone>.QueryType.global:
                        foreach(Zone z in zone._server._zones)
                            z._server.sendMessage(z, "*", pkt.payload);
                        break;
                }
            }
        }

        /// <summary>
        /// Handles a ?squad command query
        /// </summary>
        static public void Handle_CS_SquadQuery(CS_Squads<Zone> pkt, Zone zone)
        {
            //Clean up the payload
            pkt.payload = pkt.payload.Trim();
            using (InfantryDataContext db = zone._server.getContext())
            {
                //Get the associated player making the command
                Data.DB.player dbplayer = db.zones.First(z => z.id == zone._zone.id).players.First(p => p.alias1.name == pkt.alias);

                switch (pkt.queryType)
                {   //Differentiate the type of query
                    case CS_Squads<Zone>.QueryType.create:
                        //Sanity checks
                        if (dbplayer.squad != null)
                        {   //traitor is already in a squad
                            zone._server.sendMessage(zone, pkt.alias, "You cannot create a squad if you are already in one (" + dbplayer.squad1.name + ")");
                            return;
                        }
                        if (!pkt.payload.Contains(':'))
                        {   //invalid payload
                            zone._server.sendMessage(zone, pkt.alias, "Invalid syntax. Use: ?squadcreate [squadname]:[squadpassword]");
                            return;
                        }
                        if (!char.IsLetterOrDigit(pkt.payload[0]) || pkt.payload.Split(':').ElementAt(0).Length == 0)
                        {   //invalid name
                            zone._server.sendMessage(zone, pkt.alias, "Invalid squad name");
                            return;
                        }
                        if (db.squads.Count(s => s.name.ToLower().Equals(pkt.payload.Split(':').ElementAt(0).ToLower())) != 0)
                        {   //This squad already exists!
                            zone._server.sendMessage(zone, pkt.alias, "A squad with specified name already exists");
                            return;
                        }
                        //Create the new squad
                        Data.DB.squad newsquad = new Data.DB.squad();

                        newsquad.name = pkt.payload.Split(':').ElementAt(0);
                        newsquad.password = pkt.payload.Split(':').ElementAt(1);
                        newsquad.owner = dbplayer.id;
                        newsquad.dateCreated = DateTime.Now;

                        db.squads.InsertOnSubmit(newsquad);

                        dbplayer.squad = newsquad.id;

                        zone._server.sendMessage(zone, pkt.alias, "Successfully created squad: " + newsquad.name + ". Quit and rejoin to be able to use # to squad chat");
                        Log.write(TLog.Normal, "Player {0} created squad {1} in zone {2}", pkt.alias, newsquad.name, zone._zone.name);
                        break;

                    case CS_Squads<Zone>.QueryType.invite:
                        //Sanity checks
                        if (dbplayer.squad == null)
                            return;
                        if (dbplayer.squad1.owner != dbplayer.id)
                        {
                            zone._server.sendMessage(zone, pkt.alias, "Only squad owners may send or revoke squad invitations");
                            return;
                        }
                        string[] sInvite = pkt.payload.Split(':');
                        if (sInvite.Count() != 3)
                        {
                            zone._server.sendMessage(zone, pkt.alias, "Invalid syntax. Use: ?squadinvite [add/remove]:[playername]:[squadname]");
                            return;
                        }

                        //Adding or removing a squad invitation?
                        bool bAdd = (sInvite[0].ToLower().Equals("add")) ? true : false;
                        //The target player
                        Data.DB.player invitePlayer = db.players.SingleOrDefault(p => p.id == zone._server.getPlayer(sInvite[1]).dbid);
                        if (invitePlayer == null)
                        {   //No such player!
                            zone._server.sendMessage(zone, pkt.alias, "No player found in this zone by that alias");
                            return;
                        }

                        if (bAdd)
                        {   //Send a squad invite
                            if (zone._server._squadInvites.Contains(new KeyValuePair<int, Data.DB.player>((int)dbplayer.squad, invitePlayer)))
                            {
                                zone._server.sendMessage(zone, pkt.alias, "You have already sent a squad invite to " + invitePlayer.alias1.name);
                            }
                            else
                            {
                                zone._server._squadInvites.Add(new KeyValuePair<int, Data.DB.player>((int)dbplayer.squad, invitePlayer));
                                zone._server.sendMessage(zone, pkt.alias, "Squad invite sent to  " + invitePlayer.alias1.name);
                            }
                        }
                        else
                        {   //Remove a squad invite
                            if (zone._server._squadInvites.Contains(new KeyValuePair<int, Data.DB.player>((int)dbplayer.squad, invitePlayer)))
                            {
                                zone._server._squadInvites.Remove(new KeyValuePair<int, Data.DB.player>((int)dbplayer.squad, invitePlayer));
                                zone._server.sendMessage(zone, pkt.alias, "Revoked squad invitation from " + invitePlayer.alias1.name);
                            }
                            else
                            {
                                zone._server.sendMessage(zone, pkt.alias, "Found no squad invititations sent to  " + invitePlayer.alias1.name);
                            }
                        }
                        break;

                    case CS_Squads<Zone>.QueryType.kick:
                        //Sanity checks
                        if (dbplayer.squad == null)
                            return;
                        if (dbplayer.squad1.owner != dbplayer.id)
                        {
                            zone._server.sendMessage(zone, pkt.alias, "Only squad owners may kick players");
                            return;
                        }

                        //The target player
                        Data.DB.player kickPlayer = db.players.SingleOrDefault(p => p.id == zone._server.getPlayer(pkt.payload).dbid);
                        if (kickPlayer == null)
                        {   //No such player!
                            zone._server.sendMessage(zone, pkt.alias, "No player found in this zone by that alias");
                            return;
                        }
                        if (kickPlayer.squad == null || kickPlayer.squad != dbplayer.squad)
                        {   //Liar!
                            zone._server.sendMessage(zone, pkt.alias, "You may only kick players from your own squad");
                            return;
                        }

                        //Kick him!
                        kickPlayer.squad = null;
                        zone._server.sendMessage(zone, pkt.alias, "You have kicked " + kickPlayer.alias1.name + " from your squad");
                        zone._server.sendMessage(zone, kickPlayer.alias1.name, "You have been kicked from squad " + dbplayer.squad1.name);
                        break;

                    case CS_Squads<Zone>.QueryType.transfer:
                        //Sanity checks
                        if (dbplayer.squad == null || pkt.payload == "")
                            return;
                        if (dbplayer.squad1.owner != dbplayer.id)
                        {
                            zone._server.sendMessage(zone, pkt.alias, "Only squad owners may transfer squad ownership");
                            return;
                        }

                        //The target player
                        Data.DB.player transferPlayer = db.players.SingleOrDefault(p => p.id == zone._server.getPlayer(pkt.payload).dbid);
                        if (transferPlayer == null || transferPlayer.squad != dbplayer.squad)
                        {   //No such player!
                            zone._server.sendMessage(zone, pkt.alias, "No player found in your squad by that alias");
                            return;
                        }

                        //Transfer ownership to him
                        transferPlayer.squad1.owner = transferPlayer.id;
                        zone._server.sendMessage(zone, pkt.alias, "You have promoted " + transferPlayer.alias1.name + " to squad captain");
                        zone._server.sendMessage(zone, transferPlayer.alias1.name, "You have promoted to squad captain of " + transferPlayer.squad1.name);
                        break;

                    case CS_Squads<Zone>.QueryType.leave:
                        //Sanity checks
                        if (dbplayer.squad == null)
                        {
                            zone._server.sendMessage(zone, pkt.alias, "You aren't in a squad");
                            return;
                        }

                        //Get his squad brothers! (if any...)
                        IQueryable<Data.DB.player> squadmates = db.players.Where(p => p.squad == dbplayer.squad && p.squad != null);

                        //Is he the captain?
                        if (dbplayer.squad1.owner == dbplayer.id)
                        {   //We might need to dissolve the team!
                            if (squadmates.Count() == 1)
                            {   //He's the only one left on the squad... dissolve it!
                                db.squads.DeleteOnSubmit(dbplayer.squad1);
                                dbplayer.squad = null;
                                zone._server.sendMessage(zone, pkt.alias, "Your squad has been dissolved");
                                return;
                            }
                            else
                            {   //There are other people on the squad!
                                zone._server.sendMessage(zone, pkt.alias, "You can't leave a squad that you're the captain of! Either transfer ownership or kick everybody first");
                                return;
                            }
                        }

                        //Leave the squad...
                        dbplayer.squad = null;
                        zone._server.sendMessage(zone, pkt.alias, "You have left your squad");
                        //Notify his squadmates
                        foreach (Data.DB.player sm in squadmates)
                            zone._server.sendMessage(zone, sm.alias1.name, pkt.alias + " has left your squad");

                        break;

                    case CS_Squads<Zone>.QueryType.online:
                        //Do we list his own squad or another?
                        Data.DB.squad targetSquadOnline = (pkt.payload == "") ? dbplayer.squad1 : db.squads.FirstOrDefault(s => s.name == pkt.payload);
                        if (targetSquadOnline == null)
                        {   //No squad found!
                            zone._server.sendMessage(zone, pkt.alias, "No squad found");
                            return;
                        }

                        //List his online squadmates!
                        zone._server.sendMessage(zone, pkt.alias, "&Squad Online List: " + dbplayer.squad1.name + " Captain: " + db.players.First(p => p.id == dbplayer.squad1.owner).alias1.name);
                        List<string> sonline = new List<string>();
                        foreach (Data.DB.player smate in db.players.Where(p => p.squad == targetSquadOnline.id && p.id != targetSquadOnline.owner))
                            //Make sure he's online!
                            if (zone.getPlayer(smate.alias1.name) != null)
                                sonline.Add(smate.alias1.name);
                        zone._server.sendMessage(zone, pkt.alias, "*" + string.Join(", ", sonline));
                        break;

                    case CS_Squads<Zone>.QueryType.list:
                        //Do we list his own squad or another?
                        Data.DB.squad targetSquadList = (pkt.payload == "") ? dbplayer.squad1 : db.squads.FirstOrDefault(s => s.name == pkt.payload);
                        if (targetSquadList == null)
                        {   //No squad found!
                            zone._server.sendMessage(zone, pkt.alias, "No squad found");
                            return;
                        }
                        //List the squad name, captain, and members!
                        zone._server.sendMessage(zone, pkt.alias, "&Squad List: " + targetSquadList.name + " Captain: " + db.players.First(p => p.id == targetSquadList.owner).alias1.name);
                        List<string> splayers = new List<string>();
                        foreach (Data.DB.player splayer in db.players.Where(p => p.squad == targetSquadList.id && p.id != targetSquadList.owner))
                            splayers.Add(splayer.alias1.name);
                        zone._server.sendMessage(zone, pkt.alias, "*" + string.Join(", ", splayers));
                        break;

                    case CS_Squads<Zone>.QueryType.invitessquad:
                        //Lists the players squads outstanding invitations
                        if (dbplayer.squad == null || dbplayer.squad1.owner != dbplayer.id)
                        {   //No squad found!
                            zone._server.sendMessage(zone, pkt.alias, "You aren't the owner of a squad");
                            return;
                        }
                        zone._server.sendMessage(zone, pkt.alias, "&Active Player Invitations");
                        foreach (KeyValuePair<int, Data.DB.player> invite in zone._server._squadInvites)
                            if (invite.Key == dbplayer.squad)
                                zone._server.sendMessage(zone, pkt.alias, "*" + invite.Value.alias1.name);
                        break;

                    case CS_Squads<Zone>.QueryType.invitesplayer:
                        zone._server.sendMessage(zone, pkt.alias, "&Active Squad Invitations");
                        foreach (KeyValuePair<int, Data.DB.player> invite in zone._server._squadInvites)
                            if (invite.Value == dbplayer)
                                zone._server.sendMessage(zone, pkt.alias, "*" + db.squads.First(s => s.id == invite.Key).name);
                        break;

                    case CS_Squads<Zone>.QueryType.invitesreponse:
                        //Response to a squad invitation
                        string[] sResponse = pkt.payload.Split(':');
                        //Sanity checks
                        if (sResponse.Count() != 2)
                        {   //Invalid syntax
                            zone._server.sendMessage(zone, pkt.alias, "Invalid syntax. Use: ?squadIresponse [accept/reject]:[squadname]");
                            return;
                        }

                        bool bAccept = (sResponse[0].ToLower() == "accept") ? true : false;
                        Data.DB.squad responseSquad = db.squads.FirstOrDefault(s => s.name == sResponse[1]);
                        if (responseSquad == null || !zone._server._squadInvites.Contains(new KeyValuePair<int, Data.DB.player>((int)responseSquad.id, dbplayer)))
                        {   //Either squad doesn't exist... or he's a filthy liar
                            zone._server.sendMessage(zone, pkt.alias, "Invalid squad invitation response");
                            return;
                        }
                        if (bAccept)
                        {   //Acceptance! Get in there, buddy
                            if (dbplayer.squad != null)
                            {
                                zone._server.sendMessage(zone, pkt.alias, "You can't accept squad invites if you're already in a squad");
                                return;
                            }

                            //Add him to the squad!
                            dbplayer.squad = responseSquad.id;
                            zone._server.sendMessage(zone, pkt.alias, "You've joined " + dbplayer.squad1.name + "! Quit and rejoin to be able to use # to squad chat");
                            zone._server._squadInvites.Remove(new KeyValuePair<int, Data.DB.player>((int)responseSquad.id, dbplayer));
                        }
                        else
                        {   //He's getting rid of a squad invite...
                            zone._server._squadInvites.Remove(new KeyValuePair<int, Data.DB.player>((int)responseSquad.id, dbplayer));
                            zone._server.sendMessage(zone, pkt.alias, "Revoked squad invite from " + responseSquad.name);
                        }
                        break;
                }

                //Save our changes to the database!
                db.SubmitChanges();
            }
        }

        /// <summary>
        /// Registers all handlers
        /// </summary>
        [RegistryFunc]
        static public void Register()
        {
            CS_Query<Zone>.Handlers += Handle_CS_Query;
            CS_Squads<Zone>.Handlers += Handle_CS_SquadQuery;
        }
    }
}
