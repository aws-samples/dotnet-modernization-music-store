  param(
      [Parameter()]
      [string]$process,

      [Parameter()]
      [string]$githash
  )

$global:S3_EVALUATOR_PATH = "s3://codebuild-us-east-1-871153927703-output-bucket/evaluator/"
$global:S3_BASELINE_PATH = "s3://codebuild-us-east-1-871153927703-output-bucket/coderatcheting/config/baseline/master/"
$global:S3_REPORT_PATH = "s3://codebuild-us-east-1-871153927703-output-bucket/coderatcheting/upgradeassistant/reports/"

function Install-Upgrade-Assistant() {
  Write-Host "Downloading Upgrade Assistant"
  dotnet tool install -g upgrade-assistant --add-source "https://api.nuget.org/v3/index.json"
  upgrade-assistant --version
}

function Install-JQ() {
  Write-Host "Installing Chocolatey"
  Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
  choco
  Write-Host "Installing JQ from Chocolatey"
  chocolatey install jq -y
  jq --version
}

function Install-AWSCLI {
    if ((Get-Command aws -ErrorAction SilentlyContinue) -eq $null) {
        Write-Host "Unable to find aws.exe in your PATH."
        $command = "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12"
        Invoke-Expression $command
        Invoke-WebRequest -Uri "https://awscli.amazonaws.com/AWSCLIV2.msi" -Outfile C:\AWSCLIV2.msi
        $arguments = "/i `"C:\AWSCLIV2.msi`" /quiet"
        Start-Process msiexec.exe -ArgumentList $arguments -Wait
        $env:Path += ';C:\Program Files\Amazon\AWSCLIV2' 
        aws --version
        
    }
}

function Install-Dependencies() {
  Install-Upgrade-Assistant
  Install-JQ
  Install-AWSCLI
}

function Execute-Upgrade-Assistant() {
      Write-Host "Executing upgrade-assistant"
      $p = Start-Process -FilePath upgrade-assistant.exe -ArgumentList "analyze --target-tfm-support LTS .\MvcMusicStore\MvcMusicStore.csproj" -Wait -NoNewWindow -PassThru
      if ($p.ExitCode)
      {
        Write-Error "Failed to run upgrade assistant"
        Exit 1
      }
}

function Generate-Analysis-Report([string]$outputFileName) {
      Write-Host "Extracting current analysis from UA report"
      jq '[.runs[].results[] | select(.locations[].physicalLocation.region.startLine !=null) | {ruleId: .ruleId,message: .message.text,filePath: .locations[].physicalLocation.artifactLocation.uri,lineNumber: .locations[].physicalLocation.region.startLine}]' .\AnalysisReport.sarif > .\$outputFileName.json
      Write-Host "Create a local directory report"
      $path = "report"
      If(!(Test-path -PathType container $path))
      {
          Write-Host "Directory report does not exist, creating..."
          New-Item -ItemType Directory -Path $path
      }
      Copy-Item ".\AnalysisReport.sarif" -Destination $path
      Copy-Item ".\$outputFileName.json" -Destination $path
}

function Sync-File-With-S3([string]$source,[string]$destination, [string]$fileName) {
      Write-Host "Syncing $fileName from $source to $destination"
      aws s3 sync $source $destination --sse "AES256" --exclude "*" --include $fileName
}

function Evaluate-Results() {
     Generate-Analysis-Report -outputFileName "current_analysis_UA"
     Write-Host "Evaluating current analysis with master baseline"
     $q = Start-Process -FilePath .\evaluator\CodeRatchetingEvaluator.exe -ArgumentList "parse --baseline .\evaluator\current_baseline_UA.json --compareWith .\report\current_analysis_UA.json --configFile .\evaluator\config\ratchet_config.yml --tool porting-assistant" -Wait -NoNewWindow -PassThru
#      if($q.ExitCode)
#      {
#       Write-Error "Ratchet detected, Failing build..."
#       Exit 1
#      }
}

function Publish-Results-to-S3([string] $githash) {
     Copy-to-S3 "./report/current_analysis_UA.json" "$S3_REPORT_PATH$githash/current_baseline_UA.json" 
}

function Copy-to-S3([string]$source, [string]$destination) {
     Write-Host "Syncing from $source to $destination"
     aws s3 cp $source $destination --sse "AES256"
}

function Extract-Evaluator() {

    Write-Host "Create a local directory evaluator"
    $path = "evaluator"
    If(!(Test-path -PathType container $path))
    {
        Write-Host "Directory evaluator does not exist, creating..."
        New-Item -ItemType Directory -Path $path
    }
    
     Write-Host "Download Evaluator"
     Sync-File-With-S3 $S3_EVALUATOR_PATH .\ "coderatchetingevaluator.zip"
     Expand-Archive -LiteralPath "coderatchetingevaluatorV2.zip" -DestinationPath .\evaluator
     

     Write-Host "Download baseline file"
     Sync-File-With-S3 $S3_BASELINE_PATH .\evaluator "current_baseline_UA.json"
     
     Write-Host "Download config file"
     Sync-File-With-S3 $S3_EVALUATOR_PATH .\evaluator\config "ratchet_config.yml"

     Write-Host "List items"
     Get-ChildItem -Path "evaluator"
}

function Main() {
  param(
      [Parameter()]
      [string]$process,

      [Parameter()]
      [string]$githash
  )
  Write-Host $process
  Write-Host $githash

  try {
      Write-Host "Executing process switch" + $process

      Switch ($process) {
      "extract-evaluator" { Extract-Evaluator }
      "install-dependencies" { Install-Dependencies }
      "execute-upgrade-assistant" { Execute-Upgrade-Assistant }
      "evaluate-results" { Evaluate-Results }
      "publish-results" { Publish-Results-to-S3 $githash}
      }
  }
  catch {
      Write-Host $_
      throw "Unable to switch process"
  }
}


Main $process $githash
