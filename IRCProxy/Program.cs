﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace IRCProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

            int DefaultPort = 58888;
            string DefaultTargetServer = "irc.hanirc.org";
            int DefaultTargetPort = 6668;

            if ( args.Count() >= 1 )
            {
                DefaultPort = Int32.Parse(args[0]);
            }

            if (args.Count() >= 2)
            {
                DefaultTargetServer = args[1];
            }
            if (args.Count() >= 3)
            {
                DefaultTargetPort = Int32.Parse(args[2]);
            }

            ProxyServer proxy = new ProxyServer(DefaultPort, DefaultTargetServer, DefaultTargetPort);
            proxy.RunForever();

        }
    }
}
