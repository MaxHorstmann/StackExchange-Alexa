# StackExchange-Alexa
Amazon Alexa skill for doing some things on Stack Overflow and the Stack Exchange network

# Set up backend on AWS
1. Sign in to [AWS CodeBuild](https://console.aws.amazon.com)
2. Create a new project
	  - Project name: Stack-Exchange-Alexa
	  - Source provider: GitHub
	  - Repository: Use a repository in my account
	  - Choose a repository: *this repo*
	  - Environment image: Specify a Docker image
	  - Environment type: Linux
	  - Custom image type: Other
	  - Custom image ID: maxhorstmann/dotnetcore-lambda-ci (fyi, [source](https://github.com/MaxHorstmann/dotnetcore-lambda-ci))
	  - Build specification: Use the buildspec.yml in the source code root directory
	  - Artifacts type: No artifacts
	  - Service role: Create a service role in your account
	  - Role name: codebuild-SE-Alexa-service-role
	  - Expand advanced settings, go to Environment variables
	  - Add environment variable: Name="AWS_ACCESS_KEY_ID", Value="*your access key*"
	  - Add environment variable: Name="AWS_SECRET_ACCESS_KEY", Value="*your secret*"
	  - (see [Managing Access Keys for IAM Users](http://docs.aws.amazon.com/IAM/latest/UserGuide/id_credentials_access-keys.html))
3. Kick off a build


# Set up Alexa Skill

