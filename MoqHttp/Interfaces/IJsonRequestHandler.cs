using Newtonsoft.Json.Linq;

namespace MoqHttp.Interfaces
{
	public interface IJsonRequestHandler
	{
		JObject JsonObject { get; set; }
		JObject ReadJSONFromFile(string path);
	}
}