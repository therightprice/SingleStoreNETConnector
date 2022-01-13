using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace SideBySide;

public static class AppConfig
{
	private static IReadOnlyDictionary<string, string> DefaultConfig { get; } =
		new Dictionary<string, string>
		{
			["Data:NoPasswordUser"] = "",
			["Data:SupportsCachedProcedures"] = "false",
			["Data:SupportsJson"] = "false",
		};

	public static string CertsPath => Path.GetFullPath(Config.GetValue<string>("Data:CertificatesPath"));

	private static IConfiguration BuildConfiguration()
	{
		var builder = new ConfigurationBuilder()
			.AddInMemoryCollection(DefaultConfig)
			.AddJsonFile("config.json")
			.AddEnvironmentVariables();
		return builder.Build();
	}

	public static IConfiguration Config { get; } = BuildConfiguration();

	public static string ConnectionString => Config.GetValue<string>("Data:ConnectionString");

	public static string PasswordlessUser => Config.GetValue<string>("Data:PasswordlessUser");

	public static string GSSAPIUser => Config.GetValue<string>("Data:GSSAPIUser");

	public static bool HasKerberos => Config.GetValue<bool>("Data:HasKerberos");

	public static string SecondaryDatabase => Config.GetValue<string>("Data:SecondaryDatabase");

	public static string SocketPath => Config.GetValue<string>("Data:SocketPath");

	private static ServerFeatures UnsupportedFeatures => (ServerFeatures) Enum.Parse(typeof(ServerFeatures), Config.GetValue<string>("Data:UnsupportedFeatures"));

	public static ServerFeatures SupportedFeatures => ~ServerFeatures.None & ~UnsupportedFeatures & ~(IsCiBuild ? ServerFeatures.Timeout : ServerFeatures.None);

	public static bool SupportsJson => SupportedFeatures.HasFlag(ServerFeatures.Json);

	public static string MySqlBulkLoaderCsvFile => Config.GetValue<string>("Data:MySqlBulkLoaderCsvFile");
	public static string MySqlBulkLoaderLocalCsvFile => Config.GetValue<string>("Data:MySqlBulkLoaderLocalCsvFile");
	public static string MySqlBulkLoaderTsvFile => Config.GetValue<string>("Data:MySqlBulkLoaderTsvFile");
	public static string MySqlBulkLoaderLocalTsvFile => Config.GetValue<string>("Data:MySqlBulkLoaderLocalTsvFile");

	public static SingleStoreConnectionStringBuilder CreateConnectionStringBuilder() => new SingleStoreConnectionStringBuilder(ConnectionString);

	public static SingleStoreConnectionStringBuilder CreateSha256ConnectionStringBuilder()
	{
		var csb = CreateConnectionStringBuilder();
		csb.UserID = "sha256user";
		csb.Password = "Sh@256Pa55";
		csb.Database = null;
		return csb;
	}

	public static SingleStoreConnectionStringBuilder CreateCachingSha2ConnectionStringBuilder()
	{
		var csb = CreateConnectionStringBuilder();
		csb.UserID = "caching-sha2-user";
		csb.Password = "Cach!ng-Sh@2-Pa55";
		csb.Database = null;
		return csb;
	}

	public static SingleStoreConnectionStringBuilder CreateGSSAPIConnectionStringBuilder()
	{
		var csb = CreateConnectionStringBuilder();
		csb.UserID = GSSAPIUser;
		csb.Database = null;
		return csb;
	}

	public static bool IsCiBuild =>
		Environment.GetEnvironmentVariable("APPVEYOR") == "True" ||
		Environment.GetEnvironmentVariable("TRAVIS") == "true" ||
		Environment.GetEnvironmentVariable("TF_BUILD") == "True" ||
		Environment.GetEnvironmentVariable("CIRCLECI") == "true";

	// tests can run much slower in CI environments
	public static int TimeoutDelayFactor { get; } = Environment.GetEnvironmentVariable("APPVEYOR") == "True" || Environment.GetEnvironmentVariable("TRAVIS") == "true" ? 6 :
		Environment.GetEnvironmentVariable("TF_BUILD") == "True" || Environment.GetEnvironmentVariable("CIRCLECI") == "true" ? 10 : 1;
}
