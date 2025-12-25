using System.Text.Json;

namespace MoqHttp.Interfaces
{
	public interface IJsonRequestHandler
	{
		JsonElement JsonObject { get; set; }
		void ReadJSONFromFile(string path);
	}
}