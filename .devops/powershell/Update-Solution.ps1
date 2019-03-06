#requires -version 4
<#
.SYNOPSIS
  <Overview of script>

.DESCRIPTION
  <Brief description of script>

.PARAMETER <Parameter_Name>
  <Brief description of parameter input required. Repeat this attribute if required>

.INPUTS
  <Inputs if any, otherwise state None>

.OUTPUTS
  <Outputs if any, otherwise state None>

.NOTES
  Version:        1.0
  Author:         <Name>
  Creation Date:  <Date>
  Purpose/Change: Initial script development

.EXAMPLE
  <Example explanation goes here>
  
  Update-Solution.ps1 -ArtifactoryUrl https://artifactory-aff.ipipenet.com/artifactory/api/build/Affirm%20::%20affirm-library/752 -SolutionPath C:\Affirm\affirm-integration\GENR.RND
#>

#---------------------------------------------------------[Script Parameters]------------------------------------------------------

Param (
    [Parameter(Mandatory)]
    [string]$ArtifactoryUrl,
    [Parameter(Mandatory)]
    [ValidateScript( { Test-Path -Path $_ })]
    [string]$SolutionPath,
    [Parameter(Mandatory)]
    [string]$ArtifactoryUser,
    [Parameter(Mandatory)]
    [string]$ArtifactoryPw    
)

#---------------------------------------------------------[Initialisations]--------------------------------------------------------

#Set Error Action to Silently Continue
$ErrorActionPreference = 'Stop'

#Import Modules & Snap-ins
Import-Module "$($PSScriptRoot)\modules\Update-Solution.psm1"
#----------------------------------------------------------[Declarations]----------------------------------------------------------

#Any Global Declarations go here

#-----------------------------------------------------------[Functions]------------------------------------------------------------

#Script Execution goes here

#Takes Artifactory URL

try {
    $buildInfo = Get-ArtifactoryBuildInfo -ArtifactoryUrl $ArtifactoryUrl -ArtifactoryUser $ArtifactoryUser -ArtifactoryPw $ArtifactoryPw

    $packageList = Get-PackagesFromBuildInfo -BuildInfo $buildInfo

    if ($packageList.Count -lt 1) {
        Write-Host "We Found No Packages to Install"
        return 0
  
    }

    #Must Restore the solution First

    Restore-NugetPackages -SolutionPath $SolutionPath

    Update-SolutionWithPackages -PackageList $packageList -SolutionPath $SolutionPath

    #Update the Solution with the packages
    exit 0
}
catch {

    Write-Error  "Error: $($_.Exception)"
    exit 1
  
}

