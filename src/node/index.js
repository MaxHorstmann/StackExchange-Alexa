var APP_ID = "amzn1.ask.skill.03d9d112-9807-48d0-bbea-4715817a5448";

var getApiUrl = function(route) {
    var stackExchangeApiRoot = "https://api.stackexchange.com/2.2/sites/"
    return stackExchangeApiRoot + route;
}

var AlexaSkill = require('./AlexaSkill');

var StackExchangeAlexa = function () {
    AlexaSkill.call(this, APP_ID);
};

// Extend AlexaSkill
StackExchangeAlexa.prototype = Object.create(AlexaSkill.prototype);
StackExchangeAlexa.prototype.constructor = StackExchangeAlexa;

StackExchangeAlexa.prototype.eventHandlers.onSessionStarted = function (sessionStartedRequest, session) {
    console.log("StackExchangeAlexa onSessionStarted requestId: " + sessionStartedRequest.requestId
        + ", sessionId: " + session.sessionId);
    // any initialization logic goes here
};

StackExchangeAlexa.prototype.eventHandlers.onLaunch = function (launchRequest, session, response) {
    console.log("StackExchangeAlexa onLaunch requestId: " + launchRequest.requestId + ", sessionId: " + session.sessionId);
    var speechOutput = "Welcome to Stack Exchange! Say the name of a site, such as cooking, or say all sites for a list.";
    var repromptText = "Pick the name of a site, or say all sites.";
    response.ask(speechOutput, repromptText);
};

StackExchangeAlexa.prototype.eventHandlers.onSessionEnded = function (sessionEndedRequest, session) {
    console.log("StackExchangeAlexa onSessionEnded requestId: " + sessionEndedRequest.requestId
        + ", sessionId: " + session.sessionId);
    // any cleanup logic goes here
};

StackExchangeAlexa.prototype.intentHandlers = {
    // register custom intent handlers
    "StackExchangeIntent": function (intent, session, response) {
        // todo pull all site via SE API

        //var url = getApiUrl("sites");
        response.tell("Cooking, Apple, Server Fault.");
        //response.tellWithCard("Cooking, Apple.", "Cooking, Apple.", "Cooking, Apple.");
    },
    "AMAZON.HelpIntent": function (intent, session, response) {
        response.ask("Browse over 100 expert communities on the Stack Exchange network.", "Browse over 100 expert communities on the Stack Exchange network.");
    }
};

exports.handler = function (event, context) {
    var stackExchangeAlexa = new StackExchangeAlexa();
    stackExchangeAlexa.execute(event, context);
};

