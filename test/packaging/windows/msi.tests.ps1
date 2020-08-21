# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

Describe -Name "Windows MSI" -Fixture {
    BeforeAll {
        function Test-Elevated {
            [CmdletBinding()]
            [OutputType([bool])]
            Param()

            # if the current Powershell session was called with administrator privileges,
            # the Administrator Group's well-known SID will show up in the Groups for the current identity.
            # Note that the SID won't show up unless the process is elevated.
            return (([Security.Principal.WindowsIdentity]::GetCurrent()).Groups -contains "S-1-5-32-544")
        }

        function Invoke-Msiexec {
            param(
                [Parameter(ParameterSetName = 'Install', Mandatory)]
                [Switch]$Install,

                [Parameter(ParameterSetName = 'Uninstall', Mandatory)]
                [Switch]$Uninstall,

                [Parameter(Mandatory)]
                [ValidateScript({Test-Path -Path $_})]
                [String]$MsiPath,

                [Parameter(ParameterSetName = 'Install')]
                [HashTable] $Properties

            )
            $action = "$($PSCmdlet.ParameterSetName)ing"
            if ($Install.IsPresent) {
                $switch = '/I'
            } else {
                $switch = '/x'
            }

            $additionalOptions = @()
            if ($Properties) {
                foreach ($key in $Properties.Keys) {
                    $additionalOptions += "$key=$($Properties.$key)"
                }
            }

            $argumentList = "$switch $MsiPath /quiet /l*vx $msiLog $additionalOptions"
            $msiExecProcess = Start-Process msiexec.exe -Wait -ArgumentList $argumentList -NoNewWindow -PassThru
            if ($msiExecProcess.ExitCode -ne 0) {
                $exitCode = $msiExecProcess.ExitCode
                throw "$action MSI failed and returned error code $exitCode."
            }
        }

        $msiX64Path = $env:PsMsiX64Path

        # Get any existing powershell in the path
        $beforePath = @(([System.Environment]::GetEnvironmentVariable('PATH', 'MACHINE')) -split ';' |
                Where-Object {$_ -like '*files\powershell*'})

        $msiLog = Join-Path -Path $TestDrive -ChildPath 'msilog.txt'

        foreach ($pathPart in $beforePath) {
            Write-Warning "Found existing PowerShell path: $pathPart"
        }

        if (!(Test-Elevated)) {
            Write-Warning "Tests must be elevated"
        }
        $uploadedLog = $false
    }
    BeforeEach {
        $error.Clear()
    }
    AfterEach {
        if ($error.Count -ne 0 -and !$uploadedLog) {
            Copy-Item -Path $msiLog -Destination $env:temp -Force
            Write-Verbose "MSI log is at $env:temp\msilog.txt" -Verbose
            $uploadedLog = $true
        }
    }

    Context "Upgrade code" {
        BeforeAll {
            $previewUpgladeCode = '39243d76-adaf-42b1-94fb-16ecf83237c8'
        }

        It "Preview MSI should not be installed before test" -Skip:(!(Test-Elevated)) {
            $result = @(Get-CimInstance -Query "SELECT Value FROM Win32_Property WHERE Property='UpgradeCode' and Value = '{$previewUpgladeCode}'")
            $result.Count | Should -Be 0 -Because 'Query should return nothing if preview x64 is not installed'
        }

        It "MSI should install without error" -Skip:(!(Test-Elevated)) {
            {
                Invoke-MsiExec -Install -MsiPath $msiX64Path -Properties @{ADD_PATH = 1}
            } | Should -Not -Throw
        }

        It "Upgrade code should be correct" -Skip:(!(Test-Elevated)) {
            $result = @(Get-CimInstance -Query "SELECT Value FROM Win32_Property WHERE Property='UpgradeCode' and Value = '{$previewUpgladeCode}'")
            $result.Count | Should -Be 1 -Because 'Query should return 1 result if Upgrade code is for x64 preview'
        }

        It "MSI should uninstall without error" -Skip:(!(Test-Elevated)) {
            {
                Invoke-MsiExec -Uninstall -MsiPath $msiX64Path
            } | Should -Not -Throw
        }
    }

    Context "Add Path disabled" {
        It "MSI should install without error" -Skip:(!(Test-Elevated)) {
            {
                Invoke-MsiExec -Install -MsiPath $msiX64Path -Properties @{ADD_PATH = 0}
            } | Should -Not -Throw
        }

        It "MSI should have not be updated path" -Skip:(!(Test-Elevated)) {
            $psPath = ([System.Environment]::GetEnvironmentVariable('PATH', 'MACHINE')) -split ';' |
                Where-Object {$_ -like '*files\powershell*' -and $_ -notin $beforePath}

            $psPath | Should -BeNullOrEmpty
        }

        It "MSI should uninstall without error" -Skip:(!(Test-Elevated)) {
            {
                Invoke-MsiExec -Uninstall -MsiPath $msiX64Path
            } | Should -Not -Throw
        }
    }

    Context "Add Path enabled" {
        It "MSI should install without error" -Skip:(!(Test-Elevated)) {
            {
                Invoke-MsiExec -Install -MsiPath $msiX64Path -Properties @{ADD_PATH = 1}
            } | Should -Not -Throw
        }

        It "MSI should have updated path" -Skip:(!(Test-Elevated)) {
            $psPath = ([System.Environment]::GetEnvironmentVariable('PATH', 'MACHINE')) -split ';' |
                Where-Object {$_ -like '*files\powershell*\preview*' -and $_ -notin $beforePath}

            $psPath | Should -Not -BeNullOrEmpty
        }

        It "MSI should uninstall without error" -Skip:(!(Test-Elevated)) {
            {
                Invoke-MsiExec -Uninstall -MsiPath $msiX64Path
            } | Should -Not -Throw
        }
    }
}
