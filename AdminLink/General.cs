using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using Nixxis;
using ContactRoute;
using System.Windows.Data;

namespace Nixxis.Client.Admin
{

    public enum TimeRelatedQuotaGranularity
    {
        None,
        FifteenMinutes,
        HalfHour,
        Hour,
        Day,
        Week,
        Month
    }

    public enum DataSourceType
    {
        Standard,
        Custom,
        Salesforce
    }


    [Flags]
    public enum PostWrapupOption
    {
        None = 0,
        Alert = 1,
        AlertSup = 2,
        CloseScript = 4,
        ForceReady = 8,
        ReadyWhenScriptIsClosed=16,
        IncreaseWrapupDuration=32,
        DecreaseWrapupDuration=64,
        SetWrapupDuration=128,
        IncreaseAutoReadyDelay = 256,
        DecreaseAutoReadyDelay = 512,
        SetAutoReadyDelay = 1024
    }


    /// <summary>
    /// Provide methods for handling encoding of PostWrapupOption values. Each option can have 3 state: inherited, allowed, denied. 
    ///
    /// </summary>
    public static class PostWrapupOptionHelper
    {
        /// <summary>
        /// Returns the allowed part of an encoded PostWrapupOption
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static PostWrapupOption AllowedOptions(int val)
        {
            int options = val & 65535;
            int mask = val >> 16;

            return (PostWrapupOption)(options & ~mask);
        }

        /// <summary>
        /// Returns the denied part of an encoded PostWrapupOption
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static PostWrapupOption DeniedOptions(int val)
        {
            //int options = val & 65535;
            int mask = val >> 16;

            return (PostWrapupOption)(/*options &*/ mask);
        }

        /// <summary>
        /// Compute the encoded form of PostWrapupOption, given the allowed part and the denied part.
        /// </summary>
        /// <param name="allowed"></param>
        /// <param name="denied"></param>
        /// <returns></returns>
        /// <remarks>Denied part is having priority over allowed part</remarks>
        public static int ComputeEncodedForm(PostWrapupOption allowed, PostWrapupOption denied)
        {

            int returnValue = (int)denied;
            returnValue = returnValue << 16;
            returnValue += (int)allowed;
            return returnValue;
        }

        /// <summary>
        /// Compute the encoded form of PostWrapupOption, given an initial encoded value, the allowed part and the denied part.
        /// </summary>
        /// <param name="initialValue"></param>
        /// <param name="toAllow"></param>
        /// <param name="toDeny"></param>
        /// <returns>Denied part is having priority over allowed part</returns>
        public static int ComputeEncodedForm(int initialValue, PostWrapupOption toAllow, PostWrapupOption toDeny)
        {
            int returnValue = (int)(toDeny | (DeniedOptions(initialValue) & ~toAllow));
            returnValue = returnValue << 16;
            returnValue += (int)(toAllow | AllowedOptions(initialValue));
            return returnValue;
        }

        /// <summary>
        /// Compute the encoded form of PostWrapupOption, given an initial encoded value and the part that must be inherited.
        /// </summary>
        /// <param name="initialValue"></param>
        /// <param name="toInherit"></param>
        public static int ComputeEncodedForm(int initialValue, PostWrapupOption toInherit)
        {
            int returnValue = (int)(DeniedOptions(initialValue) & ~toInherit);
            returnValue = returnValue << 16;
            returnValue += (int)(AllowedOptions(initialValue) & ~toInherit);
            return returnValue;
        }


    }


    public class ReflectionHelper
    {
        internal class PropertyHelper
        {
            internal PropertyInfo m_Property;
            internal Dictionary<Type, object[]> m_Attributes = new Dictionary<Type, object[]>();

            internal PropertyHelper()
            {
            }
            internal PropertyHelper(PropertyInfo pinfo)
            {
                m_Property = pinfo;
            }
        }
        internal class TypeHelper
        {
            internal Type m_Type;
            internal Dictionary<Type, object> m_Attributes = new Dictionary<Type, object>();
            internal Dictionary<string, PropertyHelper> m_Properties = new Dictionary<string, PropertyHelper>();
        }
        private static Dictionary<Type, TypeHelper> m_Types = new Dictionary<Type,TypeHelper>();

        public static PropertyInfo GetPropertyInfo(Type tpe, string name)
        {
            TypeHelper typeHelper = null;
            PropertyHelper propHelper = null;
            if (!m_Types.TryGetValue(tpe, out typeHelper))
            {
                typeHelper = new TypeHelper();
                m_Types.Add(tpe, typeHelper);
            }
            if (!typeHelper.m_Properties.TryGetValue(name, out propHelper))
            {
                PropertyInfo pinfo = tpe.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                propHelper = new PropertyHelper(pinfo);
                typeHelper.m_Properties.Add(name, propHelper);
            }
            return propHelper.m_Property;
        }

        public static object[] GetCustomAttributes(PropertyInfo propertyinfo, Type attributeType)
        {
            Type tpe = propertyinfo.DeclaringType;
            TypeHelper typeHelper = null;
            PropertyHelper propHelper = null;
            object[] attrs = null;
            if (!m_Types.TryGetValue(tpe, out typeHelper))
            {
                typeHelper = new TypeHelper();
                m_Types.Add(tpe, typeHelper);
            }
            if (!typeHelper.m_Properties.TryGetValue(propertyinfo.Name, out propHelper))
            {
                PropertyInfo pinfo = tpe.GetProperty(propertyinfo.Name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                propHelper = new PropertyHelper(pinfo);
                typeHelper.m_Properties.Add(propertyinfo.Name, propHelper);
            }

            if (!propHelper.m_Attributes.TryGetValue(attributeType, out attrs))
            {
                attrs = propertyinfo.GetCustomAttributes(attributeType, true);
                propHelper.m_Attributes.Add(attributeType, attrs);
            }
            return attrs;
        }

    }

    public class EnumHelper<T>
    {
        public T EnumValue { get; set; }
        public string Description { get; set; }

        public EnumHelper(T enumvalue, string description)
        {
            EnumValue = enumvalue;
            Description = description;
        }
    }

    public enum OperandType
    {
        None,
        Same,
        Duration
    }

    public class OperatorEnumHelper: EnumHelper<Operator>
    {

        public static bool Compatibility(Operator op, DBTypes dbType)
        {
            switch (dbType)
            {
                case DBTypes.String:
                    return CompatibilityWithString(op);
                case DBTypes.Boolean:
                    return CompatibilityWithBool(op);
                case DBTypes.Datetime:
                    return CompatibilityWithDate(op);
                case DBTypes.Integer:
                case DBTypes.Float:
                    return CompatibilityWithInt(op);
            }

            return false;
        }

