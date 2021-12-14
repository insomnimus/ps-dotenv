[bool]$Enabled = $true
[bool]$IgnoreExportPrefix = $true
[bool]$SkipErrors = $false
[string[]]$Names = @(".env")

class DotenvConfig {
	DotenvConfig() {
		$_fields = @(
			"Names"
			"Enabled"
			"SkipErrors"
			"IgnoreExportPrefix"
			"LoggingPreference"
		)
		foreach($_f in $_fields) {
			$this | add-member CodeProperty `
				-Name $_f `
				-Value ([DotenvConfig].DeclaredMethods | where-object { $_.name -eq "Get$_f" }) `
				-SecondValue ([DotenvConfig].DeclaredMethods | where-object { $_.name -eq "Set$_f" })
		}
	}

	hidden static [string[]] GetNames([PSObject]$_) {
		return $script:names
	}

	hidden static [void] SetNames([PSObject]$_, [string[]]$val) {
		if($val -eq $null) {
			$val = @()
		}
		# deduplicate var
		$val = script::dedup $val
		if(!($script:names.equals($val))) {
			$script:names = $val
			script:Update-Dotenv -force
		}
	}

	hidden static [bool]GetEnabled([PSObject]$_) {
		return $script:enabled
	}

	hidden static [void] setEnabled([PSObject]$_, [bool]$val) {
		if($script:Enabled -eq $val) {
			return
		}
		if($val) {
			script:Enable-Dotenv
		} else {
			script:Disable-Dotenv
		}
	}

	hidden static [bool] GetSkipErrors([PSObject]$_) {
		return $script:SkipErrors
	}

	hidden static [void] SetSkipErrors([PSObject]$_, [bool]$val) {
		$script:SkipErrors = $val
	}

	hidden static [bool] GetIgnoreExportPrefix([PSObject]$_) {
		return $script:IgnoreExportPrefix
	}

	hidden static [void] SetIgnoreExportPrefix([PSObject]$_, [bool]$val) {
		$script:IgnoreExportPrefix = $val
	}

	hidden static [LoggingPreference] GetLoggingPreference([PSObject]$_) {
		return $script:Log
	}

	hidden static [void] SetLoggingPreference([PSObject]$_, [LoggingPreference]$val) {
		$script:Log = $val
	}
}
