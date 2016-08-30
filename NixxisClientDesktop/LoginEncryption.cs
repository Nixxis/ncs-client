using System;

namespace Nixxis
{

    public class LoginEncryption : IDisposable
    {
        public enum Purpose
        {
            Authentication,
            Storage
        }

        public delegate void NewStorageDelegate(object state, string storage);

        private System.Security.Cryptography.RSACryptoServiceProvider m_RSA;
        private System.Security.Cryptography.MD5CryptoServiceProvider m_MD5;
		private bool m_Disposed;

        public LoginEncryption(byte[] blob)
        {
			System.Security.Cryptography.RSACryptoServiceProvider.UseMachineKeyStore = true;

			try
			{
				m_RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
				m_RSA.PersistKeyInCsp = false;
				m_RSA.ImportCspBlob(blob);
			}
			catch
			{
				System.Security.Cryptography.RSACryptoServiceProvider.UseMachineKeyStore = false;

				m_RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
				m_RSA.PersistKeyInCsp = false;
				m_RSA.ImportCspBlob(blob);
			}

            m_MD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        }

		~LoginEncryption()
		{
			Dispose();
		}

        public string EncryptPassword(string password, Purpose purpose)
        {
            byte[] Pwd = System.Text.Encoding.Unicode.GetBytes(password);
            byte[] Hashed = m_MD5.ComputeHash(Pwd);
            byte[] Encrypted, Full;

            if (purpose == Purpose.Storage)
            {
                Encrypted = m_RSA.Encrypt(Pwd, false);

                Full = new byte[1 + Hashed.Length + Encrypted.Length];

                Full[0] = (byte)Hashed.Length;

                Buffer.BlockCopy(Hashed, 0, Full, 1, Hashed.Length);
                Buffer.BlockCopy(Encrypted, 0, Full, Hashed.Length + 1, Encrypted.Length);
            }
            else
            {
                byte[] OldPwd = System.Text.Encoding.Default.GetBytes(password);
                byte[] OldHashed = m_MD5.ComputeHash(OldPwd);

                Encrypted = m_RSA.Encrypt(Pwd, false);

                Full = new byte[2 + Hashed.Length + Encrypted.Length + OldHashed.Length];

                Full[0] = (byte)(OldHashed.Length | 0x80);
                Full[1 + OldHashed.Length] = (byte)Hashed.Length;

                Buffer.BlockCopy(OldHashed, 0, Full, 1, OldHashed.Length);
                Buffer.BlockCopy(Hashed, 0, Full, 2 + OldHashed.Length, Hashed.Length);

                Buffer.BlockCopy(Encrypted, 0, Full, 2 + OldHashed.Length + Hashed.Length, Encrypted.Length);
            }


            return Convert.ToBase64String(Full).Replace('+', '-').Replace('/', '_');
        }

        public string DecryptPassword(string password)
        {
            if (m_RSA.PublicOnly)
                return null;

            byte[] Full = Convert.FromBase64String(password.Replace('-', '+').Replace('_', '/'));

            int HashLength = (int)Full[0];

            if (HashLength >= Full.Length)
                return null;

            byte[] Encrypted = new byte[Full.Length - (1 + (int)Full[0])];

            Buffer.BlockCopy(Full, 1 + (int)Full[0], Encrypted, 0, Encrypted.Length);

            return System.Text.Encoding.Unicode.GetString(m_RSA.Decrypt(Encrypted, false));
        }

        public bool VerifyPassword(string password, string storage, bool compatibleStorage)
        {
            return VerifyPassword(password, storage, compatibleStorage, null, null);
        }

        public bool VerifyPassword(string password, string storage, bool compatibleStorage, object state, NewStorageDelegate newStorage)
        {
            bool Match = false;

            byte[] Full = Convert.FromBase64String(password.Replace('-', '+').Replace('_', '/'));
            int HashLength = (int)Full[0];
            int OldHashLength = 0;

            if ((HashLength & 0x80) != 0)
            {
                OldHashLength = (HashLength & 0x7F);
                HashLength = (int)Full[1 + OldHashLength];

            }

            byte[] Hashed = new byte[HashLength];
            byte[] OldHashed = null;

            if (OldHashLength > 0)
            {
                OldHashed = new byte[OldHashLength];

                Buffer.BlockCopy(Full, 1, OldHashed, 0, OldHashLength);
                Buffer.BlockCopy(Full, 2 + OldHashLength, Hashed, 0, HashLength);
            }
            else
            {
                Buffer.BlockCopy(Full, 1, Hashed, 0, HashLength);
            }

            try
            {
                byte[] StorageFull = Convert.FromBase64String(storage.Replace('-', '+').Replace('_', '/'));
                int StorageHashLength = (int)StorageFull[0];

                if (HashLength < Full.Length && StorageHashLength < StorageFull.Length && HashLength == StorageHashLength)
                {
                    byte[] StorageHashed = new byte[HashLength];

                    Buffer.BlockCopy(StorageFull, 1, StorageHashed, 0, HashLength);

                    Match = true;

                    for (int i = 0; i < HashLength; i++)
                    {
                        if (Hashed[i] != StorageHashed[i])
                        {
                            Match = false;
                            break;
                        }
                    }
                }
            }
            catch
            {
            }

            if (!Match && compatibleStorage && OldHashLength > 0)
            {
                Match = (System.Text.Encoding.Default.GetString(OldHashed) == storage);

                if (Match)
                {
                    if (newStorage != null)
                    {
                        string NewStorage = Convert.ToBase64String(Full, 1 + OldHashLength, Full.Length - (1 + OldHashLength)).Replace('+', '-').Replace('/', '_');

                        newStorage(state, NewStorage);
                    }
                }
            }

            return Match;
        }

		#region IDisposable Members

		public void Dispose()
		{
			if (!m_Disposed)
			{
				m_Disposed = true;

				if (m_RSA != null)
				{
					m_RSA.Clear();
					m_RSA = null;
				}

				if (m_MD5 != null)
				{
					m_MD5.Clear();
					m_MD5 = null;
				}

				GC.SuppressFinalize(this);
			}
		}

		#endregion
	}
}