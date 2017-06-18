cd src
dotnet restore
dotnet lambda deploy-function --region us-east-1 -c Release -f netcoreapp1.0 -frun dotnetcore1.0 -fh MyProject::getStackExchange.Alexa.Service::GetResponse -fn StackExchangeAlexaResponse -fms 512 -ft 15 -frole LambdaRole
cd ..
