using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Enable encryption of agents passwords.
    /// </summary>
    public class LoginEncryption : IDisposable
    {
        /// <summary>
        /// Helper class providing basing cryptography capabilities.
        /// </summary>
        protected class LoginProvider : IDisposable
        {
            /// <summary>
            /// The <see cref="System.Security.Cryptography.RSACryptoServiceProvider"/> instance.
            /// </summary>
            public System.Security.Cryptography.RSACryptoServiceProvider m_RSA;
            /// <summary>
            /// Ths <see cref="System.Security.Cryptography.MD5CryptoServiceProvider"/> instance.
            /// </summary>
            public System.Security.Cryptography.MD5CryptoServiceProvider m_MD5;

            /// <summary>
            /// Constructor. <see cref="m_RSA"/> and <see cref="m_MD5"/> are initialized and ready to be used. <see cref="m_RSA"/> is initialized by the provided byte array.
            /// </summary>
            /// <param name="blob">The byte array used to initialize <see cref="m_RSA"/>.</param>
            public LoginProvider(byte[] blob)
            {
                System.Security.Cryptography.RSACryptoServiceProvider.UseMachineKeyStore = true;

                CspParameters prms = new CspParameters();
                prms.KeyContainerName = "NixxisDefaultProvider";

                try
                {
                    m_RSA = new System.Security.Cryptography.RSACryptoServiceProvider(prms);
                    m_RSA.PersistKeyInCsp = false;
                    m_RSA.ImportCspBlob(blob);
                }
                catch
                {
                    System.Security.Cryptography.RSACryptoServiceProvider.UseMachineKeyStore = false;

                    try
                    {
                        m_RSA = new System.Security.Cryptography.RSACryptoServiceProvider(prms);
                        m_RSA.PersistKeyInCsp = false;
                        m_RSA.ImportCspBlob(blob);
                    }
                    catch
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
                    }
                }

                m_MD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            }

            #region IDisposable Members
            /// <inheritdoc/>
            public void Dispose()
            {
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

                lock (m_Providers)
                {
                    foreach (object Key in m_Providers.Keys)
                    {
                        if (m_Providers[Key] == this && m_ProvidersUsage.ContainsKey(Key))
                        {
                            if (((int)m_ProvidersUsage[Key]) == 1)
                            {
                                m_ProvidersUsage.Remove(Key);
                                m_Providers.Remove(Key);
                                break;
                            }
                            else
                            {
                                m_ProvidersUsage[Key] = ((int)m_ProvidersUsage[Key]) - 1;
                            }
                        }
                    }
                }
            }
            #endregion
        }

        static Hashtable m_Providers = new Hashtable();
        static Hashtable m_ProvidersUsage = new Hashtable();

        /// <summary>
        /// Defines goals of encryption.
        /// </summary>
        public enum Purpose
        {
            /// <summary>
            /// Specify that encryption request is done for authentication purpose.
            /// </summary>
            Authentication,
            /// <summary>
            /// Specify that encryption request is done for storage purpose.
            /// </summary>
            Storage
        }

        /// <summary>
        /// Represents the method that can be used when encrypted password needs storage. This method is used in the context of calls to the <see cref="VerifyPassword(string, string,bool, object, NewStorageDelegate)"/> method.
        /// </summary>
        /// <param name="state">The object that has been passed when calling <see cref="VerifyPassword(string, string,bool, object, NewStorageDelegate)"/>.</param>
        /// <param name="storage">The encrypted password to be stored.</param>
        public delegate void NewStorageDelegate(object state, string storage);

        LoginProvider m_Provider;
        bool m_Disposed;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="blob">The byte array that will be used to initialize the RSA provider.</param>
        public LoginEncryption(byte[] blob)
        {
            lock (m_Providers)
            {
                m_Provider = (LoginProvider)m_Providers[blob];

                if (m_Provider == null)
                {
                    m_Provider = new LoginProvider(blob);
                    m_Providers.Add(blob, m_Provider);
                    m_ProvidersUsage.Add(blob, 1);
                }
                else
                {
                    m_ProvidersUsage[blob] = ((int)m_ProvidersUsage[blob]) + 1;

                }
            }
        }
        /// <summary>
        /// Destructor.
        /// </summary>
        ~LoginEncryption()
        {
            if (!m_Disposed)
                Dispose(true);
        }
        /// <summary>
        /// Allow password encryption.
        /// </summary>
        /// <param name="password">The string that must be encrypted.</param>
        /// <param name="purpose">Indicates if the purpose of this method call is to authenticate a user or to store the encryption.</param>
        /// <returns>The encrypted password.</returns>
        /// <example>
        /// This example shows the code used to update an agent password with the value entered in the "txtPass".
        /// <code lang="C#">
        /// ContactRoute.Admin.Agent agt = new ContactRoute.Admin.Agent();
        /// agt.PassKey = m_AdminAccessor.LoginEncrypter.EncryptPassword(txtPass.Text, ContactRoute.Admin.LoginEncryption.Purpose.Storage);
        /// </code>
        /// </example>
        public string EncryptPassword(string password, Purpose purpose)
        {
            lock (m_Provider)
            {
                byte[] Pwd = System.Text.Encoding.Unicode.GetBytes(password);
                byte[] Hashed = m_Provider.m_MD5.ComputeHash(Pwd);
                byte[] Encrypted, Full;

                if (purpose == Purpose.Storage)
                {
                    Encrypted = m_Provider.m_RSA.Encrypt(Pwd, false);

                    Full = new byte[1 + Hashed.Length + Encrypted.Length];

                    Full[0] = (byte)Hashed.Length;

                    Buffer.BlockCopy(Hashed, 0, Full, 1, Hashed.Length);
                    Buffer.BlockCopy(Encrypted, 0, Full, Hashed.Length + 1, Encrypted.Length);
                }
                else
                {
                    byte[] OldPwd = System.Text.Encoding.Default.GetBytes(password);
                    byte[] OldHashed = m_Provider.m_MD5.ComputeHash(OldPwd);

                    Encrypted = m_Provider.m_RSA.Encrypt(Pwd, false);

                    Full = new byte[2 + Hashed.Length + Encrypted.Length + OldHashed.Length];

                    Full[0] = (byte)(OldHashed.Length | 0x80);
                    Full[1 + OldHashed.Length] = (byte)Hashed.Length;

                    Buffer.BlockCopy(OldHashed, 0, Full, 1, OldHashed.Length);
                    Buffer.BlockCopy(Hashed, 0, Full, 2 + OldHashed.Length, Hashed.Length);

                    Buffer.BlockCopy(Encrypted, 0, Full, 2 + OldHashed.Length + Hashed.Length, Encrypted.Length);
                }

                return System.Convert.ToBase64String(Full).Replace('+', '-').Replace('/', '_');
            }
        }
        /// <summary>
        /// Allow encrpted password decryption.
        /// </summary>
        /// <param name="password">The encrypted password to decrypt.</param>
        /// <returns>The decrypted result.</returns>
        public string DecryptPassword(string password)
        {
            lock (m_Provider)
            {
                if (m_Provider.m_RSA.PublicOnly)
                    return null;

                byte[] Full = System.Convert.FromBase64String(password.Replace('-', '+').Replace('_', '/'));

                int HashLength = (int)Full[0];

                if (HashLength >= Full.Length)
                    return null;

                byte[] Encrypted = new byte[Full.Length - (1 + (int)Full[0])];

                Buffer.BlockCopy(Full, 1 + (int)Full[0], Encrypted, 0, Encrypted.Length);

                return System.Text.Encoding.Unicode.GetString(m_Provider.m_RSA.Decrypt(Encrypted, false));
            }
        }

        /// <summary>
        /// Allow password verification.
        /// </summary>
        /// <param name="password">The encrypted password to be verified.</param>
        /// <param name="storage">The stored password to compare with.</param>
        /// <param name="compatibleStorage">Indicates if backward compatibility must be supported.</param>
        /// <returns>True if the password is valid.</returns>
        public bool VerifyPassword(string password, string storage, bool compatibleStorage)
        {
            return VerifyPassword(password, storage, compatibleStorage, null, null);
        }
        /// <summary>
        /// Allow password verification.
        /// </summary>
        /// <param name="password">The encrypted password to be verified.</param>
        /// <param name="storage">The stored password to compare with.</param>
        /// <param name="compatibleStorage">Indicates if backward compatibility must be supported.</param>
        /// <param name="newStorage">The method that will be called when new storage is needed.</param>
        /// <param name="state">The object that will be passed to the <paramref name="newStorage"/> delegate.</param>
        /// <returns>True if the password is valid.</returns>
        public bool VerifyPassword(string password, string storage, bool compatibleStorage, object state, NewStorageDelegate newStorage)
        {
            lock (m_Provider)
            {
                try
                {
                    bool Match = false;

                    byte[] Full = null;

                    Full = System.Convert.FromBase64String(password.Replace('-', '+').Replace('_', '/'));

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
                        byte[] StorageFull = System.Convert.FromBase64String(storage.Replace('-', '+').Replace('_', '/'));
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
                                string NewStorage = System.Convert.ToBase64String(Full, 1 + OldHashLength, Full.Length - (1 + OldHashLength)).Replace('+', '-').Replace('/', '_');

                                newStorage(state, NewStorage);
                            }
                        }
                    }

                    return Match;
                }
                catch
                {
                    return false;
                }
            }
        }


        #region IDisposable Members
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(false);
        }

        private void Dispose(bool fromFinalizer)
        {
            System.Diagnostics.Trace.WriteLine((new System.Diagnostics.TraceEventCache()).Callstack, "LoginEncryption");

            if (!m_Disposed)
            {
                m_Disposed = true;

                try
                {
                    lock (m_Providers)
                    {


                        foreach (DictionaryEntry dict in m_Providers)
                        {
                            if (dict.Value == m_Provider)
                            {
                                if (((int)m_ProvidersUsage[dict.Key]) == 1)
                                {
                                    m_ProvidersUsage.Remove(dict.Key);
                                    (dict.Value as LoginProvider).Dispose();
                                    m_Providers.Remove(dict.Key);
                                }
                                else
                                {
                                    m_ProvidersUsage[dict.Key] = ((int)m_ProvidersUsage[dict.Key]) - 1;
                                }
                                break;
                            }
                        }


                        if (m_Provider != null)
                        {
                            m_Provider.Dispose();
                            m_Provider = null;
                        }
                    }
                }
                catch
                {
                }

                if (!fromFinalizer)
                    GC.SuppressFinalize(this);

            }
        }
        #endregion
    }
}