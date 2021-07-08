﻿using Netch.Models;
using Netch.Servers;

namespace Netch.Interfaces
{
    public interface IServerController : IController
    {
        public ushort? Socks5LocalPort { get; set; }

        public string? LocalAddress { get; set; }

        public Socks5 Start(in Server s);
    }

    public static class ServerControllerExtension
    {
        public static ushort Socks5LocalPort(this IServerController controller)
        {
            return controller.Socks5LocalPort ?? Global.Settings.Socks5LocalPort;
        }

        public static string LocalAddress(this IServerController controller)
        {
            return controller.LocalAddress ?? Global.Settings.LocalAddress;
        }
    }
}