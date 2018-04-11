// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Xunit;
using System;
using System.Management.Automation;

namespace PSTests.Parallel
{
    public static class SecuritySupportTests
    {
        [Fact]
        public static void TestScanContent()
        {
            Assert.Equal(AmsiUtils.AmsiNativeMethods.AMSI_RESULT.AMSI_RESULT_NOT_DETECTED, AmsiUtils.ScanContent("", ""));
        }

        [Fact]
        public static void TestCloseSession()
        {
            AmsiUtils.CloseSession();
        }

        [Fact]
        public static void TestUninitialize()
        {
            AmsiUtils.Uninitialize();
        }
    }
}
