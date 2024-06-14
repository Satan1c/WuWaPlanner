using System.Text;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using WuWaPlanner.Controllers;
using WuWaPlanner.Models.KuroGamesService;
using File = Google.Apis.Drive.v3.Data.File;

namespace WuWaPlanner.Services;

public class GoogleDriveService(IGoogleAuthProvider authProvider, JsonSerializerSettings jsonSettings)
{
	private const string c_fileName        = "WuWa_User.json";
	private const string c_appDataFolder   = "appDataFolder";
	private const string c_applicationName = "AftertaleAU";
	private const string c_contentType     = "application/json";

	private static readonly string[]            s_parents      = [c_appDataFolder];
	private static readonly File                s_fileData     = new() { Parents = s_parents, Name = c_fileName };
	private readonly        IGoogleAuthProvider m_authProvider = authProvider;

	private readonly byte[] m_encodedEmptyData
			= Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Dictionary<string, string>(), jsonSettings));

	private readonly JsonSerializerSettings m_jsonSettings = jsonSettings;

	public async ValueTask<DriveService?> AuthenticateService()
	{
		var cred = await m_authProvider.GetCredentialAsync();

		return new DriveService(new BaseClientService.Initializer { HttpClientInitializer = cred, ApplicationName = c_applicationName });
	}

	public async ValueTask CreateData()
	{
		var service = await AuthenticateService();
		if (service is null) return;

		await CreateData(service);
	}

	public async ValueTask CreateData(DriveService service)
	{
		using var stream = new MemoryStream(m_encodedEmptyData);
		await CreateData(service, stream);
	}

	public async ValueTask CreateData(DriveService service, MemoryStream dataStream)
	{
		await service.Files.Create(s_fileData, dataStream, c_contentType).UploadAsync().ConfigureAwait(false);

		await PrepareFileRequest(service).ExecuteAsync().ConfigureAwait(false);
	}

	public async ValueTask WriteData(SaveData data)
	{
		var service = await AuthenticateService();

		if (service is null) return;

		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, m_jsonSettings)));

		var folder  = await PrepareFileRequest(service).ExecuteAsync();
		var existed = folder.Files.FirstOrDefault();

		if (existed is null)
		{
			await CreateData(service, stream);
			return;
		}

		await service.Files.Update(s_fileData, existed.Id, stream, c_contentType).UploadAsync();
	}

	public async ValueTask<SaveData?> ReadDataOrDefault()
	{
		var service = await AuthenticateService();

		if (service is null) return null;

		return await ReadDataOrDefault(service);
	}

	public async ValueTask<SaveData> ReadDataOrDefault(DriveService service)
	{
		var existed = await ReadData(service);

		if (existed is null) await CreateData(service);

		return existed ?? PullsController.EmptyData;
	}

	public async ValueTask<SaveData?> ReadData()
	{
		var service = await AuthenticateService();

		if (service is null) return null;

		return await ReadData(service);
	}

	public async ValueTask<SaveData?> ReadData(DriveService service)
	{
		var folder = await PrepareFileRequest(service).ExecuteAsync();

		if (folder.Files.Count <= 0) return null;

		using var data = new MemoryStream();
		await service.Files.Get(folder.Files.First().Id).DownloadAsync(data);

		var encoded = Encoding.UTF8.GetString(data.GetBuffer());
		return JsonConvert.DeserializeObject<SaveData>(encoded, m_jsonSettings)!;
	}

	private FilesResource.ListRequest PrepareFileRequest(DriveService service)
	{
		var files = service.Files.List();
		files.Spaces   = c_appDataFolder;
		files.PageSize = 1;

		return files;
	}
}
