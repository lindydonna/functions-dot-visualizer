FROM microsoft/aspnetcore:1.1
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-bin/Release/PublishOutput} .

WORKDIR /app
RUN ls && apt-get update && apt-get install -y graphviz 

ENTRYPOINT ["dotnet", "VisualizerWebApp-NetCore.dll"]

