FROM microsoft/aspnetcore:1.1
ARG source
WORKDIR /app
COPY bin/release/netcoreapp1.1 .
EXPOSE 80

RUN apt-get update && apt-get install -y graphviz 

ENTRYPOINT ["dotnet", "VisualizerWebApp-NetCore.dll"]

