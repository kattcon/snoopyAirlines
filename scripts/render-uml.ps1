param(
    [string]$InputFile = "$PSScriptRoot\..\backend\ulm.puml",
    [ValidateSet("svg", "png", "pdf")]
    [string]$Format = "svg"
)

$ErrorActionPreference = "Stop"

$plantUmlJar = "$PSScriptRoot\plantuml.jar"

if (-not (Get-Command java -ErrorAction SilentlyContinue)) {
    throw "Java is required to render PlantUML. Install Java, then run this script again."
}

if (-not (Test-Path -LiteralPath $plantUmlJar)) {
    Write-Host "Downloading PlantUML..."
    Invoke-WebRequest `
        -Uri "https://github.com/plantuml/plantuml/releases/latest/download/plantuml.jar" `
        -OutFile $plantUmlJar
}

if (-not (Get-Command dot -ErrorAction SilentlyContinue)) {
    Write-Warning "Graphviz 'dot' was not found on PATH. If rendering fails, install Graphviz: winget install Graphviz.Graphviz"
}

java -jar $plantUmlJar "-t$Format" $InputFile

$outputFile = [System.IO.Path]::ChangeExtension($InputFile, $Format)
Write-Host "Rendered: $outputFile"
