# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# get a random string of characters a-z and A-Z
function Get-RandomString
{
    param ( [int]$Length = 8 )
    $chars = .{ ([int][char]'a')..([int][char]'z');([int][char]'A')..([int][char]'Z') }
    ([char[]]($chars | Get-Random -Count $Length)) -join ""
}

# get a random string which is not the name of an existing provider
function Get-NonExistantProviderName
{
   param ( [int]$Length = 8 )
   do {
       $providerName = Get-RandomString -Length $Length
   } until ( $null -eq (Get-PSProvider -PSProvider $providername -ErrorAction SilentlyContinue) )
   $providerName
}

# get a random string which is not the name of an existing drive
function Get-NonExistantDriveName
{
    param ( [int]$Length = 8 )
    do {
        $driveName = Get-RandomString -Length $Length
    } until ( $null -eq (Get-PSDrive $driveName -ErrorAction SilentlyContinue) )
    $drivename
}

# get a random string which is not the name of an existing function
function Get-NonExistantFunctionName
{
    param ( [int]$Length = 8 )
    do {
        $functionName = Get-RandomString -Length $Length
    } until ( (Test-Path -Path function:$functionName) -eq $false )
    $functionName
}

Describe "Clear-Content cmdlet tests" -Tags "CI" {
  BeforeAll {
    $file1 = "file1.txt"
    $file2 = "file2.txt"
    $file3 = "file3.txt"
    $content1 = "This is content"
    $content2 = "This is content for alternate stream tests"
    Setup -File "$file1"
    Setup -File "$file2" -Content $content1
    Setup -File "$file3" -Content $content2
    $streamContent = "content for alternate stream"
    $streamName = "altStream1"
  }

  Context "Clear-Content should actually clear content" {
    It "should clear-Content of TestDrive:\$file1" {
      Set-Content -Path TestDrive:\$file1 -Value "ExpectedContent" -PassThru | Should -BeExactly "ExpectedContent"
      Clear-Content -Path TestDrive:\$file1
    }

    It "shouldn't get any content from TestDrive:\$file1" {
      $result = Get-Content -Path TestDrive:\$file1
      $result | Should -BeNullOrEmpty
    }

    # we could suppress the WhatIf output here if we use the testhost, but it's not necessary
    It "The filesystem provider supports should process" -skip:(!$IsWindows) {
      Clear-Content -Path TestDrive:\$file2 -WhatIf
      "TestDrive:\$file2" | Should -FileContentMatch "This is content"
    }

    It "The filesystem provider should support ShouldProcess (reference ProviderSupportsShouldProcess member)" {
      $cci = ((Get-Command -Name Clear-Content).ImplementingType)::new()
      $cci.SupportsShouldProcess | Should -BeTrue
    }

    It "Alternate streams should be cleared with clear-content" -skip:(!$IsWindows) {
      # make sure that the content is correct
      # this is here rather than BeforeAll because only windows can write to an alternate stream
      Set-Content -Path "TestDrive:/$file3" -Stream $streamName -Value $streamContent
      Get-Content -Path "TestDrive:/$file3" | Should -BeExactly $content2
      Get-Content -Path "TestDrive:/$file3" -Stream $streamName | Should -BeExactly $streamContent
      Clear-Content -Path "TestDrive:/$file3" -Stream $streamName
      Get-Content -Path "TestDrive:/$file3" | Should -BeExactly $content2
      Get-Content -Path "TestDrive:/$file3" -Stream $streamName | Should -BeNullOrEmpty
    }

    It "the '-Stream' dynamic parameter is visible to get-command in the filesystem" -Skip:(!$IsWindows) {
      try {
        Push-Location -Path TestDrive:
        (Get-Command Clear-Content -Stream foo).parameters.keys -eq "stream" | Should -Be "stream"
      }
      finally {
        Pop-Location
      }
    }

    It "the '-Stream' dynamic parameter should not be visible to get-command in the function provider" {
      try {
        Push-Location -Path function:
        Get-Command Clear-Content -Stream $streamName
        throw "ExpectedExceptionNotDelivered"
      }
      catch {
        $_.FullyQualifiedErrorId | Should -Be "NamedParameterNotFound,Microsoft.PowerShell.Commands.GetCommandCommand"
      }
      finally {
        Pop-Location
      }
    }
  }

  Context "Proper errors should be delivered when bad locations are specified" {
    It "should throw `"Cannot bind argument to parameter 'Path'`" when -Path is `$null" {
      try {
        Clear-Content -Path $null -ErrorAction Stop
        throw "expected exception was not delivered"
      }
      catch {
        $_.FullyQualifiedErrorId | Should -Be "ParameterArgumentValidationErrorNullNotAllowed,Microsoft.PowerShell.Commands.ClearContentCommand"
      }
    }

    #[BugId(BugDatabase.WindowsOutOfBandReleases, 903880)]
    It "should throw `"Cannot bind argument to parameter 'Path'`" when -Path is `$()" {
      try {
        Clear-Content -Path $() -ErrorAction Stop
        throw "expected exception was not delivered"
      }
      catch {
        $_.FullyQualifiedErrorId | Should -Be "ParameterArgumentValidationErrorNullNotAllowed,Microsoft.PowerShell.Commands.ClearContentCommand"
      }
    }

    #[DRT][BugId(BugDatabase.WindowsOutOfBandReleases, 906022)]
    It "should throw 'PSNotSupportedException' when you clear-content to an unsupported provider" {
      $functionName = Get-NonExistantFunctionName
      $null = New-Item -Path function:$functionName -Value { 1 }
      try {
        Clear-Content -Path function:$functionName -ErrorAction Stop
        throw "Expected exception was not thrown"
      }
      catch {
        $_.FullyQualifiedErrorId | Should -Be "NotSupported,Microsoft.PowerShell.Commands.ClearContentCommand"
      }
    }

    It "should throw FileNotFound error when referencing a non-existant file" {
      try {
        $badFile = "TestDrive:/badfilename.txt"
        Clear-Content -Path $badFile -ErrorAction Stop
        throw "ExpectedExceptionNotDelivered"
      }
      catch {
        $_.FullyQualifiedErrorId | Should -Be "PathNotFound,Microsoft.PowerShell.Commands.ClearContentCommand"
      }
    }

    It "should throw DriveNotFound error when referencing a non-existant drive" {
       try {
         $badDrive = "{0}:/file.txt" -f (Get-NonExistantDriveName)
         Clear-Content -Path $badDrive -ErrorAction Stop
         throw "ExpectedExceptionNotDelivered"
       }
       catch {
         $_.FullyQualifiedErrorId | Should -Be "DriveNotFound,Microsoft.PowerShell.Commands.ClearContentCommand"
       }
    }

    # we'll use a provider qualified path to produce this error
    It "should throw ProviderNotFound error when referencing a non-existant provider" {
       try {
         $badProviderPath = "{0}::C:/file.txt" -f (Get-NonExistantProviderName)
         Clear-Content -Path $badProviderPath -ErrorAction Stop
         throw "ExpectedExceptionNotDelivered"
       }
       catch {
         $_.FullyQualifiedErrorId | Should -Be "ProviderNotFound,Microsoft.PowerShell.Commands.ClearContentCommand"
       }
    }
  }
}
