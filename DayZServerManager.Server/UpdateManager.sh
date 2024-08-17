rm -rf DayZServerManager.Server DayZServerManager.Server.pdb appsettings.Development.json appsettings.json libgit2-a418d9d.so wwwroot/
wget https://github.com/SibilusTTV/DayZServerManager/releases/latest/download/linux.zip
unzip linux.zip
chmod +x DayZServerManager.Server
rm -rf linux.zip
screen -S dayz1 ./DayZServerManager.Server