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
        public List<Zone.Player> _players;      //The players in our chat..
        public string _name;                    //The name of our chat

        public Chat(DBServer server, string chat)
        {
            _server = server;
            _players = new List<Zone.Player>();
            _name = chat;
            server._chats.Add(chat, this);
        }

        public void newPlayer(Zone.Player player)
        {
            if (player == null)
            {
                Log.write(TLog.Error, "Chat.newPlayer(): Called with null player.");
                return;
            }

            if (hasPlayer(player))
            {
                Log.write(TLog.Warning, "Player '{0}' already exists in chat '{1}'.", player, _name);
                return;
            }

            _players.Add(player);
            
            SC_JoinChat<Zone> join = new SC_JoinChat<Zone>();
            join.from = player.alias;
            join.chat = _name;
            join.users = List();

            foreach (Zone z in _server._zones)
            {
                z._client.send(join);
            }
        }

        public void lostPlayer(Zone.Player player)
        {
            if (!_players.Remove(player))
            {
                Log.write(TLog.Warning, "Lost player '{0}' that wasn't in chat '{1}'.", player, _name);
                return;
            }

            SC_LeaveChat<Zone> leave = new SC_LeaveChat<Zone>();
            leave.from = player.alias;
            leave.chat = _name;
            leave.users = List();

            foreach (Zone z in _server._zones)
            {
                z._client.send(leave);
            }
        }

        public bool hasPlayer(Zone.Player player)
        {
            if (player == null)
                return false;

            return _players.Contains(player);
        }

        public string List()
        {
            List<string> members = new List<string>();
            foreach (var player in _players)
            {
                if (player == null)
                    continue;

                members.Add(player.alias);
            }
            return string.Join(", ", members);
        }
    }
}