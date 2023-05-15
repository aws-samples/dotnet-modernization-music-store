const fs = require('fs');
const { stringify } = require('querystring');

const outDir = process.argv[2];
const engine = process.argv[3];
const projectName = process.argv[4];


function aggregateRuleResults() {

    console.log('Aggregating rules for %s', projectName);
    var prefix = getEnginePrefix(engine);
    var filepath = './'+outDir+'/' + 'current_' + prefix + '.json';
    var outpath = './'+outDir+'/' + 'ruleListByCount_' + prefix + '.json';

    var encodingType = getEncoding(filepath);
    var rawData = fs.readFileSync(filepath, encodingType, (err, data) => {
        if (err) {
          console.log(err);
          return;
        }

        return(data);

      });

    var objects = JSON.parse(rawData.trim());
    switch(engine) {
      case 'porting-assistant':
        var filteredData = objects.filter(object => object.Compatibility == 'INCOMPATIBLE');
        hash = filteredData.reduce((accumulator,current) => (accumulator[current.CodeEntityType+':'+current.Signature] ? accumulator[current.CodeEntityType+':'+current.Signature].push(current) : accumulator[current.CodeEntityType+':'+current.Signature] = [current],accumulator) ,{});
        break;
      case 'upgrade-assistant':
        hash = objects.reduce((accumulator,current) => (accumulator[current.ruleId] ? accumulator[current.ruleId].push(current) : accumulator[current.ruleId] = [current],accumulator) ,{});
        break;

      default:
        console.warn("Engine not defined");
    }

    var datetime = Date.now();
    newData = Object.keys(hash).map(key =>
        (
            {
                name: "coderatchet",
                value: hash[key].length,
                timestamp: datetime,
                type: "gauge",
                attributes: {
                    "engine":engine,
                    "project" : projectName,
                    "ruleName" : nameTrimmer(key, 150)
                }
            }

        )
    );

    var metricjson = [{metrics : newData}]

    metricjson.sort(function (a,b) {
      return b.count - a.count;
    });

    fs.writeFile(outpath, JSON.stringify(metricjson), (err) => err && console.error(err));

    return metricjson
}


function nameTrimmer(name = '', limit = 0) {
  if(name.length > limit)
  {
    return name.substring(0, limit/3) + ('...') + name.substring(name.length - limit/3)
  }
  return name
}


function main(){
  console.warn("Engine not defined");
    //var metricjson = aggregateRuleResults();
}

main();
