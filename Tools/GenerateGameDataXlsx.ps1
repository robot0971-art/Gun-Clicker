param(
    [string]$OutputPath = "Assets/StreamingAssets/GameData.xlsx"
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-ColName {
    param([int]$Index)
    $name = ""
    $i = $Index
    while ($i -gt 0) {
        $i--
        $name = [char](65 + ($i % 26)) + $name
        $i = [math]::Floor($i / 26)
    }
    return $name
}

function Escape-Xml {
    param([string]$Text)
    return [System.Security.SecurityElement]::Escape($Text)
}

function New-Cell {
    param(
        [int]$ColIndex,
        [int]$RowIndex,
        $Value
    )

    $ref = "$(Get-ColName $ColIndex)$RowIndex"
    if ($null -eq $Value) {
        return "<c r=""$ref"" />"
    }

    if ($Value -is [bool]) {
        $v = if ($Value) { 1 } else { 0 }
        return "<c r=""$ref"" t=""b""><v>$v</v></c>"
    }

    if ($Value -is [byte] -or $Value -is [sbyte] -or $Value -is [int16] -or $Value -is [uint16] -or $Value -is [int] -or $Value -is [uint32] -or $Value -is [long] -or $Value -is [uint64] -or $Value -is [float] -or $Value -is [double] -or $Value -is [decimal]) {
        return "<c r=""$ref""><v>$Value</v></c>"
    }

    $text = Escape-Xml ([string]$Value)
    return "<c r=""$ref"" t=""inlineStr""><is><t>$text</t></is></c>"
}

function New-SheetXml {
    param(
        [array]$Rows
    )

    $sheetRows = New-Object System.Collections.Generic.List[string]
    for ($r = 0; $r -lt $Rows.Count; $r++) {
        $rowIndex = $r + 1
        $cells = New-Object System.Collections.Generic.List[string]
        for ($c = 0; $c -lt $Rows[$r].Count; $c++) {
            $cells.Add((New-Cell -ColIndex ($c + 1) -RowIndex $rowIndex -Value $Rows[$r][$c]))
        }
        $sheetRows.Add("<row r=""$rowIndex"">$(($cells -join ''))</row>")
    }

    return @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <sheetData>
    $($sheetRows -join "`n    ")
  </sheetData>
</worksheet>
"@
}

$guns = @(
    @("Id","Name","ClickValue","UnlockClicks","SpriteName","BaseDamage","AttackSpeed","CriticalChance","CriticalMultiplier","EvolveLevel","NextGunId","IsFinalForm"),
    @(0,"Luger",1,0,"Luger",4,1.0,0.05,1.5,8,1,$false),
    @(1,"M92",2,10,"M92",8,1.0,0.08,1.6,16,2,$false),
    @(2,"Revolver",4,25,"Revolver",15,0.9,0.10,1.8,32,3,$false),
    @(3,"M15",8,60,"M15",25,0.8,0.12,1.9,70,4,$false),
    @(4,"MP5",14,120,"MP5",40,0.12,0.15,2.0,140,5,$false),
    @(5,"SawedOffShotgun",24,250,"SawedOffShotgun",70,0.75,0.18,2.5,300,6,$false),
    @(6,"M24",40,500,"M24",120,1.5,0.10,2.2,650,7,$false),
    @(7,"AK47",65,0,"AK47",200,0.15,0.22,2.8,0,-1,$true)
)

$upgrades = @(
    @("Id","GunId","BaseCost","CostMultiplier","ValueMultiplier","MaxLevel"),
    @(0,0,25,1.25,1.10,20),
    @(1,1,75,1.28,1.12,25),
    @(2,2,150,1.30,1.15,30),
    @(3,3,300,1.32,1.18,35),
    @(4,4,600,1.35,1.20,40),
    @(5,5,1200,1.38,1.25,45),
    @(6,6,2500,1.40,1.28,50),
    @(7,7,5000,1.45,1.30,60)
)

$config = @(
    @("Id","Key","ValueInt","ValueFloat","ValueString"),
    @(0,"StartGunIndex",0,0,""),
    @(1,"ClickCooldownMs",0,0.10,""),
    @(2,"AutoSaveSeconds",0,0,"15"),
    @(3,"CriticalColor",0,0,"#FFD54A")
)

$monsters = @(
    @("Id","Name","BaseHP","BaseDefense","ExpReward","HpScaling","SpriteName"),
    @(0,"Slime",10,0,5,1.15,"Slime"),
    @(1,"Dummy",30,0,10,1.20,"Dummy"),
    @(2,"Goblin",80,1,20,1.25,"Goblin"),
    @(3,"Orc",180,2,40,1.28,"Orc"),
    @(4,"Robot",400,4,80,1.32,"Robot"),
    @(5,"Dragon",1000,8,150,1.35,"Dragon")
)

function Build-WorkbookXml {
    @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets>
    <sheet name="Guns" sheetId="1" r:id="rId1"/>
    <sheet name="Upgrades" sheetId="2" r:id="rId2"/>
    <sheet name="Config" sheetId="3" r:id="rId3"/>
    <sheet name="Monsters" sheetId="4" r:id="rId4"/>
  </sheets>
</workbook>
"@
}

function Build-WorkbookRelsXml {
    @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet2.xml"/>
  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet3.xml"/>
  <Relationship Id="rId4" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet4.xml"/>
  <Relationship Id="rId5" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
</Relationships>
"@
}

function Build-RootRelsXml {
    @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>
"@
}

function Build-ContentTypesXml {
    @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet2.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet3.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet4.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
</Types>
"@
}

function Build-StylesXml {
    @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <fonts count="1"><font><sz val="11"/><name val="Calibri"/></font></fonts>
  <fills count="1"><fill><patternFill patternType="none"/></fill></fills>
  <borders count="1"><border/></borders>
  <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
  <cellXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/></cellXfs>
</styleSheet>
"@
}

function Write-Utf8File {
    param(
        [string]$Path,
        [string]$Content
    )

    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.UTF8Encoding]::new($false))
}