        public static bool CompatibilityWithString(Operator op)
        {
            return (op == Operator.Equal || op == Operator.Inferior || op == Operator.InferiorOrEqual || op == Operator.IsNotNull || op == Operator.IsNull || op == Operator.NotEqual || op == Operator.Superior || op == Operator.SuperiorOrEqual || op == Operator.Like || op==Operator.IsNotEmpty || op==Operator.IsNotNullAndNotEmpty || op==Operator.IsEmpty || op==Operator.IsNullOrEmpty);
        }
        public static bool CompatibilityWithInt(Operator op)
        {
            return (op == Operator.Equal || op == Operator.Inferior || op == Operator.InferiorOrEqual || op == Operator.IsNotNull || op == Operator.IsNull || op == Operator.NotEqual || op == Operator.Superior || op == Operator.SuperiorOrEqual);
        }
        public static bool CompatibilityWithDate(Operator op)
        {
            return (op == Operator.Equal || op == Operator.Inferior || op == Operator.InferiorOrEqual || op == Operator.IsNotNull || op == Operator.IsNull || op == Operator.NotEqual || op == Operator.Superior || op == Operator.SuperiorOrEqual || op == Operator.IsAfter || op == Operator.IsBefore || op == Operator.IsInTheFuture || op == Operator.IsInThePast || op == Operator.IsXMinutesInTheFuture || op == Operator.IsXMinutesInThePast);
        }
        public static bool CompatibilityWithBool(Operator op)
        {
            return (op == Operator.Equal || op == Operator.IsNotNull || op == Operator.IsNull || op == Operator.NotEqual || op == Operator.IsTrue || op == Operator.IsFalse);
        }


        public OperandType OperandType { get; set; }

        public OperatorEnumHelper(Operator enumvalue, string description, OperandType operandType)
            : base(enumvalue, description)
        {
            OperandType = operandType;
        }
    }

    public class AggregatorEnumHelper : EnumHelper<Aggregator>
    {
        public AggregatorEnumHelper(Aggregator enumValue, string description)
            : base(enumValue, description)
        {
        }
    }

    public class TimeRelatedQuotaGranularitiesHelper : ObservableCollection<EnumHelper<TimeRelatedQuotaGranularity>>
    {
        public TimeRelatedQuotaGranularitiesHelper()
        {
            Add(new EnumHelper<TimeRelatedQuotaGranularity>(TimeRelatedQuotaGranularity.None, TranslationContext.Default.Translate("None")));
            Add(new EnumHelper<TimeRelatedQuotaGranularity>(TimeRelatedQuotaGranularity.FifteenMinutes, TranslationContext.Default.Translate("15 minutes")));
            Add(new EnumHelper<TimeRelatedQuotaGranularity>(TimeRelatedQuotaGranularity.HalfHour, TranslationContext.Default.Translate("30 minutes")));
            Add(new EnumHelper<TimeRelatedQuotaGranularity>(TimeRelatedQuotaGranularity.Hour, TranslationContext.Default.Translate("Hour")));            
            Add(new EnumHelper<TimeRelatedQuotaGranularity>(TimeRelatedQuotaGranularity.Day, TranslationContext.Default.Translate("Day")));
            Add(new EnumHelper<TimeRelatedQuotaGranularity>(TimeRelatedQuotaGranularity.Week, TranslationContext.Default.Translate("Week")));
            Add(new EnumHelper<TimeRelatedQuotaGranularity>(TimeRelatedQuotaGranularity.Month, TranslationContext.Default.Translate("Month")));
        }
    }


    public class ViewRestrictionTargetTypeHelper : RestrictedViewRestrictionTargetTypeHelper
    {
        public ViewRestrictionTargetTypeHelper()
        {
            Add(new EnumHelper<ViewRestrictionTargetType>(ViewRestrictionTargetType.Agent, TranslationContext.Default.Translate("Agent")));
            Add(new EnumHelper<ViewRestrictionTargetType>(ViewRestrictionTargetType.MyGroup, TranslationContext.Default.Translate("My group")));
        }
    }


    public class RestrictedViewRestrictionTargetTypeHelper : ObservableCollection<EnumHelper<ViewRestrictionTargetType>>
    {
        public RestrictedViewRestrictionTargetTypeHelper()
        {
            Add(new EnumHelper<ViewRestrictionTargetType>(ViewRestrictionTargetType.Any, TranslationContext.Default.Translate("Any")));
            Add(new EnumHelper<ViewRestrictionTargetType>(ViewRestrictionTargetType.Campaign, TranslationContext.Default.Translate("Campaign")));
            Add(new EnumHelper<ViewRestrictionTargetType>(ViewRestrictionTargetType.Team, TranslationContext.Default.Translate("Team")));
            Add(new EnumHelper<ViewRestrictionTargetType>(ViewRestrictionTargetType.MyTeam, TranslationContext.Default.Translate("My team")));
            Add(new EnumHelper<ViewRestrictionTargetType>(ViewRestrictionTargetType.Inbound, TranslationContext.Default.Translate("Inbound")));
            Add(new EnumHelper<ViewRestrictionTargetType>(ViewRestrictionTargetType.Outbound, TranslationContext.Default.Translate("Outbound")));
            Add(new EnumHelper<ViewRestrictionTargetType>(ViewRestrictionTargetType.Mail, TranslationContext.Default.Translate("Mail")));
            Add(new EnumHelper<ViewRestrictionTargetType>(ViewRestrictionTargetType.Chat, TranslationContext.Default.Translate("Chat")));
            Add(new EnumHelper<ViewRestrictionTargetType>(ViewRestrictionTargetType.Queue, TranslationContext.Default.Translate("Queue")));

        }
    }


    public class DataSourceTypeHelper : ObservableCollection<EnumHelper<DataSourceType>>
    {
        public DataSourceTypeHelper()
        {
            Add(new EnumHelper<DataSourceType>(DataSourceType.Standard, TranslationContext.Default.Translate("Standard")));
            Add(new EnumHelper<DataSourceType>(DataSourceType.Custom, TranslationContext.Default.Translate("Custom")));
            Add(new EnumHelper<DataSourceType>(DataSourceType.Salesforce, TranslationContext.Default.Translate("Salesforce")));
        }
    }


    public class QualificationActionHelperBase : ObservableCollection<EnumHelper<QualificationAction>>
    {
        public QualificationActionHelperBase()
        {
            Add(new EnumHelper<QualificationAction>(QualificationAction.None, TranslationContext.Default.Translate("Default")));
            Add(new EnumHelper<QualificationAction>(QualificationAction.DoNotRetry, TranslationContext.Default.Translate("Do not retry")));
            Add(new EnumHelper<QualificationAction>(QualificationAction.RetryAt, TranslationContext.Default.Translate("Retry at")));
            Add(new EnumHelper<QualificationAction>(QualificationAction.RetryNotBefore, TranslationContext.Default.Translate("Retry not before")));
            Add(new EnumHelper<QualificationAction>(QualificationAction.Callback, TranslationContext.Default.Translate("Callback")));
            Add(new EnumHelper<QualificationAction>(QualificationAction.TargetedCallback, TranslationContext.Default.Translate("Targeted callback")));
            Add(new EnumHelper<QualificationAction>(QualificationAction.BlackList, TranslationContext.Default.Translate("Black list")));
        }
    }

    public class DialerActionHelper : ObservableCollection<EnumHelper<QualificationAction>>
    {
        public DialerActionHelper()
        {
            Add(new EnumHelper<QualificationAction>(QualificationAction.None, TranslationContext.Default.Translate("Fresh")));
            Add(new EnumHelper<QualificationAction>(QualificationAction.DoNotRetry, TranslationContext.Default.Translate("Do not retry")));
            Add(new EnumHelper<QualificationAction>(QualificationAction.RetryAt, TranslationContext.Default.Translate("Retry at")));
            Add(new EnumHelper<QualificationAction>(QualificationAction.RetryNotBefore, TranslationContext.Default.Translate("Retry not before")));
            Add(new EnumHelper<QualificationAction>(QualificationAction.Callback, TranslationContext.Default.Translate("Callback")));
            Add(new EnumHelper<QualificationAction>(QualificationAction.TargetedCallback, TranslationContext.Default.Translate("Targeted callback")));
        }
    }

