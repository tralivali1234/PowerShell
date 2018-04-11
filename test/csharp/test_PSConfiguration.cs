// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Xunit;
using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Configuration;
using System.Management.Automation.Internal;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PSTests.Sequential
{
    public class PowerShellPolicyFixture : IDisposable
    {
        private const string configFileName = "powershell.config.json";
        private readonly string systemWideConfigFile;
        private readonly string currentUserConfigFile;

        private readonly string systemWideConfigBackupFile;
        private readonly string currentUserConfigBackupFile;

        private readonly string systemWideConfigDirectory;
        private readonly string currentUserConfigDirectory;

        private readonly JsonSerializer serializer;

        private readonly PowerShellPolicies systemWidePolicies;
        private readonly PowerShellPolicies currentUserPolicies;

        private readonly bool originalTestHookValue;

        public PowerShellPolicyFixture()
        {
            systemWideConfigDirectory = Utils.DefaultPowerShellAppBase;
            currentUserConfigDirectory = Utils.GetUserConfigurationDirectory();

            if (!Directory.Exists(currentUserConfigDirectory))
            {
                // Create the CurrentUser config directory if it doesn't exist
                Directory.CreateDirectory(currentUserConfigDirectory);
            }

            systemWideConfigFile = Path.Combine(systemWideConfigDirectory, configFileName);
            currentUserConfigFile = Path.Combine(currentUserConfigDirectory, configFileName);

            if (File.Exists(systemWideConfigFile))
            {
                systemWideConfigBackupFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                File.Move(systemWideConfigFile, systemWideConfigBackupFile);
            }
            if (File.Exists(currentUserConfigFile))
            {
                currentUserConfigBackupFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                File.Move(currentUserConfigFile, currentUserConfigBackupFile);
            }

            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.None,
                MaxDepth = 10,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            serializer = JsonSerializer.Create(settings);

            systemWidePolicies = new PowerShellPolicies()
            {
                ScriptExecution = new ScriptExecution() { ExecutionPolicy = "RemoteSigned", EnableScripts = true },
                ScriptBlockLogging = new ScriptBlockLogging() { EnableScriptBlockInvocationLogging = true, EnableScriptBlockLogging = false },
                ModuleLogging = new ModuleLogging() { EnableModuleLogging = false, ModuleNames = new string[] { "PSReadline", "PowerShellGet" } },
                ProtectedEventLogging = new ProtectedEventLogging() { EnableProtectedEventLogging = false, EncryptionCertificate = new string[] { "Joe" } },
                Transcription = new Transcription() { EnableInvocationHeader = true, EnableTranscripting = true, OutputDirectory = "c:\tmp" },
                UpdatableHelp = new UpdatableHelp() { DefaultSourcePath = "f:\temp" },
                ConsoleSessionConfiguration = new ConsoleSessionConfiguration() { EnableConsoleSessionConfiguration = true, ConsoleSessionConfigurationName = "name" }
            };

            currentUserPolicies = new PowerShellPolicies()
            {
                ScriptExecution = new ScriptExecution() { ExecutionPolicy = "RemoteSigned" },
                ScriptBlockLogging = new ScriptBlockLogging() { EnableScriptBlockLogging = false },
                ModuleLogging = new ModuleLogging() { EnableModuleLogging = false },
                ProtectedEventLogging = new ProtectedEventLogging() { EncryptionCertificate = new string[] { "Joe" } }
            };

            // Set the test hook to disable policy caching
            originalTestHookValue = InternalTestHooks.BypassGroupPolicyCaching;
            InternalTestHooks.BypassGroupPolicyCaching = true;
        }

        public void Dispose()
        {
            CleanupConfigFiles();
            if (systemWideConfigBackupFile != null)
            {
                File.Move(systemWideConfigBackupFile, systemWideConfigFile);
            }

            if (currentUserConfigBackupFile != null)
            {
                File.Move(currentUserConfigBackupFile, currentUserConfigFile);
            }
            InternalTestHooks.BypassGroupPolicyCaching = originalTestHookValue;
        }

        internal PowerShellPolicies SystemWidePolicies
        {
            get { return systemWidePolicies; }
        }

        internal PowerShellPolicies CurrentUserPolicies
        {
            get { return currentUserPolicies; }
        }

        #region Compare_Policy_Settings

        internal void CompareScriptExecution(ScriptExecution a, ScriptExecution b)
        {
            if (a == null)
            {
                Assert.Null(b);
            }
            else
            {
                Assert.Equal(a.EnableScripts, b.EnableScripts);
                Assert.Equal(a.ExecutionPolicy, b.ExecutionPolicy);
            }
        }

        internal void CompareScriptBlockLogging(ScriptBlockLogging a, ScriptBlockLogging b)
        {
            if (a == null)
            {
                Assert.Null(b);
            }
            else
            {
                Assert.Equal(a.EnableScriptBlockInvocationLogging, b.EnableScriptBlockInvocationLogging);
                Assert.Equal(a.EnableScriptBlockLogging, b.EnableScriptBlockLogging);
            }
        }

        internal void CompareModuleLogging(ModuleLogging a, ModuleLogging b)
        {
            if (a == null)
            {
                Assert.Null(b);
            }
            else
            {
                Assert.Equal(a.EnableModuleLogging, b.EnableModuleLogging);
                if (a.ModuleNames == null)
                {
                    Assert.Null(b.ModuleNames);
                }
                else
                {
                    Assert.Equal(a.ModuleNames.Length, b.ModuleNames.Length);
                    for (int i = 0; i < a.ModuleNames.Length; i++)
                    {
                        Assert.Equal(a.ModuleNames[i], b.ModuleNames[i]);
                    }
                }
            }
        }

        internal void CompareProtectedEventLogging(ProtectedEventLogging a, ProtectedEventLogging b)
        {
            if (a == null)
            {
                Assert.Null(b);
            }
            else
            {
                Assert.Equal(a.EnableProtectedEventLogging, b.EnableProtectedEventLogging);
                if (a.EncryptionCertificate == null)
                {
                    Assert.Null(b.EncryptionCertificate);
                }
                else
                {
                    Assert.Equal(a.EncryptionCertificate.Length, b.EncryptionCertificate.Length);
                    for (int i = 0; i < a.EncryptionCertificate.Length; i++)
                    {
                        Assert.Equal(a.EncryptionCertificate[i], b.EncryptionCertificate[i]);
                    }
                }
            }
        }

        internal void CompareTranscription(Transcription a, Transcription b)
        {
            if (a == null)
            {
                Assert.Null(b);
            }
            else
            {
                Assert.Equal(a.EnableTranscripting, b.EnableTranscripting);
                Assert.Equal(a.EnableInvocationHeader, b.EnableInvocationHeader);
                Assert.Equal(a.OutputDirectory, b.OutputDirectory);
            }
        }

        internal void CompareUpdatableHelp(UpdatableHelp a, UpdatableHelp b)
        {
            if (a == null)
            {
                Assert.Null(b);
            }
            else
            {
                Assert.Equal(a.DefaultSourcePath, b.DefaultSourcePath);
            }
        }

        internal void CompareConsoleSessionConfiguration(ConsoleSessionConfiguration a, ConsoleSessionConfiguration b)
        {
            if (a == null)
            {
                Assert.Null(b);
            }
            else
            {
                Assert.Equal(a.EnableConsoleSessionConfiguration, b.EnableConsoleSessionConfiguration);
                Assert.Equal(a.ConsoleSessionConfigurationName, b.ConsoleSessionConfigurationName);
            }
        }

        internal void CompareTwoPolicies(PowerShellPolicies a, PowerShellPolicies b)
        {
            // Compare 'ScriptExecution' settings
            CompareScriptExecution(a.ScriptExecution, b.ScriptExecution);

            // Compare 'ScriptBlockLogging' settings
            CompareScriptBlockLogging(a.ScriptBlockLogging, b.ScriptBlockLogging);

            // Compare 'ModuleLogging' settings
            CompareModuleLogging(a.ModuleLogging, b.ModuleLogging);

            // Compare 'ProtectedEventLogging' settings
            CompareProtectedEventLogging(a.ProtectedEventLogging, b.ProtectedEventLogging);

            // Compare 'Transcription' settings
            CompareTranscription(a.Transcription, b.Transcription);

            // Compare 'UpdatableHelp' settings
            CompareUpdatableHelp(a.UpdatableHelp, b.UpdatableHelp);

            // Compare 'ConsoleSessionConfiguration' settings
            CompareConsoleSessionConfiguration(a.ConsoleSessionConfiguration, b.ConsoleSessionConfiguration);
        }

        #endregion

        #region Configuration_File_Setup

        public void CleanupConfigFiles()
        {
            if (File.Exists(systemWideConfigFile))
            {
                File.Delete(systemWideConfigFile);
            }
            if (File.Exists(currentUserConfigFile))
            {
                File.Delete(currentUserConfigFile);
            }
        }

        public void SetupConfigFile1()
        {
            CleanupConfigFiles();

            // System wide config file has all policy settings
            var systemWideConfig = new { ConsolePrompting = true, PowerShellPolicies = systemWidePolicies };
            using (var streamWriter = new StreamWriter(systemWideConfigFile))
            {
                serializer.Serialize(streamWriter, systemWideConfig);
            }

            // Current user config file has partial policy settings
            var currentUserConfig = new { DisablePromptToUpdateHelp = false, PowerShellPolicies = currentUserPolicies };
            using (var streamWriter = new StreamWriter(currentUserConfigFile))
            {
                serializer.Serialize(streamWriter, currentUserConfig);
            }
        }

        public void SetupConfigFile2()
        {
            CleanupConfigFiles();

            // System wide config file has all policy settings
            var systemWideConfig = new { ConsolePrompting = true, PowerShellPolicies = systemWidePolicies };
            using (var streamWriter = new StreamWriter(systemWideConfigFile))
            {
                serializer.Serialize(streamWriter, systemWideConfig);
            }

            // Current user config file is empty
            CreateEmptyFile(currentUserConfigFile);
        }

        public void SetupConfigFile3()
        {
            CleanupConfigFiles();

            // System wide config file is empty
            CreateEmptyFile(systemWideConfigFile);

            // Current user config file has partial policy settings
            var currentUserConfig = new { DisablePromptToUpdateHelp = false, PowerShellPolicies = currentUserPolicies };
            using (var streamWriter = new StreamWriter(currentUserConfigFile))
            {
                serializer.Serialize(streamWriter, currentUserConfig);
            }
        }

        public void SetupConfigFile4()
        {
            CleanupConfigFiles();
            // System wide config file is empty
            CreateEmptyFile(systemWideConfigFile);
            // Current user config file is empty
            CreateEmptyFile(currentUserConfigFile);
        }

        private void CreateEmptyFile(string fileName)
        {
            File.Create(fileName).Dispose();
        }

        #endregion
    }

    public class PowerShellPolicyTests : IClassFixture<PowerShellPolicyFixture>
    {
        PowerShellPolicyFixture fixture;

        public PowerShellPolicyTests(PowerShellPolicyFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void PowerShellConfig_GetPowerShellPolicies_BothConfigFilesNotEmpty()
        {
            fixture.SetupConfigFile1();
            var sysPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.SystemWide);
            var userPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.CurrentUser);

            Assert.NotNull(sysPolicies);
            Assert.NotNull(userPolicies);

            fixture.CompareTwoPolicies(sysPolicies, fixture.SystemWidePolicies);
            fixture.CompareTwoPolicies(userPolicies, fixture.CurrentUserPolicies);
        }

        [Fact]
        public void PowerShellConfig_GetPowerShellPolicies_EmptyUserConfig()
        {
            fixture.SetupConfigFile2();
            var sysPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.SystemWide);
            var userPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.CurrentUser);

            Assert.NotNull(sysPolicies);
            Assert.Null(userPolicies);

            fixture.CompareTwoPolicies(sysPolicies, fixture.SystemWidePolicies);
        }

        [Fact]
        public void PowerShellConfig_GetPowerShellPolicies_EmptySystemConfig()
        {
            fixture.SetupConfigFile3();
            var sysPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.SystemWide);
            var userPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.CurrentUser);

            Assert.Null(sysPolicies);
            Assert.NotNull(userPolicies);

            fixture.CompareTwoPolicies(userPolicies, fixture.CurrentUserPolicies);
        }

        [Fact]
        public void PowerShellConfig_GetPowerShellPolicies_BothConfigFilesEmpty()
        {
            fixture.SetupConfigFile4();
            var sysPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.SystemWide);
            var userPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.CurrentUser);

            Assert.Null(sysPolicies);
            Assert.Null(userPolicies);
        }

        [Fact]
        public void PowerShellConfig_GetPowerShellPolicies_BothConfigFilesNotExist()
        {
            fixture.CleanupConfigFiles();
            var sysPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.SystemWide);
            var userPolicies = PowerShellConfig.Instance.GetPowerShellPolicies(ConfigScope.CurrentUser);

            Assert.Null(sysPolicies);
            Assert.Null(userPolicies);
        }

        [Fact]
        public void Utils_GetPolicySetting_BothConfigFilesNotEmpty()
        {
            fixture.SetupConfigFile1();

            ScriptExecution scriptExecution;
            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.SystemWidePolicies.ScriptExecution);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.CurrentUserPolicies.ScriptExecution);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.SystemWidePolicies.ScriptExecution);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.CurrentUserPolicies.ScriptExecution);

            ScriptBlockLogging scriptBlockLogging;
            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.SystemWidePolicies.ScriptBlockLogging);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.CurrentUserPolicies.ScriptBlockLogging);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.SystemWidePolicies.ScriptBlockLogging);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.CurrentUserPolicies.ScriptBlockLogging);

            ModuleLogging moduleLogging;
            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.SystemWidePolicies.ModuleLogging);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.CurrentUserPolicies.ModuleLogging);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.SystemWidePolicies.ModuleLogging);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.CurrentUserPolicies.ModuleLogging);

            ProtectedEventLogging protectedEventLogging;
            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.SystemWidePolicies.ProtectedEventLogging);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.CurrentUserPolicies.ProtectedEventLogging);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.SystemWidePolicies.ProtectedEventLogging);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.CurrentUserPolicies.ProtectedEventLogging);

            // The CurrentUser config doesn't contain any settings for 'Transcription', 'UpdatableHelp' and 'ConsoleSessionConfiguration'
            Transcription transcription;
            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideOnlyConfig);
            fixture.CompareTranscription(transcription, fixture.SystemWidePolicies.Transcription);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserOnlyConfig);
            fixture.CompareTranscription(transcription, null);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareTranscription(transcription, fixture.SystemWidePolicies.Transcription);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareTranscription(transcription, fixture.SystemWidePolicies.Transcription);

            UpdatableHelp updatableHelp;
            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, fixture.SystemWidePolicies.UpdatableHelp);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareUpdatableHelp(updatableHelp, fixture.SystemWidePolicies.UpdatableHelp);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareUpdatableHelp(updatableHelp, fixture.SystemWidePolicies.UpdatableHelp);

            ConsoleSessionConfiguration consoleSessionConfiguration;
            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, fixture.SystemWidePolicies.ConsoleSessionConfiguration);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, fixture.SystemWidePolicies.ConsoleSessionConfiguration);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, fixture.SystemWidePolicies.ConsoleSessionConfiguration);
        }

        [Fact]
        public void Utils_GetPolicySetting_EmptyUserConfig()
        {
            fixture.SetupConfigFile2();

            // The CurrentUser config is empty
            ScriptExecution scriptExecution;
            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.SystemWidePolicies.ScriptExecution);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, null);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.SystemWidePolicies.ScriptExecution);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.SystemWidePolicies.ScriptExecution);

            ScriptBlockLogging scriptBlockLogging;
            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.SystemWidePolicies.ScriptBlockLogging);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, null);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.SystemWidePolicies.ScriptBlockLogging);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.SystemWidePolicies.ScriptBlockLogging);

            ModuleLogging moduleLogging;
            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.SystemWidePolicies.ModuleLogging);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, null);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.SystemWidePolicies.ModuleLogging);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.SystemWidePolicies.ModuleLogging);

            ProtectedEventLogging protectedEventLogging;
            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.SystemWidePolicies.ProtectedEventLogging);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, null);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.SystemWidePolicies.ProtectedEventLogging);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.SystemWidePolicies.ProtectedEventLogging);

            Transcription transcription;
            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideOnlyConfig);
            fixture.CompareTranscription(transcription, fixture.SystemWidePolicies.Transcription);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserOnlyConfig);
            fixture.CompareTranscription(transcription, null);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareTranscription(transcription, fixture.SystemWidePolicies.Transcription);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareTranscription(transcription, fixture.SystemWidePolicies.Transcription);

            UpdatableHelp updatableHelp;
            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, fixture.SystemWidePolicies.UpdatableHelp);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareUpdatableHelp(updatableHelp, fixture.SystemWidePolicies.UpdatableHelp);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareUpdatableHelp(updatableHelp, fixture.SystemWidePolicies.UpdatableHelp);

            ConsoleSessionConfiguration consoleSessionConfiguration;
            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, fixture.SystemWidePolicies.ConsoleSessionConfiguration);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, fixture.SystemWidePolicies.ConsoleSessionConfiguration);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, fixture.SystemWidePolicies.ConsoleSessionConfiguration);
        }

        [Fact]
        public void Utils_GetPolicySetting_EmptySystemConfig()
        {
            fixture.SetupConfigFile3();

            // The SystemWide config is empty
            ScriptExecution scriptExecution;
            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, null);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.CurrentUserPolicies.ScriptExecution);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.CurrentUserPolicies.ScriptExecution);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareScriptExecution(scriptExecution, fixture.CurrentUserPolicies.ScriptExecution);

            ScriptBlockLogging scriptBlockLogging;
            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, null);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.CurrentUserPolicies.ScriptBlockLogging);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.CurrentUserPolicies.ScriptBlockLogging);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, fixture.CurrentUserPolicies.ScriptBlockLogging);

            ModuleLogging moduleLogging;
            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, null);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.CurrentUserPolicies.ModuleLogging);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.CurrentUserPolicies.ModuleLogging);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareModuleLogging(moduleLogging, fixture.CurrentUserPolicies.ModuleLogging);

            ProtectedEventLogging protectedEventLogging;
            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, null);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.CurrentUserPolicies.ProtectedEventLogging);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.CurrentUserPolicies.ProtectedEventLogging);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, fixture.CurrentUserPolicies.ProtectedEventLogging);

            // The CurrentUser config doesn't contain any settings for 'Transcription', 'UpdatableHelp' and 'ConsoleSessionConfiguration'
            Transcription transcription;
            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideOnlyConfig);
            fixture.CompareTranscription(transcription, null);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserOnlyConfig);
            fixture.CompareTranscription(transcription, null);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareTranscription(transcription, null);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareTranscription(transcription, null);

            UpdatableHelp updatableHelp;
            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            ConsoleSessionConfiguration consoleSessionConfiguration;
            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);
        }

        [Fact]
        public void Utils_GetPolicySetting_BothConfigFilesEmpty()
        {
            fixture.SetupConfigFile4();

            // Both config files are empty
            ScriptExecution scriptExecution;
            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, null);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, null);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareScriptExecution(scriptExecution, null);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareScriptExecution(scriptExecution, null);

            ScriptBlockLogging scriptBlockLogging;
            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, null);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, null);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, null);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, null);

            ModuleLogging moduleLogging;
            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, null);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, null);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareModuleLogging(moduleLogging, null);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareModuleLogging(moduleLogging, null);

            ProtectedEventLogging protectedEventLogging;
            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, null);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, null);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, null);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, null);

            // The CurrentUser config doesn't contain any settings for 'Transcription', 'UpdatableHelp' and 'ConsoleSessionConfiguration'
            Transcription transcription;
            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideOnlyConfig);
            fixture.CompareTranscription(transcription, null);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserOnlyConfig);
            fixture.CompareTranscription(transcription, null);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareTranscription(transcription, null);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareTranscription(transcription, null);

            UpdatableHelp updatableHelp;
            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            ConsoleSessionConfiguration consoleSessionConfiguration;
            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);
        }

        [Fact]
        public void Utils_GetPolicySetting_BothConfigFilesNotExist()
        {
            fixture.CleanupConfigFiles();

            // Both config files don't exist
            ScriptExecution scriptExecution;
            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, null);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptExecution(scriptExecution, null);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareScriptExecution(scriptExecution, null);

            scriptExecution = Utils.GetPolicySetting<ScriptExecution>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareScriptExecution(scriptExecution, null);

            ScriptBlockLogging scriptBlockLogging;
            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, null);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, null);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, null);

            scriptBlockLogging = Utils.GetPolicySetting<ScriptBlockLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareScriptBlockLogging(scriptBlockLogging, null);

            ModuleLogging moduleLogging;
            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, null);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareModuleLogging(moduleLogging, null);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareModuleLogging(moduleLogging, null);

            moduleLogging = Utils.GetPolicySetting<ModuleLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareModuleLogging(moduleLogging, null);

            ProtectedEventLogging protectedEventLogging;
            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, null);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserOnlyConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, null);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, null);

            protectedEventLogging = Utils.GetPolicySetting<ProtectedEventLogging>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareProtectedEventLogging(protectedEventLogging, null);

            // The CurrentUser config doesn't contain any settings for 'Transcription', 'UpdatableHelp' and 'ConsoleSessionConfiguration'
            Transcription transcription;
            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideOnlyConfig);
            fixture.CompareTranscription(transcription, null);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserOnlyConfig);
            fixture.CompareTranscription(transcription, null);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareTranscription(transcription, null);

            transcription = Utils.GetPolicySetting<Transcription>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareTranscription(transcription, null);

            UpdatableHelp updatableHelp;
            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserOnlyConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            updatableHelp = Utils.GetPolicySetting<UpdatableHelp>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareUpdatableHelp(updatableHelp, null);

            ConsoleSessionConfiguration consoleSessionConfiguration;
            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserOnlyConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.SystemWideThenCurrentUserConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);

            consoleSessionConfiguration = Utils.GetPolicySetting<ConsoleSessionConfiguration>(Utils.CurrentUserThenSystemWideConfig);
            fixture.CompareConsoleSessionConfiguration(consoleSessionConfiguration, null);
        }
    }
}