class LoggingPreference {
	[bool]$Info = $true
	[bool]$Warning = $false
	[bool]$Error = $true
	[bool]$Debug = $true
}

[LoggingPreference]$Log = [LoggingPreference]@{}

function :info {
	param(
		[parameter(mandatory, position = 0)]
		[string]$msg
	)
	if($log.info) {
		write-information "[Dotenv] $msg"
	}
}

function :error {
	param(
		[parameter(mandatory, position = 0)]
		[string]$msg
	)
	if($log.error) {
		write-error "[Dotenv] $msg"
	}
}

function :warn {
	param(
		[parameter(mandatory, position = 0)]
		[string]$msg
	)
	if($log.warning) {
		write-warning "[Dotenv] $msg"
	}
}

function :debug {
	param(
		[parameter(mandatory, position = 0)]
		[string]$msg
	)
	if($log.debug) {
		write-debug "[Dotenv] $msg"
	}
}