    public class DialDisconnectionReasonHelperBase : ObservableCollection<EnumHelper<DialDisconnectionReason>>
    {
        public DialDisconnectionReasonHelperBase()
        {
            Add(new EnumHelper<DialDisconnectionReason>(DialDisconnectionReason.Abandoned, TranslationContext.Default.Translate("Abandoned")));
            Add(new EnumHelper<DialDisconnectionReason>(DialDisconnectionReason.Agent, TranslationContext.Default.Translate("Handled by agent")));
            Add(new EnumHelper<DialDisconnectionReason>(DialDisconnectionReason.AgentUnavailable, TranslationContext.Default.Translate("Unhandled by agent")));
            Add(new EnumHelper<DialDisconnectionReason>(DialDisconnectionReason.AnsweringMachine, TranslationContext.Default.Translate("Answering machine")));
            Add(new EnumHelper<DialDisconnectionReason>(DialDisconnectionReason.Busy, TranslationContext.Default.Translate("Busy")));
            Add(new EnumHelper<DialDisconnectionReason>(DialDisconnectionReason.Congestion, TranslationContext.Default.Translate("Congested")));
            Add(new EnumHelper<DialDisconnectionReason>(DialDisconnectionReason.Disturbed, TranslationContext.Default.Translate("Disturbed")));
            Add(new EnumHelper<DialDisconnectionReason>(DialDisconnectionReason.Fax, TranslationContext.Default.Translate("Fax")));
            Add(new EnumHelper<DialDisconnectionReason>(DialDisconnectionReason.NoAnswer, TranslationContext.Default.Translate("Not answered")));
            Add(new EnumHelper<DialDisconnectionReason>(DialDisconnectionReason.ValidityEllapsed, TranslationContext.Default.Translate("Out of date")));
        }
    }

    public class DialDisconnectionReasonHelper : DialDisconnectionReasonHelperBase
    {
        public DialDisconnectionReasonHelper()
            : base()
        {
            Add(new EnumHelper<DialDisconnectionReason>(DialDisconnectionReason.None, "?"));
        }
    }

    public class QualificationActionHelper : QualificationActionHelperBase
    {
        public QualificationActionHelper(): base()
        {
            Add(new EnumHelper<QualificationAction>(QualificationAction.ChangeActivity, TranslationContext.Default.Translate("Change activity")));
        }
    }

    public class AbandonRateModeHelper : ObservableCollection<EnumHelper<AbandonRateMode>>
    {
        public AbandonRateModeHelper()
        {
            Add(new EnumHelper<AbandonRateMode>(AbandonRateMode.Standard, TranslationContext.Default.Translate("Standard")));
            Add(new EnumHelper<AbandonRateMode>(AbandonRateMode.AnsweringmachineIncluded, TranslationContext.Default.Translate("Answering machines included")));
            Add(new EnumHelper<AbandonRateMode>(AbandonRateMode.EveryDialedIncluded, TranslationContext.Default.Translate("All calls included")));
        }
    }

    public class FrequencyHelper : ObservableCollection<EnumHelper<Frequency>>
    {
        public FrequencyHelper()
        {
            Add(new EnumHelper<Frequency>(Frequency.Never, TranslationContext.Default.Translate("Never")));
            Add(new EnumHelper<Frequency>(Frequency.Once, TranslationContext.Default.Translate("Once")));
            Add(new EnumHelper<Frequency>(Frequency.WhenChanged, TranslationContext.Default.Translate("When value changes")));
            Add(new EnumHelper<Frequency>(Frequency.Continuously, TranslationContext.Default.Translate("At each loop")));
        }
    }

    public class QuotaComputationMethodsHelper : ObservableCollection<EnumHelper<QuotaComputationMethod>>
    {
        public QuotaComputationMethodsHelper()
        {
            Add(new EnumHelper<QuotaComputationMethod>(QuotaComputationMethod.Unknown, TranslationContext.Default.Translate("Unknown")));
            Add(new EnumHelper<QuotaComputationMethod>(QuotaComputationMethod.DirrectValue, TranslationContext.Default.Translate("Dirrect value")));
            Add(new EnumHelper<QuotaComputationMethod>(QuotaComputationMethod.Formula, TranslationContext.Default.Translate("Formula")));
        }
    }

    public class QuotaTargetComputationMethodsHelper : ObservableCollection<EnumHelper<QuotaTargetComputationMethod>>
    {
        public QuotaTargetComputationMethodsHelper()
        {
            Add(new EnumHelper<QuotaTargetComputationMethod>(QuotaTargetComputationMethod.CountQualified, TranslationContext.Default.Translate("Count qualified")));
            Add(new EnumHelper<QuotaTargetComputationMethod>(QuotaTargetComputationMethod.CountPositive, TranslationContext.Default.Translate("Count positive")));
            Add(new EnumHelper<QuotaTargetComputationMethod>(QuotaTargetComputationMethod.CountNegative, TranslationContext.Default.Translate("Count negative")));
            Add(new EnumHelper<QuotaTargetComputationMethod>(QuotaTargetComputationMethod.CountNeutral, TranslationContext.Default.Translate("Count neutral")));
            Add(new EnumHelper<QuotaTargetComputationMethod>(QuotaTargetComputationMethod.CountArgued, TranslationContext.Default.Translate("Count argued")));
            Add(new EnumHelper<QuotaTargetComputationMethod>(QuotaTargetComputationMethod.SumQualificationValue, TranslationContext.Default.Translate("Sum qualification values")));
            Add(new EnumHelper<QuotaTargetComputationMethod>(QuotaTargetComputationMethod.SumPositiveQualificationValue, TranslationContext.Default.Translate("Sum positive values")));
        }
    }

    public class DBTypeHelper : ObservableCollection<EnumHelper<DBTypes>>
    {
        public DBTypeHelper()
        {
            Add(new EnumHelper<DBTypes>(DBTypes.Boolean, TranslationContext.Default.Translate("Boolean")));
            Add(new EnumHelper<DBTypes>(DBTypes.Datetime, TranslationContext.Default.Translate("Date and time")));
            Add(new EnumHelper<DBTypes>(DBTypes.Float, TranslationContext.Default.Translate("Float")));
            Add(new EnumHelper<DBTypes>(DBTypes.Integer, TranslationContext.Default.Translate("Integer")));
            Add(new EnumHelper<DBTypes>(DBTypes.String, TranslationContext.Default.Translate("String")));
            Add(new EnumHelper<DBTypes>(DBTypes.Char, TranslationContext.Default.Translate("Char")));
        }
    }

    public class DynamicParameterActionHelper : ObservableCollection<EnumHelper<DynamicParameterAction>>
    {
        public DynamicParameterActionHelper()
        {
            Add(new EnumHelper<DynamicParameterAction>(DynamicParameterAction.Initialize, TranslationContext.Default.Translate("Initialize")));
            Add(new EnumHelper<DynamicParameterAction>(DynamicParameterAction.Overwrite, TranslationContext.Default.Translate("Overwrite")));
            Add(new EnumHelper<DynamicParameterAction>(DynamicParameterAction.Clear, TranslationContext.Default.Translate("Clear")));
        }
    }

