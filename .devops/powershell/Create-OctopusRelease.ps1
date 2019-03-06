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
    [string]$ProjectName,
    [Parameter(Mandatory)]
    [string]$VersionNumber,
    [Parameter(Mandatory)]
    [string]$ReleaseNotes,
    [Parameter(Mandatory)]
    [string]$OctoServerUrl,
    [Parameter(Mandatory)]
    [string]$OctoApiKey,
    [switch]$Deploy 
)

#---------------------------------------------------------[Initialisations]--------------------------------------------------------

#Set Error Action to Silently Continue
$ErrorActionPreference = 'SilentlyContinue'

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

Function Create-OctoRelease {
    Param (
        [Parameter(Mandatory)]
        [string]$ProjectName,
        [Parameter(Mandatory)]
        [string]$VersionNumber,
        [Parameter(Mandatory)]
        [string]$ReleaseNotes,
        [Parameter(Mandatory)]
        [string]$OctoServerUrl,
        [Parameter(Mandatory)]
        [string]$OctoApiKey,
        [switch]$Deploy    
    )

    Begin {
        Write-Host "Setting Octo.exe to Create a Release"
        set-alias octo "$($PSScriptRoot)\..\tools\octo\Octo.exe"
    }

    Process {

        Try {
    
            Write-Host "Creating Octopus Release for Project: Version: "

            if($Deploy){
              octo create-release --project $ProjectName --version $VersionNumber --packageversion $VersionNumber --server $OctoServerUrl `
              --apiKey $OctoApiKey --releaseNotes $ReleaseNotes --deployto QA_RnD 

            }else {
              octo create-release --project $ProjectName --version $VersionNumber --packageversion $VersionNumber --server $OctoServerUrl `
              --apiKey $OctoApiKey --releaseNotes $ReleaseNotes
            }
             
                            
            if($LASTEXITCODE -ne 0)
                {
                    Write-Host "There Was a Problem Updating"
                    exit $LASTEXITCODE
                }
        }

        Catch {
        Write-Host -BackgroundColor Red "Error: $($_.Exception)"
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
#-----------------------------------------------------------[Execution]------------------------------------------------------------

#Script Execution goes here

try {
    


  Create-OctoRelease -ProjectName $ProjectName -VersionNumber $VersionNumber -ReleaseNotes $ReleaseNotes -OctoServerUrl $OctoServerUrl -OctoApiKey $OctoApiKey
exit 0
}
catch {
    Write-Host -BackgroundColor Red "Error: $($_.Exception)"
    exit 1
}
