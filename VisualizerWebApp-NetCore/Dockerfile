FROM microsoft/aspnetcore:1.1
ARG output
COPY ${output} /app
EXPOSE 80

WORKDIR /app
RUN ls && apt-get update && apt-get install -y graphviz 

ENTRYPOINT ["dotnet", "VisualizerWebApp-NetCore.dll"]

