// VncSharp - .NET VNC Client Library
// Copyright (C) 2008 David Humphrey
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
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using Nixxis.RfbClient.Encodings;

namespace Nixxis.RfbClient
{
	/// <summary>
	/// Event Handler delegate declaration used by events that signal successful connection with the server.
	/// </summary>
	public delegate void ConnectCompleteHandler(object sender, ConnectEventArgs e);
	
	/// <summary>
	/// When connecting to a VNC Host, a password will sometimes be required.  Therefore a password must be obtained from the user.  A default Password dialog box is included and will be used unless users of the control provide their own Authenticate delegate function for the task.  For example, this might pull a password from a configuration file of some type instead of prompting the user.
	/// </summary>
	public delegate string AuthenticateDelegate();

	/// <summary>
	/// SpecialKeys is a list of the various keyboard combinations that overlap with the client-side and make it
	/// difficult to send remotely.  These values are used in conjunction with the SendSpecialKeys method.
	/// </summary>
	public enum SpecialKeys {
		CtrlAltDel,
		AltF4,
		CtrlEsc, 
		Ctrl,
		Alt
	}

	[ToolboxBitmap(typeof(RemoteDesktop), "Resources.vncviewer.ico")]
	/// <summary>
	/// The RemoteDesktop control takes care of all the necessary RFB Protocol and GUI handling, including mouse and keyboard support, as well as requesting and processing screen updates from the remote VNC host.  Most users will choose to use the RemoteDesktop control alone and not use any of the other protocol classes directly.
	/// </summary>
	public class RemoteDesktop : Panel
	{
		[Description("Raised after a successful call to the Connect() method.")]
		/// <summary>
		/// Raised after a successful call to the Connect() method.  Includes information for updating the local display in ConnectEventArgs.
		/// </summary>
		public event ConnectCompleteHandler ConnectComplete;
		
		[Description("Raised when the VNC Host drops the connection.")]
		/// <summary>
		/// Raised when the VNC Host drops the connection.
		/// </summary>
		public event EventHandler	ConnectionLost;

        [Description("Raised when the VNC Host sends text to the client's clipboard.")]
        /// <summary>
        /// Raised when the VNC Host sends text to the client's clipboard. 
        /// </summary>
        public event EventHandler   ClipboardChanged;

		/// <summary>
		/// Points to a Function capable of obtaining a user's password.  By default this means using the PasswordDialog.GetPassword() function; however, users of RemoteDesktop can replace this with any function they like, so long as it matches the delegate type.
		/// </summary>
		public AuthenticateDelegate GetPassword;
		
		Bitmap desktop;						     // Internal representation of remote image.
		Image  designModeDesktop;			     // Used when painting control in VS.NET designer
		VncClient vnc;						     // The Client object handling all protocol-level interaction
		int port = 5900;					     // The port to connect to on remote host (5900 is default)
		bool passwordPending = false;		     // After Connect() is called, a password might be required.
		bool fullScreenRefresh = false;		     // Whether or not to request the entire remote screen be sent.
        VncDesktopTransformPolicy desktopPolicy;
		RuntimeState state = RuntimeState.Disconnected;

		private enum RuntimeState {
			Disconnected,
			Disconnecting,
			Connected,
			Connecting
		}
		
		public RemoteDesktop() : base()
		{
			// Since this control will be updated constantly, and all graphics will be drawn by this class,
			// set the control's painting for best user-drawn performance.
			SetStyle(ControlStyles.AllPaintingInWmPaint | 
					 ControlStyles.UserPaint			|
					 ControlStyles.DoubleBuffer			|
                     ControlStyles.Selectable           |   // BUG FIX (Edward Cooke) -- Adding Control.Select() support
					 ControlStyles.ResizeRedraw			|
					 ControlStyles.Opaque,				
					 true);

			// Show a screenshot of a Windows desktop from the manifest and cache to be used when painting in design mode
			//designModeDesktop = Image.FromStream(Assembly.GetAssembly(GetType()).GetManifestResourceStream("VncSharp.Resources.screenshot.png"));
			
            // Use a simple desktop policy for design mode.  This will be replaced in Connect()
            desktopPolicy = new VncDesignModeDesktopPolicy(this);
            AutoScroll = desktopPolicy.AutoScroll;
            AutoScrollMinSize = desktopPolicy.AutoScrollMinSize;

			// Users of the control can choose to use their own Authentication GetPassword() method via the delegate above.  This is a default only.
			//GetPassword = new AuthenticateDelegate(PasswordDialog.GetPassword);
		}
		
