[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0) [![Build status](https://ci.appveyor.com/api/projects/status/9lfkr80jahecns88?svg=true)](https://ci.appveyor.com/project/nenitiko/vueclispaextensions) [![NuGet Badge](https://buildstats.info/nuget/numind.aspnetcore.spaservices.extensions)](https://www.nuget.org/packages/Numind.AspNetCore.SpaServices.Extensions) 
[![numind-aspnetcore MyGet Build Status](https://www.myget.org/BuildSource/Badge/numind-aspnetcore?identifier=0820550d-1cad-49bb-8036-b26826b88476)](https://www.myget.org/)

# A Vue-Cli support for AspNetCore SPAs

Allows use of vue-cli-service in ASPNetCore Spa Applications by supporting a development experience similar to both UseReactDevelopmentServer & UseAngularCliServer.
See: https://www.nuget.org/packages/Microsoft.AspNetCore.SpaServices.Extensions/
This library supports both NPM and Yarn package managers.

## Usage

The ASPNET Core framework has support for a new setup when creating SPA pplications, using the default templates generated code which uses the new Microsoft.AspNetCore.SpaServices.Extensions package. Both React and Angular development setups are supported, the code in Startup.cs looks somewhat like this:

```csharp
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ...
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    // Angular
                    spa.UseAngularCliServer(npmScript: "start");
                    ...
                    // react
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
```

This package adds a new extension method `UseVueCliDevelopmentServer` and support for Yarn Package Manager by way of a new parameter which default to NPM.

```csharp
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ...
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseVueCliDevelopmentServer(npmScript: "start", packageManager: PackageManager.Yarn);
                }
            });
        }
```

UseVueCliDevelopmentServer will call the run command (ex: `yarn run start`) of the specified package manager in `packageManager:`  argument. 

Your `package.json` script section should look like the following in order for the vue-cli development server to launch:

```javascript
  "scripts": {
    "start": "vue-cli-service serve",
    ...
  }
```

## License
https://www.apache.org/licenses/LICENSE-2.0
