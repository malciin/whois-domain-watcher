FROM mcr.microsoft.com/dotnet/sdk:8.0 as sdk

# Instaling required prerequisites
# https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=net7%2Clinux-ubuntu#tabpanel_2_linux-ubuntu
RUN apt update && apt install -y clang zlib1g-dev

WORKDIR /src
COPY src/ .

RUN dotnet publish DomainWatcher.Cli --output build
RUN rm build/*.pdb && rm build/*.dbg

FROM ubuntu:24.04 as runtime

COPY --from=sdk /src/build/. /app
WORKDIR /app

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
EXPOSE 80
CMD [ "./DomainWatcher.Cli", "--port=80" ]