		[DefaultValue(5900)]
		[Description("The port number used by the VNC Host (typically 5900)")]
		/// <summary>
		/// The port number used by the VNC Host (typically 5900).
		/// </summary>
		public int VncPort {
			get { 
				return port; 
			}
			set { 
				// Ignore attempts to use invalid port numbers
				if (value < 1 | value > 65535) value = 5900;
				port = value;	
			}
		}

		/// <summary>
		/// True if the RemoteDesktop is connected and authenticated (if necessary) with a remote VNC Host; otherwise False.
		/// </summary>
		public bool IsConnected {
			get {
				return state == RuntimeState.Connected;
			}
		}
		
		// This is a hack to get around the issue of DesignMode returning
		// false when the control is being removed from a form at design time.
		// First check to see if the control is in DesignMode, then work up 
		// to also check any parent controls.  DesignMode returns False sometimes
		// when it is really True for the parent. Thanks to Claes Bergefall for the idea.
		protected new bool DesignMode {
			get {
				if (base.DesignMode) {
					return true;
				} else {
					Control parent = Parent;
					
					while (parent != null) {
						if (parent.Site != null && parent.Site.DesignMode) {
							return true;
						}
						parent = parent.Parent;
					}
					return false;
				}
			}
		}

		/// <summary>
		/// Returns a more appropriate default size for initial drawing of the control at design time
		/// </summary>
		protected override Size DefaultSize {
			get { 
				return new Size(400, 200);
			}
		}

        [Description("The name of the remote desktop.")]
        /// <summary>
        /// The name of the remote desktop, or "Disconnected" if not connected.
        /// </summary>
        public string Hostname {
            get {
                return vnc == null ? "Disconnected" : vnc.HostName;
            }
        }

        /// <summary>
        /// The image of the remote desktop.
        /// </summary>
        public Image Desktop {
            get {
                return desktop;
            }
        }

		/// <summary>
		/// Get a complete update of the entire screen from the remote host.
		/// </summary>
		/// <remarks>You should allow users to call FullScreenUpdate in order to correct
		/// corruption of the local image.  This will simply request that the next update be
		/// for the full screen, and not a portion of it.  It will not do the update while
		/// blocking.
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not in the Connected state.  See <see cref="Nixxis.RfbClient.RemoteDesktop.IsConnected" />.</exception>
		public void FullScreenUpdate()
		{
			InsureConnection(true);
			fullScreenRefresh = true;
		}

		/// <summary>
		/// Insures the state of the connection to the server, either Connected or Not Connected depending on the value of the connected argument.
		/// </summary>
		/// <param name="connected">True if the connection must be established, otherwise False.</param>
		/// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is in the wrong state.</exception>
		private void InsureConnection(bool connected)
		{
			// Grab the name of the calling routine:
			string methodName = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
			
			if (connected) {
				System.Diagnostics.Debug.Assert(state == RuntimeState.Connected || 
												state == RuntimeState.Disconnecting, // special case for Disconnect()
												string.Format("RemoteDesktop must be in RuntimeState.Connected before calling {0}.", methodName));
				if (state != RuntimeState.Connected && state != RuntimeState.Disconnecting) {
					throw new InvalidOperationException("RemoteDesktop must be in Connected state before calling methods that require an established connection.");
				}
			} else { // disconnected
				System.Diagnostics.Debug.Assert(state == RuntimeState.Disconnected,
												string.Format("RemoteDesktop must be in RuntimeState.Disconnected before calling {0}.", methodName));
                if (state != RuntimeState.Disconnected && state != RuntimeState.Disconnecting) {
					throw new InvalidOperationException("RemoteDesktop cannot be in Connected state when calling methods that establish a connection.");
				}
			}
		}

		// This event handler deals with Frambebuffer Updates coming from the host. An
		// EncodedRectangle object is passed via the VncEventArgs (actually an IDesktopUpdater
		// object so that *only* Draw() can be called here--Decode() is done elsewhere).
		// The VncClient object handles thread marshalling onto the UI thread.
		protected void VncUpdate(object sender, VncEventArgs e)
		{
			e.DesktopUpdater.Draw(desktop);
            Invalidate(desktopPolicy.AdjustUpdateRectangle(e.DesktopUpdater.UpdateRectangle));

			if (state == RuntimeState.Connected) {
				vnc.RequestScreenUpdate(fullScreenRefresh);
				
				// Make sure the next screen update is incremental
    			fullScreenRefresh = false;
			}
		}

        /// <summary>
        /// Connect to a VNC Host and determine whether or not the server requires a password.
        /// </summary>
        /// <param name="host">The IP Address or Host Name of the VNC Host.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if host is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if display is negative.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="Nixxis.RfbClient.RemoteDesktop.IsConnected" />.</exception>
        public void Connect(string host)
        {
            // Use Display 0 by default.
            Connect(host, 0);
        }

