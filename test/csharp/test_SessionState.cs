// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Xunit;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Internal.Host;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell;

namespace PSTests.Parallel
{
    public class SessionStateTests
    {
        [SkippableFact]
        public void TestDrives()
        {
            Skip.IfNot(Platform.IsWindows);
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            PSHost hostInterface =  new DefaultHost(currentCulture,currentCulture);
            InitialSessionState iss = InitialSessionState.CreateDefault2();
            AutomationEngine engine = new AutomationEngine(hostInterface, iss);
            ExecutionContext executionContext = new ExecutionContext(engine, hostInterface, iss);
            SessionStateInternal sessionState = new SessionStateInternal(executionContext);
            Collection<PSDriveInfo> drives = sessionState.Drives(null);
            Assert.NotNull(drives);
        }
    }
}