    public class CombineOperatorHelper : ObservableCollection<EnumHelper<CombineOperator>>
    {
        public CombineOperatorHelper()
        {
            Add(new EnumHelper<CombineOperator>(CombineOperator.None, TranslationContext.Default.Translate("None")));
            Add(new EnumHelper<CombineOperator>(CombineOperator.And, TranslationContext.Default.Translate("And")));
            Add(new EnumHelper<CombineOperator>(CombineOperator.Or, TranslationContext.Default.Translate("Or")));
        }
    }

    public class AggregatorHelper : ObservableCollection<AggregatorEnumHelper>
    {
        public AggregatorHelper()
        {
            Add(new AggregatorEnumHelper(Aggregator.None, TranslationContext.Default.Translate("None")));
            Add(new AggregatorEnumHelper(Aggregator.AbsoluteAggregatedValue, TranslationContext.Default.Translate("Absolute progression")));
            Add(new AggregatorEnumHelper(Aggregator.AbsoluteRemainingAggregated, TranslationContext.Default.Translate("Absolute target distance")));
            Add(new AggregatorEnumHelper(Aggregator.RelativeAggregatedValue, TranslationContext.Default.Translate("Relative progression")));
            Add(new AggregatorEnumHelper(Aggregator.RelativeRemainingAggregated, TranslationContext.Default.Translate("Relative target distance")));
        }
    }

    public class OperatorHelper : ObservableCollection<OperatorEnumHelper>
    {
        public OperatorHelper()
        {
            Add(new OperatorEnumHelper(Operator.Equal, TranslationContext.Default.Translate("Is equal to"), OperandType.Same));
            Add(new OperatorEnumHelper(Operator.Inferior, TranslationContext.Default.Translate("Is less than"), OperandType.Same));
            Add(new OperatorEnumHelper(Operator.InferiorOrEqual, TranslationContext.Default.Translate("Is less or equal"), OperandType.Same));
            Add(new OperatorEnumHelper(Operator.IsAfter, TranslationContext.Default.Translate("Is after"), OperandType.Same));
            Add(new OperatorEnumHelper(Operator.IsBefore, TranslationContext.Default.Translate("Is before"), OperandType.Same));
            Add(new OperatorEnumHelper(Operator.IsFalse, TranslationContext.Default.Translate("Is false"), OperandType.None));
            Add(new OperatorEnumHelper(Operator.IsInTheFuture, TranslationContext.Default.Translate("Is in the future"), OperandType.None));
            Add(new OperatorEnumHelper(Operator.IsInThePast, TranslationContext.Default.Translate("Is in the past"), OperandType.None));
            Add(new OperatorEnumHelper(Operator.IsNotNull, TranslationContext.Default.Translate("Is not null"), OperandType.None));
            Add(new OperatorEnumHelper(Operator.IsNull, TranslationContext.Default.Translate("Is null"), OperandType.None));
            Add(new OperatorEnumHelper(Operator.IsTrue, TranslationContext.Default.Translate("Is true"), OperandType.None));
            Add(new OperatorEnumHelper(Operator.IsXMinutesInTheFuture, TranslationContext.Default.Translate("Is in the future for"), OperandType.Duration));
            Add(new OperatorEnumHelper(Operator.IsXMinutesInThePast, TranslationContext.Default.Translate("Is in the past for"), OperandType.Duration));
            Add(new OperatorEnumHelper(Operator.Like, TranslationContext.Default.Translate("Is like"), OperandType.Same));
            Add(new OperatorEnumHelper(Operator.NotEqual, TranslationContext.Default.Translate("Is not equal to"), OperandType.Same));
            Add(new OperatorEnumHelper(Operator.Superior, TranslationContext.Default.Translate("Is bigger than"), OperandType.Same));
            Add(new OperatorEnumHelper(Operator.SuperiorOrEqual, TranslationContext.Default.Translate("Is bigger or equal"), OperandType.Same));

            Add(new OperatorEnumHelper(Operator.IsEmpty, TranslationContext.Default.Translate("Is empty"), OperandType.None));
            Add(new OperatorEnumHelper(Operator.IsNotEmpty, TranslationContext.Default.Translate("Is not empty"), OperandType.None));
            Add(new OperatorEnumHelper(Operator.IsNullOrEmpty, TranslationContext.Default.Translate("Is null or empty"), OperandType.None));
            Add(new OperatorEnumHelper(Operator.IsNotNullAndNotEmpty, TranslationContext.Default.Translate("Is not null and not empty"), OperandType.None));
        }
    }


    public class SortOrderHelper : ObservableCollection<EnumHelper<SortOrder>>
    {
        public SortOrderHelper()
        {
            Add(new EnumHelper<SortOrder>(SortOrder.Ascending, TranslationContext.Default.Translate("Ascending")));
            Add(new EnumHelper<SortOrder>(SortOrder.Descending, TranslationContext.Default.Translate("Descending")));
        }
    }


