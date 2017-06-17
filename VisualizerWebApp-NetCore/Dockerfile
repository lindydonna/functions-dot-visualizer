FROM microsoft/aspnetcore:1.1
ARG output
WORKDIR /app
COPY ${output} .
EXPOSE 80

RUN ls && apt-get update && apt-get install -y graphviz 

ENTRYPOINT ["dotnet", "VisualizerWebApp-NetCore.dll"]

