# Dotenv

## Installation and Setup
See the [readme](readme.md).

## The `$Dotenv` variable.
This variable is exported and holds the configuration for the entire module.
You cannot remove or replace it but you can modify its properties, which take effect immediately.

For example, instead of running `Disable-Dotenv`, you can run `$Dotenv.Enabled = $false`.

