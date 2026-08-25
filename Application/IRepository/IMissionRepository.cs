using Domain.Mission.Types;

namespace Application.IRepository;

public interface IMissionRepository
{
    public void CreateDirectoriesAndFolders(string missionPath, string missionTemplatePath, bool hasExpansion);
    public TJsonFile? GetJsonFile<TJsonFile>(string filePath);
    public TXmlFile? GetXmlFile<TXmlFile>(string filePath);
    public string GetInitFile(string filePath);
    public void SaveJsonFile<TJsonFile>(string filePath, TJsonFile jsonFile);
    public void SaveXmlFile<TXmlFile>(string filePath, TXmlFile xmlFile);
    public void SaveInitFile(string filePath, string initFile);
    public List<string> GetAllCustomTypesFiles(string folderPath);
    public void CopyVanillaMissionFolder(string missionPath, string vanillaMissionPath, string backupPath);
    public void CopyExpansionTemplateFiles(string expansionTemplatePath, string missionPath, string oldMissionPath);
    public void CopyMissionTemplateFiles(string missionTemplatePath, string missionPath);
    public void CopyPersistenceData(string missionPath, string oldMissionPath);
    public void MoveOldMission(string oldPath, string backupPath);
    public string DownloadExpansionTemplates(string mapName);
}