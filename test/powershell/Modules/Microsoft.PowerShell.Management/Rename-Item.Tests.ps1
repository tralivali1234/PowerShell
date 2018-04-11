# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
Describe "Rename-Item tests" -Tag "CI" {
    BeforeAll {
        $content = "This is content"
        Setup -f originalFile.txt -content "This is content"
        $source = "$TESTDRIVE/originalFile.txt"
        $target = "$TESTDRIVE/ItemWhichHasBeenRenamed.txt"
    }
    It "Rename-Item will rename a file" {
        Rename-Item $source $target
        test-path $source | Should -BeFalse
        test-path $target | Should -BeTrue
        "$target" | Should -FileContentMatchExactly "This is content"
    }
}
