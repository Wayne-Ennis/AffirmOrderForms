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
    [string]$PackageList,
    [Parameter(Mandatory)]
    [ValidateScript( { Test-Path -Path $_ })]
    [string]$SolutionPath
)

#---------------------------------------------------------[Initialisations]--------------------------------------------------------

#Set Error Action to Silently Continue
$ErrorActionPreference = 'Stop'

#Import Modules & Snap-ins

#----------------------------------------------------------[Declarations]----------------------------------------------------------

#Any Global Declarations go here

#-----------------------------------------------------------[Functions]------------------------------------------------------------

<#

Function <FunctionName> {
Param ()

Begin {
  Write-Host '<description of what is going on>...'
}

Process {
  Try {
    <code goes here>
  }

  Catch {
    Write-Host -BackgroundColor Red "Error: $($_.Exception)"
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

#>
Import-Module "$($PSScriptRoot)\modules\Update-Solution.psm1"

#Script Execution goes here

#Takes Artifactory URL

try {
  
    $cleanlist = $PackageList -replace "`n", ";" -replace "[`r`0`b`t ]", "" 

    $list = $cleanlist.Split(';');
    Write-Host "List size "$list.Count
    if ($list.Count -lt 1) {
        Write-Host "We Found No Packages to Install"
        return 0

    }
    $list | % {

        Write-Host $_

    }
  
    #Must Restore the solution First
    Write-Host "Restore packages in solution First"
    Restore-NugetPackages -SolutionPath $SolutionPath

    Update-SolutionWithPackages -PackageList $list -SolutionPath $SolutionPath

    #Update the Solution with the packages
    exit 0
}
catch {

    Write-Error  "Error: $($_.Exception)"
    exit 1

}
