FROM mcr.microsoft.com/dotnet/sdk:6.0

LABEL org.opencontainers.image.authors="Tom Rosewell <tom.rosewell@couchbase.com>"

COPY . /dotnet-docs-repo
WORKDIR /dotnet-docs-repo/tests

RUN apt-get update -y && \
    apt-get install -y \
    jq \
    curl \
    npm

RUN npm install -g bats

RUN dotnet tool install -g dotnet-script
ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT [ "./wait-for-couchbase.sh", "1" ]