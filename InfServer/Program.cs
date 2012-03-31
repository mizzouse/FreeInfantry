﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections;

namespace InfServer
{
	class Program
	{
		static Game.ZoneServer server;

        public static void Restart()
        {
            Process process = Process.GetCurrentProcess();
            Process.Start(Environment.CurrentDirectory + "/InfServer.exe");
            process.Kill();
        }


		/// <summary>
		/// Makes a note of all unhandled exceptions
		/// </summary>
		public static void onException(object o, UnhandledExceptionEventArgs e)
		{	//Talk about the exception
			using (LogAssume.Assume(server._logger))
				Log.write(TLog.Exception, "Unhandled exception:\r\n" + e.ExceptionObject.ToString());
		}

		static void Main(string[] args)
		{	//Initialize the logging system
			if (!Log.init())
			{	//Abort..
				Console.WriteLine("Logger initialization failed, exiting..");
				Thread.Sleep(10000);
				return;
			}

			DdMonitor.bNoSync = false;
			DdMonitor.bEnabled = false;
			DdMonitor.DefaultTimeout = -1;

			//Register our catch-all exception handler
			Thread.GetDomain().UnhandledException += onException;

			//Create a logging client for the main server thread
			LogClient handlerLogger = Log.createClient("ServerHandler");
			Log.assume(handlerLogger);

			//Allow functions to pre-register
			Logic.Registrar.register();

			//Create our zone server
			server = new InfServer.Game.ZoneServer();

			//Initialize everything..
			if (!server.init())
			{
				Log.write(TLog.Error, "ZoneServer initialization failed, exiting..");
				Thread.Sleep(10000);
				return;
			}

			//Good to go!
			Log.write("");
			Log.write("Starting server..");
			server.begin();
		}
	}
}