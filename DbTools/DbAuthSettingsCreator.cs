using DbTools.Models;
using SystemToolsShared;

namespace DbTools;

public static class DbAuthSettingsCreator
{
    public static DbAuthSettingsBase? Create(bool windowsNtIntegratedSecurity, string? serverUser, string? serverPass)
    {
        if (!windowsNtIntegratedSecurity && !string.IsNullOrWhiteSpace(serverUser) &&
            !string.IsNullOrWhiteSpace(serverPass))
            return new DbAuthSettings(serverUser, serverPass);

        if (windowsNtIntegratedSecurity)
        {
            if (!string.IsNullOrWhiteSpace(serverUser) || !string.IsNullOrWhiteSpace(serverPass))
                StShared.WriteWarningLine(
                    "windowsNtIntegratedSecurity is on and serverUser is specified or serverPass is specified. both will be ignored.",
                    true);
            return new DbAuthSettingsBase();
        }

        StShared.WriteErrorLine(
            "windowsNtIntegratedSecurity is off and serverUser does not specified or serverPass does not specified",
            true);
        return null;
    }
}