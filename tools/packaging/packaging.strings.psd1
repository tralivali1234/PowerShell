@{
    Description = @'
PowerShell is an automation and configuration management platform.
It consists of a cross-platform command-line shell and associated scripting language.
'@

    RedHatAfterInstallScript = @'
#!/bin/sh
if [ ! -f /etc/shells ] ; then
    echo "{0}" > /etc/shells
else
    grep -q "^{0}$" /etc/shells || echo "{0}" >> /etc/shells
fi
'@

    RedHatAfterRemoveScript = @'
if [ "$1" = 0 ] ; then
    if [ -f /etc/shells ] ; then
        TmpFile=`/bin/mktemp /tmp/.powershellmXXXXXX`
        grep -v '^{0}$' /etc/shells > $TmpFile
        cp -f $TmpFile /etc/shells
        rm -f $TmpFile
    fi
fi
'@
    UbuntuAfterInstallScript = @'
#!/bin/sh
set -e
case "$1" in
    (configure)
        add-shell "{0}"
    ;;
    (abort-upgrade|abort-remove|abort-deconfigure)
        exit 0
    ;;
    (*)
        echo "postinst called with unknown argument '$1'" >&2
        exit 0
    ;;
esac
'@
    UbuntuAfterRemoveScript = @'
#!/bin/sh
set -e
case "$1" in
        (remove)
        remove-shell "{0}"
        ;;
esac
'@
# see https://developer.apple.com/library/content/documentation/DeveloperTools/Reference/DistributionDefinitionRef/Chapters/Distribution_XML_Ref.html
OsxDistributionTemplate = @'
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<installer-gui-script minSpecVersion="1">
    <title>{0}</title>
    <options hostArchitectures="x86_64"/>
    <options customize="never" rootVolumeOnly="true"/>
    <background file="macDialog.png" scaling="tofit" alignment="bottomleft"/>
    <allowed-os-versions>
        <os-version min="{3}" />
    </allowed-os-versions>
    <options customize="never" require-scripts="false"/>
    <product id="com.microsoft.powershell" version="{1}" />
    <choices-outline>
        <line choice="default">
            <line choice="powershell"/>
        </line>
    </choices-outline>
    <choice id="default"/>
    <choice id="powershell" visible="false">
        <pkg-ref id="com.microsoft.powershell"/>
    </choice>
    <pkg-ref id="com.microsoft.powershell" version="{1}" onConclusion="none">{2}</pkg-ref>
</installer-gui-script>
'@
NuspecTemplate = @'
<?xml version="1.0" encoding="utf-8"?>
<package
  xmlns="http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd">
  <metadata>
    <id>{0}</id>
    <version>{1}</version>
    <title>PowerShellRuntime</title>
    <authors>Powershell</authors>
    <owners>microsoft,powershell</owners>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <description>PowerShell runtime for hosting PowerShell</description>
    <copyright>Copyright (c) Microsoft Corporation. All rights reserved.</copyright>
    <language>en-US</language>
    <dependencies>
      <group targetFramework=".NETCoreApp2.0"></group>
    </dependencies>
  </metadata>
</package>
'@
RefAssemblyCsProj = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <Version>{0}</Version>
    <DelaySign>true</DelaySign>
    <AssemblyOriginatorKeyFile>{1}</AssemblyOriginatorKeyFile>
    <SignAssembly>true</SignAssembly>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Management.Infrastructure" Version="1.0.0-alpha08" />
    <PackageReference Include="System.Security.AccessControl" Version="4.4.1" />
    <PackageReference Include="System.Security.Principal.Windows" Version="4.4.1" />
  </ItemGroup>
</Project>
'@
NuGetConfigFile = @'
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="dotnet-core" value="https://dotnet.myget.org/F/dotnet-core/api/v3/index.json" />
    <add key="powershell-core" value="https://powershell.myget.org/F/powershell-core/api/v3/index.json" />
  </packageSources>
</configuration>
'@
}
