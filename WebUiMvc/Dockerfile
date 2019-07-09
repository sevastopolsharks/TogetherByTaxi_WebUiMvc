FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["WebUiMvc/WebUiMvc.csproj", "WebUiMvc/"]
RUN dotnet restore "WebUiMvc/WebUiMvc.csproj"
COPY . .
WORKDIR "/src/WebUiMvc"
RUN dotnet build "WebUiMvc.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebUiMvc.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebUiMvc.dll"]