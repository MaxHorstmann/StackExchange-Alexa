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
2. Go to "Alexa Skills Kit" pane and click "get started"
3. Click "Add a new Skill"
4. In the "Skill Information" section, select "Custom Interaction Model" and fill out the other fields
5. In the "Interaction Model" section, paste [IntentSchema.json](speechAssets/IntentSchema.json) and [SampleUtterances.txt](speechAssets/SampleUtterances.txt)
6. In the "Configuration" section, paste the ARN (Amazon Resource Name) of the Lambda function. In the "Account Linking" section, set the following values:
  - Do you allow users to create an account or link to an existing account with you? Yes
  - Authorization URL: https://stackexchange.com/oauth
  - Client Id: *get it from [StackApps.com](https://stackapps.com)*
  - Domain List: stackexchange.com
  - Scope: create four scopes: "read_inbox", "no_expiry", "write_access", and "private_info"  TODO might not need all 4
  - Authorization Grant Type: Auth Code Grant
  - Access Token URI: https://stackexchange.com/oauth/access_token/json
  - Client Secret: *get it from [StackApps.com](https://stackapps.com)*
  - Client Authentication Scheme: Credentials in request body
  - Privacy Policy URL: https://stackexchange.com/legal/privacy-policy
7. Test & publish it






