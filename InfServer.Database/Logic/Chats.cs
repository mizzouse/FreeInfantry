﻿using System;
using System.Linq;

using InfServer.Protocol;

namespace InfServer.Logic
{
    class Logic_Chats
    {
        static public void Handle_CS_JoinChat(CS_JoinChat<Zone> pkt, Zone zone)
        {
            DBServer server = zone._server;
            Zone.Player player = server.getPlayer(pkt.from);
            char[] splitArr = { ',' };
            string[] chats = pkt.chat.Split(splitArr, StringSplitOptions.RemoveEmptyEntries);

            //Hey, how'd you get here?!
            if (player == null)
                return;

            //He wants to see the player list of each chat..
            if (pkt.chat.Length == 0)
            {

                foreach (var chat in server._chats.Values)
                {
                    if (chat.hasPlayer(pkt.from))
                        server.sendMessage(zone, pkt.from, String.Format("{0}: {1}", chat._name, chat.List()));
                }
                return;
            }

            foreach (string chat in chats)
            {
                string name = chat.ToLower();
                Chat _chat = server.getChat(chat);

                //Remove him from everything..
                if (name == "off")
                {
                    foreach (var c in server._chats)
                    {
                        if (c.Value.hasPlayer(pkt.from))
                            c.Value.lostPlayer(pkt.from);
                    }
                    server.sendMessage(zone, pkt.from, "No Chat Channels Defined");
                    return;
                }

                //New chat
                if (!server._chats.ContainsValue(_chat))
                {
                    _chat = new Chat(server, chat);
                }

                //Add him
                if (!_chat.hasPlayer(pkt.from))
                    _chat.newPlayer(pkt.from, chats);

                //Send him the updated list..
                server.sendMessage(zone, pkt.from, String.Format("{0}: {1}", chat, _chat.List()));
            }

            //Remove him from any chats that didn't come over in the packet.
            foreach (Chat c in server._chats.Values)
            {
                if (!chats.Contains(c._name))
                {
                    if (c.hasPlayer(pkt.from) == true)
                        c.lostPlayer(pkt.from);
                }
            }
        }

        static public void Handle_CS_Chat(CS_PrivateChat<Zone> pkt, Zone zone)
        {
            DBServer server = zone._server;
            Chat chat = server.getChat(pkt.chat.ToLower());

            //WTF MATE?
            if (chat == null)
                return;

            SC_PrivateChat<Zone> reply = new SC_PrivateChat<Zone>();
            reply.chat = pkt.chat;
            reply.from = pkt.from;
            reply.message = pkt.message;
            reply.users = chat.List();

            foreach (Zone z in server._zones)
            {
                z._client.send(reply);
            }

        }

        static public void Handle_CS_ModCommand(CS_ModCommand<Zone> pkt, Zone zone)
        {
            using (Data.InfantryDataContext db = zone._server.getContext())
            {
                Data.DB.history hist = new Data.DB.history();
                hist.sender = pkt.sender;
                hist.recipient = pkt.recipient;
                hist.zone = pkt.zone;
                hist.arena = pkt.arena;
                hist.command = pkt.command;
                hist.date = DateTime.Now;
                db.histories.InsertOnSubmit(hist);
            }
        }




        /// <summary>
        /// Registers all handlers
        /// </summary>
        [RegistryFunc]
        static public void Register()
        {
            CS_JoinChat<Zone>.Handlers += Handle_CS_JoinChat;
            CS_PrivateChat<Zone>.Handlers += Handle_CS_Chat;
            CS_ModCommand<Zone>.Handlers += Handle_CS_ModCommand;
        }
    }


}
