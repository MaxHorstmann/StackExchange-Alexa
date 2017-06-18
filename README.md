# StackExchange-Alexa
Amazon Alexa skill for doing some things on Stack Overflow and the Stack Exchange network

# Deploy to AWS Lambda
1. Open the [IAM console](https://console.aws.amazon.com/iam/home#/roles) and create a new role "LambdaRole" based off the "AWS Lambda" service role. No additional policies need to be attached.
1. Install [.NET Core](https://www.microsoft.com/net/core)
2. Install [AWS CLI](https://aws.amazon.com/cli)
3. Run `AWS configure` and provide access keys etc.
4. Run `deploy.bat` (on Windows; port to .sh for Linux/Mac)
5. Open [Lambda Management Console](https://console.aws.amazon.com/lambda), verify that function "StackExchangeAlexaResponse" has been created. Add an "Alexa Skills Kit" trigger to the function (TODO permission issue?)

See my earlier blog post [Continuous Integration: C# to AWS Lambda](http://maxhorstmann.net/blog/2017/05/22/ci-dotnetcore-lambda) for setting up continuous integration in AWS CodeBuild.


# Set up Alexa Skill

1. Open [Amazon Developer Console](https://developer.amazon.com) and navigate to "Alexa" section
