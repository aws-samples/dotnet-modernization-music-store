  param(
      [Parameter()]
      [string]$process,

      [Parameter()]
      [string]$githash
  )

$global:S3_EVALUATOR_PATH = "s3://codebuild-us-east-1-871153927703-output-bucket/evaluator/"
$global:S3_BASELINE_PATH = "s3://codebuild-us-east-1-871153927703-output-bucket/coderatcheting/config/baseline/master/"
$global:S3_REPORT_PATH = "s3://codebuild-us-east-1-871153927703-output-bucket/coderatcheting/portingassistant/reports/"

function Bootstrap-Porting-Assistant() {
    Extract-Evaluator  
}

function Sync-File-With-S3([string]$source, [string]$destination, [string]$fileName) {
    Write-Host "Syncing from $source to $destination"
    aws s3 sync $source $destination --sse-kms-key-id %s3_kms_key% --sse "aws:kms" --exclude "*" --include $fileName
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
     Sync-File-With-S3 $S3_EVALUATOR_PATH .\ "coderatchetingevaluatorV2.zip"
     Expand-Archive -LiteralPath "coderatchetingevaluatorV2.zip" -DestinationPath .\evaluator 

     Write-Host "Download Porting Assistant Client"
     Sync-File-With-S3 $S3_EVALUATOR_PATH .\evaluator "PortingAssistant.Client.CLI.exe"
     
     Write-Host "Download baseline file"
     Sync-File-With-S3 $S3_BASELINE_PATH .\evaluator\config "current_baseline_PA.json"

     Write-Host "List items"
     Get-ChildItem -Path "evaluator"
}

function Execute-Porting-Assistant() {
   if(Test-Path ".\evaluator\PortingAssistant.Client.CLI.exe") {
       #New-Item "reports" -ItemType Directory
       #$p = Start-Process -FilePath .\evaluator\PortingAssistant.Client.CLI.exe -ArgumentList "assess -s MvcMusicStore.sln -o reports" -Wait -NoNewWindow -PassThru
       .\evaluator\PortingAssistant.Client.CLI.exe assess --solution-path=D:\a\dotnet-modernization-music-store\dotnet-modernization-music-store\MvcMusicStore.sln --output-path=.
   }
   else {
       Write-Error ".\PortingAssistant.Client.CLI.exe not found"
       Exit 1
   }
}

function Publish-Results-to-S3([string] $githash) {
     Copy-to-S3 "./current_analysis_PA.json" "$S3_REPORT_PATH$githash/current_baseline_PA.json" 
}

function Copy-to-S3([string]$source, [string]$destination) {
     Write-Host "Syncing from $source to $destination"
     aws s3 cp $source $destination --sse "AES256"
}

function Evaluate-Results() {
    Write-Host "Evaluating the result with current baseline for Porting Assistant Engine"
    $p = Start-Process -FilePath .\Evaluator\CodeRatchetingEvaluator.exe -ArgumentList "parse --baseline .\evaluator\config\current_baseline_PA.json --compareWith .\current_analysis_PA.json --configFile .\evaluator\config\ratchet_config.yml --tool porting-assistant" -Wait -NoNewWindow -PassThru
    if($q.ExitCode)
     {
      Write-Error "Ratchet detected, Failing build..."
      Exit 1
     }
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
      "bootstrap-porting-assistant" { Bootstrap-Porting-Assistant }
      "execute-porting-assistant" { Execute-Porting-Assistant }
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