        /// <summary>
        /// Connect to a VNC Host and determine whether or not the server requires a password.
        /// </summary>
        /// <param name="host">The IP Address or Host Name of the VNC Host.</param>
        /// <param name="viewOnly">Determines whether mouse and keyboard events will be sent to the host.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if host is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if display is negative.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="Nixxis.RfbClient.RemoteDesktop.IsConnected" />.</exception>
        public void Connect(string host, bool viewOnly)
        {
            // Use Display 0 by default.
            Connect(host, 0, viewOnly);
        }

        /// <summary>
        /// Connect to a VNC Host and determine whether or not the server requires a password.
        /// </summary>
        /// <param name="host">The IP Address or Host Name of the VNC Host.</param>
        /// <param name="viewOnly">Determines whether mouse and keyboard events will be sent to the host.</param>
        /// <param name="scaled">Determines whether to use desktop scaling or leave it normal and clip.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if host is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if display is negative.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="Nixxis.RfbClient.RemoteDesktop.IsConnected" />.</exception>
        public void Connect(string host, bool viewOnly, bool scaled)
        {
            // Use Display 0 by default.
            Connect(host, 0, viewOnly, scaled);
        }

        /// <summary>
        /// Connect to a VNC Host and determine whether or not the server requires a password.
        /// </summary>
        /// <param name="host">The IP Address or Host Name of the VNC Host.</param>
        /// <param name="display">The Display number (used on Unix hosts).</param>
        /// <exception cref="System.ArgumentNullException">Thrown if host is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if display is negative.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="Nixxis.RfbClient.RemoteDesktop.IsConnected" />.</exception>
        public void Connect(string host, int display)
        {
            Connect(host, display, false);
        }

        /// <summary>
        /// Connect to a VNC Host and determine whether or not the server requires a password.
        /// </summary>
        /// <param name="host">The IP Address or Host Name of the VNC Host.</param>
        /// <param name="display">The Display number (used on Unix hosts).</param>
        /// <param name="viewOnly">Determines whether mouse and keyboard events will be sent to the host.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if host is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if display is negative.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="Nixxis.RfbClient.RemoteDesktop.IsConnected" />.</exception>
        public void Connect(string host, int display, bool viewOnly)
        {
            Connect(host, display, viewOnly, false);
        }

        /// <summary>
        /// Connect to a VNC Host and determine whether or not the server requires a password.
        /// </summary>
        /// <param name="host">The IP Address or Host Name of the VNC Host.</param>
        /// <param name="display">The Display number (used on Unix hosts).</param>
        /// <param name="viewOnly">Determines whether mouse and keyboard events will be sent to the host.</param>
        /// <param name="scaled">Determines whether to use desktop scaling or leave it normal and clip.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if host is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if display is negative.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="Nixxis.RfbClient.RemoteDesktop.IsConnected" />.</exception>
        public void Connect(string host, int display, bool viewOnly, bool scaled)
        {
            // TODO: Should this be done asynchronously so as not to block the UI?  Since an event 
            // indicates the end of the connection, maybe that would be a better design.
            InsureConnection(false);

            if (host == null) throw new ArgumentNullException("host");
            if (display < 0) throw new ArgumentOutOfRangeException("display", display, "Display number must be a positive integer.");

            // Start protocol-level handling and determine whether a password is needed
            vnc = new VncClient();
            vnc.ConnectionLost += new EventHandler(VncClientConnectionLost);
            vnc.ServerCutText += new EventHandler(VncServerCutText);

            passwordPending = vnc.Connect(host, display, VncPort, viewOnly);

            SetScalingMode(scaled);

            if (passwordPending) {
                // Server needs a password, so call which ever method is refered to by the GetPassword delegate.
                string password = GetPassword();

                if (password == null) {
                    // No password could be obtained (e.g., user clicked Cancel), so stop connecting
                    return;
                } else {
                    Authenticate(password);
                }
            } else {
                // No password needed, so go ahead and Initialize here
                Initialize();
            }
        }

		/// <summary>
		/// Authenticate with the VNC Host using a user supplied password.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="Nixxis.RfbClient.RemoteDesktop.IsConnected" />.</exception>
		/// <exception cref="System.NullReferenceException">Thrown if the password is null.</exception>
		/// <param name="password">The user's password.</param>
		public void Authenticate(string password)
		{
			InsureConnection(false);
			if (!passwordPending) throw new InvalidOperationException("Authentication is only required when Connect() returns True and the VNC Host requires a password.");
			if (password == null) throw new NullReferenceException("password");

			passwordPending = false;  // repeated calls to Authenticate should fail.
			if (vnc.Authenticate(password)) {
				Initialize();
			} else {		
				OnConnectionLost();										
			}	
		}


