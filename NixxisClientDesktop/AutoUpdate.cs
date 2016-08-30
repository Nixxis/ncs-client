using System;
using System.IO;
using System.Reflection;
using System.Net;
using System.Runtime.InteropServices;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.ComponentModel;
using System.Configuration;

namespace Nixxis
{
    partial class LauncherWindow
	{
		[DllImport("kernel32.dll")]
		static extern bool SetFileTime(IntPtr hFile, ref long lpCreationTime, ref long lpLastAccessTime, ref long lpLastWriteTime);

		[DllImport("advapi32.dll", SetLastError=true)]
		public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
			int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		public extern static bool CloseHandle(IntPtr handle);

		[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public extern static bool DuplicateToken(IntPtr ExistingTokenHandle,
			int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

		public static ServiceList Services = new ServiceList();

		static string UpdateModule = null;
		static string UpdateUri = null;
		static System.Collections.Generic.Dictionary<string, byte[]> RawAssemblies = new System.Collections.Generic.Dictionary<string, byte[]>();

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string Name = args.Name;
			int Pos = Name.IndexOf(',');

			if (Pos > 0)
				Name = Name.Substring(0, Pos);

			Trace.WriteLine(string.Format("Request to resolve {0} ({1})", Name, args.Name), "NixxisClient");

			Assembly[] LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly LoadedAssembly in LoadedAssemblies)
			{
				if (LoadedAssembly.GetName().Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
				{
					Trace.WriteLine(string.Format("Assembly {0} ({1}) found in appdomain ({2})", Name, args.Name, LoadedAssembly.FullName), "NixxisClient");

					return LoadedAssembly;
				}
			}

			if (RawAssemblies.ContainsKey(Name))
			{
				Assembly Resolved = AppDomain.CurrentDomain.Load(RawAssemblies[Name]);

				RawAssemblies.Remove(Name);

				Trace.WriteLine(string.Format("Assembly {0} ({1}) loaded from memory", Name, args.Name), "NixxisClient");

				return Resolved;
			}

			return null;
		}

		private static ServiceList LoadServiceList(string baseUri, ICredentials credentials)
		{
			try
			{
                bool skipLogin = false;
                if (Boolean.TryParse(ConfigurationManager.AppSettings["SkipLogin"], out skipLogin) && skipLogin)
                {
                    Services.Reset();
                    return Services;
                }

                Uri BaseUri = null;

                try
                {
                    BaseUri = new Uri(baseUri);
                }
                catch
                {
                }

                if (BaseUri == null)
                {
                    string Service = string.Concat("_nixxisclient._tcp.", baseUri.Replace('@', '.'));
                    string[] Entries = null;

                    try
                    {
                        Entries = nDnsQuery.GetSRVRecords(Service);
                    }
                    catch
                    {
                        Service = Service + "." + System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                        Entries = nDnsQuery.GetSRVRecords(Service);
                    }

                    foreach(string Entry in Entries)
                    {
                        try
                        {
                            string Scheme = "http://";
                            string[] Parts = Entry.Split(':');
                            int Port = Convert.ToInt32(Parts[1]);

                            switch(Port)
                            {
                                case 80:
                                    Port = 0;
                                    break;
                                case 443:
                                    Scheme = "https://";
                                    Port = 0;
                                    break;
                            }

                            BaseUri = new Uri(Scheme + Parts[0] + ((Port != 0) ? (":" + Port.ToString()) : ""));
                        }
                        catch 
                        { 
                        }
                    }
                }

                WebRequest Request = null;
                WebResponse Response = null;
                Stream Content = null;

                if (BaseUri != null)
                {
                    try
                    {
                        Request = WebRequest.Create(new Uri(BaseUri, "?service="));
                    }
                    catch (Exception Ex)
                    {
                        Trace.WriteLine(Ex.Message, "NixxisClient");
                    }

                    if (Request != null)
                    {
                        if (credentials != null)
                            Request.Credentials = credentials;

                        Request.Timeout = 5000;
                        Request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                        Trace.WriteLine(string.Format("Loading services from {0}", Request.RequestUri.ToString()), "NixxisClient");

                        try
                        {
                            Response = Request.GetResponse();
                        }
                        catch
                        {
                            bool AutoBypass;

                            if (!bool.TryParse(ConfigurationManager.AppSettings["AutoBypassProxy"], out AutoBypass) || AutoBypass)
                            {
                                Trace.WriteLine("Load failed, trying to bypass proxy", "NixxisClient");

                                WebRequest.DefaultWebProxy = GlobalProxySelection.Select = GlobalProxySelection.GetEmptyWebProxy();

                                Request = WebRequest.Create(string.Concat(baseUri, "?service="));

                                if (credentials != null)
                                    Request.Credentials = credentials;

                                Request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                                try
                                {
                                    Response = Request.GetResponse();
                                }
                                catch
                                {
                                    Trace.WriteLine("Load failed", "NixxisClient");
                                }
                            }
                        }
                    }
                }

                if (Response == null)
                {
                    Services.Reset();
                    return Services;
                }

                if (Response.ContentLength > 0)
                {
                    Content = Response.GetResponseStream();

                    try
                    {
                        XmlDocument ServicesDoc = new XmlDocument();

                        ServicesDoc.Load(Content);

                        Trace.WriteLine(ServicesDoc.OuterXml);

                        Services.Load(ServicesDoc);
                    }
                    catch (Exception Ex)
                    {
                        Trace.WriteLine("Cannot load XML document: " + Ex.Message);

                        Services.Reset();
                        return Services;
                    }
                    finally
                    {
                        try
                        {
                            Response.Close();
                            Content.Close();
                        }
                        catch
                        {
                        }
                    }
                }

				if (Services.Count==0 || Services["signon"] == null)
				{
					Request = WebRequest.Create(string.Concat(baseUri, "?service=signon$"));

					if (credentials != null)
						Request.Credentials = credentials;
					Request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                    try
                    {
                        Response = Request.GetResponse();
                        Content = Response.GetResponseStream();
                    }
                    catch (Exception Ex)
                    {
                        Trace.WriteLine("Cannot access XML document: " + Ex.Message);

                        Services.Reset();
                        return Services;
                    }

					try
					{
                        XmlDocument ServicesDoc = new XmlDocument();

                        ServicesDoc.Load(Content);

						if (Services == null || Services.Count == 0)
						{
							Services.Load(ServicesDoc);
						}
						else
						{
                            XmlNode Child = ServicesDoc.SelectSingleNode("//services/signon$");

							if (Child != null)
							{
								Services.Add(new ServiceDescription(Child));
							}
						}
					}
                    catch (Exception Ex)
                    {
                        Trace.WriteLine("Cannot load XML document: " + Ex.Message);

                        Services.Reset();
                        return Services;
                    }
                    finally
					{
						try
						{
							Response.Close();
							Content.Close();
						}
						catch
						{
						}
					}
				}
			}
			finally
			{
			}

            return Services;
		}

        private static XmlDocument LoadVersionSettings(string baseUri)
		{
			try
			{
				HttpWebRequest Request;
				HttpWebResponse Response;
				XmlDocument Doc;

				Request = WebRequest.Create(new Uri(new Uri(baseUri), string.Format("./{0}?action=versionSettings", UpdateModule)).ToString()) as HttpWebRequest;
				Request.Method = "GET";

				Response = Request.GetResponse() as HttpWebResponse;

				Doc = new XmlDocument();
				Doc.Load(Response.GetResponseStream());

				Response.Close();

				return Doc;
			}
			catch
			{
			}

			return null;
		}

		internal class FileVersionSettings
		{
			public string FileName;
			public int[] FileVersion = new int[] { 0, 0, 0, 0 };
			public int[] ProductVersion = new int[] { 0, 0, 0, 0 };
			public int[] MinVersion = new int[] { 0, 0, 0, 0 };
			public int[] MaxVersion = new int[] { 0, 0, 0, 0 };
			public bool MustExist = true;
			public bool Protect = false;
			public bool Temporary = false;
			public long FileSize = 0;
			public bool MustDownload = false;
			public bool BreakIfFail = false;
			public bool MustSave = true;

			private FileVersionSettings()
			{
			}

			public FileVersionSettings(string fileName)
			{
				FileName = fileName;
			}

			internal static int[] DecodeVersion(string version)
			{
				int[] Version = new int[] { 0, 0, 0, 0 };
				string[] Splitted = version.Split('.');
				int Len = Splitted.Length;

				if (Len > 4)
					Len = 4;

				for (int i = 0; i < Len; i++)
					int.TryParse(Splitted[i], out Version[i]);

				return Version;
			}

			internal static int CompareVersions(int[] v1, int[] v2)
			{
				for (int i = 0; i < 4; i++)
				{
					if (v1[i] != v2[i])
						return (v1[i] < v2[i]) ? -1 : 1;
				}

				return 0;
			}

			internal static int CompareVersions(int[] v1, string v2)
			{
				return CompareVersions(v1, DecodeVersion(v2));
			}

			internal static bool IsVersionZero(int[] version)
			{
				for (int i = 0; i < 4; i++)
				{
					if (version[i] != 0)
						return false;
				}

				return true;
			}
		}

        static string LocalUserName = null;
        static string LocalPassword = null;
        static string LocalDomain = null;


        private static bool LoadUpdateFileList(bool useCache, string baseUri, string moduleName, out List<FileVersionSettings> filesList)
		{
			long TotalSize = 0;

			UpdateModule = moduleName;
			UpdateUri = baseUri;

            filesList = null;

			if (string.IsNullOrEmpty(baseUri))
			{
				return true;
			}

			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

			XmlDocument Settings = LoadVersionSettings(baseUri);

			if (Settings != null)
			{
				XmlNode SettingsNode = Settings.SelectSingleNode("versionSettings");

				if (SettingsNode != null)
				{
					if (SettingsNode.Attributes["userName"] != null)
						LocalUserName = SettingsNode.Attributes["userName"].Value;

					if (SettingsNode.Attributes["domain"] != null)
						LocalDomain = SettingsNode.Attributes["domain"].Value;

					if (SettingsNode.Attributes["password"] != null)
						LocalPassword = SettingsNode.Attributes["password"].Value;
				}

				filesList = new List<FileVersionSettings>();

				foreach (XmlNode FileSettings in Settings.SelectNodes("//file"))
				{
					FileVersionSettings ThisSettings = new FileVersionSettings(FileSettings.Attributes["name"].Value);

					if (FileSettings.Attributes["version"] != null)
						ThisSettings.FileVersion = FileVersionSettings.DecodeVersion(FileSettings.Attributes["version"].Value);

					if (FileSettings.Attributes["productVersion"] != null)
						ThisSettings.ProductVersion = FileVersionSettings.DecodeVersion(FileSettings.Attributes["productVersion"].Value);

					if (FileSettings.Attributes["minVersion"] != null)
						ThisSettings.MinVersion = FileVersionSettings.DecodeVersion(FileSettings.Attributes["minVersion"].Value);

					if (FileSettings.Attributes["maxVersion"] != null)
						ThisSettings.MaxVersion = FileVersionSettings.DecodeVersion(FileSettings.Attributes["maxVersion"].Value);

					if (FileSettings.Attributes["mustExist"] != null)
						bool.TryParse(FileSettings.Attributes["mustExist"].Value, out ThisSettings.MustExist);

					if (FileSettings.Attributes["protect"] != null)
						bool.TryParse(FileSettings.Attributes["protect"].Value, out ThisSettings.Protect);

					if (FileSettings.Attributes["temporary"] != null)
						bool.TryParse(FileSettings.Attributes["temporary"].Value, out ThisSettings.Temporary);

					if (FileSettings.Attributes["fileSize"] != null)
						long.TryParse(FileSettings.Attributes["fileSize"].Value, out ThisSettings.FileSize);

					try
					{
						string FullName = Path.GetFullPath(ThisSettings.FileName.Replace('\\', '/'));

						if (File.Exists(FullName))
						{
							if (!ThisSettings.Protect)
							{
								FileVersionInfo Version = FileVersionInfo.GetVersionInfo(FullName);

								if (Version.FileVersion != null)
								{
									if (FileVersionSettings.CompareVersions(ThisSettings.FileVersion, Version.FileVersion) > 0)
										ThisSettings.MustDownload = true;

									if (FileVersionSettings.CompareVersions(ThisSettings.MinVersion, Version.FileVersion) > 0)
										ThisSettings.BreakIfFail = true;

									if (!FileVersionSettings.IsVersionZero(ThisSettings.MaxVersion))
									{
										if (FileVersionSettings.CompareVersions(ThisSettings.MaxVersion, Version.FileVersion) < 0)
										{
											ThisSettings.MustDownload = true;
											ThisSettings.BreakIfFail = true;
										}
									}
								}
								else
								{
									ThisSettings.BreakIfFail = ThisSettings.MustExist;
									ThisSettings.MustSave = true;
								}
							}
						}
						else
						{
							ThisSettings.MustDownload = true;
							ThisSettings.BreakIfFail = ThisSettings.MustExist;
							ThisSettings.MustSave = FileVersionSettings.IsVersionZero(ThisSettings.FileVersion);
						}

					}
					catch
					{
					}

					if(ThisSettings.MustDownload)
						TotalSize += ThisSettings.FileSize;

					filesList.Add(ThisSettings);
				}

                return true;
            }

            return false;
        }

		private static bool UpdateFromList(BackgroundWorker worker, bool useCache, string baseUri, string moduleName, List<FileVersionSettings> filesList)
		{
			long TotalSize = 0;
			long TotalRead = 0;

            foreach (FileVersionSettings ThisSettings in filesList)
            {
                if (ThisSettings.MustDownload)
                    TotalSize += ThisSettings.FileSize;
            }

			foreach (FileVersionSettings ThisSettings in filesList)
			{
                if (ThisSettings.MustDownload)
                {
                    try
                    {
                        string FullName = Path.GetFullPath(ThisSettings.FileName.Replace('\\', '/'));

                        if (ThisSettings.MustDownload)
                        {
                            int ThisRead = 0;
                            byte[] Buffer = null;

                            int Progress = (int)(99 * TotalRead / TotalSize);

                            if (Progress > 100)
                                Progress = 100;

                            worker.ReportProgress(Progress, "Loading " + ThisSettings.FileName);

                            try
                            {
                                HttpWebRequest Request;
                                HttpWebResponse Response;

                                Request = WebRequest.Create(baseUri + string.Join("/", new string[] { UpdateModule, ThisSettings.FileName.Replace('\\', '/') })) as HttpWebRequest;
                                Request.Method = "GET";

                                Response = Request.GetResponse() as HttpWebResponse;

                                Buffer = new byte[Response.ContentLength];
                                Stream RS = Response.GetResponseStream();
                                int Read, Len = 0;

                                while ((Read = RS.Read(Buffer, Len, (Buffer.Length - Len) > 16000 ? 16000 : (Buffer.Length - Len))) > 0)
                                {
                                    Len += Read;

                                    ThisRead += Read;

                                    Progress = (int)(99 * (TotalRead + ThisRead) / TotalSize);

                                    if (Progress > 100)
                                        Progress = 100;

                                    worker.ReportProgress(Progress);
                                }

                                Response.Close();

                                if (!ThisSettings.Temporary)
                                {
                                    try
                                    {
                                        bool LoggedOn = false;
                                        IntPtr tokenHandle = IntPtr.Zero;
                                        IntPtr dupeTokenHandle = new IntPtr(0);
                                        const int LOGON32_PROVIDER_DEFAULT = 0;
                                        const int LOGON32_LOGON_INTERACTIVE = 2;

                                        if (!string.IsNullOrEmpty(LocalUserName))
                                        {
                                            LoggedOn = LogonUser(LocalUserName, LocalDomain, LocalPassword, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref tokenHandle);
                                        }

                                        try
                                        {
                                            WindowsIdentity newId = null;
                                            WindowsImpersonationContext impersonatedUser = null;

                                            if (LoggedOn)
                                            {
                                                newId = new WindowsIdentity(tokenHandle);
                                                impersonatedUser = newId.Impersonate();
                                            }

                                            if (!Directory.Exists(Path.GetDirectoryName(FullName)))
                                                Directory.CreateDirectory(Path.GetDirectoryName(FullName));

                                            FileStream FS = File.Open(FullName, FileMode.Create, FileAccess.Write, FileShare.None);

                                            try
                                            {
                                                long FileTime = Response.LastModified.ToFileTime();

                                                SetFileTime(FS.SafeFileHandle.DangerousGetHandle(), ref FileTime, ref FileTime, ref FileTime);
                                            }
                                            catch
                                            {
                                            }

                                            FS.Write(Buffer, 0, Buffer.Length);
                                            FS.Close();

                                            if (LoggedOn)
                                            {
                                                impersonatedUser.Undo();
                                            }
                                        }
                                        finally
                                        {
                                            if (tokenHandle != IntPtr.Zero)
                                                CloseHandle(tokenHandle);
                                        }
                                    }
                                    catch
                                    {
                                        if (ThisSettings.MustSave && ThisSettings.BreakIfFail)
                                            return false;

                                        if (ThisSettings.FileName.IndexOf('\\') < 0 && ThisSettings.FileName.IndexOf(".dll") > 0)
                                        {
                                            try
                                            {
                                                if (AppDomain.CurrentDomain.Load(Buffer) == null)
                                                    RawAssemblies.Add(ThisSettings.FileName.Substring(0, ThisSettings.FileName.IndexOf('.')), Buffer);
                                            }
                                            catch
                                            {
                                                RawAssemblies.Add(ThisSettings.FileName.Substring(0, ThisSettings.FileName.IndexOf('.')), Buffer);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (ThisSettings.FileName.IndexOf('\\') < 0 && ThisSettings.FileName.IndexOf(".dll") > 0)
                                    {
                                        try
                                        {
                                            if (AppDomain.CurrentDomain.Load(Buffer) == null)
                                                RawAssemblies.Add(ThisSettings.FileName.Substring(0, ThisSettings.FileName.IndexOf('.')), Buffer);
                                        }
                                        catch
                                        {
                                            RawAssemblies.Add(ThisSettings.FileName.Substring(0, ThisSettings.FileName.IndexOf('.')), Buffer);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                if (ThisSettings.BreakIfFail)
                                    return false;

                                ThisRead = (int)ThisSettings.FileSize;
                            }

                            Buffer = null;

                            TotalRead += ThisRead;
                        }
                    }
                    catch
                    {
                    }
                }
			}

            GC.Collect();

            return true;


			string[] Libraries = new string[] { "NixxisClientWindow.dll", "NixxisClientLink.dll", "NixxisClientControls.dll", "fr\\NixxisClientWindow.resources.dll" };
			string LaucherName = "NixxisClientWindow.dll";
			string LauncherPath;

			try
			{
				for (int i = 0; i < Libraries.Length; i++)
				{
					try
					{
						HttpWebRequest Request;
						HttpWebResponse Response;

						Request = WebRequest.Create(baseUri + Libraries[i]) as HttpWebRequest;
						Request.Method = "GET";

						string FullName = Path.GetFullPath(Libraries[i]);

						if (File.Exists(FullName))
							Request.IfModifiedSince = File.GetLastWriteTimeUtc(FullName);

						Response = Request.GetResponse() as HttpWebResponse;

						if (Response.StatusCode == HttpStatusCode.OK && Response.ContentLength > 0)
						{
							byte[] Buffer = new byte[Response.ContentLength];
							Stream RS = Response.GetResponseStream();

							try
							{
								int Read, Len = 0;

								while ((Read = RS.Read(Buffer, Len, Buffer.Length - Len)) > 0)
								{
									Len += Read;
								}

								if (!Directory.Exists(Path.GetDirectoryName(FullName)))
									Directory.CreateDirectory(Path.GetDirectoryName(FullName));

								FileStream FS = File.Create(FullName);

								long FileTime = Response.LastModified.ToFileTime();

								SetFileTime(FS.SafeFileHandle.DangerousGetHandle(), ref FileTime, ref FileTime, ref FileTime);

								FS.Write(Buffer, 0, Buffer.Length);
								FS.Close();

							}
							catch
							{

								if (Libraries[i].IndexOf('\\') < 0)
									RawAssemblies.Add(Libraries[i].Substring(0, Libraries[i].IndexOf('.')), Buffer);
							}

							RS.Close();
						}
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
		}
	}
}
