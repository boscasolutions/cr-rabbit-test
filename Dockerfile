FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .

RUN ls
RUN dotnet restore "RabbitMQTester/RabbitMQTester.csproj" --configfile nuget.config
WORKDIR "/src/RabbitMQTester"
RUN dotnet build "RabbitMQTester.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RabbitMQTester.csproj" -c Release -o /app/publish

FROM base AS final

RUN apt-get update && apt-get -y install \
vim \
krb5-config --fix-missing \
krb5-user

WORKDIR /app
COPY --from=publish /app/publish .
COPY krb5.conf /etc/krb5.conf

# Install the agent
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
&& echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& wget https://download.newrelic.com/548C16BF.gpg \
&& apt-key add 548C16BF.gpg \
&& apt-get update \
&& apt-get install -y newrelic-netcore20-agent

# Enable the agent
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent \
CORECLR_PROFILER_PATH=/usr/local/newrelic-netcore20-agent/libNewRelicProfiler.so \
IS_DOCKER=True


ENTRYPOINT ["dotnet", "RabbitMQTester.dll"]