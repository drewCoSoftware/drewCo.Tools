//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2009-2014 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;

namespace drewCo.Tools
{
  // ============================================================================================================================
  public class Impersonator : IDisposable
  {

    #region WIN_API

    private const int LOGON32_PROVIDER_DEFAULT = 0;
    private const int LOGON32_LOGON_INTERACTIVE = 2;

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private extern static bool CloseHandle(IntPtr handle);

    #endregion

    private IntPtr token = IntPtr.Zero;
    private WindowsImpersonationContext impersonated;
    private readonly string _ErrMsg = "";

    #region Properties

    public bool IsImpersonating
    {
      get { return (token != IntPtr.Zero) && (impersonated != null); }
    }

    public string ErrMsg
    {
      get { return _ErrMsg; }
    }

    #endregion

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Attempts to impersonate the user with the provided credentials.  This class should be used with the 'using' statement.
    /// Check the 'IsImpersonating' property to determine if initialization was successful.  Error messages may also be provided
    /// through the 'ErrMsg' property.
    /// </summary>
    /// <remarks>
    /// If a name or password is not provided, then the class will not attempt to impersonate.
    /// </remarks>
    [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
    public Impersonator(string userName, string password, string domain)
    {
      StopImpersonating();

      // If a name or password is not provided, then the class will not attempt to impersonate.
      if ((userName == "") || (password == ""))
      {
        return;
      }

      bool loggedOn = LogonUser(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token);
      if (!loggedOn)
      {
        _ErrMsg = new System.ComponentModel.Win32Exception().Message;
        return;
      }

      WindowsIdentity identity = new WindowsIdentity(token);
      impersonated = identity.Impersonate();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Ensures that existing security tokens + impersonations don't go hanging around.
    /// </summary>
    private void StopImpersonating()
    {
      if (impersonated != null)
      {
        impersonated.Undo();
        impersonated = null;
      }

      if (token != IntPtr.Zero)
      {
        CloseHandle(token);
        token = IntPtr.Zero;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Dispose()
    {
      StopImpersonating();
    }
  }
}