        /// <summary>
        /// Set the remote desktop's scaling mode.
        /// </summary>
        /// <param name="scaled">Determines whether to use desktop scaling or leave it normal and clip.</param>
        public void SetScalingMode(bool scaled)
        {
            if (scaled) {
                desktopPolicy = new VncScaledDesktopPolicy(vnc, this);
            } else {
                desktopPolicy = new VncClippedDesktopPolicy(vnc, this);
            }

            AutoScroll = desktopPolicy.AutoScroll;
            AutoScrollMinSize = desktopPolicy.AutoScrollMinSize;

            Invalidate();
        }

		/// <summary>
		/// After protocol-level initialization and connecting is complete, the local GUI objects have to be set-up, and requests for updates to the remote host begun.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already in the Connected state.  See <see cref="Nixxis.RfbClient.RemoteDesktop.IsConnected" />.</exception>		
		protected void Initialize()
		{
			// Finish protocol handshake with host now that authentication is done.
			InsureConnection(false);
			vnc.Initialize();
			SetState(RuntimeState.Connected);
			
			// Create a buffer on which updated rectangles will be drawn and draw a "please wait..." 
			// message on the buffer for initial display until we start getting rectangles
			SetupDesktop();
	
			// Tell the user of this control the necessary info about the desktop in order to setup the display
			OnConnectComplete(new ConnectEventArgs(vnc.Framebuffer.Width,
												   vnc.Framebuffer.Height, 
												   vnc.Framebuffer.DesktopName));

            // Refresh scroll properties
            AutoScrollMinSize = desktopPolicy.AutoScrollMinSize;

			// Start getting updates from the remote host (vnc.StartUpdates will begin a worker thread).
			vnc.VncUpdate += new VncUpdateHandler(VncUpdate);
			vnc.StartUpdates();
		}

		private void SetState(RuntimeState newState)
		{
			state = newState;
			
			// Set mouse pointer according to new state
            return;
			switch (state) {
				case RuntimeState.Connected:
					// Change the cursor to the "vnc" custor--a see-through dot
					Cursor = new Cursor(GetType(), "Resources.vnccursor.cur");
					break;
				// All other states should use the normal cursor.
				case RuntimeState.Disconnected:
				default:	
					Cursor = Cursors.Default;				
					break;
			}
		}

		/// <summary>
		/// Creates and initially sets-up the local bitmap that will represent the remote desktop image.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not already in the Connected state. See <see cref="Nixxis.RfbClient.RemoteDesktop.IsConnected" />.</exception>
		protected void SetupDesktop()
		{
			InsureConnection(true);

			// Create a new bitmap to cache locally the remote desktop image.  Use the geometry of the
			// remote framebuffer, and 32bpp pixel format (doesn't matter what the server is sending--8,16,
			// or 32--we always draw 32bpp here for efficiency).
			desktop = new Bitmap(vnc.Framebuffer.Width, vnc.Framebuffer.Height, PixelFormat.Format32bppPArgb);
			
			// Draw a "please wait..." message on the local desktop until the first
			// rectangle(s) arrive and overwrite with the desktop image.
			DrawDesktopMessage("Connecting to VNC host, please wait...");
		}
	
		/// <summary>
		/// Draws the given message (white text) on the local desktop (all black).
		/// </summary>
		/// <param name="message">The message to be drawn.</param>
		protected void DrawDesktopMessage(string message)
		{
			System.Diagnostics.Debug.Assert(desktop != null, "Can't draw on desktop when null.");
			// Draw the given message on the local desktop
			using (Graphics g = Graphics.FromImage(desktop)) {
				g.FillRectangle(Brushes.Black, vnc.Framebuffer.Rectangle);

				StringFormat format = new StringFormat();
				format.Alignment = StringAlignment.Center;
				format.LineAlignment = StringAlignment.Center;

				g.DrawString(message, 
							 new Font("Arial", 12), 
							 new SolidBrush(Color.White), 
							 new PointF(vnc.Framebuffer.Width / 2, vnc.Framebuffer.Height / 2), format);
			}

		}
		
