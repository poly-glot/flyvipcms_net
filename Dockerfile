FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

COPY .editorconfig Directory.Build.props ./
COPY src/FlyVip.Web/FlyVip.Web.csproj src/FlyVip.Web/
RUN dotnet restore src/FlyVip.Web -a $TARGETARCH -p:SelfContained=true -p:PublishTrimmed=true

COPY src/FlyVip.Web/ src/FlyVip.Web/
RUN dotnet publish src/FlyVip.Web -c Release -a $TARGETARCH --self-contained -p:PublishTrimmed=true --no-restore -o /app
RUN cd /app && rm createdump libmscordaccore.so libmscordbi.so libcoreclrtraceptprovider.so libclrgc*.so

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./FlyVip.Web"]
