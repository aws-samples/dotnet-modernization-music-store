  param(
      [Parameter()]
      [string]$process,

      [Parameter()]
      [string]$githash
  )

$global:S3_EVALUATOR_PATH = "s3://codebuild-us-east-1-871153927703-output-bucket/evaluator/"
$global:S3_BASELINE_PATH = "s3://codebuild-us-east-1-871153927703-output-bucket/coderatcheting/config/baseline/master/"
$global:S3_REPORT_PATH = "s3://codebuild-us-east-1-871153927703-output-bucket/coderatcheting/portingassistant/reports/"

function Sync-File-With-S3([string]$source, [string]$destination, [string]$fileName) {
    Write-Host "Syncing from $source to $destination"
    aws s3 sync $source $destination --sse-kms-key-id %s3_kms_key% --sse "aws:kms" --exclude "*" --include $fileName
}

function Execute-Porting-Assistant() {
   if(Test-Path ".\evaluator\config\PortingAssistant.Client.CLI.exe") {
       #New-Item "reports" -ItemType Directory
       #$p = Start-Process -FilePath .\evaluator\PortingAssistant.Client.CLI.exe -ArgumentList "assess -s MvcMusicStore.sln -o reports" -Wait -NoNewWindow -PassThru
       .\evaluator\config\PortingAssistant.Client.CLI.exe assess --solution-path=D:\a\dotnet-modernization-music-store\dotnet-modernization-music-store\MvcMusicStore.sln --output-path=.
   }
   else {
       Write-Error ".\PortingAssistant.Client.CLI.exe not found"
       Exit 1
   }
}

function Check-Key-In-S3([string]$objectKey) {
    Write-Host "Checking if the key $objectKey exists in S3 Bucket"
    aws s3api head-object --bucket "codebuild-us-east-1-871153927703-output-bucket" --key $objectKey
}

function Publish-Results-to-S3([string] $githash) {
     Copy-to-S3 "./current_analysis_PA.json" "$S3_REPORT_PATH$githash/current_baseline_PA.json" 
}
function Sync-File-With-S3([string]$source,[string]$destination, [string]$fileName) {
      Write-Host "Syncing $fileName from $source to $destination"
      aws s3 sync $source $destination --sse "AES256" --exclude "*" --include $fileName
}

function Copy-to-S3([string]$source, [string]$destination) {
     Write-Host "Syncing from $source to $destination"
     aws s3 cp $source $destination --sse "AES256"
}

function Fetch-Baseline([string]$githash)
{
  Check-Key-In-S3 "coderatcheting/portingassistant/reports/${githash}/current_baseline_PA.json"
   if($lastexitcode -eq "0") 
   {

     Write-Host "Baseline found for the commit ID ${githash}"
     Copy-to-S3 $S3_REPORT_PATH$githash"/current_baseline_PA.json" "$S3_BASELINE_PATH/current_baseline_PA.json"

    }
    else
    {
     Write-Host "Baseline not found for the commit ID ${githash}"
     
     Write-Host "Download Porting Assistant Client"
     Sync-File-With-S3 $S3_EVALUATOR_PATH .\evaluator\config "PortingAssistant.Client.CLI.exe"
     Execute-Porting-Assistant
     node ./.github/workflows/PARuleParser.js
     Copy-to-S3 "./current_analysis_PA.json" "$S3_BASELINE_PATH/current_baseline_PA.json" 
     
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
      "fetch-baseline" { Fetch-Baseline $githash}
      }
  }
  catch {
      Write-Host $_
      throw "Unable to switch process"
  }
}


Main $process $githash
