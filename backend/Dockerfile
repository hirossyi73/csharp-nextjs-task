FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

# ホットリロード設定
ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

EXPOSE 5000

ENTRYPOINT ["dotnet", "watch", "run", "--project", "src/TaskFlow.Api/TaskFlow.Api.csproj"]
