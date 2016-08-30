using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nixxis.Client.Controls
{
    public class WaveInfo
    {

        private string m_Extension;
        private string m_Format;
        private int m_Length;
        private short m_Channels;
        private short m_AudioFormat;
        private int m_Samplerate;
        private int m_DataLength;
        private short m_BitsPerSample;
        private bool m_InfoLoaded;
        private bool m_IsValid = false;
        private string m_Description;


        public WaveInfo(string path)
        {
            try
            {
                m_Extension = Path.GetExtension(path);
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {

                    using (BinaryReader br = new BinaryReader(fs))
                    {

                        m_Length = (int)fs.Length - 8;
                        fs.Position = 8;
                        m_Format = new string(br.ReadChars(4));
                        fs.Position = 20;
                        m_AudioFormat = br.ReadInt16();
                        fs.Position = 22;
                        m_Channels = br.ReadInt16();
                        fs.Position = 24;
                        m_Samplerate = br.ReadInt32();
                        fs.Position = 34;

                        m_BitsPerSample = br.ReadInt16();
                        m_DataLength = (int)fs.Length - 44;
                        br.Close();
                    }
                    fs.Close();
                }
                m_InfoLoaded = true;
            }
            catch
            {
            }

            CheckValidity();
        }

        public override string ToString()
        {
            return m_Description;
        } 

        private void CheckValidity()
        {
            if (m_InfoLoaded && m_Format=="WAVE")
            {
                if (m_Samplerate == 8000 && m_BitsPerSample == 16 && m_Channels == 1 && m_AudioFormat==1)
                {
                    if (".wav".Equals(m_Extension))
                    {
                        m_Description = string.Format(TranslationContext.Default.Translate("File seems valid: \".wav\" extension, PCM 8 KHz, 16 bits, mono"));
                        m_IsValid = true;
                    }
                    else
                    {
                        m_Description = string.Format(TranslationContext.Default.Translate("Unsupported: wave file, PCM 8 KHz, 16 bits, mono but incorrect extension (consider renaming to \".wav\")"));
                    }
                }
                else
                {
                    string formatDescription = "Unknown compression";
                    switch (m_AudioFormat)
                    {
                        case 1:
                            formatDescription = "PCM/uncompressed";
                            break;
                        case 2:
                            formatDescription = "Microsoft ADPCM";
                            break;
                        case 6:
                            formatDescription = "ITU G.711 a-law";
                            break;
                        case 7:
                            formatDescription = "ITU G.711 u-law";
                            break;
                        case 17:
                            formatDescription = "IMA ADPCM";
                            break;
                        case 20:
                            formatDescription = "ITU G.723 ADPCM";
                            break;
                        case 49:
                            formatDescription = "GSM 6.10";
                            break;
                        case 64:
                            formatDescription = "ITU G.721 ADPCM";
                            break;
                        case 80:
                            formatDescription = "MPEG compression";
                            break;
                        case short.MaxValue:
                            formatDescription = "experimental compression";
                            break;
                    }
                    m_Description = string.Format(TranslationContext.Default.Translate("Unsupported: wave file {0} Hz, {1} bits, {2} channel(s), {3}. The recommended format is \".wav\" extension, PCM 8 KHz, 16 bits, mono."), m_Samplerate, m_BitsPerSample, m_Channels, formatDescription);
                }
            }
            else
            {
                if (".alaw".Equals(m_Extension) || ".ulaw".Equals(m_Extension) || ".gsm".Equals(m_Extension) || ".g722".Equals(m_Extension)|| ".g729".Equals(m_Extension))
                {
                    m_Description = TranslationContext.Default.Translate("File format cannot be verified (beacuse it is not a wave).");
                }
                else
                    m_Description = TranslationContext.Default.Translate("Unsupported file format. The recommended format is \".wav\" extension, 8 KHz, 16 bits, mono.");
            }
        }

        public bool IsSupported
        {
            get
            {
                return m_IsValid;
            }
        }
    }
}
