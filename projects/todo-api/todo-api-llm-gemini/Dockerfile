## APP BUILDER
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Args
ARG distFolder=TodoApi/bin/app
ARG apiProtocol=http	
ARG apiPort=80
ARG appFile=TodoApi.dll
 
# Copy files to /app
RUN ls
COPY ${distFolder} /app

# Expose port for the Web API traffic
EXPOSE ${apiPort}

# Run application
WORKDIR /app
RUN ls
ENV appFile=$appFile
ENTRYPOINT dotnet $appFile