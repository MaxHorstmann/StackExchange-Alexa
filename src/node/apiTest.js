var request = require('request');
request('https://api.stackexchange.com/2.2/sites?key=dzqlqab4VD4bynFom)Z1Ng((', function (error, response, body) {
    if (!error && response.statusCode == 200) {
      var info = JSON.parse(body);
      console.log(body);
    }
})


var request = require('request'), zlib = require('zlib');

var headers = {
  'Accept-Encoding': 'gzip'
};

var body = '';
request({url:'https://api.stackexchange.com/2.2/sites?key=dzqlqab4VD4bynFom)Z1Ng((', headers: headers})
    .pipe(zlib.createGunzip()    // unzip
    .on("data", function(buffer) {
    	console.log(buffer);
    	//var part = buffer.read().toString();
    	//body += part;
    })
    .on("end", function() {
    	console.log("done!");
	    var foo = JSON.parse(body);
	    console.log(foo);
    }));

