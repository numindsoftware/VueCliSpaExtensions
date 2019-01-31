using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.NodeServices.Util;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.SpaServices;
using Numind.AspNetCore.SpaServices.Util;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using System.IO;
using Numind.AspNetCore.SpaServices.Extensions.Util;
using Numind.AspNetCore.NodeServices.Util;
using Numind.AspNetCore.SpaServices.Extensions;

// This is under the NodeServices namespace because post 2.1 it will be moved to that package
namespace Numind.AspNetCore.SpaServices.VueCliDevelopmentServer
{
    internal static class VueCliDevelopmentServerMiddleware
    {
        private const string LogCategoryName = "Numind.AspNetCore.SpaServices";
        private static TimeSpan RegexMatchTimeout = TimeSpan.FromSeconds(5); // This is a development-time only feature, so a very long timeout is fine

        public static void Attach(
            ISpaBuilder spaBuilder,
            string nodeScriptName,
            string packageManager = PackageManager.Npm)
        {
            var sourcePath = spaBuilder.Options.SourcePath;
            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new ArgumentException("Cannot be null or empty", nameof(sourcePath));
            }

            if (string.IsNullOrEmpty(nodeScriptName))
            {
                throw new ArgumentException("Cannot be null or empty", nameof(nodeScriptName));
            }

            // Start vue-cli-service and attach to middleware pipeline
            var appBuilder = spaBuilder.ApplicationBuilder;
            var logger = LoggerFinder.GetOrCreateLogger(appBuilder, LogCategoryName);
            var portTask = StartCreateVueCliAppServerAsync(sourcePath, nodeScriptName, logger, packageManager);

            // Everything we proxy is hardcoded to target http://localhost because:
            // - the requests are always from the local machine (we're not accepting remote
            //   requests that go directly to the vue-cli-service server)
            // - given that, there's no reason to use https, and we couldn't even if we
            //   wanted to, because in general the vue-cli-service server has no certificate
            var targetUriTask = portTask.ContinueWith(
                task => new UriBuilder("http", "localhost", task.Result).Uri);

            SpaProxyingExtensions.UseProxyToSpaDevelopmentServer(spaBuilder, () =>
            {
                // On each request, we create a separate startup task with its own timeout. That way, even if
                // the first request times out, subsequent requests could still work.
                var timeout = spaBuilder.Options.StartupTimeout;
                return targetUriTask.WithTimeout(timeout,
                    $"The vue-cli-service server did not start listening for requests " +
                    $"within the timeout period of {timeout.Seconds} seconds. " +
                    $"Check the log output for error information.");
            });
        }

        private static async Task<int> StartCreateVueCliAppServerAsync(
            string sourcePath, string nodeScriptName, ILogger logger, string packageManager)
        {
            var portNumber = TcpPortFinder.FindAvailablePort();
            logger.LogInformation($"Starting vue-cli-service server on port {portNumber}...");

            string arguments = $"--port {portNumber}";
            var envVars = new Dictionary<string, string> { };
            var nodeScriptRunner = new NodeScriptRunner(sourcePath, nodeScriptName, arguments, envVars, packageManager);

            using (var stdErrReader = new EventedStreamStringReader(nodeScriptRunner.StdErr))
            {
                try
                {
                    // Although the dev server may eventually tell us the URL it's listening on,
                    // it doesn't do so until it's finished compiling, and even then only if there were
                    // no compiler warnings. So instead of waiting for that, consider it ready as soon
                    // as it starts listening for requests.
                    await nodeScriptRunner.StdOut.WaitForMatch(new Regex("Starting development server", RegexOptions.None, RegexMatchTimeout));
                }
                catch (EndOfStreamException ex)
                {
                    throw new InvalidOperationException(
                        $"The node script '{nodeScriptName}' exited without indicating that the " +
                        $"node server was listening for requests. The error output was: " +
                        $"{stdErrReader.ReadAsString()}", ex);
                }
            }

            return portNumber;
        }
    }
}