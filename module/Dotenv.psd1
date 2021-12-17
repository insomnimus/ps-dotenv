@{
RootModule = "Dotenv.psm1"
ModuleVersion = "0.2.1"
Author = "Taylan G├╢kkaya<insomnimus.dev@gmail.com>"
CompatiblePSEditions = @("Core")
GUID = '3bb5d7a3-985c-4d74-a329-8dddb075e322'
PowerShellVersion = "6.0"

FunctionsToExport = @(
"Update-Dotenv"
"Register-DotenvName"
"Unregister-DotenvName"
"Enable-Dotenv"
"Disable-Dotenv"
)
CmdletsToExport = @("Read-Dotenv")
VariablesToExport = @("Dotenv")
AliasesToExport = @()

NestedModules = @(
"Dotenv.dll"
)
}
