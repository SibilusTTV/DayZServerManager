FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY Build/linux/wwwroot/* ./wwwroot/
COPY Build/linux/* ./
EXPOSE 5172
RUN chown -R 1000:1000 ./
RUN dpkg --add-architecture i386 \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        lib32gcc-s1 \
    && rm -rf /var/lib/apt/lists/*
USER 1000:1000
ENTRYPOINT ["./Backend"]