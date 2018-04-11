// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Xunit;
using System;
using System.Management.Automation.Language;

namespace PSTests.Parallel
{
    public static class PSEnumerableBinderTests
    {
        [Fact]
        public static void TestIsStaticTypePossiblyEnumerable()
        {
            // It just needs an arbitrary type
            Assert.False(PSEnumerableBinder.IsStaticTypePossiblyEnumerable(42.GetType()));
        }
    }
}