$tempRoot = Join-Path $env:TEMP ("GunClickerXlsx_" + [Guid]::NewGuid().ToString("N"))
$null = New-Item -ItemType Directory -Path $tempRoot -Force

try {
    $null = New-Item -ItemType Directory -Path (Join-Path $tempRoot "_rels") -Force
    $null = New-Item -ItemType Directory -Path (Join-Path $tempRoot "xl") -Force
    $null = New-Item -ItemType Directory -Path (Join-Path $tempRoot "xl/_rels") -Force
    $null = New-Item -ItemType Directory -Path (Join-Path $tempRoot "xl/worksheets") -Force

    Write-Utf8File -Path (Join-Path $tempRoot "[Content_Types].xml") -Content (Build-ContentTypesXml)
    Write-Utf8File -Path (Join-Path $tempRoot "_rels/.rels") -Content (Build-RootRelsXml)
    Write-Utf8File -Path (Join-Path $tempRoot "xl/workbook.xml") -Content (Build-WorkbookXml)
    Write-Utf8File -Path (Join-Path $tempRoot "xl/_rels/workbook.xml.rels") -Content (Build-WorkbookRelsXml)
    Write-Utf8File -Path (Join-Path $tempRoot "xl/styles.xml") -Content (Build-StylesXml)

    Write-Utf8File -Path (Join-Path $tempRoot "xl/worksheets/sheet1.xml") -Content (New-SheetXml $guns)
    Write-Utf8File -Path (Join-Path $tempRoot "xl/worksheets/sheet2.xml") -Content (New-SheetXml $upgrades)
    Write-Utf8File -Path (Join-Path $tempRoot "xl/worksheets/sheet3.xml") -Content (New-SheetXml $config)
    Write-Utf8File -Path (Join-Path $tempRoot "xl/worksheets/sheet4.xml") -Content (New-SheetXml $monsters)

    $outputDir = Split-Path -Parent $OutputPath
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    }

    if (Test-Path $OutputPath) {
        Remove-Item -LiteralPath $OutputPath -Force
    }

    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempRoot, $OutputPath)
    Write-Host "Created $OutputPath"
}
finally {
    if (Test-Path $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}
