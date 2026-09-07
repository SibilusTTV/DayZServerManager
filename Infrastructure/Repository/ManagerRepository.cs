using Application.IRepository;
using Domain.Constants;

namespace Infrastructure.Repository;

public class ManagerRepository : IManagerRepository
{
    public void CreateFolders()
    {
        if (!Directory.Exists(Folders.DeployFolderName))
        {
            Directory.CreateDirectory(Folders.DeployFolderName);
        }

        if (!Directory.Exists(Folders.ServersFolderName))
        {
            Directory.CreateDirectory(Folders.ServersFolderName);
        }

        if (!Directory.Exists(Folders.BackupsFolderName))
        {
            Directory.CreateDirectory(Folders.BackupsFolderName);
        }
    }
}