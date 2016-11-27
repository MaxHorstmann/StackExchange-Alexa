// var request = require('request');
// request('https://api.stackexchange.com/2.2/sites', function (error, response, body) {
//     if (!error && response.statusCode == 200) {
//       //var info = JSON.parse(body);
//       console.log(body);
//     }
// })


var request = require('request'), zlib = require('zlib');

var headers = {
  'Accept-Encoding': 'gzip'
};

request({url:'https://api.stackexchange.com/2.2/sites', 'headers': headers})
    .pipe(zlib.createGunzip()) // unzip
    .pipe(process.stdout); // do whatever you want with the stream

    