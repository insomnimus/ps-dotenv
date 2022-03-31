# Basics
```bash
# Lines starting with # are comments and are ignored.
# Entries are key-value pairs separated by `=`
# Keys can be preceded with `export` followed by a space, `export` will be ignored.
# This is to be more compatible with POSIX shells.
#
# The escape character is `\`.
# The line continuation is the same as in bash.
export MY_VAR=some_value
# You can have spaces surrounding the `=` symbol:
MY_VAL = something_else

# Right hand side of an entry can be quoted:
SOME_VAR = "now you can put spaces!"
SOME_VAR = 'same with above'

# Quoted strings can span many lines.
MULTI = "This is
a multiline
string"
# Same works with single quotes as well.

# All entries below are valid:
# value: "word1 word2 word3"
asdf = word1\ word2\ word3
# value: "singleline"
asdf = single\
line
# value: "like in bash"
asdf = like" "in' bash'
# value: "also works"
asdf = "also"' works'

# If the right hand side of `=` is immediately preceded by a hash sign (#)
# the text is treated as a word, like in bash
# not comment (value is "#thisIsOk"):
asdf=#thisIsOk
# Is a comment and the value is empty
asdf = #thisIsNotOk

# All below will unset the variable "asdf":
asdf =
asdf = ""
asdf = ''
```

# Parameter Expansion
A subset of bash parameter expansion is supported.

```bash
# Unquoted strings are subject to expansion:
FOO=$PATH # Unlike in bash, you don't need to quote the value even if it contains spaces!
# Double quoted strings are also subject to expansion.
# You can also reference a value set by the same file:
FOO_PATH="$FOO"
FOO_PATH=${FOO_PATH}

# Single quoted strings are verbatem; `$` and `\` have no special meaning.
asdf=1234
not_1234='$asdf' # value: $asdf

# Bashes `${x-default}` is supported.
# It will try to use `x`, if it's not set, the value will evaluate to `default`.
asdf=${non_existant-the default value} # value: the default value
asdf=${non_existant:-the default value} # value: exactly same as above
# Expansions are allowed after the operator (`:-`, `-`, `:+` etc):
i_exist=1234
asdf=${non_existant-1234 is $i_exist} # value: 1234 is 1234

# Bashes `${x+replacement}` is supported.
# It will evaluate to the part after `+`, if `x` is set. Otherwise it will evaluate to "".
asdf="${i_exist+no longer!}" # value: no longer!
asdf="${non_existant+no longer!}" # value: (empty)
# Again, expansions are allowed after `+`.

# Finally, bashes `${x?error message}` is also supported.
# It will raise an error if `x` is not set, with an optional custom message after `?`.
asdf=${non_existant?you did not set '$non_existant'} # value: (error: you did not set $non_existant)
# These errors will stop parsing the file entirely and revert any changes to the environment.

# You can escape the closing right brace (}) too:
asdf=${someVar:-this is right brace: \}}
# And the right hand side of an operator can use quotation like in bash:
asdf=${qwer:?"where is qwer???"}
# These work too:
asdf="${qwer:?"where is qwer????"}"
asdf=${qwer:+
"wow"
${nested?}
}
```