		/// <summary>
		/// Stops the remote host from sending further updates and disconnects.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not already in the Connected state. See <see cref="Nixxis.RfbClient.RemoteDesktop.IsConnected" />.</exception>
		public void Disconnect()
		{
			InsureConnection(true);
			vnc.ConnectionLost -= new EventHandler(VncClientConnectionLost);
            vnc.ServerCutText -= new EventHandler(VncServerCutText);
			vnc.Disconnect();
			SetState(RuntimeState.Disconnected);
			OnConnectionLost();
			Invalidate();
		}



		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				// Make sure the connection is closed--should never happen :)
				if (state != RuntimeState.Disconnected) {
					Disconnect();
				}

				// See if either of the bitmaps used need clean-up.  
				if (desktop != null) desktop.Dispose();
				if (designModeDesktop != null) designModeDesktop.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			// If the control is in design mode, draw a nice background, otherwise paint the desktop.
			if (!DesignMode) {
				switch(state) {
					case RuntimeState.Connected:
						System.Diagnostics.Debug.Assert(desktop != null);
						DrawDesktopImage(desktop, pe.Graphics);
						break;
                    case RuntimeState.Disconnecting:
                    case RuntimeState.Connecting:
					case RuntimeState.Disconnected:
						// Do nothing, just black background.
						break;
					default:
						// Sanity check
						throw new NotImplementedException(string.Format("RemoteDesktop in unknown State: {0}.", state.ToString()));
				}
            } else {
				// Draw a static screenshot of a Windows desktop to simulate the control in action
				System.Diagnostics.Debug.Assert(designModeDesktop != null);
				DrawDesktopImage(designModeDesktop, pe.Graphics);
			}
			base.OnPaint(pe);
		}

        protected override void OnResize(EventArgs eventargs)
        {
            // Fix a bug with a ghost scrollbar in clipped mode on maximize
            Control parent = Parent;
            while (parent != null) {
                if (parent is Form) {
                    Form form = parent as Form;
                    if (form.WindowState == FormWindowState.Maximized)
                        form.Invalidate();
                    parent = null;
                } else {
                    parent = parent.Parent;
                }
            }

            base.OnResize(eventargs);
        }

		/// <summary>
		/// Draws an image onto the control in a size-aware way.
		/// </summary>
		/// <param name="desktopImage">The desktop image to be drawn to the control's sufrace.</param>
		/// <param name="g">The Graphics object representing the control's drawable surface.</param>
		/// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not already in the Connected state.</exception>
		protected void DrawDesktopImage(Image desktopImage, Graphics g)
		{
			g.DrawImage(desktopImage, desktopPolicy.RepositionImage(desktopImage));
		}

		/// <summary>
		/// RemoteDesktop listens for ConnectionLost events from the VncClient object.
		/// </summary>
		/// <param name="sender">The VncClient object that raised the event.</param>
		/// <param name="e">An empty EventArgs object.</param>
		protected void VncClientConnectionLost(object sender, EventArgs e)
		{
			// If the remote host dies, and there are attempts to write
			// keyboard/mouse/update notifications, this may get called 
			// many times, and from main or worker thread.
			// Guard against this and invoke Disconnect once.
			if (state == RuntimeState.Connected) {
				SetState(RuntimeState.Disconnecting);
				Disconnect();
			}
		}

        // Handle the VncClient ServerCutText event and bubble it up as ClipboardChanged.
        protected void VncServerCutText(object sender, EventArgs e)
        {
            OnClipboardChanged();
        }

        protected void OnClipboardChanged()
        {
            if (ClipboardChanged != null)
                ClipboardChanged(this, EventArgs.Empty);
        }

		/// <summary>
		/// Dispatches the ConnectionLost event if any targets have registered.
		/// </summary>
		/// <param name="e">An EventArgs object.</param>
		/// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is in the Connected state.</exception>
		protected void OnConnectionLost()
		{
			if (ConnectionLost != null) {
				ConnectionLost(this, EventArgs.Empty);
			}
		}
		
		/// <summary>
		/// Dispatches the ConnectComplete event if any targets have registered.
		/// </summary>
		/// <param name="e">A ConnectEventArgs object with information about the remote framebuffer's geometry.</param>
		/// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not in the Connected state.</exception>
		protected void OnConnectComplete(ConnectEventArgs e)
		{
			if (ConnectComplete != null) {
				ConnectComplete(this, e);
			}
		}


		// Handle Keyboard Events:		 -------------------------------------------
		// These keys don't normally throw an OnKeyDown event. Returning true here fixes this.
		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData) {
				case Keys.Tab:
				case Keys.Up:
				case Keys.Down:
				case Keys.Left:
				case Keys.Right:
				case Keys.Shift:
				case Keys.RWin:
				case Keys.LWin:
					return true;
				default:
					return base.IsInputKey(keyData);
			}
		}


		
	}
}