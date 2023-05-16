const { writeFile } = require('fs');
const objs = []
const fs = require('fs');
const process = require('process');

const engine = 'porting-assistant';
const outpath = './current_analysis_PA.json';
const filepath = './MvcMusicStore-analyze/solution-analyze/MvcMusicStore/MvcMusicStore-api-analysis.json'

console.log(filepath);

if (fs.existsSync(filepath)) {
    console.log("Output from Porting Assistant found at:" +filepath+ ". Proceeding.." )
}
else {
     console.log("File does not exist")
     process.exit()
}

var rawData = fs.readFileSync(filepath, 'utf8', (err, data) => {
  if (err) {
    console.error(err);
    return;
  }

  return(data);

});

let jsonData = JSON.parse(rawData)

for (var i in jsonData['SourceFileAnalysisResults']){

    for (var y in jsonData['SourceFileAnalysisResults'][i]['ApiAnalysisResults'] ){
        if (jsonData['SourceFileAnalysisResults'][i]['ApiAnalysisResults'][y]['CompatibilityResults']['net6.0']['Compatibility'] != "COMPATIBLE")
        var doc = {
            'eventType' : "CodeRatchet",
            'engine' : engine,
            'SourceFile': jsonData['SourceFileAnalysisResults'][i]['SourceFileName'],
            'SourceFilePath' : jsonData['SourceFileAnalysisResults'][i]['SourceFilePath'],
            'CodeEntityType' :jsonData['SourceFileAnalysisResults'][i]['ApiAnalysisResults'][y]['CodeEntityDetails']['CodeEntityType'],
            'Signature' :jsonData['SourceFileAnalysisResults'][i]['ApiAnalysisResults'][y]['CodeEntityDetails']['Signature'],
            'Compatibility' :jsonData['SourceFileAnalysisResults'][i]['ApiAnalysisResults'][y]['CompatibilityResults']['net6.0']['Compatibility']
        }

        if (doc != null) {
            objs.push(doc)
        }

    }
}

writeFile(outpath, JSON.stringify(objs, null, 2), (error) => {
    if (error) {
      console.log('An error has occurred ', error);
      return;
    }
    console.log('Data written successfully to disk');
  });
