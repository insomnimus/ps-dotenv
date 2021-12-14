function :eq {
	param(
		[parameter(mandatory, position = 0)]
		[string]$a,
		[parameter(mandatory, position = 1)]
		[string]$b
	)
	if($global:IsWindows) {
		$a -ieq $b
	} else {
		$a -ceq $b
	}
}

function :contains {
	param(
		[parameter(mandatory, position = 0)]
		[string[]]$items,
		[parameter(mandatory, position = 1)]
		[string]$s
	)
	foreach($x in $items) {
		if(script::eq $s $x) {
			return $true
		}
	}
	$false
}

function :dedup {
	param(
		[parameter(mandatory, position = 0)]
		[string[]]$arr
	)
	$items = [System.Collections.Generic.List[string]]::new($arr.length)
	foreach($x in $arr) {
		if(-not (script::contains $items $x)) {
			$items.add($x)
		}
	}
	$items
}
