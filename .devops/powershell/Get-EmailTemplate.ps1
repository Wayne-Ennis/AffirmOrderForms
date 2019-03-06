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
    [string]$BuildUrl,
    [Parameter(Mandatory)]
    [string]$BuildStatus,
    [Parameter(Mandatory)]
    [string]$ParentJob,    
    [string]$FilesAffected,
    [string]$NugetPackages
)

#---------------------------------------------------------[Initialisations]--------------------------------------------------------

#Set Error Action to Silently Continue
$ErrorActionPreference = 'stop'

#Import Modules & Snap-ins

#----------------------------------------------------------[Declarations]----------------------------------------------------------

#Any Global Declarations go here

#-----------------------------------------------------------[Functions]------------------------------------------------------------

#-----------------------------------------------------------[Execution]------------------------------------------------------------

#Script Execution goes here

try {
  
    #Get the email Template
    Write-Host "Get-EmailTemplate: Loading Email Template"
    $emailTemplate = "$($PSScriptRoot)\..\assets\email-templates\build-email-template-01.html"
    $emailString = Get-Content $emailTemplate -Raw

    # Swap out Values
    Write-Host "Get-EmailTemplate: Swapping in BuildUrl: $($BuildUrl)"
    $emailString = $emailString.Replace("{{**BuildUrl**}}", $BuildUrl)

    Write-Host "Get-EmailTemplate: Swapping in BuildStatus: $($BuildStatus)"
    $emailString = $emailString.Replace("{{**BuildStatus**}}", $BuildStatus)

    Write-Host "Get-EmailTemplate: Swapping in ParentJob: $($ParentJob)"
    $emailString = $emailString.Replace("{{**ParentJob**}}", $ParentJob)
    # Return the output
    Write-Host "Get-EmailTemplate: Swapping in FilesAffected: $($FilesAffected)"
    $emailString = $emailString.Replace("{{**AffectedFiles**}}", $FilesAffected)

    Write-Host "Get-EmailTemplate: Swapping in NugetPackages: $($NugetPackages)"
    $emailString = $emailString.Replace("{{**NugetPackages**}}", $NugetPackages)
    # $emailString | Out-File -Force -FilePath "$($PSScriptRoot)\BuildNotify-SuccessEmail.html"
    return $emailstring
}
catch {
    Write-Error -BackgroundColor Red "Error: $($_.Exception)"
    exit 1
}