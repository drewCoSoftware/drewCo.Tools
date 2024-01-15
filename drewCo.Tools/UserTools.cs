using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;


namespace drewCo.Tools
{
  // ============================================================================================================================
  public static class UserTools
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public static bool UserGroupExists(string groupName)
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(groupName);
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        var output = ProcessTools.RunProcessAndGetOutput("grep", "-c " + groupName + " /etc/group");
        return output.Trim() == "1";
      }
      else
      {
        throw new PlatformNotSupportedException("User group management is not supported on this platform.");
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void CreateNewUserGroup(string groupName)
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        ProcessTools.RunProcess("net", "localgroup " + groupName + " /add");
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        ProcessTools.RunProcess("groupadd", groupName);
      }
      else
      {
        throw new PlatformNotSupportedException("User group management is not supported on this platform.");
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public static void AddUserToGroup(string username, string groupName)
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        ProcessTools.RunProcess("net", "localgroup " + groupName + " " + username + " /add");
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        ProcessTools.RunProcess("usermod", "-a -G " + groupName + " " + username);
      }
      else
      {
        throw new PlatformNotSupportedException("User group management is not supported on this platform.");
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void RemoveUserFromGroup(string username, string groupName)
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        ProcessTools.RunProcess("net", "localgroup " + groupName + " " + username + " /delete");
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        ProcessTools.RunProcess("gpasswd", "-d " + username + " " + groupName);
      }
      else
      {
        throw new PlatformNotSupportedException("User group management is not supported on this platform.");
      }
    }
  }
}
