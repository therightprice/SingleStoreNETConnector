using System;
using Microsoft.Extensions.Logging;
using SingleStoreConnector.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class SingleStoreConnectorLoggingExtensions
{
	public static IServiceProvider UseSingleStoreConnectorLogging(this IServiceProvider services)
	{
		var loggerFactory = (ILoggerFactory) services.GetService(typeof(ILoggerFactory));
		if (loggerFactory is null)
			throw new InvalidOperationException("No ILoggerFactory service has been registered.");
		SingleStoreConnectorLogManager.Provider = new MicrosoftExtensionsLoggingLoggerProvider(loggerFactory);
		return services;
	}
}
