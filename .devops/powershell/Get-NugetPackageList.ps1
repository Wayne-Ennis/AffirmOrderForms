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
  
  <Example goes here. Repeat this attribute for more than one example>
#>

#---------------------------------------------------------[Script Parameters]------------------------------------------------------

Param (
    [Parameter(Mandatory)]
    [string]$ArtifactoryUrl,
    [Parameter(Mandatory)]
    [string]$ArtifactoryUser,
    [Parameter(Mandatory)]
    [string]$ArtifactoryPw  
)

#---------------------------------------------------------[Initialisations]--------------------------------------------------------

#Set Error Action to Silently Continue
$ErrorActionPreference = 'SilentlyContinue'

#Import Modules & Snap-ins

#----------------------------------------------------------[Declarations]----------------------------------------------------------

#Any Global Declarations go here

#-----------------------------------------------------------[Functions]------------------------------------------------------------

#-----------------------------------------------------------[Execution]------------------------------------------------------------

#Script Execution goes here


try {
    Import-Module "$($PSScriptRoot)\modules\Update-Solution.psm1"

    $buildInfo = Get-ArtifactoryBuildInfo -ArtifactoryUrl $ArtifactoryUrl -ArtifactoryUser $ArtifactoryUser -ArtifactoryPw $ArtifactoryPw

    $packageList = Get-PackagesFromBuildInfo -BuildInfo $buildInfo
    
    return $packageList

}
catch {
    Write-Error  "Error: $($_.Exception)"
    exit 1
}