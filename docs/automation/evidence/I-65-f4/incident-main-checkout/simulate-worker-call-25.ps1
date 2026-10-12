param([Parameter(Mandatory = $true)][string]$Path)
# Reproduccion de la llamada 25 del Worker de F4-T4 intento 2 (R20261011T202738Z-6356) sobre una COPIA en el scratchpad.
# Unica diferencia con el original: $p es la ruta absoluta de la copia (el original usaba "RackCad.sln" relativo) y no hay git.
$p = $Path
$t = [IO.File]::ReadAllText($p)
$nl = if ($t.Contains("`r`n")) { "`r`n" } else { "`n" }
$old = "AEC8BEFC-81CB-417B-8CF9-7D27DF921852"
$new = "5C7B2D1E-3A94-4F6B-8E21-9D0A47C3B6F5"
$lines = $t -split $nl
$out = New-Object System.Collections.Generic.List[string]
$i = 0
while ($i -lt $lines.Count) {
  $l = $lines[$i]
  $out.Add($l)
  if ($l -like 'Project("{FAE04EC0*RackCad.StructuralSections.Import*') {
    $out.Add($lines[$i+1]); $i++
    $out.Add('Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "RackCad.StructuralAnalysis.RamTool", "tools\RackCad.StructuralAnalysis.RamTool\RackCad.StructuralAnalysis.RamTool.csproj", "{' + $new + '}"')
    $out.Add('EndProject')
  }
  elseif ($l -like "*{$old}.Release|x86.Build.0*") {
    foreach ($c in @('Debug|Any CPU.ActiveCfg = Debug|Any CPU','Debug|Any CPU.Build.0 = Debug|Any CPU','Debug|x64.ActiveCfg = Debug|Any CPU','Debug|x64.Build.0 = Debug|Any CPU','Debug|x86.ActiveCfg = Debug|Any CPU','Debug|x86.Build.0 = Debug|Any CPU','Release|Any CPU.ActiveCfg = Release|Any CPU','Release|Any CPU.Build.0 = Release|Any CPU','Release|x64.ActiveCfg = Release|Any CPU','Release|x64.Build.0 = Release|Any CPU','Release|x86.ActiveCfg = Release|Any CPU','Release|x86.Build.0 = Release|Any CPU')) { $out.Add("`t{$new}.$c") }
  }
  elseif ($l -like "*{$old} = {AE66CB82*") {
    $out.Add("`t{$new} = {AE66CB82-AE70-412D-B03B-38ACB3CA8D2F}")
  }
  $i++
}
[IO.File]::WriteAllText($p, ($out -join $nl))
