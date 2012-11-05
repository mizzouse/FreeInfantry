﻿using System;
using System.Data.SqlClient;
using MiniAccountServer.Models;

namespace MiniAccountServer.Database
{
    /// <summary>
    /// Preliminary database client.
    /// </summary>
    public class DatabaseClient
    {
        private SqlConnection _connection;
        private string _connString;

        #region Stored Proc strings

        private String _strLatestIdentity = "SELECT @@IDENTITY";

        private String _strCreateAccount =
            "INSERT INTO account (name, password, ticket, dateCreated, lastAccess, permission, email) VALUES " +
            "(@name, @password, @ticket, @dateCreated, @lastAccess, @permission, @email)";

        private String _strUsernameExists = "SELECT * FROM account WHERE name LIKE @name";

        private String _strAccountValid = "SELECT * FROM account WHERE name LIKE @name AND password LIKE @password";

        private String _strLoginUpdate =
            "UPDATE account SET ticket=@ticket WHERE name LIKE @name;" +
            "UPDATE account SET lastAccess=@time WHERE name LIKE @name;" +
            "UPDATE account SET IPAddress=@ipaddress WHERE name LIKE @name";

        #endregion
        private String _strEmailExists = "SELECT * FROM account WHERE email LIKE @email";
        public DatabaseClient()
        {
            _connString = "Server=INFANTRY\\SQLEXPRESS;Database=Data;Trusted_Connection=True;";

            _connection = new SqlConnection(_connString);

            _connection.Open();
        }

        public Account AccountCreate(string username, string password, string ticket, DateTime dateCreated, DateTime lastAccess, int permission, string email)
        {
            if (UsernameExists(username))
            {
                return null;
            }

            var _createAccountCmd = new SqlCommand(_strCreateAccount, _connection);

            _createAccountCmd.Parameters.AddWithValue("@name", username);
            _createAccountCmd.Parameters.AddWithValue("@password", password);
            _createAccountCmd.Parameters.AddWithValue("@ticket", ticket);
            _createAccountCmd.Parameters.AddWithValue("@dateCreated", dateCreated);
            _createAccountCmd.Parameters.AddWithValue("@lastAccess", lastAccess);
            _createAccountCmd.Parameters.AddWithValue("@permission", permission);
            _createAccountCmd.Parameters.AddWithValue("@email", email);

            if(_createAccountCmd.ExecuteNonQuery() != 1)
            {
                return null;
            }

            return new Account
                       {
                           DateCreated = dateCreated,
                           LastAccessed = lastAccess,
                           SessionId = Guid.Parse(ticket),
                           Username = username,
                           Password = password,
                           Permission = permission,
                           Email = email
                       };
        }

        public bool UsernameExists(string username)
        {
            var cmd = new SqlCommand(_strUsernameExists, _connection);

            cmd.Parameters.AddWithValue("@name", username);

            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                    return false;
            }

            return true;
        }

        public bool IsAccountValid(string username, string password)
        {
            var cmd = new SqlCommand(_strAccountValid, _connection);

            cmd.Parameters.AddWithValue("@name", username);
            cmd.Parameters.AddWithValue("@password", password);

            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                    return false;
            }

            return true;
        }
        public bool EmailExists(string email)
        {
            var cmd = new SqlCommand(_strEmailExists, _connection);

            cmd.Parameters.AddWithValue("@email", email);

            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                    return false;
            }
            return true;
        }
        public Account AccountLogin(string username, string password, string IPAddress)
        {
            //Update some stuff first, mang

            var update = new SqlCommand(_strLoginUpdate, _connection);

            update.Parameters.AddWithValue("@name", username);
            update.Parameters.AddWithValue("@ticket", Guid.NewGuid().ToString());
            update.Parameters.AddWithValue("@time", DateTime.Now);
            update.Parameters.AddWithValue("@ipaddress", IPAddress);

            update.ExecuteNonQuery();
            
            var cmd = new SqlCommand(_strAccountValid, _connection);

            cmd.Parameters.AddWithValue("@name", username);
            cmd.Parameters.AddWithValue("@password", password);

            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                    return null;

                reader.Read();

                return new Account
                           {
                               Id = reader.GetInt64(0),
                               Username = reader.GetString(1),
                               Password = reader.GetString(2),
                               SessionId = Guid.Parse(reader.GetString(3)),
                               DateCreated = reader.GetDateTime(4),
                               LastAccessed = reader.GetDateTime(5),
                               Permission = reader.GetInt32(6),
                               Email = reader.GetString(7),
                           };
            }
        }
    }
}