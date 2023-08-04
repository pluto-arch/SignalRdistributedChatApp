namespace ChatService.Infra.Constants;

public class DbConstants
{
    public const string DefaultTableSchema = null!;

    public const string DEFAULT_CONNECTIONSTRING_NAME = "Default";
}


public static class EnvironmentConstants
{
    public static string EnvHostName = Environment.GetEnvironmentVariable("HOSTNAME");
}