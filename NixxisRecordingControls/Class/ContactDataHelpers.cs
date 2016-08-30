using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using ContactRoute.Recording.Config;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Recording
{    
    public enum MediaSubTypes
    {
        Undefined,
        DirectCall,
        Inbound,
        ManualCall,
        Outbound,
        Chat,
    }
    public enum MediaTypes
    {
        Voice,
        Chat,
        Mail,
    }

    public class ContactList : ObservableCollection<ContactData>
    {
    }

    public class ContactData : INotifyPropertyChanged
    {
        public static TranslationContext TranslationContext = new TranslationContext("ContactDataHelpers");

        #region enums
        public enum OriginatorType
        {
            Undefined = 0,
            Agent = 1,
            Ivr = 2,
            Activity = 3,
            Script = 4,
            Queue = 5,
            RecordingPlaybackUser = 6,
        }
        public enum KeepRecordingStates
        {
            Default,
            Keep,
            NotKeep,
        }
        public enum RecordingStates
        {
            /// <summary>
            /// Default state for the contact (in V1 gray cricle)
            /// </summary>
            Unknown,

            /// <summary>
            /// The recording file is being downloaded (NEW)
            /// </summary>
            Loading,

            /// <summary>
            /// The recording file is being played or showed (in V1 Green play icon)
            /// </summary>
            Playing,

            /// <summary>
            /// The recording is no hold (NEW)
            /// </summary>
            Pause,

            ///// <summary>
            ///// The playing of the recording ended (NEW)
            ///// </summary>
            //Played,

            /// <summary>
            /// The recording is found on the server (in v1 yellow circle)
            /// </summary>
            Found,

            /// <summary>
            /// The recording was not found (in the v1 red cross)
            /// </summary>
            NotFound,
        }
        #endregion

        #region Class data
        private RecordingStates m_RecodingState = RecordingStates.Unknown;
        
        private string m_ContactId = string.Empty;
        private DateTime m_LocalDateTime;
        private string m_Originator = string.Empty;
        private string m_Destination = string.Empty;
        private string m_RecordingId = string.Empty;
        private int m_ContactStateId;
        private string m_ActivityId = string.Empty;
        private int m_ContactTypeId;
        private int m_Duration;
        private string m_CustomerId = string.Empty;
        private string m_Memo = string.Empty;
        private int m_Positive;
        private bool m_Argued;
        private string m_QualificationDescription = string.Empty;
        private string m_QualificationFullDescription = string.Empty;
        private string m_ContactQualificationId = string.Empty;
        private string m_QualificationIdOriginal = string.Empty;
        private string m_QualOriginatorId = string.Empty;
        private int m_QualOriginatorTypeId;
        private string m_OrigContactQualificationId = string.Empty;
        private string m_OrigQualOriginatorId = string.Empty;
        private int m_OrigQualOriginatorTypeId;
        private string m_CampaignId = string.Empty;
        private string m_EndReason = string.Empty;
        private string m_ContactListId = string.Empty;
        private string m_Extension = string.Empty;
        private int m_SetupDuration;
        private int m_ComDuration;
        private string m_RecScoreOriginator = string.Empty;
        private DateTime m_RecScoreDateTimeUtc;
        private int m_RecScoreTimeZone;
        private string m_RecScore = string.Empty;
        private string m_RecScoreOriginal = string.Empty;
        private string m_RecCommentOriginator = string.Empty;
        private DateTime m_RecCommentDateTimeUtc;
        private int m_RecCommentTimeZone;
        private string m_RecComment = string.Empty;
        private string m_RecCommentOriginal = string.Empty;
        private string m_UserMemo = string.Empty;
        private KeepRecordingStates m_KeepRecording;
        private string m_CampDescription = string.Empty;
        private string m_Description = string.Empty;
        private string m_FirstName = string.Empty;
        private string m_LastName = string.Empty;
        private string m_Account = string.Empty;
        private string m_AgtScoreName = string.Empty;
        private string m_AgtScoreAccount = string.Empty;
        private string m_AgtCommentName = string.Empty;
        private string m_AgtCommentAccount = string.Empty;
        private string m_AgtQualName = string.Empty;
        private string m_AgtQualAccount = string.Empty;
        private string m_AgtOrigQualName = string.Empty;
        private string m_AgtOrigQualAccount = string.Empty;

        private bool m_ForcedSaveCommentAndScore = false;
        private int m_RecordingMarker = 0;
        private int m_NbOfFiles = 0;
        private MediaSubTypes m_MediaSubType = MediaSubTypes.Undefined;
        #endregion

        #region Properties
        public RecordingStates RecodingState
        {
            get { return m_RecodingState; }
            set { m_RecodingState = value; FirePropertyChanged("RecodingState"); }
        }
        
        public string ContactId
        {
            get { return m_ContactId; }
            set { m_ContactId = value; FirePropertyChanged("ContactId"); }
        }
        public DateTime LocalDateTime
        {
            get { return m_LocalDateTime; }
            set { m_LocalDateTime = value; FirePropertyChanged("LocalDateTime"); }
        }
        public string Originator
        {
            get { return m_Originator; }
            set { m_Originator = value; FirePropertyChanged("Originator"); }
        }
        public string Destination
        {
            get { return m_Destination; }
            set { m_Destination = value; FirePropertyChanged("Destination"); }
        }
        public string RecordingId
        {
            get { return m_RecordingId; }
            set { m_RecordingId = value; FirePropertyChanged("RecordingId"); }
        }
        public int ContactStateId
        {
            get { return m_ContactStateId; }
            set { m_ContactStateId = value; FirePropertyChanged("ContactStateId"); }
        }
        public string ActivityId
        {
            get { return m_ActivityId; }
            set { m_ActivityId = value; FirePropertyChanged("ActivityId"); }
        }
        public int ContactTypeId
        {
            get { return m_ContactTypeId; }
            set { m_ContactTypeId = value; FirePropertyChanged("ContactTypeId"); }
        }
        public int Duration
        {
            get { return m_Duration; }
            set { m_Duration = value; FirePropertyChanged("Duration"); }
        }
        public string CustomerId
        {
            get { return m_CustomerId; }
            set { m_CustomerId = value; FirePropertyChanged("CustomerId"); }
        }
        public string Memo
        {
            get { return m_Memo; }
            set { m_Memo = value; FirePropertyChanged("Memo"); }
        }
        public int Positive
        {
            get { return m_Positive; }
            set { m_Positive = value; FirePropertyChanged("Positive"); }
        }
        public bool Argued
        {
            get { return m_Argued; }
            set { m_Argued = value; FirePropertyChanged("Argued"); }
        }
        public string CampaignId
        {
            get { return m_CampaignId; }
            set { m_CampaignId = value; FirePropertyChanged("CampaignId"); }
        }
        public string EndReason
        {
            get { return m_EndReason; }
            set { m_EndReason = value; FirePropertyChanged("EndReason"); }
        }
        public string ContactListId
        {
            get { return m_ContactListId; }
            set { m_ContactListId = value; FirePropertyChanged("ContactListId"); }
        }
        public string Extension
        {
            get { return m_Extension; }
            set { m_Extension = value; FirePropertyChanged("Extension"); }
        }
        public int SetupDuration
        {
            get { return m_SetupDuration; }
            set { m_SetupDuration = value; FirePropertyChanged("SetupDuration"); }
        }
        public int ComDuration
        {
            get { return m_ComDuration; }
            set { m_ComDuration = value; FirePropertyChanged("ComDuration"); }
        }

        public string QualificationDescription
        {
            get { return m_QualificationDescription; }
            set { m_QualificationDescription = value; FirePropertyChanged("QualificationDescription"); }
        }
        public string QualificationFullDescription
        {
            get { return m_QualificationFullDescription; }
            set { m_QualificationFullDescription = value; FirePropertyChanged("QualificationFullDescription"); }
        }
        public string QualificationId
        {
            get { return m_ContactQualificationId; }
            set { m_ContactQualificationId = value; FirePropertyChanged("QualificationId"); }
        }
        public string QualificationIdOriginal
        {
            get { return m_QualificationIdOriginal; }
            internal set { m_QualificationIdOriginal = value; FirePropertyChanged("QualificationIdOriginal"); }
        }
        public string QualificationOriginatorId
        {
            get { return m_QualOriginatorId; }
            set { m_QualOriginatorId = value; FirePropertyChanged("QualificationOriginatorId"); }
        }
        public int QualificationOriginatorTypeId
        {
            get { return m_QualOriginatorTypeId; }
            set { m_QualOriginatorTypeId = value; FirePropertyChanged("QualificationOriginatorTypeId"); FirePropertyChanged("QualificationOriginatorType"); }
        }
        public OriginatorType QualificationOriginatorType
        {
            get { return (OriginatorType)m_QualOriginatorTypeId; }
            set { this.QualificationOriginatorTypeId = (int)value; }
        }
        public string AgtQualName
        {
            get { return m_AgtQualName; }
            set { m_AgtQualName = value; FirePropertyChanged("AgtQualName"); }
        }
        public string AgtQualAccount
        {
            get { return m_AgtQualAccount; }
            set { m_AgtQualAccount = value; FirePropertyChanged("AgtQualAccount"); }
        }

        public string OrigQualificationId
        {
            get { return m_OrigContactQualificationId; }
            set { m_OrigContactQualificationId = value; FirePropertyChanged("OrigQualificationId"); }
        }
        public string OrigQualificationOriginatorId
        {
            get { return m_OrigQualOriginatorId; }
            set { m_OrigQualOriginatorId = value; FirePropertyChanged("OrigQualificationOriginatorId"); }
        }
        public int OrigQualificationOriginatorTypeId
        {
            get { return m_OrigQualOriginatorTypeId; }
            set { m_OrigQualOriginatorTypeId = value; FirePropertyChanged("OrigQualificationOriginatorTypeId"); ; FirePropertyChanged("OrigQualificationOriginatorType"); }
        }
        public OriginatorType OrigQualificationOriginatorType
        {
            get { return (OriginatorType)m_OrigQualOriginatorTypeId; }
            set { this.OrigQualificationOriginatorTypeId = (int)value; }
        }
        public string AgtOrigQualName
        {
            get { return m_AgtOrigQualName; }
            set { m_AgtOrigQualName = value; FirePropertyChanged("AgtOrigQualName"); }
        }
        public string AgtOrigQualAccount
        {
            get { return m_AgtOrigQualAccount; }
            set { m_AgtOrigQualAccount = value; FirePropertyChanged("AgtOrigQualAccount"); }
        }

        public string ScoreOriginator
        {
            get { return m_RecScoreOriginator; }
            set { m_RecScoreOriginator = value; FirePropertyChanged("ScoreOriginator"); }
        }
        public string ScoreAgentName
        {
            get { return m_AgtScoreName; }
            set { m_AgtScoreName = value; FirePropertyChanged("ScoreAgentName"); }
        }
        public string ScoreAgentAccount
        {
            get { return m_AgtScoreAccount; }
            set { m_AgtScoreAccount = value; FirePropertyChanged("ScoreAgentAccount"); }
        }
        public DateTime ScoreDateTimeUtc
        {
            get { return m_RecScoreDateTimeUtc; }
            set { m_RecScoreDateTimeUtc = value; FirePropertyChanged("ScoreDateTimeUtc"); FirePropertyChanged("ScoreLocalDatetime"); }
        }
        public int ScoreTimeZone
        {
            get { return m_RecScoreTimeZone; }
            set { m_RecScoreTimeZone = value; FirePropertyChanged("ScoreTimeZone"); FirePropertyChanged("ScoreLocalDatetime"); }
        }
        public DateTime ScoreLocalDatetime
        {
            get { return this.ScoreDateTimeUtc.AddMinutes(this.ScoreTimeZone); }
        }
        public string Score
        {
            get { return m_RecScore; }
            set { m_RecScore = value; FirePropertyChanged("Score"); }
        }
        public string ScoreOriginal
        {
            get { return m_RecScoreOriginal; }
            internal set { m_RecScoreOriginal = value; FirePropertyChanged("ScoreOriginal"); }
        }

        public string CommentOriginator
        {
            get { return m_RecCommentOriginator; }
            set { m_RecCommentOriginator = value; FirePropertyChanged("CommentOriginator"); }
        }
        public string CommentAgentName
        {
            get { return m_AgtCommentName; }
            set 
            {
                m_AgtCommentName = value; 
                FirePropertyChanged("CommentAgentName");
            }
        }
        public string CommentAgentAccount
        {
            get { return m_AgtCommentAccount; }
            set 
            { 
                m_AgtCommentAccount = value; 
                FirePropertyChanged("CommentAgentAccount");
            }
        }
        public DateTime CommentDateTimeUtc
        {
            get { return m_RecCommentDateTimeUtc; }
            set { m_RecCommentDateTimeUtc = value; FirePropertyChanged("CommentDateTimeUtc"); FirePropertyChanged("CommentLocalDatetime"); }
        }
        public int CommentTimeZone
        {
            get { return m_RecCommentTimeZone; }
            set { m_RecCommentTimeZone = value; FirePropertyChanged("CommentTimeZone"); FirePropertyChanged("CommentLocalDatetime"); }
        }
        public DateTime CommentLocalDatetime
        {
            get { return CommentDateTimeUtc.AddMinutes(CommentTimeZone); }
        }
        public string Comment
        {
            get { return m_RecComment; }
            set { m_RecComment = value; FirePropertyChanged("Comment"); }
        }
        public string CommentOriginal
        {
            get { return m_RecCommentOriginal; }
            internal set { m_RecCommentOriginal = value; FirePropertyChanged("CommentOriginal"); }
        }

        public string UserMemo
        {
            get { return m_UserMemo; }
            set { m_UserMemo = value; FirePropertyChanged("UserMemo"); }
        }
        public KeepRecordingStates KeepRecording
        {
            get { return m_KeepRecording; }
            set { m_KeepRecording = value; FirePropertyChanged("KeepRecording"); }
        }
        public string CampDescription
        {
            get { return m_CampDescription; }
            set { m_CampDescription = value; FirePropertyChanged("CampDescription"); }
        }
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; FirePropertyChanged("Description"); }
        }
        public string FirstName
        {
            get { return m_FirstName; }
            set { m_FirstName = value; FirePropertyChanged("FirstName"); }
        }
        public string LastName
        {
            get { return m_LastName; }
            set { m_LastName = value; FirePropertyChanged("LastName"); }
        }
        public string Account
        {
            get { return m_Account; }
            set { m_Account = value; FirePropertyChanged("Account"); }
        }

        public bool ForcedSaveCommentAndScore
        {
            get { return m_ForcedSaveCommentAndScore; }
            set { m_ForcedSaveCommentAndScore = value; }
        }
        public int RecordingMarker
        {
            get { return m_RecordingMarker; }
            set { m_RecordingMarker = value; FirePropertyChanged("RecordingMarker"); }
        }
        public int NbOfFiles
        {
            get { return m_NbOfFiles; }
            set { m_NbOfFiles = value; FirePropertyChanged("NbOfFiles"); }
        }
        public string MediaTypeDescription
        {
            get { return ContactData.GetMediaTypeDescription(m_MediaSubType); }
        }
        public MediaSubTypes MediaSubType
        {
            get { return m_MediaSubType; }
            set { m_MediaSubType = value; FirePropertyChanged("MediaTypeDescription"); FirePropertyChanged("MediaSubType"); }
        }

        public MediaTypes MediaType
        {
            get 
            {
                switch (m_MediaSubType)
                {
                    case MediaSubTypes.Chat:
                        return MediaTypes.Chat;

                    case MediaSubTypes.Undefined:
                    case MediaSubTypes.DirectCall:
                    case MediaSubTypes.Inbound:
                    case MediaSubTypes.ManualCall:
                    case MediaSubTypes.Outbound:
                    default:
                        return MediaTypes.Voice;
                }
            }
        }

        private bool? m_CampIsWritable;
        private bool? m_CampIsPower;
        private bool? m_CampIsFull;
        private bool? m_ActIsWritable;
        private bool? m_ActIsPower;
        private bool? m_ActIsFull;
        private bool? m_AgtIsWritable;
        private bool? m_AgtIsPower;
        private bool? m_AgtIsFull;


        public bool? CampIsWritable
        {
            get { return m_CampIsWritable; }
            set { m_CampIsWritable = value; FirePropertyChanged("CampIsWritable"); }
        }
        public bool? CampIsPower
        {
            get { return m_CampIsPower; }
            set { m_CampIsPower = value; FirePropertyChanged("CampIsPower"); }
        }
        public bool? CampIsFull
        {
            get { return m_CampIsFull; }
            set { m_CampIsFull = value; FirePropertyChanged("CampIsFull"); }
        }
        public bool? ActIsWritable
        {
            get { return m_ActIsWritable; }
            set { m_ActIsWritable = value; FirePropertyChanged("ActIsWritable"); }
        }
        public bool? ActIsPower
        {
            get { return m_ActIsPower; }
            set { m_ActIsPower = value; FirePropertyChanged("ActIsPower"); }
        }
        public bool? ActIsFull
        {
            get { return m_ActIsFull; }
            set { m_ActIsFull = value; FirePropertyChanged("ActIsFull"); }
        }
        public bool? AgtIsWritable
        {
            get { return m_AgtIsWritable; }
            set { m_AgtIsWritable = value; FirePropertyChanged("AgtIsWritable"); }
        }
        public bool? AgtIsPower
        {
            get { return m_AgtIsPower; }
            set { m_AgtIsPower = value; FirePropertyChanged("AgtIsPower"); }
        }
        public bool? AgtIsFull
        {
            get { return m_AgtIsFull; }
            set { m_AgtIsFull = value; FirePropertyChanged("AgtIsFull"); }
        }


        public bool UpdateCommandEnabled
        {
            get
            {
                bool?[] rights = new bool?[]{CampIsWritable, ActIsWritable, AgtIsWritable};
                bool atleatsoneistrue = false;

                foreach (bool? b in rights)
                {
                    if (b.HasValue)
                    {
                        if (!b.Value)
                            return false;
                        atleatsoneistrue = true;
                    }
                }
                return atleatsoneistrue;
            }
        }
        #endregion

        #region Members
        public static string GetMediaTypeDescription(MediaSubTypes mediaType)
        {
            switch (mediaType)
            {
                case MediaSubTypes.DirectCall:
                    return TranslationContext.Translate("Direct call");
                case MediaSubTypes.Inbound:
                    return TranslationContext.Translate("Inbound");
                case MediaSubTypes.ManualCall:
                    return TranslationContext.Translate("Manual call");
                case MediaSubTypes.Outbound:
                    return TranslationContext.Translate("Outbound");
                case MediaSubTypes.Chat:
                    return TranslationContext.Translate("Chat");
                default:
                    return "";
            }
        }
        public void UpdateOriginalValue()
        {
            this.ScoreOriginal = m_RecScore;
            this.CommentOriginal = m_RecCommentOriginal;
            this.QualificationIdOriginal = m_ContactQualificationId;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion
    }

    public class RecordingStateIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                switch((ContactData.RecordingStates)value)
                {
                    case ContactData.RecordingStates.Found:
                        return @"Images\RecordingStates\RecordingState_Found.png";
                    case ContactData.RecordingStates.Loading:
                        return @"Images\RecordingStates\RecordingState_Loading.png";
                    case ContactData.RecordingStates.NotFound:
                        return @"Images\RecordingStates\RecordingState_NotFound.png";
                    case ContactData.RecordingStates.Pause:
                        return @"Images\RecordingStates\RecordingState_Pause.png";
                    case ContactData.RecordingStates.Playing:
                        return @"Images\RecordingStates\RecordingState_Playing.png";
                    default:
                        return @"Images\RecordingStates\RecordingState_Unknown.png";

                }
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MediaTypeIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool small = parameter as string == "small" ? true : false; 

                switch ((MediaSubTypes)value)
                {
                    case MediaSubTypes.DirectCall:
                        return small ? @"Images\MediaTypes\MediaType_DirectCall25.png" : @"Images\MediaTypes\MediaType_DirectCall.png";
                    case MediaSubTypes.Inbound:
                        return small ? @"Images\MediaTypes\MediaType_Inbound25.png" : @"Images\MediaTypes\MediaType_Inbound.png";
                    case MediaSubTypes.ManualCall:
                        return small ? @"Images\MediaTypes\MediaType_Manual25.png" : @"Images\MediaTypes\MediaType_Manual.png";
                    case MediaSubTypes.Outbound:
                        return small ? @"Images\MediaTypes\MediaType_Outbound25.png" : @"Images\MediaTypes\MediaType_Outbound.png";
                    case MediaSubTypes.Chat:
                        return small ? @"Images\MediaTypes\MediaType_Chat25.png" : @"Images\MediaTypes\MediaType_Chat.png";
                    default:
                        return small ? @"Images\MediaTypes\MediaType_Undefined25.png" : @"Images\MediaTypes\MediaType_Undefined.png";
                }
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //CommentIcon
    public class KeepRecordingIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                switch ((ContactData.KeepRecordingStates)value)
                {
                    case ContactData.KeepRecordingStates.Keep:
                        return @"Images\KeepRecording\KeepRec_Keep25.png";
                    case ContactData.KeepRecordingStates.NotKeep:
                        return @"Images\KeepRecording\KeepRec_NotKeep25.png";
                    default:
                        return @"Images\KeepRecording\KeepRec_Default25.png";
                }
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CommentIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (string.IsNullOrEmpty(value as string))
                    return @"Images\CommentIcon\uncommented25.png";
                else
                    return @"Images\CommentIcon\commented25.png";
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
