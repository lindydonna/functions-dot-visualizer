FROM microsoft/aspnetcore:1.1
ARG source
WORKDIR /app
RUN mkdir obj && mkdir obj/Docker && mkdir obj/Docker/publish
COPY ${source:-obj/Docker/publish} .
EXPOSE 80

RUN apt-get update && apt-get install -y graphviz 

ENTRYPOINT ["dotnet", "VisualizerWebApp-NetCore.dll"]

