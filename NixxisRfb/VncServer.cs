// NVNC - .NET VNC Server Library
// Copyright (C) 2014 T!T@N
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Drawing;
using System.Threading;

namespace Nixxis.RfbServer
{
    public class Server: ContactRoute.IViewServerAddon
    {
        private RfbProtocol host;
        private Framebuffer fb;

        private int _port;
        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }

        private string _name;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public Server()
            : this("NixPass", 5900, "Default")
        { }

        public Server(string password, int port, string name)
        {
            _password = password;
            _port = port;
            _name = name;

            Size screenSize = ScreenSize();
            fb = new Framebuffer(screenSize.Width, screenSize.Height);

            fb.BitsPerPixel = 32;
            fb.Depth = 24;
            fb.BigEndian = false;
            fb.TrueColor = true;
            fb.RedShift = 16;
            fb.GreenShift = 8;
            fb.BlueShift = 0;
            fb.RedMax = fb.GreenMax = fb.BlueMax = 0xFF;
            fb.DesktopName = name;
        }

        public void Start()
        {
            if (String.IsNullOrEmpty(Name))
                throw new ArgumentNullException("Name", "The Server Name cannot be empty.");
            if (Port == 0)
                throw new ArgumentNullException("Port", "The Server port cannot be zero.");
            
            host = new RfbProtocol(Port, Name);

            host.Start();

            if (!host.isRunning)
                return;

            host.WriteProtocolVersion();

            host.ReadProtocolVersion();

            bool authOk = false;
            try
            {
                authOk = host.WriteAuthentication(Password);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
            if (!authOk)
            {
                System.Diagnostics.Trace.WriteLine("Authentication failed !", "RFB server");
                host.Close();
                Start();
            }
            else
            {

                bool share = host.ReadClientInit();
                host.WriteServerInit(this.fb);

                while ((host.isRunning))
                {
                    switch (host.ReadServerMessageType())
                    {
                        case RfbProtocol.ClientMessages.SET_PIXEL_FORMAT:
                            Console.WriteLine("Read SetPixelFormat");
                            Framebuffer f = host.ReadSetPixelFormat(fb.Width, fb.Height);
                            if (f != null)
                            {
                                f.DesktopName = _name;
                                fb = f;
                            }
                            break;
                        case RfbProtocol.ClientMessages.READ_COLOR_MAP_ENTRIES:
                            host.ReadColorMapEntry();
                            break;
                        case RfbProtocol.ClientMessages.SET_ENCODINGS:
                            host.ReadSetEncodings();
                            break;
                        case RfbProtocol.ClientMessages.FRAMEBUFFER_UPDATE_REQUEST:
                            host.ReadFrameBufferUpdateRequest(fb);
                            break;
                        case RfbProtocol.ClientMessages.KEY_EVENT:
                            host.ReadKeyEvent();
                            break;
                        case RfbProtocol.ClientMessages.POINTER_EVENT:
                            host.ReadPointerEvent();
                            break;
                        case RfbProtocol.ClientMessages.CLIENT_CUT_TEXT:
                            host.ReadClientCutText();
                            break;
                    }
                }
                if (!host.isRunning)
                    Start();
            }
        }

        public void Stop()
        {
            if(this.host!=null)
                this.host.Close();
        }
        private Size ScreenSize()
        {
            Size s = new Size();
            s.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            s.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;


            return s;
        }


        public void SetClientIpAdresses(string[] ipaddresses)
        {
        }
    }
}