    public class UserFieldMeaningHelper : ObservableCollection<EnumHelper<UserFieldMeanings>>
    {
        public UserFieldMeaningHelper()
        {
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.Address1, TranslationContext.Default.Translate("Address part 1")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.Address2, TranslationContext.Default.Translate("Address part 2")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.AppointmentArea, TranslationContext.Default.Translate("Appointment area")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.AppointmentDateTime, TranslationContext.Default.Translate("Appointment date/time")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.AppointmentId, TranslationContext.Default.Translate("Appointment identifier")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.BusinessChat, TranslationContext.Default.Translate("Business chat address")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.BusinessEmail, TranslationContext.Default.Translate("Business email address")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.BusinessFax, TranslationContext.Default.Translate("Business fax number")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.BusinessMobileNumber, TranslationContext.Default.Translate("Business mobile phone")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.BusinessPhoneNumber, TranslationContext.Default.Translate("Business phone number")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.City, TranslationContext.Default.Translate("City")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.ContactDescription, TranslationContext.Default.Translate("Contact description")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.Country, TranslationContext.Default.Translate("Country")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.CustomerId, TranslationContext.Default.Translate("Customer id")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.HomeChat, TranslationContext.Default.Translate("Private chat address")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.HomeEmail, TranslationContext.Default.Translate("Private email address")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.HomeFax, TranslationContext.Default.Translate("Private fax number")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.HomeMobileNumber, TranslationContext.Default.Translate("Private mobile phone")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.HomePhoneNumber, TranslationContext.Default.Translate("Home phone number")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.Latitude, TranslationContext.Default.Translate("Latitude")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.Longitude, TranslationContext.Default.Translate("Longitude")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.MaxDial, TranslationContext.Default.Translate("Maximum dial attempts")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.None, TranslationContext.Default.Translate("None")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.PostalCode, TranslationContext.Default.Translate("Postal code")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.PotentialProfit, TranslationContext.Default.Translate("Potential profit")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.Region, TranslationContext.Default.Translate("Region")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.OptInDate, TranslationContext.Default.Translate("OptIn date")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.OptInStatus, TranslationContext.Default.Translate("OptIn status")));
            Add(new EnumHelper<UserFieldMeanings>(UserFieldMeanings.Quota, TranslationContext.Default.Translate("Quota")));
        }
    }

    public class QuotaFieldMeaningHelper : ObservableCollection<EnumHelper<QuotaFieldMeanings>>
    {
        public QuotaFieldMeaningHelper()
        {
            Add(new EnumHelper<QuotaFieldMeanings>(QuotaFieldMeanings.MaxAbsoluteProgress, TranslationContext.Default.Translate("Maximum absolute progress")));
            Add(new EnumHelper<QuotaFieldMeanings>(QuotaFieldMeanings.MaxAbsoluteRemaining, TranslationContext.Default.Translate("Maximum absolute target distance")));
            Add(new EnumHelper<QuotaFieldMeanings>(QuotaFieldMeanings.MaxRelativeProgress, TranslationContext.Default.Translate("Maximum relative progress")));
            Add(new EnumHelper<QuotaFieldMeanings>(QuotaFieldMeanings.MaxRelativeRemaining, TranslationContext.Default.Translate("Maximum relative target distance")));
            Add(new EnumHelper<QuotaFieldMeanings>(QuotaFieldMeanings.MinAbsoluteProgress, TranslationContext.Default.Translate("Minimum absolute progress")));
            Add(new EnumHelper<QuotaFieldMeanings>(QuotaFieldMeanings.MinAbsoluteRemaining, TranslationContext.Default.Translate("Minimum absolute target distance")));
            Add(new EnumHelper<QuotaFieldMeanings>(QuotaFieldMeanings.MinRelativeProgress, TranslationContext.Default.Translate("Minimum relative progress")));
            Add(new EnumHelper<QuotaFieldMeanings>(QuotaFieldMeanings.MinRelativeRemaining, TranslationContext.Default.Translate("Minimum relative target distance")));
        }
    }

    public class SystemFieldMeaningHelper : ObservableCollection<EnumHelper<SystemFieldMeanings>>
    {
        public SystemFieldMeaningHelper()
        {
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.Internal__Id__, TranslationContext.Default.Translate("Internal identifier")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.AppointmentId, TranslationContext.Default.Translate("Appointment identifier")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.AreaId, TranslationContext.Default.Translate("Area identifier")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.CreationTime, TranslationContext.Default.Translate("Creation time")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.CurrentActivity,TranslationContext.Default.Translate("Current activity")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.CustomSortInfo, TranslationContext.Default.Translate("Custom sort information")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.DialedCurrentActivity, TranslationContext.Default.Translate("Attempts in current activity")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.DialEndDate, TranslationContext.Default.Translate("Dialing end date")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.DialingModeOverride, TranslationContext.Default.Translate("Dialing mode override")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.DialStartDate, TranslationContext.Default.Translate("Dialing start date")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.Excluded, TranslationContext.Default.Translate("Excluded")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.ExpectedProfit, TranslationContext.Default.Translate("Expected profit")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.ExportSequence, TranslationContext.Default.Translate("Export sequence")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.ExportTime, TranslationContext.Default.Translate("Export time")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.ImportSequence, TranslationContext.Default.Translate("Import sequence")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.ImportTag, TranslationContext.Default.Translate("Import tag")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastActivityChange, TranslationContext.Default.Translate("Last activity affectation")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastContactId, TranslationContext.Default.Translate("Last contact id")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastDialedDestination, TranslationContext.Default.Translate("Last dialed destination")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastDialStatus, TranslationContext.Default.Translate("Last dialing status")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastDialStatusCount, TranslationContext.Default.Translate("Last dialing status count")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastHandler, TranslationContext.Default.Translate("Last handler")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastHandlerActivity, TranslationContext.Default.Translate("Last handler activity")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastHandlingDuration, TranslationContext.Default.Translate("Last handling duration")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastHandlingTime, TranslationContext.Default.Translate("Last handling time")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastQualification, TranslationContext.Default.Translate("Last qualification")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastQualificationArgued, TranslationContext.Default.Translate("Last qualification's argued attribute")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastQualificationExportable, TranslationContext.Default.Translate("Last qualification's exportable attribute")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastQualificationPositive, TranslationContext.Default.Translate("Last qualification's positive attribute")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.LastRecycle, TranslationContext.Default.Translate("The date and time of the last recycling")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.MaxDialAttempts, TranslationContext.Default.Translate("Max. number of attempts")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.PreviousActivity, TranslationContext.Default.Translate("Previous activity")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.PreviousState, TranslationContext.Default.Translate("Previous state")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.Priority, TranslationContext.Default.Translate("Priority")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.RecycleCount, TranslationContext.Default.Translate("Number of times the record has been recycled")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.SortInfo, TranslationContext.Default.Translate("Sort information")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.State, TranslationContext.Default.Translate("Current state")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.TargetDestination, TranslationContext.Default.Translate("Target destination")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.TargetHandler, TranslationContext.Default.Translate("Target handler")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.PreferredAgent, TranslationContext.Default.Translate("Preferred agent")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.TargetMedia, TranslationContext.Default.Translate("Target media")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.TotalDialed, TranslationContext.Default.Translate("Total attempts")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.TotalHandlers, TranslationContext.Default.Translate("Total number of handlers")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.TotalHandlingDuration, TranslationContext.Default.Translate("Total handling duration")));
            Add(new EnumHelper<SystemFieldMeanings>(SystemFieldMeanings.VMFlagged, TranslationContext.Default.Translate("VM Flagged")));
        }
    }


    public class SalesforceModesHelper : ObservableCollection<EnumHelper<SalesforceCampaignMode>>
    {
        public SalesforceModesHelper()
        {
            Add(new EnumHelper<SalesforceCampaignMode>(SalesforceCampaignMode.CampaignMembers, "Campaign members"));
            Add(new EnumHelper<SalesforceCampaignMode>(SalesforceCampaignMode.Activities, "Tasks"));
        }
    }

    public class DialingModeHelper : ExpressDialingModeHelper
    {
        public static EnumHelper<DialingMode> Search = new EnumHelper<DialingMode>(DialingMode.Search, TranslationContext.Default.Translate("Search"));
        public static EnumHelper<DialingMode> Preview = new EnumHelper<DialingMode>(DialingMode.Preview, TranslationContext.Default.Translate("Preview"));
        public static EnumHelper<DialingMode> CallbacksOnly = new EnumHelper<DialingMode>(DialingMode.CallbacksOnly, TranslationContext.Default.Translate("Only callbacks"));
        public static EnumHelper<DialingMode> Progressive = new EnumHelper<DialingMode>(DialingMode.Progressive, TranslationContext.Default.Translate("Progressive"));
        public static EnumHelper<DialingMode> Power = new EnumHelper<DialingMode>(DialingMode.Power, TranslationContext.Default.Translate("Power"));
        public static EnumHelper<DialingMode> RestrictedPower = new EnumHelper<DialingMode>(DialingMode.RestrictedPower, TranslationContext.Default.Translate("Restricted power"));
        public static EnumHelper<DialingMode> Predictive = new EnumHelper<DialingMode>(DialingMode.Predictive, TranslationContext.Default.Translate("Predictive"));
        public static EnumHelper<DialingMode> Fixed = new EnumHelper<DialingMode>(DialingMode.Fixed, TranslationContext.Default.Translate("Unattended"));

        public static EnumHelper<int> intNone = new EnumHelper<int>(0, TranslationContext.Default.Translate("None"));
        public static EnumHelper<int> intPreview = new EnumHelper<int>((int)DialingMode.Preview, TranslationContext.Default.Translate("Preview"));
        public static EnumHelper<int> intProgressive = new EnumHelper<int>((int)DialingMode.Progressive, TranslationContext.Default.Translate("Progressive"));

        public DialingModeHelper(): base()
        {
            Insert(2, CallbacksOnly);
            Add(Fixed);
        }
    }

    public class RestrictedDialingModeHelper :ExpressRestrictedDialingModeHelper
    {
        public RestrictedDialingModeHelper(): base()
        {
            Insert(1, DialingModeHelper.CallbacksOnly);
            Add(DialingModeHelper.Fixed);
        }
    }

    public class ExpressDialingModeHelper : ObservableCollection<EnumHelper<DialingMode>>
    {
        public ExpressDialingModeHelper()
        {
            Add(DialingModeHelper.Search);
            Add(DialingModeHelper.Preview);
            Add(DialingModeHelper.Progressive);
            Add(DialingModeHelper.Power);
            Add(DialingModeHelper.RestrictedPower);
            Add(DialingModeHelper.Predictive);
        }
    }

    public class ExpressRestrictedDialingModeHelper : ObservableCollection<EnumHelper<DialingMode>>
    {
        public ExpressRestrictedDialingModeHelper()
        {
            Add(DialingModeHelper.Preview);
            Add(DialingModeHelper.Progressive);
            Add(DialingModeHelper.Power);
            Add(DialingModeHelper.RestrictedPower);
            Add(DialingModeHelper.Predictive);
        }
    }

    public class ProgressiveOrPreviewDialingModeHelper: ObservableCollection<EnumHelper<int>>
    {
        public ProgressiveOrPreviewDialingModeHelper()
        {
            Add(DialingModeHelper.intNone);
            Add(DialingModeHelper.intProgressive);
            Add(DialingModeHelper.intPreview);
        }
    }


    public class OverflowActionsHelper : ObservableCollection<EnumHelper<OverflowActions>>
    {
        public OverflowActionsHelper()
        {
            Add(new EnumHelper<OverflowActions>(OverflowActions.None, TranslationContext.Default.Translate("None")));
            Add(new EnumHelper<OverflowActions>(OverflowActions.Disconnect, TranslationContext.Default.Translate("Disconnect")));
            Add(new EnumHelper<OverflowActions>(OverflowActions.Message, TranslationContext.Default.Translate("Play prompt")));
            Add(new EnumHelper<OverflowActions>(OverflowActions.IVR, TranslationContext.Default.Translate("Execute IVR")));
            Add(new EnumHelper<OverflowActions>(OverflowActions.Reroute, TranslationContext.Default.Translate("Reroute the call")));
        }
    }

    public class OutboundClosingActionsHelper : ObservableCollection<EnumHelper<OutboundClosingAction>>
    {
        public OutboundClosingActionsHelper()
        {
            Add(new EnumHelper<OutboundClosingAction>(OutboundClosingAction.None, TranslationContext.Default.Translate("None")));
            Add(new EnumHelper<OutboundClosingAction>(OutboundClosingAction.PauseActivity, TranslationContext.Default.Translate("Pause activity")));
            Add(new EnumHelper<OutboundClosingAction>(OutboundClosingAction.ChangeDialingMode, TranslationContext.Default.Translate("Change dialing mode")));
        }
    }

    
    public class OverflowConditionsHelper : ObservableCollection<EnumHelper<OverflowConditions>>
    {
        public OverflowConditionsHelper()
        {
            Add(new EnumHelper<OverflowConditions>(OverflowConditions.None, TranslationContext.Default.Translate("Never")));
            Add(new EnumHelper<OverflowConditions>(OverflowConditions.Always, TranslationContext.Default.Translate("Always")));
            Add(new EnumHelper<OverflowConditions>(OverflowConditions.NoAgentLoggedIn, TranslationContext.Default.Translate("No agent logged in")));
            Add(new EnumHelper<OverflowConditions>(OverflowConditions.ItemsInQueueThreshold, TranslationContext.Default.Translate("Number of items in queue bigger than")));
            Add(new EnumHelper<OverflowConditions>(OverflowConditions.RatioInQueueAgents, TranslationContext.Default.Translate("Ratio \"items waiting / working agents\" bigger than")));
            Add(new EnumHelper<OverflowConditions>(OverflowConditions.MaxWait, TranslationContext.Default.Translate("Waiting time bigger than")));
            Add(new EnumHelper<OverflowConditions>(OverflowConditions.MaxEWT, TranslationContext.Default.Translate("EWT bigger than")));
            Add(new EnumHelper<OverflowConditions>(OverflowConditions.AgentsReadySmallerThan, TranslationContext.Default.Translate("Number of agents ready smaller than")));
        }
    }

    public class NumberingCallTypesHelper : ObservableCollection<EnumHelper<NumberingCallType>>
    {
        public NumberingCallTypesHelper()
        {
            Add(new EnumHelper<NumberingCallType>(NumberingCallType.InboundGeneral, TranslationContext.Default.Translate("Inbound calls")));
            Add(new EnumHelper<NumberingCallType>(NumberingCallType.OutboundGeneral, TranslationContext.Default.Translate("Manual outbound calls")));
            Add(new EnumHelper<NumberingCallType>(NumberingCallType.OutboundActivity, TranslationContext.Default.Translate("Automated outbound calls")));
        }
    }

    public class TypeHelper
    {
        public static T GetAttribute<T>(MemberInfo info, bool inherit)
        {
            object[] Attrs = info.GetCustomAttributes(typeof(T), inherit);

            if (Attrs.Length > 0)
                return (T)Attrs[0];

            return default(T);
        }

        public static T GetAttributeOrDefault<T>(MemberInfo info, bool inherit)
        {
            object[] Attrs = info.GetCustomAttributes(typeof(T), inherit);

            if (Attrs.Length > 0)
                return (T)Attrs[0];

            return (T)typeof(T).InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[0]);
        }
    }

    public class Cache
    {
        private Dictionary<string, Tuple<string, DateTime>> m_CacheEntries = new Dictionary<string, Tuple<string, DateTime>>();

        public TimeSpan DefaultValidity { get; set; }

        public delegate string RetrieveCallbackDelegate(string key);

        public RetrieveCallbackDelegate DefaultRetrieveCallback { get; set; }

        public string GetEntry(string key)
        {
            return GetEntry(key, DefaultRetrieveCallback, DefaultValidity);
        }

        public string GetEntry(string key, TimeSpan validity)
        {
            return GetEntry(key, DefaultRetrieveCallback, validity);
        }

        public string GetEntry(string key, RetrieveCallbackDelegate retrieveCallback)
        {
            return GetEntry(key, retrieveCallback, DefaultValidity);
        }

        public string GetEntry(string key, RetrieveCallbackDelegate retrieveCallback,  TimeSpan validity)
        {
            if (retrieveCallback == null)
                return null;

            if (m_CacheEntries.ContainsKey(key))
            {
                Tuple<string, DateTime> t = m_CacheEntries[key];
                if (t == null)
                {
                    // better remove the entry
                    m_CacheEntries.Remove(key);

                    string strVal = retrieveCallback(key);
                    m_CacheEntries.Add(key, new Tuple<string, DateTime>(strVal, DateTime.Now + validity));
                    return strVal;
                }
                else
                {
                    if (t.Item2 < DateTime.Now)
                    {
                        // validity ellapsed...
                        // Must refresh
                        string strVal = retrieveCallback(key);
                        m_CacheEntries[key] = new Tuple<string, DateTime>(strVal, DateTime.Now + validity);
                        return strVal;
                    }
                    else
                    {
                        return t.Item1;
                    }

                }

            }
            else
            {
                string strVal = retrieveCallback(key);
                m_CacheEntries.Add(key, new Tuple<string, DateTime>(strVal, DateTime.Now + validity));
                return strVal;
            }
        }
    }

    public class Information : AdminObject
    {
        private bool m_AutoRecord;
        private static TranslationContext m_TranslationContext = new TranslationContext("General");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        public Information(AdminCore core)
            : base(core)
        {
            Init();
        }

        public Information(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
        {
            Id = "globalinfo";
        }

        public DateTime Validity
        {
            get
            {
                object obj = AppDomain.CurrentDomain.GetData("service_validity");
                if (obj != null)
                {
                    return DateTime.Parse((string)obj);
                }
                return DateTime.MaxValue;
            }
        }

        public bool AutomaticRecording
        {
            get
            {
                return m_AutoRecord;
            }
            set
            {
                m_AutoRecord = value;
            }
        }

        public string ValidityString
        {
            get
            {
                return Validity.ToString("d");
            }
        }

        public string ServerEditionString
        {
            get
            {
                object obj = AppDomain.CurrentDomain.GetData("service_demo");
                if (obj != null && obj is bool && (bool)obj)
                {
                    return string.Format("DEMO {0} {1}", AppDomain.CurrentDomain.GetData("service_product"), AppDomain.CurrentDomain.GetData("service_mode"));
                }
                else
                {
                    return string.Format("{0} {1}", AppDomain.CurrentDomain.GetData("service_product"), AppDomain.CurrentDomain.GetData("service_mode"));
                }
            }
        }



        private string LicenseString
        {
            get
            {
                object obj = AppDomain.CurrentDomain.GetData("service_license");
                return obj as string;
            }
        }
        public bool LicenseInfoAvailable
        {
            get
            {
                return string.IsNullOrEmpty(LicenseString);
            }
        }
        public bool LicenseInfoAgentsAvailable
        {
            get
            {
                int temp = -1;
                if (int.TryParse(Agents, out temp))
                    return temp >= 0;
                return false;
            }
        }
        public string Agents
        {
            get
            {
                if (string.IsNullOrEmpty(LicenseString))
                    return "-1";
                try
                {
                    return LicenseString.Split(' ')[0];
                }
                catch
                {
                    return "-1";
                }
            }
        }
        public bool LicenseInfoInboundAvailable
        {
            get
            {
                int temp = -1;
                if (int.TryParse(Inbound, out temp))
                    return temp >= 0;
                return false;
            }
        }

        public string Inbound
        {
            get
            {
                if (string.IsNullOrEmpty(LicenseString))
                    return "-1";

                try
                {
                    return LicenseString.Split(' ')[2];
                }
                catch
                {
                    return "-1";
                }
            }
        }
        public bool LicenseInfoOutboundAvailable
        {
            get
            {
                int temp = -1;
                if (int.TryParse(Outbound, out temp))
                    return temp >= 0;
                return false;
            }
        }

        public string Outbound
        {
            get
            {
                if (string.IsNullOrEmpty(LicenseString))
                    return "-1";

                try
                {
                    return LicenseString.Split(' ')[3];
                }
                catch
                {
                    return "-1";
                }
            }
        }
        public bool LicenseInfoPredictiveAvailable
        {
            get
            {
                int temp = -1;
                if (int.TryParse(Predictive, out temp))
                    return temp >= 0;
                return false;
            }
        }

        public string Predictive
        {
            get
            {
                if (string.IsNullOrEmpty(LicenseString))
                    return "-1";

                try
                {
                    return LicenseString.Split(' ')[4];
                }
                catch
                {
                    return "-1";
                }
            }
        }
        public bool LicenseInfoMailAvailable
        {
            get
            {
                int temp = -1;
                if (int.TryParse(Mail, out temp))
                    return temp >= 0;
                return false;
            }
        }

        public string Mail
        {
            get
            {
                if (string.IsNullOrEmpty(LicenseString))
                    return "-1";

                try
                {
                    return LicenseString.Split(' ')[5];
                }
                catch
                {
                    return "-1";
                }
            }
        }
        public bool LicenseInfoChatAvailable
        {
            get
            {
                int temp = -1;
                if (int.TryParse(Chat, out temp))
                    return temp >= 0;
                return false;
            }
        }

        public string Chat
        {
            get
            {
                if (string.IsNullOrEmpty(LicenseString))
                    return "-1";

                try
                {
                    return LicenseString.Split(' ')[6];
                }
                catch
                {
                    return "-1";
                }
            }
        }
        public bool LicenseInfoSupervisorsAvailable
        {
            get
            {
                int temp = -1;
                if (int.TryParse(Supervisors, out temp))
                    return temp >= 0;
                return false;
            }
        }

        public string Supervisors
        {
            get
            {
                if (string.IsNullOrEmpty(LicenseString))
                    return "-1";

                try
                {
                    return LicenseString.Split(' ')[1];
                }
                catch
                {
                    return "-1";
                }
            }
        }


        public string Description
        {
            get
            {
                return Translate("Informations");
            }
        }

    }


    public static class DurationHelpers
    {

        public static string GetDefaultDurationString(Decimal duration, bool forceSign)
        {
            try
            {
                string symbol = string.Empty;
                if (forceSign)
                    symbol = "+";


                if (duration < 0)
                    symbol = "-";

                duration = Math.Abs(duration);
                long value = (long)(Decimal.Floor(duration));
                decimal milisec = duration - (Decimal)value;
                long days = 0;
                int hours = 0;
                int minutes = 0;
                int seconds = 0;


                days = value / (24 * 60 * 60);
                value = value - days * (24 * 60 * 60);
                hours = (int)(value / (60 * 60));
                value = value - hours * (60 * 60);
                minutes = (int)(value / 60);
                value = value - minutes * 60;
                seconds = (int)value;


                if (days > 0)
                {
                    if (hours > 0)
                    {
                        if (minutes > 0)
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{8}{0} {1} {2} {3} {4} {5} {6} {7}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"), symbol);
                            }
                            else
                            {
                                return string.Format("{6}{0} {1} {2} {3} {4} {5}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"), symbol);
                            }
                        }
                        else
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{8}{0} {1} {2} {3} {4} {5} {6} {7}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"), symbol);
                            }
                            else
                            {
                                return string.Format("{4}{0} {1} {2} {3}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"), symbol
                                    );
                            }
                        }
                    }
                    else
                    {
                        if (minutes > 0)
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{8}{0} {1} {2} {3} {4} {5} {6} {7}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"), symbol);
                            }
                            else
                            {
                                return string.Format("{6}{0} {1} {2} {3} {4} {5}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"), symbol);
                            }
                        }
                        else
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{8}{0} {1} {2} {3} {4} {5} {6} {7}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"), symbol);
                            }
                            else
                            {
                                return string.Format("{2}{0} {1}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"), symbol
                                    );
                            }
                        }
                    }
                }
                else
                {
                    if (hours > 0)
                    {
                        if (minutes > 0)
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{6}{0} {1} {2} {3} {4} {5}",
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"), symbol);
                            }
                            else
                            {
                                return string.Format("{4}{0} {1} {2} {3}",
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("minutes") : TranslationContext.Default.Translate("minute"), symbol);
                            }
                        }
                        else
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{6}{0} {1} {2} {3} {4} {5}",
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"), symbol);
                            }
                            else
                            {
                                return string.Format("{2}{0} {1}",
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"), symbol
                                    );
                            }
                        }
                    }
                    else
                    {
                        if (minutes > 0)
                        {
                            if (seconds > 0)
                            {
                                if (milisec == 0)
                                {
                                    return string.Format("{4}{0} {1} {2} {3}",
                                        minutes, minutes > 1 ? TranslationContext.Default.Translate("minutes") : TranslationContext.Default.Translate("minute"),
                                        seconds, seconds > 1 ? TranslationContext.Default.Translate("seconds") : TranslationContext.Default.Translate("second"), symbol);
                                }
                                else
                                {
                                    return string.Format("{4}{0} {1} {2:0.000} {3}",
                                        minutes, minutes > 1 ? TranslationContext.Default.Translate("minutes") : TranslationContext.Default.Translate("minute"),
                                        seconds + milisec, seconds > 1 ? TranslationContext.Default.Translate("seconds") : TranslationContext.Default.Translate("second"), symbol);
                                }

                            }
                            else
                            {
                                if (milisec == 0)
                                {
                                    return string.Format("{2}{0} {1}",
                                        minutes, minutes > 1 ? TranslationContext.Default.Translate("minutes") : TranslationContext.Default.Translate("minute"), symbol);
                                }
                                else
                                {
                                    return string.Format("{4}{0} {1} {2:0.000} {3}",
                                        minutes, minutes > 1 ? TranslationContext.Default.Translate("minutes") : TranslationContext.Default.Translate("minute"), milisec, TranslationContext.Default.Translate("second"), symbol);
                                }
                            }
                        }
                        else
                        {
                            if (seconds > 0)
                            {
                                if (milisec == 0)
                                {
                                    return string.Format("{2}{0} {1}",
                                        seconds, seconds > 1 ? TranslationContext.Default.Translate("seconds") : TranslationContext.Default.Translate("second"), symbol);
                                }
                                else
                                {
                                    return string.Format("{2}{0:0.000} {1}",
                                        seconds + milisec, seconds > 1 ? TranslationContext.Default.Translate("seconds") : TranslationContext.Default.Translate("second"), symbol);
                                }
                            }
                            else
                            {
                                if (milisec == 0)
                                {
                                    return string.Format("{2}{0} {1}",
                                        0, TranslationContext.Default.Translate("second"), symbol
                                        );
                                }
                                else
                                {
                                    return string.Format("{2}{0:0.000} {1}",
                                        milisec, TranslationContext.Default.Translate("second"), symbol
                                        );

                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static Decimal SplitDurationString(string strWorking)
        {
            Decimal nDays = 0;
            Decimal nHours = 0;
            Decimal nMinutes = 0;
            Decimal nSeconds = 0;

            string strNumeric = "0123456789,.";
            strWorking = strWorking.ToLower().Replace(" ", string.Empty);
            string strNumber = string.Empty;
            string strUnit = string.Empty;
            bool inUnit = false;
            bool isNegative = false;

            if (strWorking.StartsWith("-"))
            {
                isNegative = true;
                strWorking = strWorking.Substring(1);
            }
            else if (strWorking.StartsWith("+"))
            {
                strWorking = strWorking.Substring(1);
            }


            for (int i = 0; i < strWorking.Length; i++)
            {
                string curChar = strWorking.Substring(i, 1);

                if (inUnit)
                {
                    if (strNumeric.Contains(curChar))
                    {
                        // check what is the unit to affect right value;
                        int counter = 0;
                        bool unitMatch = false;
                        while (true)
                        {
                            if (counter < strDays.Length && strDays[counter].Equals(strUnit))
                            {
                                if (!Decimal.TryParse(strNumber, out nDays))
                                    return -1;
                                unitMatch = true;
                                break;
                            }
                            else if (counter < strHours.Length && strHours[counter].Equals(strUnit))
                            {
                                if (!Decimal.TryParse(strNumber, out nHours))
                                    return -1;
                                unitMatch = true;
                                break;
                            }
                            else if (counter < strMinutes.Length && strMinutes[counter].Equals(strUnit))
                            {
                                if (!Decimal.TryParse(strNumber, out nMinutes))
                                    return -1;
                                unitMatch = true;
                                break;
                            }
                            else if (counter < strSeconds.Length && strSeconds[counter].Equals(strUnit))
                            {
                                if (!Decimal.TryParse(strNumber, out nSeconds))
                                    return -1;
                                unitMatch = true;
                                break;
                            }

                            counter++;
                            if (counter >= strDays.Length && counter >= strHours.Length && counter >= strMinutes.Length && counter >= strSeconds.Length)
                                break;
                        }

                        if (!unitMatch)
                            return -1;

                        inUnit = false;
                        strNumber = curChar;
                    }
                    else
                    {
                        strUnit = string.Concat(strUnit, curChar);
                    }

                }
                else
                {
                    if (strNumeric.Contains(curChar))
                    {
                        strNumber = string.Concat(strNumber, curChar);
                    }
                    else
                    {
                        inUnit = true;
                        strUnit = curChar;
                    }
                }
            }

            if (inUnit)
            {
                int counter = 0;
                bool unitMatch = false;
                while (true)
                {
                    if (counter < strDays.Length && strDays[counter].Equals(strUnit))
                    {
                        if (!Decimal.TryParse(strNumber, out nDays))
                            return -1;
                        unitMatch = true;
                        break;

                    }
                    else if (counter < strHours.Length && strHours[counter].Equals(strUnit))
                    {
                        if (!Decimal.TryParse(strNumber, out nHours))
                            return -1;
                        unitMatch = true;
                        break;

                    }
                    else if (counter < strMinutes.Length && strMinutes[counter].Equals(strUnit))
                    {
                        if (!Decimal.TryParse(strNumber, out nMinutes))
                            return -1;
                        unitMatch = true;
                        break;

                    }
                    else if (counter < strSeconds.Length && strSeconds[counter].Equals(strUnit))
                    {
                        if (!Decimal.TryParse(strNumber, out nSeconds))
                            return -1;
                        unitMatch = true;
                        break;

                    }

                    counter++;
                    if (counter >= strDays.Length && counter >= strHours.Length && counter >= strMinutes.Length && counter >= strSeconds.Length)
                        break;
                }
                if (!unitMatch)
                    return -1;

            }
            else
            {
                // let's imagine it is about seconds?
                if (!Decimal.TryParse(strNumber, out nSeconds))
                    return -1;
            }
            return (nDays * 60 * 60 * 24 + nHours * 60 * 60 + nMinutes * 60 + nSeconds) * (isNegative ? -1 : 1);
        }

        private static string[] strDays = TranslationContext.Default.Translate("days;day;d").Split(';');
        private static string[] strHours = TranslationContext.Default.Translate("hours;hour;h").Split(';');
        private static string[] strMinutes = TranslationContext.Default.Translate("minutes;minute;min;m").Split(';');
        private static string[] strSeconds = TranslationContext.Default.Translate("seconds;second;sec;s").Split(';');
    }

    public delegate void progressReportDelegate(int percent, string message, string progressMessage);

}
