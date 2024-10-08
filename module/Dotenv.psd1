@{
	RootModule = "Dotenv.psm1"
	ModuleVersion = "1.1.0"
	Author = "Taylan Gökkaya<insomnimus@proton.me>"
	CompatiblePSEditions = @("Core")
	GUID = '3bb5d7a3-985c-4d74-a329-8dddb075e322'
	PowerShellVersion = "6.0"

	FunctionsToExport = @(
		"Update-Dotenv"
		"Register-DotenvName"
		"Unregister-DotenvName"
		"Enable-Dotenv"
		"Disable-Dotenv"
		"Approve-DotenvFile"
		"Approve-DotenvDir"
		"Deny-DotenvFile"
		"Add-DotenvPattern"
		"Remove-DotenvPattern"
	)
	CmdletsToExport = @("Read-Dotenv")
	VariablesToExport = @("Dotenv")
	AliasesToExport = @()

	NestedModules = @("Dotenv.dll")
}
