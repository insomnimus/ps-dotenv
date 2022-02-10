# Basics
```bash
# Lines starting with # are comments and are ignored.
# Entries are key-value pairs separated by `=`
# and keys can be preceded by "export ", if $Dotenv.IgnoreExportPrefix is set to $true.
#
MY_VAR=some_value
# You can have spaces surrounding the `=` symbol:
MY_VAL = something_else

# Right hand side of an entry can be quoted:
SOME_VAR = "now you can put spaces!"
SOME_VAR = 'same with above'

# Multiline strings are possible by tripling the surrounding quotation marks:
MULTI = """
This is
a multiline
string
"""

# The terminating triplet of quotes must be alone in a line.
# Same works with single quotes as well.
```

# Parameter Expansion
A subset of bash parameter expansion is supported.

```bash
# Unquoted strings are subject to expansion:
FOO=$PATH # Unlike in bash, you don't need to quote the value even if it contains spaces!
# Double quoted strings, single or multiline, are also subject to expansion.
# You can also reference a value set by the same file:
FOO_PATH="$FOO"
FOO_PATH="""
$FOO
"""

# Single quoted strings, single or multiline, are verbatem; `$` has no speccial meaning.
asdf=1234
not_1234='$asdf' # value: $asdf

# Bashes `${x-default}` is supported.
# It will try to use `x`, if it's not set, the value will evaluate to `default`.
asdf=${non_existant-the default value} # value: the default value
# Expansions are allowed after the `-`:
i_exist=1234
asdf=${non_existant-1234 is $i_exist} # value: 1234 is 1234

# Bashes `${x:replacement}` is supported.
# It will evaluate to the part after `+`, if `x` is set. Otherwise it will evaluate to "".
asdf="${i_exist+no longer!}" # value: no longer!
asdf="${non_existant+no longer!}" # value: (empty)
# Again, expansions are allowed after `+`.

# Finally, bashes `${x?error message}` is also supported.
# It will raise an error if `x` is not set, with an optional custom message after `?`.
asdf=${non_existant?you did not set `non_existant`} # value: (error: you did not set `non_existant`)
# These errors will stop parsing the file entirely and revert any changes to the environment.
```
