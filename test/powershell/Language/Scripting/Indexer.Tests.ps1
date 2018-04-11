# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
Describe 'Tests for indexers' -Tags "CI" {
    It 'Indexer in dictionary' {

        $hashtable = @{ "Hello"="There" }
        $hashtable["Hello"] | Should -BeExactly "There"
    }

    It 'Accessing a Indexed property of a dictionary that does not exist should return $NULL' {
        $hashtable = @{ "Hello"="There" }
        $hashtable["Hello There"] | Should -BeNullOrEmpty
        }

    It 'Wmi object implements an indexer' -Skip:$IsCoreCLR  {

        $service = Get-WmiObject -List -Amended Win32_Service

        $service.Properties["DisplayName"].Name | Should -BeExactly 'DisplayName'
    }

    It 'Accessing a Indexed property of a wmi object that does not exist should return $NULL' -skip:$IsCoreCLR {

        $service = Get-WmiObject -List -Amended Win32_Service
        $service.Properties["Hello There"] | Should -BeNullOrEmpty
    }
}
