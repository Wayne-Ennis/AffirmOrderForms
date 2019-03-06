function Get-AffectedFiles() {
    <#
	.SYNOPSIS  
		High level Desiption of the Function
    
	.DESCRIPTION  
		The DESCRIPTION For the Function
    
	.NOTES  
		File Name   : File Name  
		Author      : Author Name
		Requires    : PowerShell V5 
    
	.EXAMPLE  
		PS C:\Powershell> Test-Function -TargetFolder C:\Path\To\Some\Folder
    
	.PARAMETER TargetFolder
		String that needs to be encrypted.
    
	.INPUTS    
		None
    
	.OUTPUTS
		Hashed value of the specified parameter.
	#> 
    [CmdletBinding()]
    Param (
        [Parameter(Mandatory)]
        [ValidateScript( { Test-Path -Path $_ -PathType Container })]
        [string]$TargetFolder
    )
    Process {  

        try {

            #Functuonality goes here
               
        }
        catch {
            Write-Host "Caught Exception in Script "
            Write-Warning $_.Exception.Message
            #return Non-Zero if Necessary
        }
    }
}

Export-ModuleMember Get-AffectedFiles
function Convert-ParamsToCommitMessage() {
    <#
	.SYNOPSIS  
		High level Desiption of the Function
    
	.DESCRIPTION  
		The DESCRIPTION For the Function
    
	.NOTES  
		File Name   : File Name  
		Author      : Author Name
		Requires    : PowerShell V5 
    
	.EXAMPLE  
		PS C:\Powershell> Test-Function -TargetFolder C:\Path\To\Some\Folder
    
	.PARAMETER TargetFolder
		String that needs to be encrypted.
    
	.INPUTS    
		None
    
	.OUTPUTS
		Hashed value of the specified parameter.
	#> 
    [CmdletBinding()]
    Param (
        [Parameter(Mandatory)]
        [hashtable]$ParamTable
    )
    Process {  

        try {
            $msg = ""
			

            foreach ($key in $ParamTable.Keys) {
                Write-Host "Processing Parameter $($key)"
                Write-Host "With Value: $($ParamTable[$key])"
				
                $msg = "$($msg){0}{0}$($key):`r`n$($ParamTable[$key])" -f "`r`n"
				
            }
            #Functuonality goes here
            return $msg
        }
        catch {
            Write-Host "Caught Exception in Script "
            Write-Warning $_.Exception.Message
            #return Non-Zero if Necessary
        }
    }
}
Export-ModuleMember Convert-ParamsToCommitMessage
function Format-CommitMessage() {
    <#
	.SYNOPSIS  
		High level Desiption of the Function
    
	.DESCRIPTION  
		The DESCRIPTION For the Function
    
	.NOTES  
		File Name   : File Name  
		Author      : Author Name
		Requires    : PowerShell V5 
    
	.EXAMPLE  
		PS C:\Powershell> Test-Function -TargetFolder C:\Path\To\Some\Folder
    
	.PARAMETER TargetFolder
		String that needs to be encrypted.
    
	.INPUTS    
		None
    
	.OUTPUTS
		Hashed value of the specified parameter.
	#> 
    [CmdletBinding()]
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
        [string]$AffectedFiles
    )
    Process {  

        try {
 
            $msg = @"
Integration Build-{0}

Build Url:
{1}

Story Id:
{2}

Nuget Packages:
{3}

Affected Files:
{4}

"@
            $msg = $msg -f $JobNumber, $ParentJob, $BuildUrl, $NugetPackages, $AffectedFiles
            return $msg
               
        }
        catch {
            Write-Host "Caught Exception in Script "
            Write-Warning $_.Exception.Message
            throw
        }
    }
}

Export-ModuleMember Format-CommitMessage