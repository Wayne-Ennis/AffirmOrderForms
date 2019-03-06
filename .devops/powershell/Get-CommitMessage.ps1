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
    [string]$JobNumber,
    [Parameter(Mandatory)]
    [string]$BuildUrl,
    [Parameter(Mandatory)]
    [string]$ParentJob,
    [Parameter(Mandatory)]
    [string]$NugetPackages,
    [Parameter(Mandatory)]
    [string]$AffectedFiles,      
    [string]$ProjectName,
    [string]$StoryId,
    [string]$GitBranch,
    [string]$GitBaseBranch
)

#---------------------------------------------------------[Initialisations]--------------------------------------------------------

#Set Error Action to Silently Continue
$ErrorActionPreference = 'Stop'

#Import Modules & Snap-ins
Import-Module "$($PSScriptRoot)\modules\Affirm-Git.psm1"
#----------------------------------------------------------[Declarations]----------------------------------------------------------

#Any Global Declarations go here

#-----------------------------------------------------------[Functions]------------------------------------------------------------


#-----------------------------------------------------------[Execution]------------------------------------------------------------

#Script Execution goes here

Try {
    $paramHashTable = @{}
    $msg = Format-CommitMessage -JobNumber $JobNumber -ParentJob $ParentJob -BuildUrl $BuildUrl -NugetPackages $NugetPackages -AffectedFiles $AffectedFiles
    
    if ($ProjectName) {
        $paramHashTable.Add("Project Name", $ProjectName)
    }
    if ($StoryId) {
        $paramHashTable.Add("Story Id", $StoryId)
    } 
    if ($GitBranch) {
        $paramHashTable.Add("Git Branch", $GitBranch)
    }     
    if ($GitBaseBranch) {
        $paramHashTable.Add("Base Branch", $GitBaseBranch)
    } 
    if ($BuildUrl) {
        $paramHashTable.Add("Build Url", $BuildUrl)
    }
    
    if ($JobNumber) {
        $paramHashTable.Add("Job Number", $JobNumber)
    }

    if ($ParentJob) {
        $paramHashTable.Add("Parent Job", $ParentJob)
    }

    if ($NugetPackages) {
        $paramHashTable.Add("Nuget Packages", $NugetPackages)
    }
    if ($AffectedFiles) {
        $paramHashTable.Add("Affected Files", $AffectedFiles)
    }


    return $msg
}

Catch {
    Write-Host -BackgroundColor Red "Error: $($_.Exception)"
    exit 1
}
