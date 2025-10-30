FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
# (Better layer caching) copy solution and project files first
COPY SueChef_Project.sln ./
COPY SueChef/SueChef.csproj SueChef/
# Restore using the actual project path inside the repo
RUN dotnet restore ./SueChef/SueChef.csproj
# now copy the rest of the source
COPY SueChef/. ./SueChef/
# publish self-contained output to /app (framework-dependent is fine too)
RUN dotnet publish ./SueChef/SueChef.csproj -c Release -o /app
# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
# Copy published output
COPY --from=build /app .
# Render sets PORT; Kestrel must bind to 0.0.0.0:<PORT>.
# Do NOT hardcode a number here. Let your app read PORT and bind in code.
# (If you insist on ENV, you'd need a shell entrypoint to expand $PORT.)
# EXPOSE is optional on Render, but harmless:
EXPOSE 10000
# Optional production hints
ENV ASPNETCORE_ENVIRONMENT=Production
# Start the app (make sure the DLL name matches your assembly)
ENTRYPOINT ["dotnet", "SueChef.dll"]