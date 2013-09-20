using SBXA.Runtime;
using SBXA.Shared;
using SBXA.UI.Client;
using SBXA.UI.WPFControls;
using SBXAThemeSupport.ViewModels;

namespace SBXAThemeSupport.DebugAssistant.ViewModels
{
#pragma warning disable 1570
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// *
    /// * This process will make life easier and more consistent to
    /// * debug process
    /// *
    /// * P.DEBUG,[message]
    /// *
    /// LOCAL PNAME, MESSAGE, ORTN.FLAG, OREFRESH
    /// LOCAL ASSEMBLY_NAME, TYPE, METHOD_NAME, PARAMETERS, RETVAL, STATUS, STATUSDESC
    /// *
    /// IF NOT(IS.DEV) THEN EXIT
    /// ORTN.FLAG = @RTN.FLAG
    /// OREFRESH=@REFRESH
    /// PNAME = @PROC.NAME<2>
    /// MESSAGE = @PARAM"0,1"
    /// PARAMETERS = "[":PNAME:"] ":MESSAGE
    /// *
    /// IF @XUI THEN
    /// ASSEMBLY_NAME = "CustomSBPlusTheme"
    ///   TYPE = "CustomSBPlusTheme.DebugAssistant.ViewModels.DebugViewModel"
    ///   METHOD_NAME = "LogInformation"
    ///   RETVAL = ""
    ///   STATUS = ""
    ///   STATUSDESC = ""
    ///   CALL SB.CALL.STATIC.METHOD("", ASSEMBLY_NAME, TYPE, METHOD_NAME, PARAMETERS, RETVAL, STATUS, STATUSDESC)
    /// *  DISP 4, PARAMETERS
    /// *
    /// END ELSE
    ///   DISP 4, PARAMETERS
    /// END
    /// @RTN.FLAG = ORTN.FLAG
    /// @REFRESH=OREFRESH
    /// </example>
#pragma warning restore 1570
    public class DebugViewModel : ViewModel
    {
        public static void LogInformation(string message)
        {
            EnableInformationLog(true);
            SBPlusClient.LogInformation(message);
        }

        public static void EnableInformationLog(bool enable)
        {
            SBPlus.Current.ApplicationDefinition.LogCustomInfo = enable;
            if (enable)
            {
                SessionManager.SessionsLog.EnableLogLevel(Log.MT_CUSTOM_INFO);
                SessionManager.SessionsLog.EnableLogLevel(Log.LL_CUSTOM);
            }
            else
            {
                SessionManager.SessionsLog.DisableLogLevel(Log.MT_CUSTOM_INFO);
            }
        }
        
        public static void EnableWarningLog(bool enable)
        {
            SBPlus.Current.ApplicationDefinition.LogCustomInfo = enable;
            if (enable)
            {
                SessionManager.SessionsLog.EnableLogLevel(Log.MT_CUSTOM_WARNING);
                SessionManager.SessionsLog.EnableLogLevel(Log.LL_CUSTOM);
            }
            else
            {
                SessionManager.SessionsLog.DisableLogLevel(Log.MT_CUSTOM_WARNING);
            }
        }

        public static void EnableErrorLog(bool enable)
        {
            SBPlus.Current.ApplicationDefinition.LogCustomError = enable;
            if (enable)
            {
                SessionManager.SessionsLog.EnableLogLevel(Log.MT_CUSTOM_ERROR);
                SessionManager.SessionsLog.EnableLogLevel(Log.LL_CUSTOM);
            }
            else
            {
                SessionManager.SessionsLog.DisableLogLevel(Log.MT_CUSTOM_ERROR);
            }
        }

        public static void EnableLogging(bool enable)
        {
            if (enable)
            {
                SBPlusClient.EnableCustomLogging();
            }
            else
            {
                SBPlusClient.DisableCustomLogging();
            }
        }
    }
}
