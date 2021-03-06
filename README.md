# Stack Exchange Alexa

Amazon Alexa skill for [Stack Exchange](https://stackexchange.com) / [Stack Overflow](https://stackoverflow.com). 

You can install it on your Echo device [here](https://t.co/TvjgA2L1Ik).

Check your inbox and browse hot questions using voice commands! (Requires [account linking](https://developer.amazon.com/public/solutions/alexa/alexa-skills-kit/docs/linking-an-alexa-user-with-a-user-in-your-system))

Sample dialog:

"Alexa, open Stack Exchange" - "Welcome to Stack Exchange. You have 3 unread messages. Please say: messages, hot questions, help, or I'm done."

"Messages!" - "You have 3 unread messages. First message: Chat reply in 'Tavern on the Meta': Good point. I added a comment to your question. - Second message: Jobs message from Grubhub Seamless. - Third message: Answer on Stack Overflow to your question 
'How to exit the vim editor?'."

"Hot questions!" - "Please say: technology, business, or life!"

"Business!" - "Here's a hot question from 'The Workplace': 'As a developer, what can I do about bad/lack of requirements from the product owner?' Want to hear more?"

# Technical Details

* Written in C# on .NET Core 1.0
* Uses [Tim Heuer](https://twitter.com/timheuer)'s [Alexa.NET](https://www.nuget.org/packages/Alexa.NET) library
* Uses the public [Stack Exchange API](https://api.stackexchange.com). See [StackApps Post](https://stackapps.com/questions/7480).
* Hosted on Amazon Lambda ([setup instructions](https://github.com/MaxHorstmann/StackExchange-Alexa/blob/master/setup.md)).

