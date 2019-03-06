Function Get-ArtifactoryBuildInfo {
    Param (
        [Parameter(Mandatory)]
        [string]$ArtifactoryUrl,
        [Parameter(Mandatory)]
        [string]$ArtifactoryUser,
        [Parameter(Mandatory)]
        [string]$ArtifactoryPw  
    )

    Begin {
        Write-Host 'Entering Method: Get-ArtifactoryBuildInfo'
        Write-Host "Get-ArtifactoryBuildInfo| Artifactory Url: $($ArtifactoryUrl)"
    }

    Process {
        Try {
            
            $webClient = New-Object System.Net.WebClient
            $webClient.Credentials = New-Object System.Net.NetworkCredential($ArtifactoryUser, $ArtifactoryPw)

            Write-Host "Making Request to Artifactory Url"
            $response = $webClient.DownloadString($ArtifactoryUrl) | ConvertFrom-Json

            return $response
        }

        Catch {
            Write-Error "Error: $($_.Exception)"
            throw
        }
    }

    End {
        If ($?) {
            Write-Host 'Completed Successfully.'
            Write-Host ' '
        }
    }
}
Export-ModuleMember Get-ArtifactoryBuildInfo
Function Get-PackagesFromBuildInfo {
    Param (
        [Parameter(Mandatory)]
        $BuildInfo
    )

    Begin {
        Write-Host 'Entering Method: Get-PackagesFromBuildInfo'
        Write-Host "Get-PackagesFromBuildInfo| With Parameter Build Info"
    }

    Process {
        Try {
            #Retrieving package names
            Write-Host "Get-PackagesFromBuildInfo | Retrieving Package Names from BuildInfo"
            $artifacts = $BuildInfo.buildInfo.modules.artifacts | ? { $_.name -cnotmatch ".symbols" } | sort name | % {$_.name}

            Write-Host "Found $($artifacts.Count) Packages to Install"
            return $artifacts
        }

        Catch {
            Write-Error "Error: $($_.Exception)"
            throw
        }
    }

    End {
        If ($?) {
            Write-Host 'Completed Successfully.'
            Write-Host ' '
        }
    }
}
Export-ModuleMember Get-PackagesFromBuildInfo
Function Update-NugetPackage {
    Param (
        [Parameter(Mandatory)]
        [ValidateScript( { Test-Path -Path $_ })]
        [string]$SolutionPath,
        [Parameter(Mandatory)]
        [string]$PackageId,
        [Parameter(Mandatory)]
        [string]$PackageVersion

    )

    Begin {
        Write-Host "Setting Nuget Alias"
        Set-Alias nuget "$($PSScriptRoot)\..\..\tools\Nuget41\nuget.exe"
    }

    Process {
        Try {

            Write-Host "Updating Solution With Package: $($PackageId) - $($PackageVersion)"
            nuget update $SolutionPath -Id $PackageId -Version $PackageVersion -PreRelease  -FileConflictAction "overwrite" -NonInteractive

            if ($LASTEXITCODE -ne 0) {
                Write-Host "There Was a Problem Updating $($PackageId) - $($PackageVersion)"
                exit $LASTEXITCODE
            }
          
        }

        Catch {
            Write-Error "Error: $($_.Exception)"
            throw
        }
    }

    End {
        If ($?) {
            Write-Host "Solution has been Updated  With Package: $($PackageId) - $($PackageVersion)"
            Write-Host ' '
        }
    }
}
Export-ModuleMember Update-NugetPackage
Function Restore-NugetPackages {
    Param (
        [Parameter(Mandatory)]
        [ValidateScript( { Test-Path -Path $_ })]
        [string]$SolutionPath

    )

    Begin {
        Write-Host "Setting Nuget Alias"
        Set-Alias nuget "$($PSScriptRoot)\..\..\tools\Nuget41\nuget.exe"
    }

    Process {
        Try {
      
            Write-Host "Restoring Nuget Packages in Solution"
            nuget restore $SolutionPath -Source "http://artifactory-aff.ipipenet.com/artifactory/api/nuget/affirm-nuget-dev-local;http://nuget.ipipenet.com/nuget;https://api.nuget.org/v3/index.json"
            if ($LASTEXITCODE -ne 0) {
                Write-Host -BackgroundColor Red "There Was a Problem Updating Restoring Nuget Packages"
                exit $LASTEXITCODE
            }

        }

        Catch {
            Write-Error  "Error: $($_.Exception)"
            throw
        }
    }

    End {
        If ($?) {
            Write-Host 'Packages Were Successfully Restored in the Soluton'
            Write-Host ' '
        }
    }
}
Export-ModuleMember Restore-NugetPackages

Function Update-SolutionWithPackages {
    Param (
        [Parameter(Mandatory)]
        $PackageList,
        [Parameter(Mandatory)]
        [ValidateScript( { Test-Path -Path $_ })]
        [string]$SolutionPath

    )

    Begin {
        Write-Host "Ready to Update Solution"

    }

    Process {
        Try {
      
            Write-Host "Packages to Install:"
            Write-Host $PackageList

            
            Foreach ($pkg in $PackageList) {

                $versionRegex = "([^\.]*\d+\.\d+\.\d+.*[^*.nupkg])"

                $versionData = [regex]::matches($pkg, $versionRegex)

                #If there are no matches then you got to drop it
                if ($versionData.Count -lt 1) {
                    Write-Warning "No Matches for this Package: $($pkg)"
                    Continue
                }

                $version = $versionData[0].Value


                $id = $pkg.Replace(".$($version).nupkg", "")


                Write-Host "Restoring Package: $($id)"
                Write-Host "Package Verion: $($version)"
                Update-NugetPackage -PackageId $id -PackageVersion $version -SolutionPath $SolutionPath
            }

        }

        Catch {
            Write-Error "Error: $($_.Exception)"
            Break
        }
    }

    End {
        If ($?) {
            Write-Host 'Completed Successfully.'
            Write-Host ' '
        }
    }
}
Export-ModuleMember Update-SolutionWithPackages

