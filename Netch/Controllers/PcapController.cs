﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netch.Forms;
using Netch.Interfaces;
using Netch.Models;
using Netch.Servers;
using Netch.Utils;

namespace Netch.Controllers
{
    public class PcapController : Guard, IModeController
    {
        private readonly LogForm _form;
        private Mode? _mode;
        private Server? _server;

        public PcapController() : base("pcap2socks.exe", encoding: Encoding.UTF8)
        {
            _form = new LogForm(Global.MainForm);
            _form.CreateControl();
        }

        protected override IEnumerable<string> StartedKeywords { get; } = new[] { "└" };

        public override string Name => "pcap2socks";

        public void Start(Server server, Mode mode)
        {
            _server = server;
            _mode = mode;

            var outboundNetworkInterface = NetworkInterfaceUtils.GetBest();

            var argument = new StringBuilder($@"-i \Device\NPF_{outboundNetworkInterface.Id}");
            if (_server is Socks5 socks5 && !socks5.Auth())
                argument.Append($" --destination  {socks5.AutoResolveHostname()}:{socks5.Port}");
            else
                argument.Append($" --destination  127.0.0.1:{Global.Settings.Socks5LocalPort}");

            argument.Append($" {_mode.GetRules().FirstOrDefault() ?? "-P n"}");
            StartGuard(argument.ToString());
        }

        public override void Stop()
        {
            _form.Close();
            StopGuard();
        }

        ~PcapController()
        {
            _form.Dispose();
        }

        protected override void OnReadNewLine(string line)
        {
            Global.MainForm.BeginInvoke(new Action(() =>
            {
                if (!_form.IsDisposed)
                    _form.richTextBox1.AppendText(line + "\n");
            }));
        }

        protected override void OnStarted()
        {
            Global.MainForm.BeginInvoke(new Action(() => _form.Show()));
        }

        protected override void OnStartFailed()
        {
            if (new FileInfo(LogPath).Length == 0)
            {
                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    Utils.Utils.Open("https://github.com/zhxie/pcap2socks#dependencies");
                });

                throw new MessageException("Pleases install pcap2socks's dependency");
            }

            Utils.Utils.Open(LogPath);
        }
    }
}