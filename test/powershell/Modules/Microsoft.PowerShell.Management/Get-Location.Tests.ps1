# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
Describe "Get-Location" -Tags "CI" {
    $currentDirectory=[System.IO.Directory]::GetCurrentDirectory()
    BeforeEach {
	pushd $currentDirectory
    }

    AfterEach {
	popd
    }

    It "Should list the output of the current working directory" {

	(Get-Location).Path | Should -BeExactly $currentDirectory
    }

    It "Should do exactly the same thing as its alias" {
	(pwd).Path | Should -BeExactly (Get-Location).Path
    }
}
