// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SbProcessHandler.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="SbProcessHandler.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// #define SHOW_DEBUG
namespace SBXAThemeSupport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.Shared.Definitions;
    using SBXA.UI.Client;
    using SBXA.UI.WPFControls;


    /// <summary>
    ///     The sb process handler.
    /// </summary>
    public static class SbProcessHandler
    {
        #region Static Fields

        private static readonly List<CallbackRuntime<string, string>> CallbacksRtnFlag = new List<CallbackRuntime<string, string>>();

        private static readonly List<CallbackRuntime<bool, string>> CallbacksSubroutine = new List<CallbackRuntime<bool, string>>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The call process.
        /// </summary>
        /// <param name="processName">
        /// The process name.
        /// </param>
        /// <param name="isInContext">
        /// The is in context.
        /// </param>
        public static void CallProcess(string processName, bool isInContext)
        {
            CallProcess(processName, isInContext, string.Empty);
        }

        /// <summary>
        /// The call process.
        /// </summary>
        /// <param name="processName">
        /// The process name.
        /// </param>
        /// <param name="isInContext">
        /// The is in context.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public static void CallProcess(string processName, bool isInContext, string parameter, string name = null)
        {
            string[] parameters = string.IsNullOrEmpty(parameter) ? null : new[] { parameter };
            CallProcess(processName, isInContext, parameters, name);
        }

        /// <summary>
        /// The call process.
        /// </summary>
        /// <param name="processName">
        /// The process name.
        /// </param>
        /// <param name="isInContext">
        /// The is in context.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public static void CallProcess(string processName, bool isInContext, string[] parameter, string name = null)
        {
            string param = string.Empty;
            if (parameter != null)
            {
                param = string.Join(",", parameter);
            }

            CallProcess(processName, isInContext, new SBString(param), name);
        }

        /// <summary>
        /// The call process rtn flag.
        /// </summary>
        /// <param name="processName">
        /// The process name.
        /// </param>
        /// <param name="callbackMethod">
        /// The callback method.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// processName
        /// </exception>
        public static void CallProcessRtnFlag(string processName, Action<string, string> callbackMethod, params string[] parameter)
        {
            if (string.IsNullOrEmpty(processName))
            {
                throw new ArgumentNullException("processName");
            }

            string param = string.Join(",", parameter);
            if (!string.IsNullOrEmpty(param))
            {
                if (param.StartsWith(","))
                {
                    param = param.Substring(1, param.Length);
                }
            }

            processName = processName + param;

            var callbackRuntime = new CallbackRuntime<string, string>();
            callbackRuntime.Callback = callbackMethod;

            // add guid to the processname
            processName = string.Format("{0}{1}{2}", callbackRuntime.Identification, Delimiters.FileDelimitter, processName);

            // execute subroutine
            CallProcess("P.YY.SBXA.EXECUTE", false, processName.ToSbParam());

            CallbacksRtnFlag.Add(callbackRuntime);
        }

        /// <summary>
        /// This method will execute a subroutine on the server synchronously. If the server is busy, then based on the ignore
        ///     if busy flag will either ignore the call or throw an exception.
        /// </summary>
        /// <param name="subroutineName">
        /// The name of the subroutine
        /// </param>
        /// <param name="parCount">
        /// The numbr of parameters
        /// </param>
        /// <param name="parameter">
        /// The actual parameters being passed to the subroutine
        /// </param>
        /// <param name="commandCouldCauseUiAction">
        /// If this is true it means that the basic subroutine will not do anything to cause the server to make a call to the
        ///     client and therefore we do not have to worry about checking if the UI is busy.
        /// </param>
        /// <returns>
        /// The values that were passed back from the subroutine.
        /// </returns>
        public static SBString[] CallSubroutine(
            string subroutineName, 
            int parCount, 
            SBString[] parameter, 
            bool commandCouldCauseUiAction = false)
        {
            if (subroutineName == null)
            {
                throw new ArgumentNullException("subroutineName");
            }

            if (parameter.Length < parCount)
            {
                Array.Resize(ref parameter, parCount);
            }
#if SHOW_DEBUG
            CustomLogger.LogDebug(
                () => string.Format("Die Subroutine : {0} wird aufgerufen.", string.Format("{0},{1}", subroutineName, parameter)));
            CustomLogger.LogDebug(
                () =>
                string.Format(
                    "IsServerReady {0} commandCouldCauseUiAction {1}", 
                    ApplicationHelper.CanSendServerCommands(false), 
                    commandCouldCauseUiAction));
#endif
            SBString[] retunSbStrings = null;
            if (Application.Current.Dispatcher.CheckAccess())
            {
                retunSbStrings = ExecuteSubroutine(subroutineName, parameter, commandCouldCauseUiAction: commandCouldCauseUiAction);
            }
            else
            {
                object myReturnObject = null;
                JobManager.RunSyncInUIThread(
                    DispatcherPriority.Normal, 
                    () =>
                    myReturnObject =
                    ExecuteSubroutineReturningException(subroutineName, parameter, commandCouldCauseUiAction: commandCouldCauseUiAction));
                var possibleException = myReturnObject as Exception;
                if (possibleException != null)
                {
                    throw possibleException;
                }

                retunSbStrings = (SBString[])myReturnObject;
            }

            return retunSbStrings;
        }

        /// <summary>
        /// The call subroutine.
        /// </summary>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <param name="subroutineName">
        /// The subroutine name.
        /// </param>
        /// <param name="parCount">
        /// The par count.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        public static void CallSubroutine(Action<bool, SBString[]> callback, string subroutineName, int parCount, SBString[] parameter)
        {
            if (parCount > 0 && parameter != null && parameter.Length != parCount)
            {
                Array.Resize(ref parameter, parCount);
            }

            var sbString = parameter ?? new SBString[parCount];
            SbProcessRunner.Instance.ExecuteMethod(() => SubroutineCallback(subroutineName, sbString, callback));
        }

        /// <summary>
        /// The call subroutine.
        /// </summary>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <param name="subroutineName">
        /// The subroutine name.
        /// </param>
        /// <param name="parCount">
        /// The par count.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        public static void CallSubroutine(Action<bool, SBString[]> callback, string subroutineName, int parCount, params string[] parameter)
        {
            var sbString = PrepareSubroutineParam(parCount, parameter);
            SbProcessRunner.Instance.ExecuteMethod(() => SubroutineCallback(subroutineName, sbString, callback));
        }

        /// <summary>
        /// Calls the subroutine.
        /// </summary>
        /// <param name="subroutineCallCompleted">
        /// The subroutine call completed.
        /// </param>
        /// <param name="subroutineName">
        /// Name of the subroutine.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="callbackRuntime">
        /// The callback runtime.
        /// </param>
        /// <param name="onlyServerSide">
        /// if set to <c>true</c> [only server side].
        /// </param>
        public static void CallSubroutine(
            SubroutineCallCompleted subroutineCallCompleted, 
            string subroutineName, 
            SBString[] parameters, 
            object callbackRuntime, 
            bool onlyServerSide = false)
        {
            SbProcessRunner.Instance.ExecuteMethod(
                () => SubroutineCallback(subroutineName, parameters, callbackRuntime, subroutineCallCompleted));
        }

        /// <summary>
        /// The basic routine makes no callback to the client
        /// </summary>
        /// <param name="subroutineName">
        /// Name of the subroutine.
        /// </param>
        /// <param name="parCount">
        /// The par count.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="SBString[]"/>.
        /// </returns>
        public static SBString[] CallSubroutineBasicOnly(string subroutineName, int parCount, SBString[] parameter)
        {
            return CallSubroutine(subroutineName, parCount, parameter, commandCouldCauseUiAction: false);
        }

        /// <summary>
        /// The Basic routine makes no callbacks to the client
        /// </summary>
        /// <param name="subroutineName">
        /// Name of the subroutine
        /// </param>
        /// <param name="parCount">
        /// Parameter count
        /// </param>
        /// <param name="parameter">
        /// The parameter
        /// </param>
        /// <returns>
        /// The <see cref="SBString[]"/>.
        /// </returns>
        public static SBString[] CallSubroutineBasicOnly(string subroutineName, int parCount, params string[] parameter)
        {
            var sbString = PrepareSubroutineParam(parCount, parameter);
            return CallSubroutine(subroutineName, parCount, sbString, commandCouldCauseUiAction: false);
        }

        /// <summary>
        /// The callback from sbxa.
        /// </summary>
        /// <param name="callbackdata">
        /// The callbackdata.
        /// </param>
        /// <param name="otherData">
        /// The other data.
        /// </param>
        public static void CallbackFromSbxa(string callbackdata, string otherData)
        {
            if (string.IsNullOrEmpty(callbackdata))
            {
                callbackdata = string.Empty;
            }

            var splitData = callbackdata.Split(Delimiters.FileDelimitter);
            if (splitData.Length < 3)
            {
                Array.Resize(ref splitData, 3);
            }

            var guid = splitData[0];
            var rtnFlag = splitData[1];
            var callbackRuntime = CallbacksRtnFlag.Where(c => c.Identification.ToString() == guid).SingleOrDefault();
            if (callbackRuntime != null)
            {
                callbackRuntime.Callback(rtnFlag, otherData);
                CallbacksRtnFlag.Remove(callbackRuntime);
            }
        }

        /// <summary>
        /// The logto.
        /// </summary>
        /// <param name="accountName">
        /// The account name.
        /// </param>
        public static void Logto(string accountName)
        {
            if (string.IsNullOrEmpty(accountName))
            {
                return;
            }

            CallProcess("P.YY.LOGTO", false, accountName);
        }

        #endregion

        #region Methods

        private static void CallProcess(string processName, bool isInContext, SBString parameter, string name = null)
        {
            string param = parameter.GetRawString();

#if SHOW_DEBUG
            CustomLogger.LogDebug(
                () => string.Format("Check Application.IsServerReady: {0}", ApplicationHelper.CanSendServerCommands(false)));
#endif
            string procIncludingParam = processName;
            if (!string.IsNullOrEmpty(param))
            {
                procIncludingParam = string.Format("{0},{1}", processName, param);
            }

            // each call of process will confirm a DISP message unexpected, so it canCauseUnexpectedResponsesToServer 
            SbProcessRunner.Instance.ExecuteMethod(
                () => CallProcessInternal(procIncludingParam, ApplicationHelper.CurrentFormSbHandle, SBPlus.Current, isInContext), 
                canCauseUnexpectedResponsesToServer: true, 
                name: name);
        }

        private static void CallProcessInternal(object parameter, string currentFormHandle, IInputElement target, bool isInContext)
        {
            if (parameter == null)
            {
                CustomLogger.LogWarning(() => "parameter is null. This should never be the case");
                return;
            }

            string myLogParameter = parameter.ToString();

            // if the current form is no longer the current from so ignore the call
            if (isInContext && ApplicationHelper.CurrentFormSbHandle != currentFormHandle)
            {
                CustomLogger.LogWarning(
                    () => string.Format("Ignore the call before the form is no longer the current form. Parameter: {0}", myLogParameter));
                CustomLogger.LogWarning(() => "Ignore the call before the form is no longer the current form.");
                return;
            }

            // change the incontext);
            if (isInContext && SBPlus.Current != null && SBPlus.Current.CurrentForm != null)
            {
                GuiObjectDefinition guiObjectDefinition;
                if (SBPlus.Current.CurrentForm is SBMultiForm)
                {
                    guiObjectDefinition = ((SBMultiForm)SBPlus.Current.CurrentForm).CurrentForm.GuiObjectDefinition;
                }
                else
                {
                    guiObjectDefinition = SBPlus.Current.CurrentForm.GuiObjectDefinition;
                }

                var formObjectDefinition = guiObjectDefinition as FormObjectDefinition;
                isInContext = formObjectDefinition != null && formObjectDefinition.ProcessType == ProcessTypes.I;
            }

#if SHOW_DEBUG
            CustomLogger.LogDebug(() => string.Format("CallProcessInternal isInContext: {0} Parameter: {1}", isInContext, myLogParameter));

            CustomLogger.LogDebug(() => string.Format("Der Process : {0} wird aufgerufen.", myLogParameter));
#endif
            if (isInContext)
            {
                SBCommands.ExecuteSBPlusProcessInContextCommand.Execute(parameter, target);
            }
            else
            {
                SBCommands.ExecuteSBPlusProcessCommand.Execute(parameter, target);
            }

#if SHOW_DEBUG
            CustomLogger.LogDebug(() => string.Format("Der Process : {0} wurde aufgerufen.", myLogParameter));
#endif
        }

        private static void ExecuteSubroutine(
            string subroutineName, 
            SBString[] parameters, 
            object userState, 
            SubroutineCallCompleted subroutineCallCompleted)
        {
            SBPlusClient.Current.ExecuteSubroutine(subroutineName, parameters, userState, subroutineCallCompleted);
        }

        private static SBString[] ExecuteSubroutine(string subroutineName, SBString[] arguments, bool commandCouldCauseUiAction = false)
        {
            SBString[] retunSbStrings = null;
            var notReadyException =
                new ServerNotReadyException(
                    string.Format("The server is not able to accept requests at this time. Subroutine name is {0}", subroutineName));

#if SHOW_DEBUG
            CustomLogger.LogDebug(
                () =>
                string.Format(
                    "IsServerWaiting {0} CanSendServerCommands {1} commandCouldCauseUiAction {2}", 
                    ApplicationHelper.CanSendServerCommands(false), 
                    ApplicationHelper.CanSendServerCommands(), 
                    commandCouldCauseUiAction));
#endif

            if (ApplicationHelper.CanSendServerCommands(commandCouldCauseUiAction))
            {
                try
                {
                    retunSbStrings = SBPlusClient.Current.ExecuteSubroutineSynchronous(subroutineName, arguments);
                }
                catch (Exception exception)
                {
                    CustomLogger.LogException(exception, string.Format("Executing subroutine {0}", subroutineName));
                    if (exception.Message.Contains("The server is currently busy and cannot accept requests."))
                    {
                        throw notReadyException;
                    }

                    throw;
                }
            }
            else
            {
                CustomLogger.LogWarning(
                    () =>
                    string.Format(
                        "Call to subroutine {0} was ignored because the server is busy. Arguments {1}", 
                        subroutineName, 
                        arguments != null && arguments.Length > 0 ? arguments[0].GetStandardString() : string.Empty));
                throw notReadyException;
            }

            return retunSbStrings;
        }

        private static void ExecuteSubroutineCallback(bool isOk, SBString[] parameters, object userstate)
        {
            var callbackRuntime = userstate as CallbackRuntime<bool, SBString[]>;
            if (callbackRuntime == null)
            {
                return;
            }

            callbackRuntime.Callback(isOk, parameters);
        }

        /// <summary>
        /// Dispatcher does not propagate the exception
        /// </summary>
        /// <param name="subroutineName">
        /// Name of the subroutine.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="commandCouldCauseUiAction">
        /// if set to <c>true</c> [command could cause UI action].
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        private static object ExecuteSubroutineReturningException(
            string subroutineName, 
            SBString[] arguments, 
            bool commandCouldCauseUiAction = false)
        {
            try
            {
                return ExecuteSubroutine(subroutineName, arguments, commandCouldCauseUiAction);
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static SBString[] PrepareSubroutineParam(int parCount, string[] parameter)
        {
            var sbString = new SBString[parCount];
            for (int i = 0; i < parCount; i++)
            {
                if (i < parameter.Length)
                {
                    sbString[i] = new SBString(parameter[i]);
                }
                else
                {
                    sbString[i] = new SBString();
                }
            }

            return sbString;
        }

        private static void SubroutineCallback(string subroutinename, SBString[] parameters, Action<bool, SBString[]> callback)
        {
            var callbackRuntime = new CallbackRuntime<bool, SBString[]>();
            callbackRuntime.Callback = callback;
            SBPlusClient.Current.ExecuteSubroutine(
                subroutinename, 
                parameters, 
                callbackRuntime, 
                SubroutineOkCallback, 
                SubroutineFailedCallback);
        }

        private static void SubroutineCallback(
            string subroutineName, 
            SBString[] parameters, 
            object callbackRuntime, 
            SubroutineCallCompleted subroutineCallCompleted, 
            SubroutineCallFailed subroutineCallFailed = null)
        {
            SBPlusClient.Current.ExecuteSubroutine(
                subroutineName, 
                parameters, 
                callbackRuntime, 
                subroutineCallCompleted, 
                subroutineCallFailed);
        }

        private static void SubroutineFailedCallback(string subroutinename, SBString[] parameters, object userstate, Exception exception)
        {
            ExecuteSubroutineCallback(false, parameters, userstate);
        }

        private static void SubroutineOkCallback(string subroutinename, SBString[] parameters, object userstate)
        {
            ExecuteSubroutineCallback(true, parameters, userstate);
        }

        #endregion

        private class CallbackRuntime<T, T2>
        {
            #region Constructors and Destructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="CallbackRuntime{T,T2}" /> class.
            /// </summary>
            public CallbackRuntime()
            {
                this.Identification = Guid.NewGuid();
            }

            #endregion

            #region Properties

            internal Action<T, T2> Callback { get; set; }

            internal SbData Data { get; set; }

            internal Guid Identification { get; private set; }

            #endregion
        }
    }
}