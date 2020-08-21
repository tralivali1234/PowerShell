// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

using Microsoft.PowerShell;
using Xunit;

namespace PSTests.Parallel
{
    public static class PSCommandLineParserTests
    {
        [Fact]
        public static void TestDefaults()
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(new string[0]);

            Assert.False(cpp.AbortStartup);
            Assert.Empty(cpp.Args);
            Assert.Null(cpp.ConfigurationName);
            Assert.Null(cpp.CustomPipeName);
            Assert.Null(cpp.ErrorMessage);
            Assert.Null(cpp.ExecutionPolicy);
            Assert.Equal((uint)ConsoleHost.ExitCodeSuccess, cpp.ExitCode);
            Assert.False(cpp.ExplicitReadCommandsFromStdin);
            Assert.Null(cpp.File);
            Assert.Null(cpp.InitialCommand);
            Assert.Equal(Microsoft.PowerShell.Serialization.DataFormat.Text, cpp.InputFormat);
            Assert.False(cpp.NamedPipeServerMode);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.NonInteractive);
            Assert.False(cpp.NoPrompt);
            Assert.Equal(Microsoft.PowerShell.Serialization.DataFormat.Text, cpp.OutputFormat);
            Assert.False(cpp.OutputFormatSpecified);
#if !UNIX
            Assert.False(cpp.RemoveWorkingDirectoryTrailingCharacter);
#endif
            Assert.False(cpp.ServerMode);
            Assert.True(cpp.ShowBanner);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowVersion);
            Assert.False(cpp.SkipProfiles);
            Assert.False(cpp.SocketServerMode);
            Assert.False(cpp.SSHServerMode);
            Assert.True(cpp.StaMode);
            Assert.False(cpp.ThrowOnReadAndPrompt);
            Assert.False(cpp.WasInitialCommandEncoded);
            Assert.Null(cpp.WorkingDirectory);
            Assert.False(cpp.NonInteractive);
        }

        [Fact]
        public static void Test_Throws_On_Reuse()
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(new string[0]);

            Assert.Throws<System.InvalidOperationException>(() => cpp.Parse(new string[0]));
        }

        [Theory]
        [InlineData("noexistfilename")]
        public static void TestDefaultParameterIsFileName_Not_Exist(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.False(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal(CommandLineParameterParser.NormalizeFilePath("noexistfilename"), cpp.File);
            Assert.Equal(
                string.Format(CommandLineParameterParserStrings.ArgumentFileDoesNotExist, "noexistfilename"),
                cpp.ErrorMessage);
        }

        [Fact]
        public static void TestDefaultParameterIsFileName_Exist()
        {
            var fileName = System.IO.Path.GetTempFileName();

            var cpp = new CommandLineParameterParser();

            cpp.Parse(new string[] { fileName });

            Assert.False(cpp.AbortStartup);
            Assert.False(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal(CommandLineParameterParser.NormalizeFilePath(fileName), cpp.File);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-file", "-")]
        public static void TestParameterIsFileName_Dash(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.False(cpp.NoPrompt);
            Assert.True(cpp.ExplicitReadCommandsFromStdin);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-prof")]
        public static void TestParameterIs_Wrong_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.False(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.False(cpp.NoPrompt);
            Assert.Contains(commandLine[0], cpp.ErrorMessage);
            Assert.Contains("-noprofile", cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-Version")]
        [InlineData("-V")]
        [InlineData("-Version", "abbra")] // Ignore all after the parameter
        public static void TestParameter_Version(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.False(cpp.NoExit);
            Assert.True(cpp.NonInteractive);
            Assert.False(cpp.ShowBanner);
            Assert.True(cpp.ShowVersion);
            Assert.True(cpp.SkipProfiles);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-Help")]
        [InlineData("-h")]
        public static void TestParameter_Help(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.ShowShortHelp);
            Assert.True(cpp.ShowExtendedHelp);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-Login")]
        [InlineData("-l")]
        public static void TestParameter_Login(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            // Parser does not change any internal properties for the parameter.
            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-noexit")]
        [InlineData("-noe")]
        public static void TestParameter_NoExit(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-noprofile")]
        [InlineData("-nop")]
        public static void TestParameter_NoProfile(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.True(cpp.SkipProfiles);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-nologo")]
        [InlineData("-nol")]
        public static void TestParameter_NoLogo(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-noninteractive")]
        [InlineData("-noni")]
        public static void TestParameter_NoInteractive(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.True(cpp.NonInteractive);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-socketservermode")]
        [InlineData("-so")]
        public static void TestParameter_SocketServerMode(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.True(cpp.SocketServerMode);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-servermode")]
        [InlineData("-s")]
        public static void TestParameter_ServerMode(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.True(cpp.ServerMode);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-namedpipeservermode")]
        [InlineData("-nam")]
        public static void TestParameter_NamedPipeServerMode(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.True(cpp.NamedPipeServerMode);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-sshservermode")]
        [InlineData("-sshs")]
        public static void TestParameter_SSHServerMode(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.True(cpp.SSHServerMode);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-interactive")]
        [InlineData("-i")]
        public static void TestParameter_Interactive(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.False(cpp.NonInteractive);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-configurationname")]
        [InlineData("-config")]
        public static void TestParameter_ConfigurationName_No_Name(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MissingConfigurationNameArgument, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-configurationname", "qwerty")]
        [InlineData("-config", "qwerty")]
        public static void TestParameter_ConfigurationName_With_Name(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Equal("qwerty", cpp.ConfigurationName);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-custompipename")]
        [InlineData("-cus")]
        public static void TestParameter_CustomPipeName_No_Name(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MissingCustomPipeNameArgument, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-custompipename", "qwerty")]
        [InlineData("-cus", "qwerty")]
        public static void TestParameter_CustomPipeName_With_Name(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Equal("qwerty", cpp.CustomPipeName);
            Assert.Null(cpp.ErrorMessage);
        }

        public static System.Collections.Generic.IEnumerable<object[]> Data =>
            new System.Collections.Generic.List<string[]>
            {
                new string[] { "-custompipename", new string('q', CommandLineParameterParser.MaxNameLength() + 1) }
            };

        [SkippableTheory]
        [MemberData(nameof(Data))]
        public static void TestParameter_CustomPipeName_With_Too_Long_Name(params string[] commandLine)
        {
            Skip.If(Platform.IsWindows);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(
                string.Format(
                    CommandLineParameterParserStrings.CustomPipeNameTooLong,
                    CommandLineParameterParser.MaxNameLength(),
                    commandLine[1],
                    commandLine[1].Length),
                cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-command")]
        [InlineData("-c")]
        public static void TestParameter_Command_No_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MissingCommandParameter, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-command", "qwerty")]
        [InlineData("-c", "qwerty")]
        public static void TestParameter_Command_With_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.False(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal("qwerty", cpp.InitialCommand);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-command", "-")]
        [InlineData("-c", "-")]
        public static void TestParameter_Command_With_Dash_And_Not_ConsoleInputRedirected(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.TestHookConsoleInputRedirected = false;

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.True(cpp.NoPrompt);
            Assert.True(cpp.ExplicitReadCommandsFromStdin);
            Assert.Equal(CommandLineParameterParserStrings.StdinNotRedirected, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-command", "-", "abbra")]
        [InlineData("-c", "-", "abbra")]
        public static void TestParameter_Command_With_Dash_And_Tail(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.TooManyParametersToCommand, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-command", "-")]
        [InlineData("-c", "-")]
        public static void TestParameter_Command_With_Dash_And_ConsoleInputRedirected(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.TestHookConsoleInputRedirected = true;

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeSuccess, cpp.ExitCode);
            Assert.True(cpp.ExplicitReadCommandsFromStdin);
            Assert.Null(cpp.ErrorMessage);
        }

        [SkippableTheory]
        [InlineData("-windowstyle")]
        [InlineData("-w")]
        public static void TestParameter_WindowsStyle_On_Unix(params string[] commandLine)
        {
            Skip.If(Platform.IsWindows);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.WindowStyleArgumentNotImplemented, cpp.ErrorMessage);
        }

        [SkippableTheory]
        [InlineData("-windowstyle")]
        [InlineData("-w")]
        public static void TestParameter_WindowsStyle_No_Value(params string[] commandLine)
        {
            Skip.IfNot(Platform.IsWindows);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MissingWindowStyleArgument, cpp.ErrorMessage);
        }

        [SkippableTheory]
        [InlineData("-windowstyle", "abbra")]
        [InlineData("-w", "abbra")]
        public static void TestParameter_WindowsStyle_With_Wrong_Value(params string[] commandLine)
        {
            Skip.IfNot(Platform.IsWindows);

            string errorMessage = null;
            try
            {
                ProcessWindowStyle style = (ProcessWindowStyle)LanguagePrimitives.ConvertTo(
                    commandLine[1], typeof(ProcessWindowStyle), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (PSInvalidCastException e)
            {
                errorMessage = e.Message;
            }

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(
                string.Format(CommandLineParameterParserStrings.InvalidWindowStyleArgument, commandLine[1], errorMessage),
                cpp.ErrorMessage);
        }

        [SkippableTheory]
        [InlineData("-windowstyle", "Maximized")]
        [InlineData("-w", "Maximized")]
        public static void TestParameter_WindowsStyle_With_Right_Value(params string[] commandLine)
        {
            Skip.IfNot(Platform.IsWindows);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeSuccess, cpp.ExitCode);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-outputformat")]
        [InlineData("-o")]
        [InlineData("-of")]
        public static void TestParameter_OutputFormat_No_Value(params string[] commandLine)
        {
            var index = CommandLineParameterParserStrings.MissingOutputFormatParameter.IndexOf('.');
            var errorMessage = CommandLineParameterParserStrings.MissingOutputFormatParameter.Substring(0, index);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(errorMessage, cpp.ErrorMessage.Substring(0, index));
        }

        [Theory]
        [InlineData("-outputformat", "abbra")]
        [InlineData("-o", "abbra")]
        [InlineData("-of", "abbra")]
        public static void TestParameter_OutputFormat_With_Wrong_Value(params string[] commandLine)
        {
            var index = CommandLineParameterParserStrings.BadFormatParameterValue.IndexOf('.');
            var errorMessage = CommandLineParameterParserStrings.BadFormatParameterValue.Substring(0, index);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(errorMessage, cpp.ErrorMessage.Substring(0, index));
        }

        [Theory]
        [InlineData("-outputformat", "XML")]
        [InlineData("-o", "XML")]
        [InlineData("-of", "XML")]
        public static void TestParameter_OutputFormat_With_Right_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Equal(Microsoft.PowerShell.Serialization.DataFormat.XML, cpp.OutputFormat);
            Assert.Equal((uint)ConsoleHost.ExitCodeSuccess, cpp.ExitCode);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-inputformat")]
        [InlineData("-inp")]
        [InlineData("-if")]
        public static void TestParameter_InputFormat_No_Value(params string[] commandLine)
        {
            var index = CommandLineParameterParserStrings.MissingInputFormatParameter.IndexOf('.');
            var errorMessage = CommandLineParameterParserStrings.MissingInputFormatParameter.Substring(0, index);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(errorMessage, cpp.ErrorMessage.Substring(0, index));
        }

        [Theory]
        [InlineData("-inputformat", "abbra")]
        [InlineData("-inp", "abbra")]
        [InlineData("-if", "abbra")]
        public static void TestParameter_InputFormat_With_Wrong_Value(params string[] commandLine)
        {
            var index = CommandLineParameterParserStrings.BadFormatParameterValue.IndexOf('.');
            var errorMessage = CommandLineParameterParserStrings.BadFormatParameterValue.Substring(0, index);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(errorMessage, cpp.ErrorMessage.Substring(0, index));
        }

        [Theory]
        [InlineData("-inputformat", "XML")]
        [InlineData("-inp", "XML")]
        [InlineData("-if", "XML")]
        public static void TestParameter_InputFormat_With_Right_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.Equal(commandLine[1], cpp.InputFormat.ToString());
            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Equal(Microsoft.PowerShell.Serialization.DataFormat.XML, cpp.InputFormat);
            Assert.Equal((uint)ConsoleHost.ExitCodeSuccess, cpp.ExitCode);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-executionpolicy")]
        [InlineData("-ex")]
        [InlineData("-ep")]
        public static void TestParameter_ExecutionPolicy_No_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MissingExecutionPolicyParameter, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-executionpolicy", "XML")]
        [InlineData("-ex", "XML")]
        [InlineData("-ep", "XML")]
        public static void TestParameter_ExecutionPolicy_With_Right_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeSuccess, cpp.ExitCode);
            Assert.Equal(commandLine[1], cpp.ExecutionPolicy);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-encodedcommand")]
        [InlineData("-e")]
        [InlineData("-ec")]
        public static void TestParameter_EncodedCommand_No_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MissingCommandParameter, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-encodedcommand", "YQBiAGIAcgBhAA==")] // 'abbra' in Base64 format
        [InlineData("-e", "YQBiAGIAcgBhAA==")]
        [InlineData("-ec", "YQBiAGIAcgBhAA==")]
        public static void TestParameter_EncodedCommand_With_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.False(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal("abbra", cpp.InitialCommand);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-encodedcommand", "-")]
        [InlineData("-e", "-")]
        [InlineData("-ec", "-")]
        public static void TestParameter_EncodedCommand_With_Dash(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.False(cpp.NoPrompt);
            Assert.False(cpp.ExplicitReadCommandsFromStdin);
            Assert.Equal(CommandLineParameterParserStrings.BadCommandValue, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-encodedcommand", "-", "YQBiAGIAcgBhAA==")]
        [InlineData("-e", "-", "YQBiAGIAcgBhAA==")]
        [InlineData("-ec", "-", "YQBiAGIAcgBhAA==")]
        public static void TestParameter_EncodedCommand_With_Dash_And_Tail(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.BadCommandValue, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-encodedcommand", "-")]
        [InlineData("-e", "-")]
        [InlineData("-ec", "-")]
        public static void TestParameter_EncodedCommand_With_Dash_And_ConsoleInputRedirected(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.TestHookConsoleInputRedirected = true;

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.False(cpp.ExplicitReadCommandsFromStdin);
            Assert.Equal(CommandLineParameterParserStrings.BadCommandValue, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-encodedarguments")]
        [InlineData("-encodeda")]
        [InlineData("-ea")]
        public static void TestParameter_EncodedArguments_No_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MissingArgsValue, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-encodedarguments", "abbra")]
        [InlineData("-encodeda", "abbra")]
        [InlineData("-ea", "abbra")]
        public static void TestParameter_EncodedArguments_With_Wrong_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.True(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.BadArgsValue, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-encodedarguments", "PABPAGIAagBzACAAVgBlAHIAcwBpAG8AbgA9ACIAMQAuADEALgAwAC4AMQAiACAAeABtAGwAbgBzAD0AIgBoAHQAdABwADoALwAvAHMAYwBoAGUAbQBhAHMALgBtAGkAYwByAG8AcwBvAGYAdAAuAGMAbwBtAC8AcABvAHcAZQByAHMAaABlAGwAbAAvADIAMAAwADQALwAwADQAIgA+AA0ACgAgACAAPABPAGIAagAgAFIAZQBmAEkAZAA9ACIAMAAiAD4ADQAKACAAIAAgACAAPABUAE4AIABSAGUAZgBJAGQAPQAiADAAIgA+AA0ACgAgACAAIAAgACAAIAA8AFQAPgBTAHkAcwB0AGUAbQAuAEMAbwBsAGwAZQBjAHQAaQBvAG4AcwAuAEEAcgByAGEAeQBMAGkAcwB0ADwALwBUAD4ADQAKACAAIAAgACAAIAAgADwAVAA+AFMAeQBzAHQAZQBtAC4ATwBiAGoAZQBjAHQAPAAvAFQAPgANAAoAIAAgACAAIAA8AC8AVABOAD4ADQAKACAAIAAgACAAPABMAFMAVAA+AA0ACgAgACAAIAAgACAAIAA8AFMAPgAtAGEAYgBiAHIAYQA8AC8AUwA+AA0ACgAgACAAIAAgADwALwBMAFMAVAA+AA0ACgAgACAAPAAvAE8AYgBqAD4ADQAKADwALwBPAGIAagBzAD4A")] // '-abbra' in Base64 format
        [InlineData("-encodeda", "PABPAGIAagBzACAAVgBlAHIAcwBpAG8AbgA9ACIAMQAuADEALgAwAC4AMQAiACAAeABtAGwAbgBzAD0AIgBoAHQAdABwADoALwAvAHMAYwBoAGUAbQBhAHMALgBtAGkAYwByAG8AcwBvAGYAdAAuAGMAbwBtAC8AcABvAHcAZQByAHMAaABlAGwAbAAvADIAMAAwADQALwAwADQAIgA+AA0ACgAgACAAPABPAGIAagAgAFIAZQBmAEkAZAA9ACIAMAAiAD4ADQAKACAAIAAgACAAPABUAE4AIABSAGUAZgBJAGQAPQAiADAAIgA+AA0ACgAgACAAIAAgACAAIAA8AFQAPgBTAHkAcwB0AGUAbQAuAEMAbwBsAGwAZQBjAHQAaQBvAG4AcwAuAEEAcgByAGEAeQBMAGkAcwB0ADwALwBUAD4ADQAKACAAIAAgACAAIAAgADwAVAA+AFMAeQBzAHQAZQBtAC4ATwBiAGoAZQBjAHQAPAAvAFQAPgANAAoAIAAgACAAIAA8AC8AVABOAD4ADQAKACAAIAAgACAAPABMAFMAVAA+AA0ACgAgACAAIAAgACAAIAA8AFMAPgAtAGEAYgBiAHIAYQA8AC8AUwA+AA0ACgAgACAAIAAgADwALwBMAFMAVAA+AA0ACgAgACAAPAAvAE8AYgBqAD4ADQAKADwALwBPAGIAagBzAD4A")]
        [InlineData("-ea", "PABPAGIAagBzACAAVgBlAHIAcwBpAG8AbgA9ACIAMQAuADEALgAwAC4AMQAiACAAeABtAGwAbgBzAD0AIgBoAHQAdABwADoALwAvAHMAYwBoAGUAbQBhAHMALgBtAGkAYwByAG8AcwBvAGYAdAAuAGMAbwBtAC8AcABvAHcAZQByAHMAaABlAGwAbAAvADIAMAAwADQALwAwADQAIgA+AA0ACgAgACAAPABPAGIAagAgAFIAZQBmAEkAZAA9ACIAMAAiAD4ADQAKACAAIAAgACAAPABUAE4AIABSAGUAZgBJAGQAPQAiADAAIgA+AA0ACgAgACAAIAAgACAAIAA8AFQAPgBTAHkAcwB0AGUAbQAuAEMAbwBsAGwAZQBjAHQAaQBvAG4AcwAuAEEAcgByAGEAeQBMAGkAcwB0ADwALwBUAD4ADQAKACAAIAAgACAAIAAgADwAVAA+AFMAeQBzAHQAZQBtAC4ATwBiAGoAZQBjAHQAPAAvAFQAPgANAAoAIAAgACAAIAA8AC8AVABOAD4ADQAKACAAIAAgACAAPABMAFMAVAA+AA0ACgAgACAAIAAgACAAIAA8AFMAPgAtAGEAYgBiAHIAYQA8AC8AUwA+AA0ACgAgACAAIAAgADwALwBMAFMAVAA+AA0ACgAgACAAPAAvAE8AYgBqAD4ADQAKADwALwBPAGIAagBzAD4A")]
        public static void TestParameter_EncodedArguments_With_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.Null(cpp.ErrorMessage);
            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Equal("-abbra", cpp.Args[0].Value);
        }

        [Theory]
        [InlineData("-settingsfile")]
        [InlineData("-settings")]
        public static void TestParameter_SettingsFile_No_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MissingSettingsFileArgument, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-settingsfile", "noexistfilename")]
        [InlineData("-settings", "noexistfilename")]
        public static void TestParameter_SettingsFile_Not_Exists(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(
                string.Format(CommandLineParameterParserStrings.SettingsFileNotExists, Path.GetFullPath("noexistfilename")),
                cpp.ErrorMessage);
        }

        public class TestDataSettingsFile : IEnumerable<object[]>
        {
            private string _fileName = Path.GetTempFileName();

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "-settingsfile", _fileName };
                yield return new object[] { "-settings", _fileName };
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(TestDataSettingsFile))]
        public static void TestParameter_SettingsFile_Exists(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeSuccess, cpp.ExitCode);
            Assert.Null(cpp.ErrorMessage);
            Assert.Equal(commandLine[1], cpp.SettingsFile);
        }

        [SkippableTheory]
        [InlineData("-sta")]
        public static void TestParameter_STA_Not_IsWindowsDesktop(params string[] commandLine)
        {
            Skip.If(Platform.IsWindowsDesktop);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.STANotImplemented, cpp.ErrorMessage);
        }

        [SkippableTheory]
        [InlineData("-mta", "-sta")]
        public static void TestParameter_STA_And_MTA_Mutually_Exclusive(params string[] commandLine)
        {
            Skip.IfNot(Platform.IsWindows);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MtaStaMutuallyExclusive, cpp.ErrorMessage);
        }

        [SkippableTheory]
        [InlineData("-sta")]
        public static void TestParameter_STA(params string[] commandLine)
        {
            Skip.IfNot(Platform.IsWindows);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.True(cpp.StaMode);
            Assert.Null(cpp.ErrorMessage);
        }

        [SkippableTheory]
        [InlineData("-mta")]
        public static void TestParameter_MTA_Not_IsWindowsDesktop(params string[] commandLine)
        {
            Skip.If(Platform.IsWindowsDesktop);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MTANotImplemented, cpp.ErrorMessage);
        }

        [SkippableTheory]
        [InlineData("-sta", "-mta")]
        public static void TestParameter_MTA_And_STA_Mutually_Exclusive(params string[] commandLine)
        {
            Skip.IfNot(Platform.IsWindows);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MtaStaMutuallyExclusive, cpp.ErrorMessage);
        }

        [SkippableTheory]
        [InlineData("-mta")]
        public static void TestParameter_MTA(params string[] commandLine)
        {
            Skip.IfNot(Platform.IsWindows);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.False(cpp.StaMode);
            Assert.Null(cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-workingdirectory")]
        [InlineData("-wo")]
        [InlineData("-wd")]
        public static void TestParameter_WorkingDirectory_No_Value(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.True(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.Equal((uint)ConsoleHost.ExitCodeBadCommandLineParameter, cpp.ExitCode);
            Assert.Equal(CommandLineParameterParserStrings.MissingWorkingDirectoryArgument, cpp.ErrorMessage);
        }

        [Theory]
        [InlineData("-workingdirectory", "dirname")]
        [InlineData("-wo", "dirname")]
        [InlineData("-wd", "dirname")]
        public static void TestParameter_WorkingDirectory(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Equal(commandLine[1], cpp.WorkingDirectory);
            Assert.Null(cpp.ErrorMessage);
        }

        [SkippableTheory]
        [InlineData("-workingdirectory", "dirname", "-removeworkingdirectorytrailingcharacter")]
        [InlineData("-wo", "dirname", "-removeworkingdirectorytrailingcharacter")]
        [InlineData("-wd", "dirname", "-removeworkingdirectorytrailingcharacter")]
        public static void TestParameter_WorkingDirectory_RemoveTrailingCharacter(params string[] commandLine)
        {
            Skip.IfNot(Platform.IsWindows);

            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.True(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.True(cpp.ShowBanner);
            Assert.Equal(commandLine[1].Remove(commandLine[1].Length - 1), cpp.WorkingDirectory);
            Assert.Null(cpp.ErrorMessage);
        }

        public class TestDataLastFile : IEnumerable<object[]>
        {
            private string _fileName = Path.GetTempFileName();

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "-noprofile", _fileName };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(TestDataLastFile))]
        public static void TestParameter_LastParameterIsFileName_Exist(params string[] commandLine)
        {
            var cpp = new CommandLineParameterParser();

            cpp.Parse(commandLine);

            Assert.False(cpp.AbortStartup);
            Assert.False(cpp.NoExit);
            Assert.False(cpp.ShowShortHelp);
            Assert.False(cpp.ShowBanner);
            Assert.True(cpp.StaMode);
            Assert.Equal(CommandLineParameterParser.NormalizeFilePath(commandLine[commandLine.Length - 1]), cpp.File);
            Assert.Null(cpp.ErrorMessage);
        }
    }
}
