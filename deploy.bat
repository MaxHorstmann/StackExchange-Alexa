cd src
dotnet restore 
dotnet lambda deploy-function --region us-east-1 -c Release -f netcoreapp1.1 -frun dotnetcore1.1 -fh StackExchange.Alexa::StackExchange.Alexa.Service::GetResponse -fn StackExchangeAlexaResponse -fms 512 -ft 15 -frole LambdaRole
cd ..
