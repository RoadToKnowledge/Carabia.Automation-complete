namespace Carabia.Automation.Framework.Configuration;

/// <summary>Target environment. Only <see cref="Dev"/> may run write or destructive tests.</summary>
public enum EnvironmentType
{
    Dev,
    Local,
    Prod
}
