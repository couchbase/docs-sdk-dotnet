FROM mcr.microsoft.com/dotnet/sdk:6.0

LABEL org.opencontainers.image.authors="Hakim Cassimally <hakim.cassimally@couchbase.com>"

RUN apt-get update -y \
    && \
    apt-get install -y \
    curl vim git

# edit these variables as required
ENV PRE_RELEASE_VERSION 3.2.8-pre
ENV PRE_RELEASE_BUILD r5705
ENV PRE_RELEASE_SOURCE http://sdk.jenkins.couchbase.com/job/dotnet/job/sdk/job/couchbase-net-client-scripted-build-pipeline/lastSuccessfulBuild/artifact/couchbase-net-client-${PRE_RELEASE_VERSION}-${PRE_RELEASE_BUILD}.zip

RUN mkdir -p /app/nuget-sources/
WORKDIR /app/nuget-sources/
RUN curl -O ${PRE_RELEASE_SOURCE}
RUN dotnet nuget add source /app/nuget-sources/

WORKDIR /app
RUN dotnet new console
RUN dotnet add package CouchbaseNetClient -v ${PRE_RELEASE_VERSION}

RUN dotnet tool install -g dotnet-script
RUN export PATH="$PATH:/root/.dotnet/tools"

# RUN git clone https://github.com/couchbase/docs-sdk-dotnet.git
# NB: instead we will mount working directory in docker-compose.yml

ENTRYPOINT ["/bin/bash", "-l", "-c"]
CMD ["/bin/bash"]
