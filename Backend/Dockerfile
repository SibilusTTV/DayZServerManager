FROM mcr.microsoft.com/dotnet/aspnet:10.0
USER 1000:1000
WORKDIR /app
COPY Build/linux/wwwroot/* ./wwwroot/
COPY Build/linux/* ./
EXPOSE 5172
ENTRYPOINT ["./Backend"]