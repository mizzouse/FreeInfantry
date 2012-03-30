﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using InfServer.Network;
using InfServer.Protocol;
using InfServer.Data;

namespace InfServer
{
    // Chat Class
    /// Represents a single private chat
    ///////////////////////////////////////////////////////
    public class Chat
    {
        DBServer _server;                       //Who we work for..
        public List<string> _players;   //The players in our chat..
        public string _name;                    //The name of our chat             

        public Chat(DBServer server, string chat)
        {
            _server = server;
            _players = new List<string>();
            _name = chat;
            server._chats.Add(chat, this);
        }

        public void newPlayer(string player)
        {
            _players.Add(player);
        }

        public void lostPlayer(string player)
        {
            _players.Remove(player);
        }

        public bool hasPlayer(string player)
        {
            if (_players.Contains(player))
                return true;
            return false;
        }



        public void sendList(string player)
        {
            SC_JoinChat<Zone> reply = new SC_JoinChat<Zone>();
            reply.chat = _name;
            reply.from = player;
            reply.users = List();
            _server.getPlayer(player).zone._client.send(reply);
        }


        public string List()
        {
            StringBuilder builder = new StringBuilder();
            foreach (string player in _players)
                builder.Append(player).Append(",");

            return builder.ToString().TrimEnd(',');
        }

    }

}