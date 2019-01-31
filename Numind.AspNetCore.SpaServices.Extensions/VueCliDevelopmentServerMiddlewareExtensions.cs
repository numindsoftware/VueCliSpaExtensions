using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Numind.AspNetCore.SpaServices.VueCliDevelopmentServer
{
    public static class VueCliDevelopmentServerMiddlewareExtensions
    {
        /// <summary>
        /// Handles requests by passing them through to an instance of the vue-cli-service server.
        /// This means you can always serve up-to-date CLI-built resources without having
        /// to run the vue-cli-service server manually.
        ///
        /// This feature should only be used in development. For production deployments, be
        /// sure not to enable the vue-cli-service server.
        /// </summary>
        /// <param name="spaBuilder">The <see cref="ISpaBuilder"/>.</param>
        /// <param name="npmScript">The name of the script in your package.json file that launches the vue-cli-service server.</param>
        public static void UseVueCliDevelopmentServer(
            this ISpaBuilder spaBuilder,
            string npmScript)
        {
            if (spaBuilder == null)
            {
                throw new ArgumentNullException(nameof(spaBuilder));
            }

            var spaOptions = spaBuilder.Options;

            if (string.IsNullOrEmpty(spaOptions.SourcePath))
            {
                throw new InvalidOperationException($"To use {nameof(UseVueCliDevelopmentServer)}, you must supply a non-empty value for the {nameof(SpaOptions.SourcePath)} property of {nameof(SpaOptions)} when calling {nameof(SpaApplicationBuilderExtensions.UseSpa)}.");
            }

            VueCliDevelopmentServerMiddleware.Attach(spaBuilder, npmScript);
        }
    }
}
