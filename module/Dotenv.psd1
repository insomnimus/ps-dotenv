@{
	RootModule = "Dotenv.psm1"
	ModuleVersion = "0.1.0"
	Author = "Taylan Gökkaya<insomnimus.dev@gmail.com>"
	CompatiblePSEditions = @("Core")
	GUID = '3bb5d7a3-985c-4d74-a329-8dddb075e322'
	PowerShellVersion = "6.0"

	FunctionsToExport = @(
		"Update-Dotenv"
		"Enable-Dotenv"
		"Disable-Dotenv"
		"Show-Dotenv"
		"Register-DotenvName"
		"Unregister-DotenvName"
		"Set-DotenvLogLevel"
		"Get-DotenvLogLevel"
	)
	CmdletsToExport = @("Read-Dotenv")
	VariablesToExport = @()
	AliasesToExport = @()

	NestedModules = @(
		"Dotenv.dll"
		"Dotenv.Logging.ps1"
	)
}
